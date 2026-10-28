using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DirectPackageInstaller.Host;
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

        public static void Add(string source, string? title = null, string? size = null, byte[]? iconData = null)
        {
            if (string.IsNullOrWhiteSpace(source))
                return;

            string name = Path.GetFileName(source.Split('?').First());
            if (string.IsNullOrEmpty(name))
                name = source;

            var item = new QueueItem
            {
                Source = source,
                FileName = name,
                Title = title ?? name,
                FormattedSize = size ?? string.Empty,
                IconData = iconData,
                Status = QueueItemStatus.Pending
            };

            Items.Add(item);
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
                    nextItem.Status = QueueItemStatus.Installing;
                    OnItemStatusChanged?.Invoke(nextItem);

                    try
                    {
                        bool success = await installFunc(nextItem.Source);
                        if (success)
                        {
                            nextItem.Status = QueueItemStatus.Completed;
                            nextItem.Progress = 100.0;
                        }
                        else
                        {
                            nextItem.Status = QueueItemStatus.Failed;
                            nextItem.ErrorMessage = "Installation returned failure.";
                        }
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
