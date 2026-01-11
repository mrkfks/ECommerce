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

```
┌─────────────────────────────────────────────────────────────┐
│                       Client Layer                          │
│  ┌─────────────────────┐      ┌─────────────────────┐      │
│  │  Angular Frontend   │      │   Admin Dashboard   │      │
│  │   (Port: 4200)      │      │    (Port: 5001)     │      │
│  └──────────┬──────────┘      └──────────┬──────────┘      │
└─────────────┼───────────────────────────┼──────────────────┘
              │                            │
              └────────────┬───────────────┘
                           │ HTTP/HTTPS
              ┌────────────▼───────────────┐
              │      REST API Layer         │
              │     (Port: 5000)            │
              │  ┌──────────────────────┐  │
              │  │  Controllers         │  │
              │  │  Filters/Middleware  │  │
              │  │  API Response Format │  │
              │  └──────────┬───────────┘  │
              └─────────────┼───────────────┘
                            │
              ┌─────────────▼───────────────┐
              │   Application Layer         │
              │  ┌──────────────────────┐  │
              │  │  CQRS Commands       │  │
              │  │  MediatR Handlers    │  │
              │  │  FluentValidation    │  │
              │  └──────────┬───────────┘  │
              └─────────────┼───────────────┘
                            │
              ┌─────────────▼───────────────┐
              │    Domain Layer             │
              │  ┌──────────────────────┐  │
              │  │  Entities            │  │
              │  │  Value Objects       │  │
              │  │  Domain Events       │  │
              │  └──────────┬───────────┘  │
              └─────────────┼───────────────┘
                            │
              ┌─────────────▼───────────────┐
              │  Infrastructure Layer       │
              │  ┌──────────────────────┐  │
              │  │  EF Core             │  │
              │  │  Repository Pattern  │  │
              │  │  Unit of Work        │  │
              │  └──────────┬───────────┘  │
              └─────────────┼───────────────┘
                            │
              ┌─────────────▼───────────────┐
              │     Database Layer          │
              │     SQLite / SQL Server     │
              └─────────────────────────────┘
```

### Mimari Prensipleri
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
- **API Key Authentication** - X-Api-Key header ile şirkete özel erişim

### 🆕 Son Eklenen Özellikler

#### Standard API Response Format
Tüm API yanıtları standart formatta:
```json
{
  "success": true,
  "message": "İşlem başarılı",
  "data": { ... }
}
```

#### API Key Authentication
`X-Api-Key` header ile company-scoped erişim:
```bash
GET /api/v1/product
X-Api-Key: demo-key-123
# Otomatik olarak ilgili şirketin verilerini döner
```

#### Rich Text Editor (Banner Yönetimi)
Admin Panel'de Summernote entegrasyonu ile zengin metin editörü:
- Banner açıklamalarında HTML formatı
- Resim yükleme desteği
- Link ekleme
- Liste ve formatlandırma araçları

#### Product Image Management
Çoklu ürün resmi desteği:
- Her ürün için birden fazla resim
- Primary/ana resim belirleme
- Sıralama (Order) özelliği
- `/api/product/{id}/images` endpoint'leri ile yönetim

#### Angular Frontend
- Global error interceptor (SSR uyumlu)
- Loading spinner (HTTP isteklerinde otomatik)
- Custom email validators
- 404/500 error pages
- Interceptor zinciri (loading → api → auth → error)

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
## 📮 Postman Collection

Projeyi test etmek için Postman collection'ı hazır:

1. **Collection'ı İçe Aktar**: `ECommerce.postman_collection.json`
2. **Environment Ayarla**: 
   - `base_url`: `http://localhost:5000`
   - `jwt_token`: (otomatik doldurulur)
3. **Test Senaryosu**:
   - Authentication → Login - SuperAdmin (token otomatik set edilir)
   - Products → Get All Products (JWT ile)
   - Products → Get All Products (API Key) (X-Api-Key header ile)
   - Product Images → Add Product Image
   - Banners → Create Banner

**Not**: Login request'i otomatik olarak JWT token'ı environment'a kaydeder.
## � Demo Kullanıcılar

Seed data ile yüklenmiş test kullanıcıları:

| Rol | Email | Şifre | Açıklama |
|-----|-------|-------|----------|
| **SuperAdmin** | superadmin@system.com | Admin123! | Tüm şirketlere tam erişim |
| **Admin** | admin@techshop.com | Admin123! | TechShop yöneticisi |
| **User** | user@techshop.com | User123! | TechShop müşterisi |
| **Admin** | admin@fashionstore.com | Admin123! | FashionStore yöneticisi |

### Şirketler (Companies)
- **TechShop** (ID: 1) - Teknoloji ürünleri
- **FashionStore** (ID: 2) - Giyim ve aksesuar

### Test için
```bash
# SuperAdmin ile giriş (tüm şirketlere erişim)
POST /api/v1/auth/login
{
  "email": "superadmin@system.com",
  "password": "Admin123!"
}

# TechShop Admin ile giriş
POST /api/v1/auth/login
{
  "email": "admin@techshop.com",
  "password": "Admin123!"
}
```

## �🔐 Güvenlik

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
