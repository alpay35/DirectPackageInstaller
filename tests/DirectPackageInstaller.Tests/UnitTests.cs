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

    [Collection("QueueCollection")]
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

        [Fact]
        public void Queue_Inserts_At_Top_When_Requested()
        {
            InstallQueueManager.Clear();

            InstallQueueManager.Add("https://example.com/game1.pkg", "Game 1");
            InstallQueueManager.Add("https://example.com/game2.pkg", "Game 2");

            // Direct installer adds Game 3 at top priority
            InstallQueueManager.Add("https://example.com/game3.pkg", "Game 3", insertAtTop: true);

            Assert.Equal(3, InstallQueueManager.Items.Count);
            Assert.Equal("Game 3", InstallQueueManager.Items[0].Title);
            Assert.Equal("Game 1", InstallQueueManager.Items[1].Title);
            Assert.Equal("Game 2", InstallQueueManager.Items[2].Title);

            InstallQueueManager.Clear();
        }

        [Fact]
        public void Queue_Moves_Existing_Item_To_Top_When_Requested()
        {
            InstallQueueManager.Clear();

            InstallQueueManager.Add("https://example.com/game1.pkg", "Game 1");
            InstallQueueManager.Add("https://example.com/game2.pkg", "Game 2");

            // Move Game 2 to the top of the queue
            InstallQueueManager.Add("https://example.com/game2.pkg", "Game 2", insertAtTop: true);

            Assert.Equal(2, InstallQueueManager.Items.Count);
            Assert.Equal("Game 2", InstallQueueManager.Items[0].Title);
            Assert.Equal("Game 1", InstallQueueManager.Items[1].Title);

            InstallQueueManager.Clear();
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

    [Collection("QueueCollection")]
    public class ConsoleStorageTests
    {
        [Theory]
        [InlineData("0x80990085", true)]
        [InlineData("{\"status\":\"fail\",\"error_code\":2157510789}", true)]
        [InlineData("{\"status\":\"fail\",\"error_code\":2157510792}", true)]
        [InlineData("{\"status\":\"fail\",\"error_code\":\"0x80990088\"}", true)]
        [InlineData("Not enough free space on target device", true)]
        [InlineData("{\"status\":\"success\",\"task_id\":1}", false)]
        [InlineData("", false)]
        public void ConsoleStorage_Detects_ErrorCodes(string input, bool expected)
        {
            bool detected = ConsoleStorageService.IsOutOfSpaceError(input, out string message);
            Assert.Equal(expected, detected);
            if (expected)
            {
                Assert.False(string.IsNullOrWhiteSpace(message));
            }
        }

        [Fact]
        public void Queue_Stores_SizeBytes_And_Format()
        {
            InstallQueueManager.Clear();
            long testSize = 25L * 1024 * 1024 * 1024; // 25 GB
            InstallQueueManager.Add("https://example.com/cusa12345.pkg", "Test Game", null, null, testSize);

            Assert.Single(InstallQueueManager.Items);
            var item = InstallQueueManager.Items[0];
            Assert.Equal(testSize, item.SizeBytes);
            Assert.Contains("25", item.FormattedSize);
            InstallQueueManager.Clear();
        }
    }
}
