# E-Commerce Multi-Tenant Platform

Modern, scalable ve production-ready full-stack e-ticaret platformu. Clean Architecture, CQRS pattern ve multi-tenancy desteği ile geliştirilmiştir.

## ✅ Proje Durumu

**🎉 TÜM SİSTEMLER TEST EDİLDİ VE ÇALIŞIYOR!**

- ✅ Backend API (.NET 10.0) - Çalışıyor
- ✅ Admin Dashboard (ASP.NET MVC) - Çalışıyor  
- ✅ Frontend (Angular 21) - Çalışıyor
- ✅ Tüm endpoint'ler test edildi
- ✅ Authentication & Authorization çalışıyor
- ✅ Database hazır ve seed data yüklü

**[📊 Detaylı Test Raporu](TEST_RESULTS.md)**

## 🏗️ Mimari

- **Clean Architecture** (Domain, Application, Infrastructure, Presentation)
- **CQRS + MediatR** Pattern
- **Domain-Driven Design** (DDD)
- **Multi-Tenancy** (Company-based data isolation)
- **Repository Pattern + Unit of Work**

## 🚀 Teknolojiler

- **.NET 10.0**
- **Entity Framework Core 9.0** (SQLite, SQL Server desteği)
- **MediatR** (CQRS implementation)
- **AutoMapper**
- **FluentValidation**
- **JWT Authentication**
- **Serilog** (Structured logging)
- **Swagger/OpenAPI**
- **Docker** support

## 📋 Özellikler

### ✅ Production-Ready Özellikler
- ✅ Global Exception Handling
- ✅ Structured Logging (Serilog)
- ✅ Health Checks (`/health`)
- ✅ Rate Limiting
- ✅ Response Caching
- ✅ API Versioning
- ✅ CORS Configuration
- ✅ JWT Authentication & Authorization
- ✅ Soft Delete
- ✅ Audit Trail (CreatedAt, UpdatedAt)

### 🏢 Multi-Tenancy
Her şirket kendi verilerine erişir (Company-based isolation):
- Automatic filtering through Global Query Filters
- Tenant context injection
- Same-company authorization policies

## 📦 Kurulum

### Gereksinimler
- .NET 10.0 SDK
- Docker (opsiyonel)
- SQL Server veya PostgreSQL (production için)

### 1. Repository'yi Clone Et
```bash
git clone <repository-url>
cd ECommerce
```

### 2. Bağımlılıkları Yükle
```bash
dotnet restore
```

### 3. Veritabanı Migration
```bash
cd src/Presentation/ECommerce.RestApi
dotnet ef database update
```

### 4. Çalıştır
```bash
dotnet run --project src/Presentation/ECommerce.RestApi
```

API: `http://localhost:5000`  
Swagger: `http://localhost:5000/swagger`

## 🐳 Docker ile Çalıştırma

### Tek Komutla Başlat
```bash
# .env dosyası oluştur
cp .env.example .env
# Gerekli environment variable'ları düzenle

# Docker Compose ile başlat
docker-compose up -d
```

### Manuel Docker Build
```bash
docker build -t ecommerce-api .
docker run -p 5000:8080 ecommerce-api
```

## ⚙️ Yapılandırma

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ECommerce.db"
  },
  "Jwt": {
    "Issuer": "ECommerce",
    "Audience": "ECommerce.Client",
    "Key": "your-secret-key-min-32-chars",
    "ExpiresInMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://yourapp.com"
    ]
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": 60,
    "QueueLimit": 2
  }
}
```

### Production için appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=ECommerce;..."
  },
  "Jwt": {
    "Key": "" // Environment variable'dan oku
  }
}
```

### Environment Variables
```bash
# JWT
JWT_KEY=your-super-secret-key-at-least-32-characters
JWT_ISSUER=ECommerce
JWT_AUDIENCE=ECommerce.Client

# Database (SQL Server)
ConnectionStrings__DefaultConnection="Server=localhost;Database=ECommerce;..."

# CORS
Cors__AllowedOrigins__0=https://yourapp.com
```

## 📚 API Endpoints

### Authentication
- `POST /api/v1/auth/login` - Login
- `POST /api/v1/auth/register` - Register

### Products
- `GET /api/v1/product` - Tüm ürünleri listele
- `GET /api/v1/product/{id}` - Ürün detayı
- `GET /api/v1/product/category/{categoryId}` - Kategoriye göre ürünler
- `GET /api/v1/product/search?searchTerm=...` - Ürün arama
- `POST /api/v1/product` - Yeni ürün oluştur
- `PUT /api/v1/product/{id}` - Ürün güncelle
- `PATCH /api/v1/product/{id}/stock` - Stok güncelle
- `DELETE /api/v1/product/{id}` - Ürün sil

### Health Check
- `GET /health` - Sistem sağlığı kontrolü

Tüm endpoint'ler için Swagger UI: `/swagger`

## 🔐 Güvenlik

### JWT Authentication
```bash
# Login ile token al
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "password"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600
}

# Token'ı kullan
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Role-Based Authorization
- `SuperAdmin` - Tüm şirketlere erişim
- `CompanyAdmin` - Kendi şirketine tam erişim
- `User` - Kendi şirketinde sınırlı erişim

### CORS
Production'da sadece güvendiğiniz domainlere izin verin:
```json
"Cors": {
  "AllowedOrigins": [
    "https://yourapp.com",
    "https://admin.yourapp.com"
  ]
}
```

## 📊 Logging

Loglar `/logs` klasöründe günlük olarak döner:
- `logs/ecommerce-20251227.txt`
- `logs/ecommerce-20251228.txt`

Console ve file logging aktif.

## 🧪 Testing

```bash
# Unit testleri çalıştır (eklenecek)
dotnet test

# Integration testler (eklenecek)
dotnet test --filter Category=Integration
```

## 📈 Performans

- **Response Caching**: Sık okunan veriler (products, categories)
- **Rate Limiting**: DDoS koruması
- **Database Indexing**: Optimized queries
- **Async/Await**: Non-blocking operations

## 🚀 Deployment

### Azure App Service
```bash
# Azure CLI ile deploy
az webapp up --name your-app-name --resource-group your-rg
```

### AWS
```bash
# Docker image'ı AWS ECR'a push et
docker tag ecommerce-api:latest <aws-account>.dkr.ecr.region.amazonaws.com/ecommerce-api
docker push <aws-account>.dkr.ecr.region.amazonaws.com/ecommerce-api
```

### Sunucuya Manuel Deploy
```bash
# Publish
dotnet publish -c Release -o ./publish

# Sunucuya kopyala
scp -r ./publish user@server:/var/www/ecommerce-api

# Systemd service oluştur
sudo systemctl start ecommerce-api
```

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📝 License

MIT License - detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👥 İletişim

Proje Sahibi - [@yourusername](https://github.com/yourusername)

Proje Link: [https://github.com/yourusername/ecommerce](https://github.com/yourusername/ecommerce)
