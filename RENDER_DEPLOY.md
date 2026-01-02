# Render.com Deployment Kılavuzu

Bu projede iki ayrı servis bulunmaktadır:
1. **ECommerce API** - REST API servisi
2. **Dashboard** - Admin Panel web uygulaması

Her iki servis için de ayrı Render Web Service oluşturmalısınız.

---

## 🚀 Servis 1: ECommerce API

### Dockerfile
```
Dockerfile
```

### Environment Variables (API için)

| Key | Value | Açıklama |
|-----|-------|----------|
| JWT_KEY | **En az 32 karakterlik güçlü key** | `openssl rand -base64 32` ile oluştur |
| JWT_ISSUER | ECommerce | Token issuer |
| JWT_AUDIENCE | ECommerce.Client | Token audience |
| ASPNETCORE_ENVIRONMENT | Production | Uygulama ortamı |
| DOTNET_DATA_PROTECTION_KEY_DIRECTORY | /app/keys | Data Protection anahtarları |
| CORS_ORIGIN_1 | https://your-dashboard.onrender.com | Dashboard URL |
| CORS_ORIGIN_2 | http://localhost:3000 | Local frontend URL |

### Disk'ler (API için zorunlu)
- **data**: `/app/data` - 1 GB (SQLite veritabanı)
- **keys**: `/app/keys` - 100 MB (Data Protection anahtarları)

### API Endpoint'leri Test Et
```
https://senin-api.onrender.com/health
https://senin-api.onrender.com/api/products
https://senin-api.onrender.com/swagger
```

---

## 🖥️ Servis 2: Dashboard (Admin Panel)

### Dockerfile
```
Dockerfile.dashboard
```

### Environment Variables (Dashboard için)

| Key | Value | Açıklama |
|-----|-------|----------|
| API_BASE_URL | https://ecommerce-hov4.onrender.com | API servisi URL'i |
| JWT_KEY | **API ile AYNI key** | API ile aynı değer olmalı |
| JWT_ISSUER | ECommerce | API ile aynı |
| JWT_AUDIENCE | ECommerce.Client | API ile aynı |
| ASPNETCORE_ENVIRONMENT | Production | Uygulama ortamı |

> ⚠️ **Kritik**: JWT_KEY, JWT_ISSUER ve JWT_AUDIENCE değerleri **her iki serviste de aynı** olmalıdır. Aksi halde login işlemleri çalışmaz!

---

## 🔐 JWT Key Oluşturma

```bash
openssl rand -base64 32
# Örnek çıktı: K3xP9mN2vQ8rT5wY1zB4cF7hJ0kL6nU9
```

> ⚠️ **Önemli**: JWT_KEY en az 32 karakter olmalı ve her iki serviste de **aynı değer** kullanılmalı.

---

## 📋 Deploy Adımları

### Adım 1: API Servisini Oluştur

1. **Render Dashboard'a Git**: https://dashboard.render.com
2. **New → Web Service** seç
3. **Git repo'yu bağla**
4. **Ayarlar**:
   - Name: `ecommerce-api`
   - Root Directory: (boş bırak)
   - Environment: `Docker`
   - Dockerfile Path: `Dockerfile`
5. **Environment Variables** ekle (yukarıdaki API tablosunu kullan)
6. **Disk'leri ekle** (data ve keys)
7. **Deploy**

### Adım 2: Dashboard Servisini Oluştur

1. **New → Web Service** seç
2. **Aynı Git repo'yu bağla**
3. **Ayarlar**:
   - Name: `ecommerce-dashboard`
   - Root Directory: (boş bırak)
   - Environment: `Docker`
   - Dockerfile Path: `Dockerfile.dashboard`
4. **Environment Variables** ekle (yukarıdaki Dashboard tablosunu kullan)
   - API_BASE_URL = API servisinin URL'i
   - JWT_KEY, JWT_ISSUER, JWT_AUDIENCE = API ile aynı değerler
5. **Deploy**

### Adım 3: CORS Ayarını Güncelle

1. **API servisinin Environment Variables**'ına git
2. **CORS_ORIGIN_1** değerini Dashboard URL'i ile güncelle:
   - `https://ecommerce-dashboard.onrender.com`
3. **API'yi yeniden deploy et**

---

## ✅ Yapılan Düzeltmeler

### 1. Swagger JSON Hatası Düzeltildi ✅
- Primitive types ([FromBody] string, int, enum) için wrapper DTO'lar oluşturuldu
- `RoleAssignmentDto`, `StockUpdateDto`, `UpdateOrderStatusDto` eklendi
- Swagger konfigürasyonu iyileştirildi

### 2. JWT Ayarları ✅
- JWT_KEY environment variable'dan okunuyor
- Minimum 32 karakter zorunluluğu
- JWT_ISSUER ve JWT_AUDIENCE değerleri doğrulandı

### 3. Dashboard Environment Variables ✅
- API_BASE_URL environment variable desteği eklendi
- JWT ayarları environment variable'dan okunuyor
- API ile JWT ayarları senkronize çalışıyor

### 4. EF Core Global Query Filter ✅
- Child entity'ler için uyumlu query filter eklendi
- Navigation property'ler optional yapıldı

### 5. SQLite Migration ✅
- PRAGMA komutu `suppressTransaction: true` ile çalıştırılıyor

### 6. Data Protection ✅
- Anahtarlar `/app/keys` dizininde kalıcı olarak saklanıyor

---

## 🔧 Sorun Giderme

### Swagger 500 Hatası
- Düzeltildi! Primitive type'lar için DTO wrapper'lar eklendi
- Swagger JSON artık sorunsuz çalışıyor

### JWT Key Hatası
- Environment variable'ların doğru ayarlandığından emin ol
- JWT_KEY en az 32 karakter olmalı
- **Her iki serviste de aynı JWT_KEY kullanılmalı**

### Dashboard API'ye Bağlanamıyor
- API_BASE_URL'in doğru ayarlandığını kontrol et
- URL'in sonunda `/` olmamalı
- CORS ayarlarının Dashboard URL'ini içerdiğini doğrula

### Database Hatası
- Disk'in doğru mount edildiğini kontrol et
- Connection string'in `/app/data/ECommerce.db` olduğunu doğrula

### CORS Hatası
- CORS_ORIGIN_1'in Dashboard URL'i ile eşleştiğini kontrol et

### Data Protection Uyarısı
- `/app/keys` disk'inin mount edildiğinden emin ol
- Disk yoksa anahtarlar container restart'ında kaybolur
