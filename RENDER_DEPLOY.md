# Render.com Deployment Kılavuzu

## Environment Variables (Render Dashboard'da ayarla)

| Key | Value | Açıklama |
|-----|-------|----------|
| JWT_KEY | **En az 32 karakterlik güçlü key** | `openssl rand -base64 32` ile oluştur |
| JWT_ISSUER | ECommerce | Token issuer |
| JWT_AUDIENCE | ECommerce.Client | Token audience |
| ASPNETCORE_ENVIRONMENT | Production | Uygulama ortamı |
| DOTNET_DATA_PROTECTION_KEY_DIRECTORY | /app/keys | Data Protection anahtarları |
| CORS_ORIGIN_1 | http://localhost:3000 | Frontend URL |
| CORS_ORIGIN_2 | https://myfrontend.com | Production frontend URL |

> ⚠️ **Önemli**: JWT_KEY en az 32 karakter olmalı ve güçlü olmalıdır. Örnek:
> ```bash
> openssl rand -base64 32
> # Çıktı: K3xP9mN2vQ8rT5wY1zB4cF7hJ0kL6nU9
> ```

## Deploy Adımları

1. **Render Dashboard'a Git**
   - https://dashboard.render.com

2. **Environment Variables Ekle**
   - Service → Environment → Add Environment Variable
   - Yukarıdaki tabloyu kullanarak tüm değişkenleri ekle
   - **JWT_KEY için güçlü bir değer kullan!**

3. **Disk Ekle (Zorunlu)**
   - Service → Disks → Add Disk
   - Name: `data`
   - Mount Path: `/app/data`
   - Size: 1 GB
   
   - Name: `keys`
   - Mount Path: `/app/keys`
   - Size: 100 MB

4. **Manual Deploy**
   - "Manual Deploy" → "Clear build cache & deploy" seç
   - Bu, önceki build cache'ini temizler ve taze bir deploy yapar

5. **Logları Kontrol Et**
   - Deploy tamamlandıktan sonra "Logs" sekmesine git
   - "JWT Key bulunamadı" hatası artık görünmemeli
   - "✅ Database migrations completed" mesajını gör

6. **API Endpoint'leri Test Et**
   ```
   https://senin-api.onrender.com/health
   https://senin-api.onrender.com/api/products
   https://senin-api.onrender.com/swagger
   ```

## Yapılan Düzeltmeler

### 1. JWT Ayarları ✅
- JWT_KEY environment variable'dan okunuyor
- Minimum 32 karakter zorunluluğu
- JWT_ISSUER ve JWT_AUDIENCE değerleri doğrulandı

### 2. EF Core Global Query Filter ✅
- Child entity'ler (Address, OrderItem, UserRole, CategoryAttribute, ProductVariantAttribute) için uyumlu query filter eklendi
- Navigation property'ler optional yapıldı

### 3. Shadow State Foreign Key ✅
- User entity'sindeki duplicate Customer navigation kaldırıldı
- Customer.UserId için açık FK tanımı eklendi

### 4. SQLite Migration ✅
- PRAGMA komutu `suppressTransaction: true` ile çalıştırılıyor
- Migration transaction dışında execute ediliyor

### 5. Statik Dosyalar ✅
- wwwroot klasörü RestApi projesinde mevcut
- Dockerfile güncellendi, publish edilen dosyalar wwwroot içeriyor

### 6. Data Protection ✅
- Anahtarlar `/app/keys` dizininde kalıcı olarak saklanıyor
- Container restart'larında anahtarlar korunuyor

## Sorun Giderme

### JWT Key Hatası
- Environment variable'ların doğru ayarlandığından emin ol
- JWT_KEY en az 32 karakter olmalı
- Özel karakterler için escape gerekebilir

### Database Hatası
- Disk'in doğru mount edildiğini kontrol et
- Connection string'in `/app/data/ECommerce.db` olduğunu doğrula
- Migration loglarını kontrol et

### CORS Hatası
- CORS_ORIGIN_1 ve CORS_ORIGIN_2'nin frontend URL'leri ile eşleştiğini kontrol et

### Data Protection Uyarısı
- `/app/keys` disk'inin mount edildiğinden emin ol
- Disk yoksa anahtarlar container restart'ında kaybolur
