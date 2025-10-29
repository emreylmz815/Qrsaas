# KuaceMenu – QR Menü SaaS

KuaceMenu, işletmelerin yıllık abonelikle kendi QR menülerini yönetebildiği, çoklu kiracılı (subdomain bazlı) bir ASP.NET Core 8 MVC + Razor Pages örnek projesidir. Uygulama sade kod yapısına sahiptir, Entity Framework Core + Identity kullanır ve Iyzico sandbox ödemesi için basit bir akış içerir.

## Özellikler

- ✅ ASP.NET Core 8 MVC + Razor Pages, EF Core (SQL Server) ve Identity
- ✅ Çoklu kiracılık: `isletme.kuacemenu.com` alt alan adı mantığı ve localhost geliştirme için `?tenant=` desteği
- ✅ Hangfire ile günlük abonelik durumu kontrolü ve /hangfire dashboard'u
- ✅ QRCoder + QuestPDF ile PNG/PDF QR kodu üretimi
- ✅ Basit Iyzico sandbox ödeme simülasyonu (callback endpoint hazır)
- ✅ Panelden kategori ve ürün yönetimi, görsel yükleme (wwwroot/uploads)
- ✅ SMTP (MailKit) ile e-posta servisi, dosyaya log yazımı
- ✅ Seed verisi: admin hesabı ve `demo.kuacemenu.com` örnek menü

## Hızlı Başlangıç

1. **Gereksinimler**
   - .NET SDK 8.0+
   - SQL Server veya LocalDB

2. **Projeyi derleme**

   ```bash
   dotnet restore
   dotnet build
   dotnet run --project KuaceMenu.Web
   ```

3. **Veritabanı migrasyonu**

   ```bash
   dotnet ef migrations add InitialCreate --project KuaceMenu.Web
   dotnet ef database update --project KuaceMenu.Web
   ```

4. **Seed kullanıcıları**
   - Admin hesabı: `admin@kuacemenu.com / Admin*123`
   - Demo tenant: `https://demo.kuacemenu.com/` (30 gün aktif, iki kategori ve beş ürün)

5. **Geliştirme modu**
   - Panel veya ödeme gibi alanlara giderken `https://localhost:5001/panel?tenant=demo` formatını kullanın.

## Konfigürasyon

`KuaceMenu.Web/appsettings.json` dosyasındaki bölümler:

- `ConnectionStrings:DefaultConnection`: SQL Server bağlantısı.
- `Iyzico`: Sandbox API anahtarları ve callback adresi.
- `Email`: SMTP sunucu bilgileri (MailKit kullanır).
- `Domain`: Ana domain (`kuacemenu.com`) ve wildcard ayarı.
- `Hangfire`: Dashboard basic auth kullanıcı adı/şifresi.

Geliştirme ortamına özel değerler için `appsettings.Development.json` dosyasını düzenleyin.

## Iyzico Sandbox

- Sandbox URL: `https://sandbox-api.iyzipay.com`
- Test kartı: `5528790000000008` (CVC 123, Son Kullanma 12/2030)
- Callback endpoint: `POST /payment/callback`
- `PaymentsController` içindeki `SimulateSuccess` aksiyonu lokal test için hazırdır.

Gerçek entegrasyon için `PaymentService.CreateCheckoutSessionAsync` metodundaki HTML çıktısını Iyzico checkout formu ile değiştirin ve callback doğrulamasını iyileştirin.

## Hangfire

- Dashboard: `https://localhost:5001/hangfire` (Basic Auth, `appsettings.json`'daki kullanıcı/şifre)
- `SubscriptionCheckJob` günlük olarak abonelik bitiş/grace sürelerini kontrol eder ve durumları günceller.

## QR Kod Üretimi

- `/panel/qr` sayfasından PNG ve PDF çıktıları indirilebilir.
- PDF çıktısı A6 boyutunda tek sayfadır ve QuestPDF kullanır.

## Dosya Yükleme

- Görsel dosyaları `wwwroot/uploads` içine GUID isimle kaydedilir.
- Uzantı filtrelemesi (jpg/png) ve 2 MB boyut sınırı vardır.

## Loglama

- Console + `logs/kuacemenu.log` dosyasına sade log yazımı.
- Log seviyesi Information ve üzeri için aktiftir.

## Dağıtım Notları

1. **IIS / Web Deploy**
   - `dotnet publish -c Release -o publish` komutu ile çıktı alın.
   - IIS'te application pool'u `No Managed Code` olarak ayarlayın.
   - `web.config` otomatik üretilir.

2. **DNS**
   - `*.kuacemenu.com` için A kaydı oluşturup uygulama sunucusuna yönlendirin.

3. **HTTPS**
   - Let’s Encrypt veya benzeri sertifika sağlayıcı ile HTTPS yapılandırması.

4. **Hangfire**
   - Dashboard erişimi için reverse proxy’de Basic Auth veya VPN önerilir.

## Geliştirici İpuçları

- `SubdomainTenantResolverMiddleware` host başlığından tenant slug'ını çözer; localhost için `?tenant=` fallback'ı vardır.
- `TenantAuthorizationFilter` panel isteklerini tenant sahibine veya admin rolüne sınırlar.
- `SubscriptionGateMiddleware` aktif olmayan aboneliklerde public menüyü bloke eder.
- Yapıyı sade tutmak için tek web projesi kullanılmıştır; servis katmanları DbContext’i doğrudan kullanır.

