using System;
using System.IO;
using DirectPackageInstaller.FileHosts;
using DirectPackageInstaller.Others;
using DirectPackageInstaller.Tasks;
using Xunit;

namespace DirectPackageInstaller.Tests
{
    public class LocalizationTests
    {
        [Fact]
        public void Localization_Returns_Turkish_Strings()
        {
            var loc = LocalizationManager.Instance;
            loc.CurrentLanguage = "tr";

            Assert.Equal("Seçenekler", loc["Options"]);
            Assert.Equal("Konsolları Otomatik Bul", loc["AutoDiscover"]);
            Assert.Equal("Kurulum Kuyruğu", loc["Queue"]);
        }

        [Fact]
        public void Localization_Returns_English_Strings()
        {
            var loc = LocalizationManager.Instance;
            loc.CurrentLanguage = "en";

            Assert.Equal("Options", loc["Options"]);
            Assert.Equal("Auto-Discover Consoles", loc["AutoDiscover"]);
            Assert.Equal("Install Queue", loc["Queue"]);
        }

        [Fact]
        public void Localization_FallsBack_To_Key_If_Missing()
        {
            var loc = LocalizationManager.Instance;
            Assert.Equal("NonExistentKey123", loc["NonExistentKey123"]);
        }
    }

    public class FileHostTests
    {
        [Theory]
        [InlineData("https://torbox.app/download/12345", true)]
        [InlineData("https://otherhost.com/file", false)]
        public void TorBox_Validates_Urls(string url, bool expected)
        {
            var host = new TorBox();
            Assert.Equal(expected, host.IsValidUrl(url));
        }

        [Theory]
        [InlineData("https://gofile.io/d/abc123", true)]
        [InlineData("https://gofile.io/download", false)]
        public void GoFile_Validates_Urls(string url, bool expected)
        {
            var host = new GoFile();
            Assert.Equal(expected, host.IsValidUrl(url));
        }

        [Theory]
        [InlineData("https://buzzheavier.com/abc123", true)]
        [InlineData("https://google.com/test", false)]
        public void Buzzheavier_Validates_Urls(string url, bool expected)
        {
            var host = new Buzzheavier();
            Assert.Equal(expected, host.IsValidUrl(url));
        }

        [Fact]
        public void FileHostBase_Contains_All_Hosts()
        {
            var hosts = FileHostBase.Hosts;
            Assert.Contains(hosts, h => h is TorBox);
            Assert.Contains(hosts, h => h is GoFile);
            Assert.Contains(hosts, h => h is Buzzheavier);
            Assert.Contains(hosts, h => h.HostName == "PixelDrain");
            Assert.Contains(hosts, h => h.HostName == "Mediafire");
        }
    }

    public class QueueTests
    {
        [Fact]
        public void Queue_Adds_And_Clears_Items()
        {
            InstallQueueManager.Clear();
            Assert.Empty(InstallQueueManager.Items);

            InstallQueueManager.Add("https://example.com/game.pkg");
            InstallQueueManager.Add("https://example.com/update.pkg");

            Assert.Equal(2, InstallQueueManager.Items.Count);
            Assert.Equal("game.pkg", InstallQueueManager.Items[0].FileName);
            Assert.Equal("update.pkg", InstallQueueManager.Items[1].FileName);
            Assert.Equal(QueueItemStatus.Pending, InstallQueueManager.Items[0].Status);

            InstallQueueManager.Clear();
            Assert.Empty(InstallQueueManager.Items);
        }
    }

    public class DownloaderMemoryTests
    {
        [Fact]
        public void DownloaderTask_Dispose_DoesNotThrow()
        {
            var task = new DownloaderTask("https://example.com/test.pkg", "temp.pkg", () => Stream.Null);
            task.Dispose();
            // Calling dispose again should be safe
            task.Dispose();
        }
    }

    public class DiscoveryTests
    {
        [Fact]
        public void Discovery_Ports_Are_Valid()
        {
            Assert.Contains(12800, ConsoleDiscoveryService.ProbedPorts);
            Assert.Contains(9090, ConsoleDiscoveryService.ProbedPorts);
            Assert.Contains(9898, ConsoleDiscoveryService.ProbedPorts);
        }
    }
}
