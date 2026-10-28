using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using DirectPackageInstaller.Others;
using DirectPackageInstaller.Tasks;
using ReactiveUI;

namespace DirectPackageInstaller.ViewModels
{
    public class MainViewModel : ReactiveObject
    {

        private string _PS4IP = null;
        public string PS4IP
        {
            get => _PS4IP;
            set
            {
                _PS4IP = value;
                
                if (IPAddress.TryParse(value, out _))
                    PCIP = IPHelper.FindLocalIP(value) ?? PCIP;
                
                this.RaisePropertyChanged();
            }
        }
        
        
        private string _PCIP = null;
        public string PCIP
        {
            get => _PCIP;
            set => this.RaiseAndSetIfChanged(ref _PCIP, value);
        }

        private string _CurrentURL = null;
        public string CurrentURL
        {
            get => _CurrentURL;
            set
            {
                _CurrentURL = value;
                this.RaisePropertyChanged();
            }
        }

        private string _allDebridApiKey = null;
        public string AllDebridApiKey
        {
            get => _allDebridApiKey;
            set => this.RaiseAndSetIfChanged(ref _allDebridApiKey, value);
        }

        private string _realDebridApiKey = null;
        public string RealDebridApiKey
        {
            get => _realDebridApiKey;
            set => this.RaiseAndSetIfChanged(ref _realDebridApiKey, value);
        }

        private string _debridLinkApiKey = null;
        public string DebridLinkApiKey
        {
            get => _debridLinkApiKey;
            set => this.RaiseAndSetIfChanged(ref _debridLinkApiKey, value);
        }

        private bool _ProxyMode = false;
        public bool ProxyMode 
        {
            get => _ProxyMode;
            set => this.RaiseAndSetIfChanged(ref _ProxyMode, value);
        }

        private bool _SegmentedMode = false;
        public bool SegmentedMode 
        {
            get => _SegmentedMode;
            set => this.RaiseAndSetIfChanged(ref _SegmentedMode, value);
        }

        private bool _ShowTransferProgress = true;
        public bool ShowTransferProgress
        {
            get => _ShowTransferProgress;
            set => this.RaiseAndSetIfChanged(ref _ShowTransferProgress, value);
        }
        
        private bool _useAllDebrid = false;
        public bool UseAllDebrid 
        {
            get => _useAllDebrid;
            set => this.RaiseAndSetIfChanged(ref _useAllDebrid, value);
        }
        
        private bool _useRealDebrid = false;
        public bool UseRealDebrid 
        {
            get => _useRealDebrid;
            set => this.RaiseAndSetIfChanged(ref _useRealDebrid, value);
        }

        private bool _useDebridLink = false;
        public bool UseDebridLink
        {
            get => _useDebridLink;
            set => this.RaiseAndSetIfChanged(ref _useDebridLink, value);
        }

        private bool _CNLService = false;
        public bool CNLService 
        {
            get => _CNLService;
            set => this.RaiseAndSetIfChanged(ref _CNLService, value);
        }

        private bool _DHCPService = false;
        public bool DHCPService
        {
            get => _DHCPService;
            set => this.RaiseAndSetIfChanged(ref _DHCPService, value);
        }

        private List<PkgParamInfo> _PKGParams = new List<PkgParamInfo>();
        public List<PkgParamInfo> PKGParams
        {
            get => _PKGParams;
            set => this.RaiseAndSetIfChanged(ref _PKGParams, value);
        }

        // NAS & Library Properties
        private string _nasDirectoryPath = string.Empty;
        public string NasDirectoryPath
        {
            get => _nasDirectoryPath;
            set => this.RaiseAndSetIfChanged(ref _nasDirectoryPath, value);
        }

        private string _librarySearchQuery = string.Empty;
        public string LibrarySearchQuery
        {
            get => _librarySearchQuery;
            set
            {
                this.RaiseAndSetIfChanged(ref _librarySearchQuery, value);
                LibraryService.Filter(value);
            }
        }

        public ObservableCollection<LibraryGameItem> LibraryGames => LibraryService.FilteredGames;
        public ObservableCollection<QueueItem> QueueItems => InstallQueueManager.Items;

        // Active Transfer & Queue Dashboard
        private string _activeTransferTitle = "Kuyruk Boş / Bekleniyor";
        public string ActiveTransferTitle
        {
            get => _activeTransferTitle;
            set => this.RaiseAndSetIfChanged(ref _activeTransferTitle, value);
        }

        private double _activeTransferPercent = 0;
        public double ActiveTransferPercent
        {
            get => _activeTransferPercent;
            set => this.RaiseAndSetIfChanged(ref _activeTransferPercent, value);
        }

        private string _activeTransferSpeed = "0 MB/s";
        public string ActiveTransferSpeed
        {
            get => _activeTransferSpeed;
            set => this.RaiseAndSetIfChanged(ref _activeTransferSpeed, value);
        }

        private string _activeTransferProgressText = "0 MB / 0 MB";
        public string ActiveTransferProgressText
        {
            get => _activeTransferProgressText;
            set => this.RaiseAndSetIfChanged(ref _activeTransferProgressText, value);
        }

        private string _activeTransferStatus = "Hazır";
        public string ActiveTransferStatus
        {
            get => _activeTransferStatus;
            set => this.RaiseAndSetIfChanged(ref _activeTransferStatus, value);
        }

        private bool _isActiveTransferRunning = false;
        public bool IsActiveTransferRunning
        {
            get => _isActiveTransferRunning;
            set => this.RaiseAndSetIfChanged(ref _isActiveTransferRunning, value);
        }

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }

        private string _consoleStorageStatus = "💾 Konsol Alanı: Bilinmiyor";
        public string ConsoleStorageStatus
        {
            get => _consoleStorageStatus;
            set => this.RaiseAndSetIfChanged(ref _consoleStorageStatus, value);
        }

        private long _consoleFreeBytes = -1;
        public long ConsoleFreeBytes
        {
            get => _consoleFreeBytes;
            set => this.RaiseAndSetIfChanged(ref _consoleFreeBytes, value);
        }

        private ObservableCollection<DiscoveredConsole> _discoveredConsoles = new ObservableCollection<DiscoveredConsole>();
        public ObservableCollection<DiscoveredConsole> DiscoveredConsoles
        {
            get => _discoveredConsoles;
            set => this.RaiseAndSetIfChanged(ref _discoveredConsoles, value);
        }

        private DiscoveredConsole? _selectedConsole;
        public DiscoveredConsole? SelectedConsole
        {
            get => _selectedConsole;
            set => this.RaiseAndSetIfChanged(ref _selectedConsole, value);
        }

        public ObservableCollection<LogEntry> LogEntries => AppLogService.Entries;
        public string LogCountText => $"{AppLogService.Entries.Count} kayıt";

        public MainViewModel()
        {
            AppLogService.OnLogAdded += _ => this.RaisePropertyChanged(nameof(LogCountText));
        }

        private string _activeConsoleDisplay = "🎮 Hedef Konsol: Tanımlanmadı";
        public string ActiveConsoleDisplay
        {
            get => _activeConsoleDisplay;
            set => this.RaiseAndSetIfChanged(ref _activeConsoleDisplay, value);
        }
    }

    public class PkgParamInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
