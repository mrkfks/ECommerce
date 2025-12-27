# 🚀 HIZLI BAŞLANGIÇ - ECommerce Projesi

Bu dosya projeyi hızlıca çalıştırmanız için hazırlanmıştır.

## ⚡ Tek Adımda Başlat (Önerilen)

### Windows (PowerShell):
```powershell
# 1. API'yi başlat (Yeni terminal)
dotnet run --project src/Presentation/ECommerce.RestApi

# 2. Dashboard'ı başlat (Yeni terminal)
dotnet run --project AdminPanel/Dashboard.Web
```

### VS Code Tasks ile:
1. `Ctrl + Shift + P`
2. `Tasks: Run Task`
3. `dev:run-all` seçin

## 🔐 Test Kullanıcıları

Giriş yapmak için bu bilgileri kullanın:

### Kullanıcı 1 (SuperAdmin):
- **Email:** omerkafkas55@gmail.com
- **Şifre:** S5s5mr.kfks

### Kullanıcı 2 (CompanyAdmin - alican):
- **Email:** alican@company.com  
- **Şifre:** Alican123!

### Kullanıcı 3 (CompanyAdmin - velican):
- **Email:** velican@company.com
- **Şifre:** Velican123!

## 📊 Erişim Adresleri

| Servis | URL | Açıklama |
|--------|-----|----------|
| **API** | http://localhost:5027 | REST API Backend |
| **Swagger** | http://localhost:5027/swagger | API Dokümantasyonu |
| **Health Check** | http://localhost:5027/health | Sağlık Kontrolü |
| **Dashboard** | http://localhost:5041 | Admin Panel |

## ✅ Kontrol Listesi

Her şeyin çalıştığından emin olun:

```powershell
# API çalışıyor mu?
curl http://localhost:5027/health

# Dashboard çalışıyor mu?
curl http://localhost:5041
```

## 🛠️ Sorun Giderme

### API başlamıyor
```powershell
# Build hatalarını kontrol et
dotnet build src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj

# Başlat
dotnet run --project src/Presentation/ECommerce.RestApi
```

### Dashboard başlamıyor
```powershell
# Build hatalarını kontrol et
dotnet build AdminPanel/Dashboard.Web/Dashboard.Web.csproj

# Başlat
dotnet run --project AdminPanel/Dashboard.Web
```

### Port zaten kullanımda
```powershell
# Portları kontrol et
netstat -ano | findstr :5027
netstat -ano | findstr :5041

# Process'i öldür (PID numarasını yukarıdaki komuttan alın)
taskkill /PID <PID> /F
```

### Database hatası
```powershell
# Migration'ları uygula
cd src/Presentation/ECommerce.RestApi
dotnet ef database update
```

## 📝 İlk Giriş Adımları

1. **API'yi başlatın** (Terminal 1)
2. **Dashboard'ı başlatın** (Terminal 2)
3. Tarayıcıda http://localhost:5041/Auth/Login adresine gidin
4. Test kullanıcı bilgileriyle giriş yapın
5. Dashboard kullanıma hazır! 🎉

## 🔥 Hızlı Komutlar

```powershell
# Her şeyi temizle ve yeniden başlat
dotnet clean
dotnet build
dotnet run --project src/Presentation/ECommerce.RestApi

# Sadece build
dotnet build

# Test et
dotnet test

# Docker ile başlat
docker-compose up -d
```

## 📚 Daha Fazla Bilgi

- [README.md](README.md) - Detaylı proje dokümantasyonu
- [DEPLOYMENT.md](DEPLOYMENT.md) - Production deployment
- [API_USAGE_EXAMPLES.md](API_USAGE_EXAMPLES.md) - API kullanım örnekleri

## 💡 İpuçları

- Her iki servisi de **ayrı terminal**lerde çalıştırın
- API önce başlamalı, sonra Dashboard
- **Ctrl + C** ile servisleri durdurun
- Değişiklik yaptıktan sonra servisleri yeniden başlatın

---

**Sorun mu yaşıyorsunuz?** Önce API'nin çalıştığından emin olun: http://localhost:5027/health
