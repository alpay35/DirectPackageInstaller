using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;

namespace DirectPackageInstaller.Others
{
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string TimeString => Timestamp.ToString("HH:mm:ss");
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Category { get; set; } = "BİLGİ";
        public string Message { get; set; } = string.Empty;

        public string FormattedText => $"[{TimeString}] [{Category}] {Message}";

        public string LevelColor => Level switch
        {
            LogLevel.Success => "#42b983", // Green
            LogLevel.Warning => "#e3b341", // Amber / Yellow
            LogLevel.Error => "#f85149",   // Red
            _ => "#58a6ff"                // Blue / Info
        };

        public string BadgeBackground => Level switch
        {
            LogLevel.Success => "#163828",
            LogLevel.Warning => "#382d12",
            LogLevel.Error => "#3d1418",
            _ => "#122338"
        };

        public string TextColor => Level switch
        {
            LogLevel.Error => "#ff7b72",
            LogLevel.Warning => "#f0e68c",
            _ => "#c9d1d9"
        };
    }

    public static class AppLogService
    {
        public const int MaxLogs = 1000;
        public static ObservableCollection<LogEntry> Entries { get; } = new ObservableCollection<LogEntry>();

        public static event Action<LogEntry>? OnLogAdded;

        public static void Log(string message, LogLevel level = LogLevel.Info, string category = "BİLGİ")
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var entry = new LogEntry
            {
                Message = message.Trim(),
                Level = level,
                Category = category
            };

            void AddAction()
            {
                if (Entries.Count >= MaxLogs)
                {
                    Entries.RemoveAt(0);
                }
                Entries.Add(entry);
                OnLogAdded?.Invoke(entry);
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                AddAction();
            }
            else
            {
                Dispatcher.UIThread.Post(AddAction);
            }
        }

        public static void Info(string message, string category = "BİLGİ") => Log(message, LogLevel.Info, category);
        public static void Success(string message, string category = "BAŞARILI") => Log(message, LogLevel.Success, category);
        public static void Warning(string message, string category = "UYARI") => Log(message, LogLevel.Warning, category);
        public static void Error(string message, string category = "HATA") => Log(message, LogLevel.Error, category);

        public static void Clear()
        {
            void ClearAction() => Entries.Clear();

            if (Dispatcher.UIThread.CheckAccess())
            {
                ClearAction();
            }
            else
            {
                Dispatcher.UIThread.Post(ClearAction);
            }
        }

        public static string GetAllText()
        {
            return string.Join(Environment.NewLine, Entries.Select(x => x.FormattedText));
        }
    }
}
