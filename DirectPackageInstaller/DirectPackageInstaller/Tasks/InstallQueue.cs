using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DirectPackageInstaller.Others;
using ReactiveUI;

namespace DirectPackageInstaller.Tasks
{
    public enum QueueItemStatus
    {
        Pending,
        Analyzing,
        Installing,
        Completed,
        Failed,
        Cancelled
    }

    public class QueueItem : ReactiveObject
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");

        private string _source = string.Empty;
        public string Source
        {
            get => _source;
            set => this.RaiseAndSetIfChanged(ref _source, value);
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _formattedSize = string.Empty;
        public string FormattedSize
        {
            get => _formattedSize;
            set => this.RaiseAndSetIfChanged(ref _formattedSize, value);
        }

        private long _sizeBytes;
        public long SizeBytes
        {
            get => _sizeBytes;
            set => this.RaiseAndSetIfChanged(ref _sizeBytes, value);
        }

        public byte[]? IconData { get; set; }

        private Bitmap? _icon;
        public Bitmap? Icon
        {
            get
            {
                if (_icon == null && IconData != null && IconData.Length > 0)
                {
                    try
                    {
                        using var ms = new MemoryStream(IconData);
                        _icon = Bitmap.DecodeToHeight(ms, 100);
                    }
                    catch { }
                }
                return _icon;
            }
        }

        private QueueItemStatus _status = QueueItemStatus.Pending;
        public QueueItemStatus Status
        {
            get => _status;
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
                this.RaisePropertyChanged(nameof(StatusBadge));
            }
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? FileName : Title;

        public string StatusBadge => Status switch
        {
            QueueItemStatus.Pending => "⏳ Bekliyor",
            QueueItemStatus.Analyzing => "🔍 Taranıyor...",
            QueueItemStatus.Installing => "⚡ Aktarılıyor",
            QueueItemStatus.Completed => "✅ Tamamlandı",
            QueueItemStatus.Failed => "❌ Başarısız",
            QueueItemStatus.Cancelled => "⏹ İptal",
            _ => Status.ToString()
        };

        public DateTime AddedAt { get; } = DateTime.Now;
    }

    public static class InstallQueueManager
    {
        public static ObservableCollection<QueueItem> Items { get; } = new ObservableCollection<QueueItem>();

        private static CancellationTokenSource? _queueCts;
        private static bool _isRunning;
        public static bool IsRunning => _isRunning;

        public static QueueItem? CurrentItem { get; private set; }

        public static event Action<QueueItem>? OnItemStatusChanged;
        public static event Action<double, string, string>? OnProgressUpdated;
        public static event Action<QueueItem, string>? OnStorageErrorOccurred;

        public static QueueItem? Add(string source, string? title = null, string? size = null, byte[]? iconData = null, long sizeBytes = 0, bool insertAtTop = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            // Zaten kuyrukta varsa ve insertAtTop istendiyse en başa taşı
            var existing = Items.FirstOrDefault(x => string.Equals(x.Source, source, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (insertAtTop)
                {
                    Items.Remove(existing);
                    if (existing.Status == QueueItemStatus.Failed || existing.Status == QueueItemStatus.Cancelled)
                    {
                        existing.Status = QueueItemStatus.Pending;
                        existing.ErrorMessage = null;
                        existing.Progress = 0;
                    }
                    Items.Insert(0, existing);
                }
                return existing;
            }

            string name = Path.GetFileName(source.Split('?').First());
            if (string.IsNullOrEmpty(name))
                name = source;

            if (sizeBytes <= 0 && File.Exists(source))
            {
                try { sizeBytes = new FileInfo(source).Length; } catch { }
            }

            var item = new QueueItem
            {
                Source = source,
                FileName = name,
                Title = title ?? name,
                FormattedSize = size ?? (sizeBytes > 0 ? sizeBytes.ToFileSize() : string.Empty),
                SizeBytes = sizeBytes,
                IconData = iconData,
                Status = QueueItemStatus.Pending
            };

            if (insertAtTop)
                Items.Insert(0, item);
            else
                Items.Add(item);

            return item;
        }

        public static void Remove(QueueItem item)
        {
            Items.Remove(item);
        }

        public static void Clear()
        {
            if (!_isRunning)
                Items.Clear();
        }

        public static void Stop()
        {
            _queueCts?.Cancel();
            _isRunning = false;
            CurrentItem = null;
        }

        public static void ReportProgress(double percent, string speed, string transferred)
        {
            if (CurrentItem != null)
            {
                CurrentItem.Progress = percent;
            }
            OnProgressUpdated?.Invoke(percent, speed, transferred);
        }

        public static async Task WaitForTransferCompletionAsync(CancellationToken ct)
        {
            if (Installer.Server == null)
                return;

            // 1. Konsolun PC sunucusuna bağlanmasını bekle (en fazla 15 saniye)
            int waitConnectCount = 0;
            while (!ct.IsCancellationRequested && Installer.Server.Connections == 0 && waitConnectCount < 30)
            {
                await Task.Delay(500, ct);
                waitConnectCount++;

                if (Installer.Server.LastTransferProgress?.Completed == true)
                    break;
            }

            // 2. Konsol indirmeyi sürdürdüğü sürece (bağlantı açıkken) bekle
            while (!ct.IsCancellationRequested && 
                   (Installer.Server.Connections > 0 || (Installer.Server.Decompress?.Tasks.Any(x => x.Value.Running) ?? false)))
            {
                await Task.Delay(1000, ct);
            }

            // 3. Paket geçişleri arasında konsol servisinin rahatlaması için kısa bekleme
            if (!ct.IsCancellationRequested)
            {
                await Task.Delay(2000, ct);
            }
        }

        public static async Task RunQueueAsync(Settings config, Func<string, Task<bool>> installFunc)
        {
            if (_isRunning) return;

            _isRunning = true;
            _queueCts = new CancellationTokenSource();
            var ct = _queueCts.Token;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var nextItem = Items.FirstOrDefault(x => x.Status == QueueItemStatus.Pending);
                    if (nextItem == null)
                        break;

                    CurrentItem = nextItem;

                    // 1. Kurulum öncesi depolama alanı kontrolü
                    if (!string.IsNullOrEmpty(config.PSIP) && config.PSIP != "0.0.0.0" && nextItem.SizeBytes > 0)
                    {
                        var spaceCheck = await ConsoleStorageService.CheckStorageBeforeInstallAsync(config.PSIP, nextItem.SizeBytes);
                        if (!spaceCheck.HasEnoughSpace)
                        {
                            nextItem.Status = QueueItemStatus.Failed;
                            nextItem.ErrorMessage = "Yetersiz Disk Alanı!";
                            OnItemStatusChanged?.Invoke(nextItem);
                            OnStorageErrorOccurred?.Invoke(nextItem, spaceCheck.Message);
                            break; // Disk dolu olduğunda sonraki oyunları boşuna denemeyip durdur
                        }
                    }

                    nextItem.Status = QueueItemStatus.Installing;
                    OnItemStatusChanged?.Invoke(nextItem);

                    try
                    {
                        bool success = await installFunc(nextItem.Source);
                        if (success)
                        {
                            // 2. Aktarımın tamamlanmasını bekle (PS4'ün HTTP üzerinden indirmeyi bitirmesi)
                            await WaitForTransferCompletionAsync(ct);

                            if (!ct.IsCancellationRequested)
                            {
                                nextItem.Status = QueueItemStatus.Completed;
                                nextItem.Progress = 100.0;
                            }
                        }
                        else
                        {
                            nextItem.Status = QueueItemStatus.Failed;
                            string err = Installer.LastErrorMessage ?? "Kurulum başlatılamadı.";
                            nextItem.ErrorMessage = err;

                            if (ConsoleStorageService.IsOutOfSpaceError(err, out string friendlyMsg))
                            {
                                OnItemStatusChanged?.Invoke(nextItem);
                                OnStorageErrorOccurred?.Invoke(nextItem, friendlyMsg);
                                break; // Konsolda yer kalmadığı için kuyruğu durdur
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        nextItem.Status = QueueItemStatus.Cancelled;
                        break;
                    }
                    catch (Exception ex)
                    {
                        nextItem.Status = QueueItemStatus.Failed;
                        nextItem.ErrorMessage = ex.Message;
                    }

                    OnItemStatusChanged?.Invoke(nextItem);
                }
            }
            finally
            {
                _isRunning = false;
                CurrentItem = null;
            }
        }
    }
}
