using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPackageInstaller.Others
{
    public class ConsoleStorageResult
    {
        public bool Success { get; set; }
        public long FreeBytes { get; set; }
        public string FormattedFreeSpace => FreeBytes > 0 ? FreeBytes.ToFileSize() : "Bilinmiyor";
        public string Message { get; set; } = string.Empty;
        public bool IsFtpAvailable { get; set; }
    }

    public static class ConsoleStorageService
    {
        /// <summary>
        /// GoldHEN veya konsol üzerindeki FTP servisine (Port 2121) bağlanarak dahili (/user veya /data) boş depolama alanını bayt cinsinden sorgular.
        /// </summary>
        public static async Task<ConsoleStorageResult> QueryConsoleStorageAsync(string psIp, int port = 2121, int timeoutMs = 3000)
        {
            var result = new ConsoleStorageResult();

            if (string.IsNullOrWhiteSpace(psIp) || psIp == "0.0.0.0")
            {
                result.Message = "Konsol IP adresi yapılandırılmamış.";
                return result;
            }

            try
            {
                using var client = new TcpClient();
                using var cts = new CancellationTokenSource(timeoutMs);

                await client.ConnectAsync(psIp, port, cts.Token);
                result.IsFtpAvailable = true;

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII);
                using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                // 1. Karşılama mesajını oku (220 ...)
                string? line = await reader.ReadLineAsync(cts.Token);
                if (line == null || !line.StartsWith("220"))
                {
                    result.Message = "FTP sunucusu geçerli bir karşılama yanıtı dönmedi.";
                    return result;
                }

                // 2. USER anonim oturumu
                await writer.WriteLineAsync("USER anonymous");
                line = await reader.ReadLineAsync(cts.Token);

                // 3. PASS oturumu
                await writer.WriteLineAsync("PASS anonymous");
                line = await reader.ReadLineAsync(cts.Token);

                // 4. PS4 /user bölümünü sorgula (Uygulama ve oyunların yüklendiği ana bölüm)
                await writer.WriteLineAsync("AVBL /user");
                line = await reader.ReadLineAsync(cts.Token);

                if (TryParseAvblResponse(line, out long freeBytes))
                {
                    result.Success = true;
                    result.FreeBytes = freeBytes;
                    result.Message = $"Dahili depolama alanı başarıyla okundu: {result.FormattedFreeSpace} boş.";
                    try { await writer.WriteLineAsync("QUIT"); } catch { }
                    return result;
                }

                // 5. /user yanıt vermezse /data bölümünü dene
                await writer.WriteLineAsync("AVBL /data");
                line = await reader.ReadLineAsync(cts.Token);

                if (TryParseAvblResponse(line, out freeBytes))
                {
                    result.Success = true;
                    result.FreeBytes = freeBytes;
                    result.Message = $"Depolama alanı (/data) okundu: {result.FormattedFreeSpace} boş.";
                    try { await writer.WriteLineAsync("QUIT"); } catch { }
                    return result;
                }

                try { await writer.WriteLineAsync("QUIT"); } catch { }
                result.Message = "FTP sunucusu AVBL disk sorgulama komutunu desteklemiyor.";
                return result;
            }
            catch (OperationCanceledException)
            {
                result.Message = $"Konsola ({psIp}:{port}) bağlantı zaman aşımına uğradı.";
                return result;
            }
            catch (SocketException ex)
            {
                result.Message = $"Konsol FTP servisine bağlanılamadı ({ex.SocketErrorCode}).";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"Alan sorgusu hatası: {ex.Message}";
                return result;
            }
        }

        private static bool TryParseAvblResponse(string? response, out long freeBytes)
        {
            freeBytes = 0;
            if (string.IsNullOrWhiteSpace(response))
                return false;

            // Yanıt biçimi: "213 123456789000" veya "213 <free_bytes>"
            if (response.StartsWith("213"))
            {
                var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && long.TryParse(parts[1], out freeBytes) && freeBytes > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kurulum öncesi konsol alan kontrolü yapar.
        /// FTP servisi erişilebiliyorsa tam bayt kıyaslaması yapar.
        /// FTP servisi yoksa doğrudan kuruluma izin verir (RPI Sony BGFT kontrolüne devreder).
        /// </summary>
        public static async Task<(bool HasEnoughSpace, string Message)> CheckStorageBeforeInstallAsync(string psIp, long requiredBytes)
        {
            if (string.IsNullOrWhiteSpace(psIp) || psIp == "0.0.0.0")
                return (true, "Konsol IP adresi belirtilmemiş.");

            if (requiredBytes <= 0)
                return (true, "Paket boyutu bilinmiyor, aktarım doğrudan başlatılacak.");

            var storage = await QueryConsoleStorageAsync(psIp);
            if (storage.Success && storage.FreeBytes > 0)
            {
                if (storage.FreeBytes < requiredBytes)
                {
                    string msg = $"❌ Konsolda Yetersiz Depolama Alanı!\n\n" +
                                 $"Gerekli Alan: {requiredBytes.ToFileSize()}\n" +
                                 $"Konsolda Kalan Alan: {storage.FormattedFreeSpace}\n\n" +
                                 $"Lütfen PS4/PS5 konsolunuzda yer açtıktan sonra tekrar deneyin.";
                    return (false, msg);
                }

                return (true, $"Yeterli alan mevcut (Kalan: {storage.FormattedFreeSpace}, Gerekli: {requiredBytes.ToFileSize()}).");
            }

            // FTP kapalıysa veya AVBL desteklenmiyorsa konsolu engellemeyelim, RPI'ın kendi kontrolüne bırakalım
            return (true, "FTP alan sorgulanamadı; konsol doğrudan kontrol edecek.");
        }

        /// <summary>
        /// PS4 RPI veya etaHEN API yanıtındaki yetersiz depolama alanı hata kodlarını kontrol eder.
        /// 0x80990085 (decimal 2157510789): SCE_BGFT_ERROR_STORAGE_SPACE_SHORTAGE
        /// 0x80990088 (decimal 2157510792): Görev zaten mevcut / kuyruk dolu
        /// </summary>
        public static bool IsOutOfSpaceError(string? response, out string friendlyMessage)
        {
            friendlyMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(response))
                return false;

            // 0x80990085 / 2157510789 -> Yetersiz Disk Alanı
            if (response.Contains("0x80990085", StringComparison.OrdinalIgnoreCase) ||
                response.Contains("2157510789") ||
                response.Contains("storage_space_shortage", StringComparison.OrdinalIgnoreCase) ||
                response.Contains("not enough free space", StringComparison.OrdinalIgnoreCase) ||
                response.Contains("low storage", StringComparison.OrdinalIgnoreCase))
            {
                friendlyMessage = "❌ Konsolda Yetersiz Depolama Alanı!\n\n" +
                                  "Konsolunuzun dahili hafızasında bu paketi kurmak için yeterli boş alan bulunmuyor (Hata Kodu: 0x80990085).\n" +
                                  "Lütfen konsolunuzdan oyun veya veri silerek yer açın.";
                return true;
            }

            // 0x80990088 / 2157510792 -> Görev zaten mevcut veya indirme listesi dolu
            if (response.Contains("0x80990088", StringComparison.OrdinalIgnoreCase) ||
                response.Contains("2157510792"))
            {
                friendlyMessage = "⚠️ Konsolda Bu Oyun İçin Zaten Bir Görev Mevcut! (0x80990088)\n\n" +
                                  "Lütfen konsolunuzun İndirilenler / Bildirimler menüsünü kontrol edin.";
                return true;
            }

            return false;
        }
    }
}
