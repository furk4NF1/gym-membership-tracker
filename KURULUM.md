# Spor Salonu Üye Takip Sistemi

Bu Windows Forms uygulaması SQL Server'da kullanıcı girişi ve spor salonu üye kayıtları yönetimi sağlar. Admin kullanıcı yönetebilir ve üye ekleyip güncelleyip silebilir. User yalnızca üyeleri görüntüler. Parolalar PBKDF2 ve kullanıcıya özel salt ile saklanır; yeni hesap ilk girişte parola değiştirir.

## Gerekenler

- Visual Studio ve **.NET masaüstü geliştirme** bileşeni; proje .NET Framework 4.8 kullanır.
- SQL Server Express veya LocalDB; SQL betiğini çalıştırmak için SSMS.

## İlk kurulum veya eski veritabanını yükseltme

1. SSMS'te daha önce bağlandığın sunucuya bağlan.
2. `SQL/Kurulum.sql` dosyasını açıp tamamını çalıştır. Betik `GymTracking` veritabanını kurar veya mevcut Users tablosuna yeni alanları ekler; mevcut üye kayıtlarını silmez.
3. `Database.cs` içindeki `ConnectionString` satırının SQL Server sunucunu gösterdiğini kontrol et. `localhost` ile bağlandıysan `Data Source=localhost` olarak bırak. Express veya LocalDB kullanıyorsan yönergede verilen sunucu adına göre değiştir.
4. `SporSalonuTakip.sln` dosyasını Visual Studio'da aç ve F5'e bas.
5. İlk hesaplar: **admin / admin123** ve **user / user123**. İlk girişte yeni parola belirlemek zorunludur. Parola en az 6 karakter olmalı; büyük harf, küçük harf, rakam ve özel karakter içermeli; kullanıcı adı, ardışık kolay bir dizi veya art arda gelen dört rakam içermemelidir. Son üç parola tekrar kullanılamaz.

Önceki basit proje sürümünün veritabanını kullanıyorsan aynı `Kurulum.sql` betiğini çalıştır. Mevcut admin parolası yükseltilir; ilk başarılı girişte yeni parola belirlenir. Betiği yeniden çalıştırmak üye kayıtlarını silmez.

## E-posta ile parola sıfırlama

Sıfırlama e-postası için proje klasöründeki `App.config` dosyasına e-posta sağlayıcının SMTP sunucusu, portu, kullanıcı adı, uygulama parolası ve gönderen adresi girilir. Bu dosyayı başkalarıyla paylaşma; SMTP uygulama parolasını gerçek hesap parolan olarak kullanma.

Kurulum betiği örnek e-posta olarak `admin@example.com` ve `user@example.com` yazar. SSMS'te test hesabının e-posta adresini alabileceğin bir posta kutusuyla değiştir:

```sql
USE GymTracking;
UPDATE Users SET Email=N'kendi-adresin@example.com' WHERE Username=N'admin';
```

Uygulamada giriş ekranından **Şifremi Unuttum** seçilir. Tek kullanımlık kod e-postaya gönderilir; 15 dakika geçerlidir. Kod veritabanında yalnızca özetiyle saklanır, kullanıldığında geçersizleşir. İstek sınırı hesap ve bilgisayar başına 60 saniyede 1, 15 dakikada 3 ve günde 10 istektir.

## Uygulamadaki yetkiler

- **Admin:** üye ekleme, güncelleme ve silme; kullanıcı oluşturma ve rol değiştirme.
- **User:** üye listesini görüntüleme. Üye değişiklikleri ve kullanıcı yönetimi BLL katmanında da engellenir.
- Admin kendi rolünü değiştiremez ve sistemdeki son Admin rolünü kaldıramaz.
- 14 dakika hareketsizlikte uyarı görünür; 15 dakikada oturum kapatılır.
- **Kullanıcılar / Roller** ekranı Admin hesabında görünür. Yeni hesap oluşturulunca üretilen geçici parola yalnızca bir kez gösterilir; kullanıcıya güvenli biçimde iletilip ilk girişte değiştirilmelidir.

## Üyelik süresi takibi

Üyelik bitiş tarihi başlangıç tarihi ve seçilen üyelik süresinden hesaplanır. Son gün üyeliğe dahildir. Son 7 gün içindeki üyelikler **Yakında bitiyor**, bitiş tarihi geçmiş olanlar **Süresi dolmuş**, diğerleri **Aktif** olarak listelenir.

## Parola sıfırlama kodunun süre ve kullanım kontrolü

Kod 15 dakika sonra veya ilk kullanımından sonra reddedilir. Güvenlik testinde eski kodu tekrar kullanmayı ve süresi geçtikten sonra denemeyi kontrol et. Test sonuçlarını formlara yalnızca gerçekten denedikten sonra yaz.

## Proje yapısı

- `LoginForm.cs`, `MainForm.cs`, `ChangePasswordForm.cs`, `PasswordResetForm.cs`, `UserManagementForm.cs`: Windows Forms ekranları.
- `BLL`: giriş, rol, üye ve oturum kuralları.
- `Database.cs`: SQL Server veri erişimi.
- `Entities`: oturumdaki kullanıcı ve parola verisi.
- `Helpers`: PBKDF2, parola kuralları, geçici parola ve e-posta gönderimi.
- `SQL/Kurulum.sql`: veritabanı yükseltme/kurulum betiği ve sentetik başlangıç verileri.

