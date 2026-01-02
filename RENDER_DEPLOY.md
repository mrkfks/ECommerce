# Render.com Deployment Kılavuzu

## Environment Variables (Render Dashboard'da ayarla)

| Key | Value |
|-----|-------|
| JWT_ISSUER | ECommerce |
| JWT_AUDIENCE | ECommerce.Client |
| JWT_KEY | MyUltraStrongSecretKeyForECommerce2026!!! |
| ASPNETCORE_ENVIRONMENT | Production |
| CORS_ORIGIN_1 | http://localhost:3000 |
| CORS_ORIGIN_2 | https://myfrontend.com |

## Deploy Adımları

1. **Render Dashboard'a Git**
   - https://dashboard.render.com

2. **Environment Variables Ekle**
   - Service → Environment → Add Environment Variable
   - Yukarıdaki tabloyu kullanarak tüm değişkenleri ekle

3. **Manual Deploy**
   - "Manual Deploy" → "Clear build cache & deploy" seç
   - Bu, önceki build cache'ini temizler ve taze bir deploy yapar

4. **Logları Kontrol Et**
   - Deploy tamamlandıktan sonra "Logs" sekmesine git
   - "JWT Key bulunamadı" hatası artık görünmemeli
   - "✅ Database migrations completed" mesajını gör

5. **API Endpoint'leri Test Et**
   ```
   https://senin-api.onrender.com/health
   https://senin-api.onrender.com/api/products
   https://senin-api.onrender.com/swagger
   ```

## Disk Ayarları (SQLite için)

Render'da SQLite kullanıyorsanız, kalıcı disk eklemeniz gerekir:

1. Service → Disks → Add Disk
2. Name: `data`
3. Mount Path: `/app/data`
4. Size: 1 GB (veya ihtiyacınıza göre)

Bu sayede veritabanı dosyası deploy'lar arasında korunur.

## Sorun Giderme

### JWT Key Hatası
- Environment variable'ların doğru ayarlandığından emin ol
- JWT_KEY en az 32 karakter olmalı

### Database Hatası
- Disk'in doğru mount edildiğini kontrol et
- Connection string'in `/app/data/ECommerce.db` olduğunu doğrula

### CORS Hatası
- CORS_ORIGIN_1 ve CORS_ORIGIN_2'nin frontend URL'leri ile eşleştiğini kontrol et
