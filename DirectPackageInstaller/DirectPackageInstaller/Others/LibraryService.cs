using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DirectPackageInstaller.Host;
using ReactiveUI;

namespace DirectPackageInstaller.Others
{
    public class LibraryGameItem : ReactiveObject
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TitleId { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public string Category { get; set; } = "Game";
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
                        _icon = Bitmap.DecodeToHeight(ms, 140);
                    }
                    catch { }
                }
                return _icon;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? FileName : Title;
        public string Subtitle => $"[{TitleId}] • {Category} • {FormattedSize}";
    }

    public static class LibraryService
    {
        public static ObservableCollection<LibraryGameItem> AllGames { get; } = new ObservableCollection<LibraryGameItem>();
        public static ObservableCollection<LibraryGameItem> FilteredGames { get; } = new ObservableCollection<LibraryGameItem>();

        private static string _lastScanDirectory = string.Empty;
        public static string LastScanDirectory => _lastScanDirectory;

        private static CancellationTokenSource? _scanCts;
        public static bool IsScanning { get; private set; }

        public static void Filter(string query)
        {
            FilteredGames.Clear();
            if (string.IsNullOrWhiteSpace(query))
            {
                foreach (var g in AllGames)
                    FilteredGames.Add(g);
                return;
            }

            string q = query.Trim().ToLowerInvariant();
            foreach (var g in AllGames)
            {
                if (g.DisplayTitle.ToLowerInvariant().Contains(q) ||
                    g.TitleId.ToLowerInvariant().Contains(q) ||
                    g.FileName.ToLowerInvariant().Contains(q))
                {
                    FilteredGames.Add(g);
                }
            }
        }

        public static void CancelScan()
        {
            _scanCts?.Cancel();
            IsScanning = false;
        }

        public static async Task<int> ScanDirectoryAsync(
            string directoryPath,
            Action<LibraryGameItem>? onGameFound = null,
            Action<int, int>? onProgress = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found or inaccessible: {directoryPath}");

            CancelScan();
            _scanCts = new CancellationTokenSource();
            var ct = _scanCts.Token;

            IsScanning = true;
            _lastScanDirectory = directoryPath;
            AllGames.Clear();
            FilteredGames.Clear();

            return await Task.Run(() =>
            {
                var pkgFiles = new List<string>();
                try
                {
                    // Enumerate files safely without throwing on permission denied folders
                    var queue = new Queue<string>();
                    queue.Enqueue(directoryPath);

                    while (queue.Count > 0 && !ct.IsCancellationRequested)
                    {
                        string currentDir = queue.Dequeue();
                        try
                        {
                            foreach (var file in Directory.EnumerateFiles(currentDir, "*.pkg", SearchOption.TopDirectoryOnly))
                            {
                                if (ct.IsCancellationRequested) break;
                                pkgFiles.Add(file);
                            }

                            foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                            {
                                if (ct.IsCancellationRequested) break;
                                string dirName = Path.GetFileName(subDir);
                                if (!dirName.StartsWith(".") && !dirName.StartsWith("$"))
                                {
                                    queue.Enqueue(subDir);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                int total = pkgFiles.Count;
                int processed = 0;

                // Process files with bounded concurrency to not overload NAS network bandwidth
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(6, Environment.ProcessorCount),
                    CancellationToken = ct
                };

                Parallel.ForEach(pkgFiles, parallelOptions, file =>
                {
                    if (ct.IsCancellationRequested) return;

                    try
                    {
                        var item = ParsePkgFile(file);
                        if (item != null)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                AllGames.Add(item);
                                FilteredGames.Add(item);
                                onGameFound?.Invoke(item);
                            });
                        }
                    }
                    catch { }

                    int count = Interlocked.Increment(ref processed);
                    onProgress?.Invoke(count, total);
                });

                IsScanning = false;
                return AllGames.Count;
            }, ct);
        }

        public static LibraryGameItem? ParsePkgFile(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length < 1024)
                    return null;

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var info = stream.GetPKGInfo();

                if (info != null)
                {
                    var val = info.Value;
                    return new LibraryGameItem
                    {
                        FilePath = filePath,
                        FileName = fileInfo.Name,
                        Title = string.IsNullOrWhiteSpace(val.FriendlyName) ? Path.GetFileNameWithoutExtension(filePath) : val.FriendlyName,
                        TitleId = string.IsNullOrWhiteSpace(val.TitleID) ? "PS4" : val.TitleID,
                        SizeBytes = fileInfo.Length,
                        FormattedSize = TransferProgressInfo.FormatBytes(fileInfo.Length),
                        Category = string.IsNullOrWhiteSpace(val.ContentType) ? "Game" : val.FirendlyContentType,
                        IconData = val.IconData
                    };
                }

                return new LibraryGameItem
                {
                    FilePath = filePath,
                    FileName = fileInfo.Name,
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    TitleId = "PS4",
                    SizeBytes = fileInfo.Length,
                    FormattedSize = TransferProgressInfo.FormatBytes(fileInfo.Length),
                    Category = "Game"
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
