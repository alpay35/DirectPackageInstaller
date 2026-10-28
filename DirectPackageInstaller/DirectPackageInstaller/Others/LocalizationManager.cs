using System;
using System.Collections.Generic;
using ReactiveUI;

namespace DirectPackageInstaller.Others
{
    public class LocalizationManager : ReactiveObject
    {
        private static LocalizationManager? _instance;
        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        private string _currentLanguage = "tr"; // Default to Turkish as requested by user
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    this.RaiseAndSetIfChanged(ref _currentLanguage, value);
                    this.RaisePropertyChanged("Item[]");
                    OnLanguageChanged?.Invoke(value);
                }
            }
        }

        public static event Action<string>? OnLanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
        {
            ["tr"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "DirectPackageInstaller - PS4 & PS5 Paket Yükleyici",
                ["Options"] = "Seçenekler",
                ["ProxyDownloads"] = "Vekil İndirmeler (Proxy)",
                ["SegmentedDownloads"] = "Parçalı İndirmeler (Hızlı)",
                ["TransferProgress"] = "Aktarım İlerlemesini Göster",
                ["CNLSupport"] = "JDownloader CNL Desteği",
                ["DHCPServer"] = "Dahili DHCP Sunucusu",
                ["IPAddress"] = "IP Yapılandırması",
                ["DebridSettings"] = "Debrid & Barındırma Ayarları",
                ["AllDebrid"] = "AllDebrid",
                ["RealDebrid"] = "RealDebrid",
                ["DebridLink"] = "DebridLink",
                ["Language"] = "Dil / Language",
                ["Turkish"] = "Türkçe",
                ["English"] = "İngilizce",
                ["AutoDiscover"] = "Konsolları Otomatik Bul",
                ["AutoDiscoverSearching"] = "Ağdaki PlayStation konsolları taranıyor...",
                ["AutoDiscoverFound"] = "{0} konsol bulundu: {1}",
                ["AutoDiscoverNone"] = "Ağda aktif PlayStation bulunamadı.",
                ["Load"] = "Yükle / İncele",
                ["Install"] = "Konsola Yükle",
                ["AddToQueue"] = "Kuyruğa Ekle",
                ["Queue"] = "Kurulum Kuyruğu",
                ["QueueStart"] = "Kuyruğu Başlat",
                ["QueueClear"] = "Kuyruğu Temizle",
                ["StatusReady"] = "Hazır",
                ["StatusLoading"] = "PKG bilgileri alınıyor...",
                ["StatusInstalling"] = "Konsola gönderiliyor...",
                ["StatusCompleted"] = "Yükleme tamamlandı!",
                ["StatusFailed"] = "Yükleme başarısız oldu.",
                ["PS4IPLabel"] = "Konsol (PS) IP:",
                ["PCIPLabel"] = "Bilgisayar (PC) IP:",
                ["UrlPlaceholder"] = "PKG URL'si, debrid linki veya yerel dosya yolu girin...",
                ["Title"] = "Oyun Başlığı",
                ["AppID"] = "Uygulama Kimliği",
                ["Version"] = "Sürüm",
                ["Size"] = "Boyut",
                ["Digest"] = "Özet (Digest)"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "DirectPackageInstaller - PS4 & PS5 Package Installer",
                ["Options"] = "Options",
                ["ProxyDownloads"] = "Proxy Downloads",
                ["SegmentedDownloads"] = "Segmented Downloads",
                ["TransferProgress"] = "Show Transfer Progress",
                ["CNLSupport"] = "CNL Support",
                ["DHCPServer"] = "DHCP Server",
                ["IPAddress"] = "IP Address",
                ["DebridSettings"] = "Debrid Settings",
                ["AllDebrid"] = "AllDebrid",
                ["RealDebrid"] = "RealDebrid",
                ["DebridLink"] = "DebridLink",
                ["Language"] = "Language",
                ["Turkish"] = "Turkish",
                ["English"] = "English",
                ["AutoDiscover"] = "Auto-Discover Consoles",
                ["AutoDiscoverSearching"] = "Scanning local network for consoles...",
                ["AutoDiscoverFound"] = "Found {0} console: {1}",
                ["AutoDiscoverNone"] = "No active consoles found on the local network.",
                ["Load"] = "Load",
                ["Install"] = "Install",
                ["AddToQueue"] = "Add to Queue",
                ["Queue"] = "Install Queue",
                ["QueueStart"] = "Start Queue",
                ["QueueClear"] = "Clear Queue",
                ["StatusReady"] = "Ready",
                ["StatusLoading"] = "Loading PKG info...",
                ["StatusInstalling"] = "Sending to console...",
                ["StatusCompleted"] = "Installation completed!",
                ["StatusFailed"] = "Installation failed.",
                ["PS4IPLabel"] = "PS IP:",
                ["PCIPLabel"] = "PC IP:",
                ["UrlPlaceholder"] = "Paste PKG URL, debrid link or local file path...",
                ["Title"] = "Title",
                ["AppID"] = "App ID",
                ["Version"] = "Version",
                ["Size"] = "Size",
                ["Digest"] = "Digest"
            }
        };

        public string this[string key]
        {
            get
            {
                if (Translations.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
                    return val;

                if (Translations["en"].TryGetValue(key, out var fallback))
                    return fallback;

                return key;
            }
        }

        public static string Get(string key) => Instance[key];
    }
}
