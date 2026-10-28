using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private QueueItemStatus _status = QueueItemStatus.Pending;
        public QueueItemStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
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

        public DateTime AddedAt { get; } = DateTime.Now;
    }

    public static class InstallQueueManager
    {
        public static ObservableCollection<QueueItem> Items { get; } = new ObservableCollection<QueueItem>();

        private static CancellationTokenSource? _queueCts;
        private static bool _isRunning;
        public static bool IsRunning => _isRunning;

        public static event Action<QueueItem>? OnItemStatusChanged;

        public static void Add(string source)
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
            }
        }
    }
}
