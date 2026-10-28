# DirectPackageInstaller

Doğrudan indirme bağlantılarını kullanarak PKG dosyalarını önizlemenizi ve PS4 veya PS5 konsolunuza göndermenizi sağlayan güçlü bir araçtır.  
Bu araç, konsola PKG göndermek için üç farklı yöntemi destekler:

- **Remote Package Installer Homebrew**
- **GoldHEN Payload Server**
- **etaHEN DPIv2 API** (PS5 desteği)

---

## Özellikler

- PKG dosyalarını önizleme (ikon, başlık, boyut, sürüm vb.)
- Bağımsız PS4 paket indirmeleri
- PS5 etaHEN desteği
- PS4 indirme hızını artırmak için otomatik yerel proxy (vekil sunucu)
- RAR ve 7z sıkıştırılmış arşiv dosyalarını doğrudan açıp gönderme desteği
- Duraklatılabilir ve devam ettirilebilir indirmeler (yalnızca sıkıştırılmamış dosyalar için)
- Parçalı / Segmentli indirmeler (Segmented Downloads)
- Popüler dosya barındırma (file hosting) ve debrid servisleri desteği
- PSN güncelleme bildirimleri (bölünmüş PKG / split PKG) desteği
- Remote Package Installer olmadan da çalışabilme (dahili payload desteği)
- Komut Satırı Arayüzü (CLI) desteği
- JDownloader Click'n Load (CNL) entegrasyonu
- Otomatik LAN Kablo Bağlantısı (Yalnızca Windows)
- NFS (Ağ Dosya Sistemi) üzerinden PKG yükleyebilme

---

## Desteklenen Dosya Barındırma ve Debrid Servisleri

- Herhangi bir doğrudan bağlantı (Direct Link - kimlik doğrulama gerektirmeyen)
- **AllDebrid** ([API anahtarı gereklidir](https://alldebrid.com/apikeys/))
- **RealDebrid** ([API anahtarı gereklidir](https://real-debrid.com/apitoken))
- **DebridLink** ([API anahtarı gereklidir](https://debrid-link.com/webapp/apikey))
- **Google Drive**
- **Mediafire**
- **PixelDrain**
- **1Fichier** (Her indirme arasında 60 dakika bekleme süresi bulunur)

---

## Nasıl Kurulur?

1. Platformunuza uygun sürümü indirin (aşağıdaki indirme listesine bakın).
2. Windows, Linux ve Android üzerinde önce [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) kurun.  
   *(macOS sürümleri bağımsızdır; .NET çalışma zamanı içine gömülü gelir ve ek kurulum gerektirmez).*

---

## İndirmeler

<details>
<summary><b>Windows</b></summary>

- [ARM](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Windows-ARM.zip)    
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Windows-ARM64.zip)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Windows-X64.zip)    
- [x86](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Windows-X86.zip)    

</details>

<details>
<summary><b>Android</b></summary>

- [ARM](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Android-ARM.zip)    
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Android-ARM64.zip)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Android-X64.zip)    
- [x86](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Android-X86.zip)    

</details>

<details>
<summary><b>Linux</b></summary>

- [ARM](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Linux-ARM.zip)    
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Linux-ARM64.zip)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/Linux-X64.zip)    

</details>

<details>
<summary><b>macOS</b></summary>

**CLI (Komut Satırı) Sürümleri:**
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-ARM64.zip)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-X64.zip)    

**Uygulama Sürümleri (.zip):**
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-ARM64-APP.zip)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-X64-APP.zip)   

**Uygulama Sürümleri (DMG, Uygulamalar klasörüne sürükle-bırak):**
- [ARM64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-ARM64-APP.dmg)    
- [x64](https://raw.githubusercontent.com/marcussacana/DirectPackageInstaller/Updater/OSX-X64-APP.dmg)

macOS sürümleri bağımsızdır (.NET 8 çalışma zamanı pakete dahildir) ve .app paketi Apple Silicon üzerinde sorunsuz açılabilmesi için ad-hoc imzalanmıştır. Zip dosyasını çıkardıktan sonra Gatekeeper uygulamayı yine de engellerse terminalde şu komutu çalıştırın:

```bash
xattr -dr com.apple.quarantine /Applications/DirectPackageInstaller.app
```

</details>

---

## Nasıl Kullanılır?

#### Doğrudan İndirme Modu (Direct Download)
1. Konsolunuzda **Remote Package Installer** uygulamasını başlatın veya **GoldHEN Payload Server** özelliğini etkinleştirin.
2. DirectPackageInstaller uygulamasını açın.
3. "PKG URL" alanına doğrudan PKG bağlantısını yapıştırın.
4. **"Load"** butonuna tıklayın ve bilgilerin yüklenmesini bekleyin.
5. **"Install"** butonuna tıklayın ve konsola aktarımın başlamasını bekleyin.
6. Tamamlandı! Artık uygulamayı veya bilgisayarınızı kapatabilirsiniz.

#### Proxy Modu (İnternet Erişimi Olmayan veya Yavaş PS4'ler İçin)
1. Konsolunuzda Remote Package Installer'ı başlatın veya GoldHEN Payload Server'ı açın.
2. DirectPackageInstaller'ı açın.
3. `Options -> Proxy Downloads` seçeneğini aktif edin.
4. Doğrudan PKG bağlantısını yapıştırın.
5. Önce **"Load"**, ardından **"Install"** butonuna basın.
6. Yükleme tamamlanana kadar bekleyin (veriler bilgisayarınız üzerinden konsola iletilir).

#### Parçalı İndirme Modu (Segmented Download - En Hızlı Yöntem)
1. Bilgisayarınızda yeterli boş disk alanı olduğundan emin olun.
2. Konsolda Remote Package Installer'ı başlatın veya GoldHEN Payload Server'ı açın.
3. DirectPackageInstaller'ı açın.
4. `Options -> Segmented Downloads` seçeneğini aktif edin.
5. PKG bağlantısını yapıştırın.
6. **"Load"** ve ardından **"Install"** butonuna tıklayın.
7. PKG, çoklu iş parçacığıyla bilgisayarınıza indirilirken aynı anda konsola aktarılır.

> **Not:** Parçalı indirme modunda PS4 indirme ilerlemesini tam olarak hesaplayamaz. Gerçek ağ kullanımını bilgisayarınızdaki Görev Yöneticisi'nden takip edebilirsiniz.

#### Sıkıştırılmış Dosya Modu (RAR / 7z - Daima Proxy Kullanır)
1. Bilgisayarınızda yeterli boş disk alanı olduğundan emin olun.
2. Konsolda Remote Package Installer'ı veya GoldHEN Payload Server'ı açın.
3. Sıkıştırılmış arşiv dosyasının doğrudan bağlantısını yapıştırın.
4. **"Load"** butonuna basın ve arşiv taranırken bekleyin.
5. *(İsteğe bağlı)* Dosya listesinden yüklemek istediğiniz PKG'yi seçin.
6. **"Install"** butonuna tıklayın. İndirme, çıkarma ve konsola aktarım anlık (streaming) olarak gerçekleşecektir.

---

## macOS CLI Kullanımı

Terminal üzerinden uygulamayı çalıştırmak için:
```bash
./DirectPackageInstaller.Desktop
```
Gerekli durumlarda şu komutla da çalıştırabilirsiniz:
```bash
dotnet DirectPackageInstaller.Desktop.dll
```

---

## NFS PKG Kaynağı

DPI için kaynak olarak bir NFS sunucusu kullanmak için desteklenen biçimde bir URL girmeniz yeterlidir.  
Desteklenen iki farklı NFS URL formatı mevcuttur:

- `nfs://NFS_IP_VEYA_SUNUCU_ADI/DisaAktarilanYol/DosyaYolu/DosyaAdi.pkg`
- `nfs://NFS_IP_VEYA_SUNUCU_ADI/DosyaYolu/DosyaAdi.pkg`

İlk formatta yol, NFS sunucusunun `/etc/exports` dosyasında yapılandırılan dışa aktarılan dizin ile başlar. DPI doğrudan bu paylaşıma bağlanır ve dosyayı arar.

İkinci formatta belirli bir dışa aktarma yolu belirtilmez; bu durumda DPI istenen dosyayı bulmak için NFS sunucusundaki tüm paylaşımları tarar. Sunucunuzda çok fazla paylaşım varsa bu işlem ilk erişimi yavaşlatabilir.

> **Dipnot:** DPI, [#88 numaralı hata](https://github.com/winnfsd/winnfsd/issues/88) nedeniyle winnfsd ile uyumsuzdur. Windows için alternatif olarak [haneWIN NFS Server](https://www.hanewin.net/nfs-e.htm) kullanabilirsiniz.

---

## Nasıl Çalışır?

DirectPackageInstaller, PKG indirmelerini doğrudan PS4 veya PS5 konsolunuza eklemek için Remote Package Installer API'sini ve etaHEN protokolünü kullanan hafif ve pratik bir araçtır.

Ayrıca araç, indirmeleri hızlandırmak için otomatik bir dahili yerel vekil sunucu (proxy) barındırır. PS4 konsolları internet (WAN) hızına kıyasla yerel ağda (LAN) çok daha yüksek hızlara ulaştığından, indirmeleri bilgisayarınız üzerinden tünellemek aktarım performansını katbekat artırır.

Konsolunuzda Remote Package Installer kurulu olmasa bile, GoldHEN binloader açıksa araç hangi yöntemi kullanacağını otomatik olarak algılar ve aktarımı sağlar.

Sıkıştırılmış arşivlerde (RAR/7z) her işlem eş zamanlı ve paralel olarak yürütülür: indirme, arşivden çıkarma ve konsola yükleme aynı anda gerçekleşir. Bu sayede hem zaman kazanılır hem de diskte devasa geçici kopyalar oluşturulmaz.

---

## Komut Satırı Arayüzü (CLI)

Deneysel bir CLI desteği mevcuttur.

> **Not:** RAR/7z açma ve parçalı indirmeler henüz CLI modunda desteklenmemektedir.

Kullanılabilir parametreleri görmek için:
- **Windows:** `DirectPackageInstaller.Desktop.exe -help`
- **Diğer sistemler:** `./DirectPackageInstaller.Desktop -help`

---

## Gereksinimler

- Windows, Linux ve Android sürümleri [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) gerektirir.
- macOS sürümleri bağımsızdır ve çalışma zamanı içine entegre olarak gelir.

---

## Projeyi Derleme (Build)

1. [.NET 8.x SDK](https://dotnet.microsoft.com/download) yükleyin.
2. `Build.cmd` betiğini çalıştırın (hem Windows hem de Unix benzeri sistemlerde çalışır).
3. Derlenen ikili dosyalar `Release` dizini altında oluşturulacaktır.

Özel payload geliştirmek için `Payload` dizininde `make` çalıştırın ve oluşturulan `payload.bin` dosyasını `DirectPackageInstaller/DirectPackageInstaller/payload` içine kopyalayın.

#### Android Derlemeleri:
- Hedef API seviyesi: 26
- Android SDK ve NDK kurulumu gereklidir.
- SDK lisanslarını onaylayın.
- `Build.cmd` dosyasını düzenleyerek SDK/NDK yollarını tanımlayın.
- İlk seferde `Build.cmd` dosyasını yönetici olarak çalıştırın.

*(Alternatif olarak projeyi fork'layıp GitHub Actions üzerinden derleyebilirsiniz.)*

---

## Sıkça Sorulan Sorular (SSS)

**İndirmem neden geç başlıyor?**  
Muhtemelen çok parçalı sıkıştırılmış bir arşiv indiriyorsunuz. Parça sayısı ne kadar fazla olursa, arşivin taranması ve doğrulanması o kadar uzun sürer. Bu bir hata değil, arşiv yapısının doğası gereğidir. Lütfen bekleyin.

**Parçalı modda bile indirme hızı yavaş görünüyor?**  
Parçalı mod, PS4 ekranına anlık doğru ilerleme raporlayamaz. Gerçek ağ kullanımını bilgisayarınızın Görev Yöneticisi'nden izleyin.

**Segmentli moddayım ama hız sadece 2 Mbps?**  
Bazı barındırma siteleri (ör. Zippyshare vb.) bant genişliğini sınırlar. Gerçek performansı test etmek için Google Drive bağlantılarını deneyin.

**Önerilen dosya barındırma servisleri nelerdir?**
1. AllDebrid / RealDebrid / DebridLink
2. Google Drive
3. Mediafire
4. PixelDrain
5. Diğerleri
6. 1Fichier (en yavaşı - bekleme süreli)

**Yükleme sırasında hata aldım, ne yapmalıyım?**  
GitHub üzerinde bir Issue (Hata Kaydı) açmaktan çekinmeyin! Uygulama çökerse ayrıntılar için `DPI-Crash.log` dosyasını kontrol edin.

---

## Yasal Uyarı

Bu araç yalnızca yasal ve meşru amaçlar için geliştirilmiştir. Kullanıcıların kendi kişisel yedeklerini kurmalarına yardımcı olmak amaçlanmıştır. Korsan içerik veya telif hakkı ihlalleri desteklenmez. Yazılımın kullanımından doğan sorumluluk tamamen kullanıcıya aittir.

---

## Önemli Uyarılar

- Aynı anda birden fazla sıkıştırılmış arşivi açmaktan kaçının; dosyaları sırayla işleyin.
- Sıkıştırılmış dosya indirmeleri geçici bir PKG önbelleği oluşturur; bu nedenle diskinizde nihai PKG boyutu kadar boş alan bulunmalıdır.

---

## Teşekkürler ve Katkıda Bulunanlar

- **LibOrbisPkg** - _maxton_
- **HttpServerLite** - _jchristn_
- Payload şablonu - _sleirsgoevy_
- PS4 çekirdek ve sistem desteği - _LM_
- PS4 dışa aktarma tanımları - _OpenOrbis SDK_
- **DirectPackageInstaller** - _marcussacana_
