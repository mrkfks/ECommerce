# 🛒 ECommerce — Multi-Tenant E-Ticaret Platformu

> **ASP.NET Core 9 · Angular 21 · Clean Architecture · Multi-Tenant · SSR**

ECommerce, birden fazla şirketin (tenant) tek bir altyapı üzerinde bağımsız mağazalarını yönetmesine olanak tanıyan, uçtan uca bir e-ticaret çözümüdür. Backend REST API, Admin Dashboard (MVC) ve Müşteri Frontend (Angular SSR) olmak üzere üç ana uygulama katmanından oluşur.

---

## 📑 İçindekiler

- [Mimari Genel Bakış](#-mimari-genel-bakış)
- [Teknoloji Yığını](#-teknoloji-yığını)
- [Proje Yapısı](#-proje-yapısı)
- [Katmanlar ve Sorumluluklar](#-katmanlar-ve-sorumluluklar)
- [Veritabanı ve Entity Modeli](#-veritabanı-ve-entity-modeli)
- [API Endpoint'leri](#-api-endpointleri)
- [Admin Dashboard (MVC)](#-admin-dashboard-mvc)
- [Frontend (Angular SSR)](#-frontend-angular-ssr)
- [Kimlik Doğrulama ve Yetkilendirme](#-kimlik-doğrulama-ve-yetkilendirme)
- [Multi-Tenant Mimari](#-multi-tenant-mimari)
- [Kurulum ve Çalıştırma](#-kurulum-ve-çalıştırma)
- [Docker ile Dağıtım](#-docker-ile-dağıtım)
- [Ortam Değişkenleri](#-ortam-değişkenleri)
- [Test Verileri ve Seeding](#-test-verileri-ve-seeding)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Katkıda Bulunma](#-katkıda-bulunma)

---

## 🏗 Mimari Genel Bakış

Proje, **Clean Architecture** (Temiz Mimari) prensiplerine göre katmanlara ayrılmıştır:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ REST API     │  │ Dashboard    │  │ Angular Frontend  │  │
│  │ (Port 5010)  │  │ (Port 5001)  │  │ (Port 4200)       │  │
│  └──────┬───────┘  └──────┬───────┘  └───────┬───────────┘  │
│         │                 │                   │              │
├─────────┼─────────────────┼───────────────────┼──────────────┤
│         ▼                 ▼                   │              │
│  ┌──────────────────────────────┐             │              │
│  │    Application Layer         │◄────────────┘              │
│  │  (DTOs, Interfaces, Mappers) │                            │
│  └──────────────┬───────────────┘                            │
│                 │                                            │
├─────────────────┼────────────────────────────────────────────┤
│                 ▼                                            │
│  ┌──────────────────────────────┐                            │
│  │    Domain Layer              │                            │
│  │  (Entities, Enums, Rules)    │                            │
│  └──────────────────────────────┘                            │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────┐                            │
│  │   Infrastructure Layer       │                            │
│  │  (EF Core, Repos, Services)  │                            │
│  └──────────────────────────────┘                            │
└──────────────────────────────────────────────────────────────┘
```

---

## 🧰 Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| **Backend API** | ASP.NET Core 9, Entity Framework Core 9, Serilog, FluentValidation, AutoMapper |
| **Veritabanı** | SQLite (geliştirme), PostgreSQL (üretim) — otomatik algılama |
| **Kimlik Doğrulama** | JWT Bearer Token, BCrypt şifre hashleme |
| **Arama** | Elasticsearch (NEST 7.x) |
| **Gerçek Zamanlı İletişim** | SignalR (NotificationHub) |
| **Önbellek** | Distributed Memory Cache, Response Caching |
| **Görüntü İşleme** | SixLabors.ImageSharp |
| **Admin Dashboard** | ASP.NET Core MVC, Razor Views, Bootstrap |
| **Frontend** | Angular 21, Bootstrap 5, SSR (Server-Side Rendering) |
| **Containerization** | Docker, Docker Compose, Nginx (reverse proxy) |
| **Bulut Dağıtımı** | Render.com (Blueprint desteği) |
| **Loglama** | Serilog (Console + File sinks) |
| **API Versiyonlama** | Microsoft.AspNetCore.Mvc.Versioning |
| **Rate Limiting** | Yerleşik ASP.NET Core Rate Limiter |

---

## 📁 Proje Yapısı

```
ECommerce/
├── ECommerce.slnx                    # Çözüm dosyası
├── docker-compose.yml                # Docker Compose yapılandırması
├── nginx.conf / nginx.docker.conf    # Nginx reverse proxy yapılandırması
├── render.yaml                       # Render.com deployment blueprint
│
├── src/
│   ├── Core/
│   │   ├── ECommerce.Domain/         # Entities, Enums, Interfaces (DDD)
│   │   └── ECommerce.Application/    # DTOs, Interfaces, Validators, Mappings
│   │
│   ├── Infrastructure/
│   │   └── ECommerce.Infrastructure/ # EF Core, Repositories, Services, Hubs
│   │
│   └── Presentation/
│       └── ECommerce.RestApi/        # ASP.NET Core Web API (Controllers)
│
├── AdminPanel/
│   └── Dashboard.Web/                # ASP.NET Core MVC Admin Dashboard
│       ├── Controllers/              # 17 Controller (Auth, Product, Order…)
│       ├── Views/                    # Razor Views (Auth, Product, Campaign…)
│       ├── Models/                   # ViewModel'ler
│       ├── Services/                 # API istemci servisleri
│       ├── Helpers/                  # Tag helper'lar, Notification uzantıları
│       └── Middleware/               # Hata yönetimi middleware
│
└── Frontend/
    └── ECommerce-Frontend/           # Angular 21 SSR uygulaması
        ├── src/app/
        │   ├── pages/                # Lazy-loaded sayfa bileşenleri
        │   ├── components/           # Paylaşılan bileşenler
        │   ├── core/                 # Servisler, modeller, interceptor'lar
        │   ├── guards/               # Route guard'lar
        │   └── state/                # Durum yönetimi (CartState vb.)
        └── proxy.conf.json           # Angular dev proxy yapılandırması
```

---

## 🧱 Katmanlar ve Sorumluluklar

### Domain Layer (`ECommerce.Domain`)
En iç katman — hiçbir dış bağımlılığı yoktur. İş kurallarını ve entity'leri barındırır.

| Bileşen | Açıklama |
|---------|----------|
| `BaseEntity` | Tüm entity'lerin temel sınıfı (Id, CreatedAt, UpdatedAt) |
| `IAuditable` | Denetim alanları interface'i |
| `ISoftDeletable` | Yumuşak silme (IsDeleted, DeletedAt) |
| `ITenantEntity` | Multi-tenant filtreleme (CompanyId) |
| **38 Entity** | Product, Order, Customer, Company, Campaign ve daha fazlası |
| **3 Enum** | `OrderStatus`, `ReturnRequestStatus`, `NotificationType` |

### Application Layer (`ECommerce.Application`)
İş mantığı sözleşmeleri, DTO'lar ve doğrulama kuralları.

| Bileşen | Açıklama |
|---------|----------|
| `Interfaces/` | 28 servis arayüzü (IProductService, IOrderService…) |
| `DTOs/` | 41+ veri transfer nesnesi |
| `Validators/` | FluentValidation kuralları |
| `Mappings/` | AutoMapper profilleri |
| `Responses/` | Standart API yanıt modelleri |
| `Exceptions/` | Özel hata sınıfları |

### Infrastructure Layer (`ECommerce.Infrastructure`)
Veritabanı erişimi, harici servisler ve altyapısal implementasyonlar.

| Bileşen | Açıklama |
|---------|----------|
| `Data/AppDbContext.cs` | EF Core DbContext — 30+ DbSet, global query filter'lar |
| `Data/DataSeeder.cs` | Veritabanı tohumlama (roller, şirketler, örnek veriler) |
| `Data/Configurations/` | EF Core Fluent API yapılandırmaları |
| `Repositories/` | Generic + özel repository implementasyonları |
| `Services/` | 29 servis implementasyonu |
| `Hubs/NotificationHub.cs` | SignalR gerçek zamanlı bildirim hub'ı |
| `Migrations/` | EF Core migration'ları (SQLite tabanlı) |

### Presentation Layer (`ECommerce.RestApi`)
HTTP endpoint'leri, middleware'ler, filtreler.

| Bileşen | Açıklama |
|---------|----------|
| `Controllers/` | 25 API Controller |
| `Middleware/` | Global exception handler |
| `Filters/` | API response filtresi |
| `Authorization/` | Özel yetkilendirme handler'ları |
| `Options/` | API Key yapılandırması |

---

## 🗄 Veritabanı ve Entity Modeli

### Temel Entity'ler

| Entity | Açıklama | Tenant? | Soft Delete? |
|--------|----------|---------|--------------|
| `Company` | Şirket/mağaza (tenant) | — | ✅ |
| `User` | Sistem kullanıcıları | — | ✅ |
| `Role` | Kullanıcı rolleri | — | — |
| `UserRole` | User ↔ Role ilişkisi | — | — |
| `Customer` | Müşteriler | ✅ | — |
| `Address` | Müşteri adresleri | — | — |
| `Product` | Ürünler | ✅ | — |
| `ProductImage` | Ürün görselleri | — | — |
| `ProductSpecification` | Ürün teknik özellikleri | ✅ | — |
| `ProductVariant` | Ürün varyantları | ✅ | ✅ |
| `ProductVariantAttribute` | Varyant nitelikleri | — | — |
| `Category` | Ürün kategorileri (hiyerarşik) | ✅ | — |
| `Brand` | Markalar | ✅ | — |
| `Model` | Marka modelleri | ✅ | — |
| `Order` | Siparişler | ✅ | ✅ |
| `OrderItem` | Sipariş kalemleri | — | — |
| `Cart` | Alışveriş sepeti | ✅ | ✅ |
| `CartItem` | Sepet kalemleri | — | ✅ |
| `Wishlist` | İstek listesi | ✅ | ✅ |
| `WishlistItem` | İstek listesi kalemleri | — | ✅ |
| `Review` | Ürün değerlendirmeleri | ✅ | ✅ |
| `Campaign` | Kampanyalar | ✅ | ✅ |
| `ProductCampaign` | Ürün ↔ Kampanya | — | — |
| `CategoryCampaign` | Kategori ↔ Kampanya | — | — |
| `Banner` | Ana sayfa banner'ları | ✅ | ✅ |
| `ReturnRequest` | İade talepleri | ✅ | ✅ |
| `Request` | Genel talepler | ✅ | — |
| `Notification` | Bildirimler | ✅ | ✅ |
| `CustomerMessage` | Müşteri mesajları | ✅ | ✅ |
| `LoginHistory` | Giriş geçmişi | — | — |
| `GlobalAttribute` | Global ürün nitelikleri | ✅ | ✅ |
| `GlobalAttributeValue` | Global nitelik değerleri | — | ✅ |
| `CategoryAttribute` | Kategori bazlı nitelikler | ✅ | ✅ |
| `CategoryAttributeValue` | Kategori nitelik değerleri | — | ✅ |
| `CategoryGlobalAttribute` | Kategori ↔ GlobalAttribute | — | ✅ |
| `BrandCategory` | Marka ↔ Kategori ilişkisi | — | — |

### Özel Özellikler
- **Optimistic Concurrency**: `Product` entity'sinde `Version` (Guid) ile eşzamanlılık kontrolü
- **Global Query Filter'lar**: Tenant izolasyonu ve yumuşak silme otomatik filtreleme
- **Audit Fields**: `CreatedAt`, `UpdatedAt` otomatik yönetim (`SaveChangesAsync` override)

---

## 🌐 API Endpoint'leri

API varsayılan olarak `http://localhost:5010` adresinde çalışır. Swagger dokümantasyonu: `http://localhost:5010/swagger`

### Kimlik Doğrulama
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/auth/login` | Kullanıcı girişi (JWT döner) |
| POST | `/api/auth/register` | Yeni kullanıcı kaydı |
| POST | `/api/auth/refresh` | Token yenileme |

### Ürünler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/products` | Ürün listesi (sayfalı) |
| GET | `/api/products/{id}` | Ürün detayı |
| POST | `/api/products` | Yeni ürün oluştur |
| PUT | `/api/products/{id}` | Ürün güncelle |
| DELETE | `/api/products/{id}` | Ürün sil |

### Kategoriler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/categories` | Kategori listesi |
| GET | `/api/categories/{id}` | Kategori detayı |
| POST | `/api/categories` | Yeni kategori |
| PUT | `/api/categories/{id}` | Kategori güncelle |
| DELETE | `/api/categories/{id}` | Kategori sil |

### Markalar & Modeller
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/brands` | Marka listesi |
| POST | `/api/brands` | Yeni marka |
| GET | `/api/models` | Model listesi |
| POST | `/api/models` | Yeni model |

### Siparişler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/orders` | Sipariş listesi |
| GET | `/api/orders/{id}` | Sipariş detayı |
| POST | `/api/orders` | Yeni sipariş oluştur |
| PUT | `/api/orders/{id}/status` | Sipariş durumu güncelle |

### Müşteriler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/customers` | Müşteri listesi |
| GET | `/api/customers/{id}` | Müşteri detayı |
| POST | `/api/customers` | Yeni müşteri |

### Sepet & İstek Listesi
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/cart` | Sepeti getir |
| POST | `/api/cart/items` | Sepete ürün ekle |
| DELETE | `/api/cart/items/{id}` | Sepetten ürün çıkar |
| GET | `/api/wishlist` | İstek listesi |
| POST | `/api/wishlist/items` | İstek listesine ekle |

### Kampanyalar
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/campaigns` | Kampanya listesi |
| POST | `/api/campaigns` | Yeni kampanya |
| PUT | `/api/campaigns/{id}` | Kampanya güncelle |

### Diğer Endpoint'ler
| Controller | Kapsam |
|------------|--------|
| `BannerController` | Ana sayfa banner yönetimi |
| `ReviewController` | Ürün değerlendirmeleri |
| `ReturnRequestController` | İade talepleri |
| `NotificationController` | Bildirim yönetimi |
| `CustomerMessageController` | Müşteri mesajları |
| `FileUploadController` | Dosya/görüntü yükleme |
| `GlobalAttributeController` | Global ürün nitelikleri |
| `DashboardController` | Dashboard KPI verileri |
| `CompanyController` | Şirket/tenant yönetimi |
| `RoleController` | Rol yönetimi |
| `UserController` | Kullanıcı yönetimi |
| `UserManagementController` | Gelişmiş kullanıcı işlemleri |
| `LoginHistoryController` | Giriş geçmişi |
| `RequestController` | Genel talepler |

---

## 🖥 Admin Dashboard (MVC)

Admin Dashboard, `http://localhost:5001` adresinde çalışan ASP.NET Core MVC uygulamasıdır. Backend API'ye HTTP istekleri ile bağlanır.

### Modüller

| Modül | Açıklama |
|-------|----------|
| **Home** | Dashboard ana sayfa, KPI kartları, grafikler |
| **Products** | Ürün CRUD, resim yükleme, stok yönetimi |
| **Categories** | Hiyerarşik kategori yönetimi |
| **Brands** | Marka yönetimi |
| **Models** | Model yönetimi |
| **Orders** | Sipariş listeleme, durum güncelleme |
| **Customers** | Müşteri yönetimi, segmentasyon |
| **Campaigns** | Kampanya oluşturma, ürün/kategori eşleştirme |
| **Banners** | Ana sayfa banner yönetimi |
| **Reviews** | Ürün değerlendirme moderasyonu |
| **Return Requests** | İade talebi yönetimi |
| **Requests** | Genel talep yönetimi |
| **Users** | Kullanıcı ve rol yönetimi |
| **Settings** | Şirket branding ayarları (logo, renkler, domain) |
| **Auth** | Giriş / çıkış |

### Teknik Özellikler
- JWT token tabanlı API iletişimi (`AuthTokenHandler`)
- Bildirim sistemi (TempData tabanlı Toast)
- Aktif route highlight (`ActiveRouteTagHelper`)
- Resim optimizasyonu (`ImageHelper`, `ImageTagHelper`)
- Global hata yönetimi middleware

### Dashboard Ekran Görüntüleri

#### Giriş & Kayıt
| Giriş Ekranı | Kayıt Ekranı |
|:---:|:---:|
| ![Dashboard Giriş](ScreenShots/DshLogin.png) | ![Dashboard Kayıt](ScreenShots/DshRegister.png) |

#### Ana Panel & İstatistikler
| Dashboard Panel 1 | Dashboard Panel 2 |
|:---:|:---:|
| ![Panel 1](ScreenShots/DshPanel1.png) | ![Panel 2](ScreenShots/DshPanel2.png) |

#### Navigasyon
![Dashboard Navbar](ScreenShots/DshNavbar.png)

#### Ürün Yönetimi
![Ürünler](ScreenShots/DshProducts.png)

#### Kategori Yönetimi
![Kategoriler](ScreenShots/DshCategoryIndex.png)

#### Marka Yönetimi
![Markalar](ScreenShots/DshBrandIndex.png)

#### Özellik Yönetimi
![Özellikler](ScreenShots/DshFeatureIndex.png)

#### Sipariş Yönetimi
![Siparişler](ScreenShots/DshOrder.png)

#### Müşteri Yönetimi
![Müşteriler](ScreenShots/DshCustomer.png)

#### Kampanya Yönetimi
![Kampanyalar](ScreenShots/DshCampaing.png)

#### Şirket Yönetimi
![Şirket](ScreenShots/DshCompany.png)

#### Değerlendirme Yönetimi
![Değerlendirmeler](ScreenShots/DshReview.png)

#### İade Talepleri
![İade Talepleri](ScreenShots/DshReturnRequest.png)

#### Genel Talepler
![Talepler](ScreenShots/DshRequestIndex.png)

#### Bildirimler
![Bildirimler](ScreenShots/DshNotofication.png)

#### Dashboard Genel Görünüm
![Dashboard Genel](ScreenShots/Dsh.png)

---

## 🌍 Frontend (Angular SSR)

Angular 21 ile geliştirilmiş müşteri-yüzlü SPA/SSR uygulaması. `http://localhost:4200` (geliştirme) veya `http://localhost:4000` (üretim) portunda çalışır.

### Sayfalar (Lazy-loaded)

| Sayfa | Route | Açıklama |
|-------|-------|----------|
| Ana Sayfa | `/home` | Ürün vitrin, banner carousel, kampanyalar |
| Kategori Ürünleri | `/products/:categoryId` | Kategoriye göre ürün listeleme |
| Ürün Detay | `/product/:productId` | Ürün bilgileri, görseller, yorumlar |
| Sepet | `/cart` | Alışveriş sepeti |
| Ödeme | `/checkout` | Sipariş tamamlama (🔒 auth gerekir) |
| Sipariş Onay | `/order/:orderId` | Sipariş detayı (🔒 auth gerekir) |
| Sipariş Geçmişi | `/orders` | Geçmiş siparişler (🔒 auth gerekir) |
| Profil | `/profile` | Kullanıcı profili (🔒 auth gerekir) |
| İstek Listesi | `/wishlist` | Favori ürünler |
| Giriş | `/login` | Kullanıcı girişi |
| Kayıt | `/register` | Yeni hesap oluşturma |
| 404 | `/404` | Sayfa bulunamadı |
| Hata | `/error` | Sunucu hatası |

### Paylaşılan Bileşenler
- `NavbarComponent` — Üst navigasyon, sepet sayacı, kullanıcı menüsü
- `FooterComponent` — Alt bilgi
- `ProductCardComponent` — Ürün kartı (liste görünümü)
- `CampaignCarouselComponent` — Kampanya kaydırıcısı
- `CampaignPriceDisplayComponent` — İndirimli fiyat göstergesi
- `ModalComponent` — Genel amaçlı modal

### Servisler
| Servis | Sorumluluk |
|--------|------------|
| `AuthService` | Login, register, token yönetimi |
| `ProductService` | Ürün CRUD & arama |
| `CategoryService` | Kategori verisi |
| `BrandService` | Marka verisi |
| `CartService` | Sepet işlemleri |
| `WishlistService` | İstek listesi |
| `OrderService` | Sipariş oluşturma & geçmiş |
| `ReviewService` | Ürün yorumları |
| `BannerService` | Banner verisi |
| `ReturnRequestService` | İade talepleri |
| `CompanyContextService` | Tenant/şirket bağlamı |
| `DesignService` | Dinamik tema (şirket renkleri) |
| `ImageUrlService` | API görüntü URL çözümleme |
| `LoadingService` | Yükleniyor göstergesi |

### State Management
- `CartStateService` — Reaktif sepet durumu (BehaviorSubject tabanlı)

### Frontend Ekran Görüntüleri

#### Ana Sayfa
![Ana Sayfa](ScreenShots/FrntHome.png)

#### Ürün Listeleme
![Ürünler](ScreenShots/FrntProducts.png)

#### Alışveriş Sepeti
![Sepet](ScreenShots/FrntBskt.png)

#### Favoriler / İstek Listesi
![Favoriler](ScreenShots/FrntFvrt.png)

#### Giriş & Kayıt
| Giriş Ekranı | Kayıt Ekranı |
|:---:|:---:|
| ![Frontend Giriş](ScreenShots/FrntLgn.png) | ![Frontend Kayıt](ScreenShots/FrntRgstr.png) |

---

## 🔐 Kimlik Doğrulama ve Yetkilendirme

### JWT Token Akışı
1. Kullanıcı `POST /api/auth/login` ile giriş yapar
2. API, kullanıcı bilgilerini doğrular ve JWT token döner
3. İstemci, sonraki isteklerde `Authorization: Bearer <token>` header'ı kullanır
4. Token süresi dolduğunda `POST /api/auth/refresh` ile yenilenir

### Roller ve Yetkiler

| Rol | Açıklama | Yetki Kapsamı |
|-----|----------|---------------|
| `SuperAdmin` | Sistem yöneticisi | Tüm şirketler, tüm veriler |
| `CompanyAdmin` | Şirket yöneticisi | Kendi şirketinin tüm verileri |
| `User` | Şirket personeli | Kendi şirketinin belirli verileri |
| `Customer` | Müşteri | Alışveriş, profil, sipariş geçmişi |

### Politikalar
- **SuperAdminOnly**: Yalnızca SuperAdmin erişimi
- **CompanyAccess**: CompanyAdmin, SuperAdmin veya User
- **SameCompanyOrSuperAdmin**: Aynı şirkete ait kullanıcılar veya SuperAdmin

---

## 🏢 Multi-Tenant Mimari

Her şirket (Company) bir **tenant**'tır. Veriler `CompanyId` ile izole edilir.

### Tenant İzolasyon Mekanizması
1. **Global Query Filter**: `ITenantEntity` implement eden tüm entity'lere otomatik `CompanyId` filtresi uygulanır
2. **TenantService**: HTTP context'ten (JWT claim) aktif şirket ID'sini çözer
3. **Soft Delete Filter**: `ISoftDeletable` entity'ler otomatik olarak `IsDeleted = false` ile filtrelenir
4. **SuperAdmin Bypass**: `CompanyId = null` olduğunda tüm veriler görünür (admin panel)

### Şirket Branding
Her tenant kendi görsel kimliğini özelleştirebilir:
- **Domain**: Özel alt-alan (ör. `tenant1.myshop.com`)
- **Logo**: Şirket logosu URL'i
- **Renkler**: Birincil ve ikincil tema renkleri
- Frontend, `CompanyContextService` ve `DesignService` ile dinamik tema uygular

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) ve npm 10+
- (Opsiyonel) [Docker](https://www.docker.com/) — konteyner dağıtımı için

### 1. Projeyi Klonlayın
```bash
git clone <repo-url>
cd ECommerce
```

### 2. Backend API'yi Başlatın
```bash
# Çözümü derleyin
dotnet build ECommerce.slnx

# REST API'yi çalıştırın (http://localhost:5010)
dotnet run --project src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj
```
> İlk çalıştırmada EF Core migration'ları otomatik uygulanır, roller ve SuperAdmin hesabı oluşturulur, geliştirme ortamında örnek veriler tohumlanır.

### 3. Admin Dashboard'u Başlatın
```bash
# Dashboard MVC uygulamasını çalıştırın (http://localhost:5001)
dotnet run --project AdminPanel/Dashboard.Web/Dashboard.Web.csproj
```

### 4. Angular Frontend'i Başlatın
```bash
cd Frontend/ECommerce-Frontend
npm install
npm start
# → http://localhost:4200
```

### Varsayılan Giriş Bilgileri

Uygulama ilk çalıştırıldığında aşağıdaki **SuperAdmin** hesabı otomatik olarak oluşturulur:

| Alan | Değer |
|------|-------|
| **Kullanıcı Adı** | `superadmin` |
| **E-posta** | `superadmin@ecommerce.com` |
| **Şifre** | `SuperAdmin123!` |
| **Ad Soyad** | Super Admin |
| **Rol** | `SuperAdmin` |
| **Bağlı Şirket** | System (master tenant) |

> **SuperAdmin**, tüm şirketlere ve tüm kaynaklara erişim yetkisine sahip en üst düzey yönetici hesabıdır.  
> Bu hesap `SuperAdminOnly` politikası ile korunan tüm endpoint'lere erişebilir.

#### Diğer Test Hesapları

| Hesap | E-posta | Şifre | Rol |
|-------|---------|-------|-----|
| Test Müşteri | `customer1@test.com` | `Test123!` | Customer |

> ⚠️ **Güvenlik Notu:** Üretim ortamına geçmeden önce SuperAdmin şifresini mutlaka değiştirin.

---

## 🐳 Docker ile Dağıtım

Tüm servisleri tek komutla başlatma:

```bash
docker-compose up --build -d
```

### Docker Servis Haritası

| Servis | Port | Açıklama |
|--------|------|----------|
| `api` | 5000 | Backend REST API |
| `dashboard` | 5001 | Admin Dashboard |
| `frontend` | 4000 | Angular SSR Frontend |
| `nginx` | 80/443 | Reverse proxy |

### Volume'lar
- `api-data` — SQLite veritabanı kalıcı depolama
- `api-uploads` — Yüklenen dosyalar (görseller)

---

## ⚙️ Ortam Değişkenleri

### Backend API

| Değişken | Varsayılan | Açıklama |
|----------|-----------|----------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ortam (Development/Production) |
| `ConnectionStrings__DefaultConnection` | `Data Source=ECommerce.db` | Veritabanı bağlantı dizesi |
| `JWT_KEY` | (appsettings'den) | JWT imzalama anahtarı (min 32 karakter) |
| `JWT_ISSUER` | `ECommerce` | JWT issuer |
| `JWT_AUDIENCE` | `ECommerce.Client` | JWT audience |
| `ELASTICSEARCH_URI` | `http://localhost:9200` | Elasticsearch adresi |
| `ELASTICSEARCH_INDEX` | `products` | Elasticsearch varsayılan index |
| `DOTNET_DATA_PROTECTION_KEY_DIRECTORY` | `./keys` | Data Protection anahtar dizini |

### Admin Dashboard

| Değişken | Varsayılan | Açıklama |
|----------|-----------|----------|
| `ApiSettings__BaseUrl` | `http://localhost:5010/api` | Backend API adresi |

### Veritabanı Seçimi
Bağlantı dizesine göre otomatik algılama yapılır:
- `Host=` veya `postgresql` içeriyorsa → **PostgreSQL** (Npgsql)
- Diğer durumlarda → **SQLite**

---

## 🌱 Test Verileri ve Seeding

Uygulama ilk başlatıldığında `Development` ortamında otomatik olarak aşağıdaki veriler oluşturulur:

### Otomatik Oluşturulan Veriler

| Veri Türü | Miktar | Açıklama |
|-----------|--------|----------|
| Şirketler | 3 tenant + 2 sistem | Tenant 1-3 Store, System, ECommerce Global Management |
| Roller | 5 | SuperAdmin, CompanyAdmin, Admin, User, Customer |
| Kategoriler | 8 × 3 tenant = 24+ | Her tenant için 8 kategori |
| Markalar | 6 × 3 tenant = 18+ | Her tenant için 6 marka |
| Modeller | 10 × 3 tenant = 30+ | Her tenant için 10 model |
| Ürünler | 50 × 3 tenant = 150+ | Rastgele fiyat, stok, SKU, picsum.photos görselleri |
| Ürün Görselleri | 1-3 × ürün = 225+ | Her ürün için 1-3 görsel (picsum.photos) |
| Müşteriler | 200 × 3 tenant = 600+ | İsim, e-posta, telefon, doğum tarihi |
| Adresler | 1 × müşteri = 600+ | İstanbul bazlı örnek adresler |
| Siparişler | 3+ | Örnek sipariş ve sipariş kalemleri |
| Kampanyalar | 1+ | Yılbaşı kampanyası (%25 indirim) |
| Kullanıcılar | 7+ | SuperAdmin + 5 test müşteri |

### Görsel Kaynağı
Ürün görselleri [picsum.photos](https://picsum.photos) servisinden dinamik olarak çekilir — her ürün için benzersiz seed değeri ile rastgele fotoğraflar.

### Seeding'i Sıfırlama
Veritabanını sıfırlamak için `ECommerce.db` dosyasını silip uygulamayı yeniden başlatın:
```bash
rm src/Presentation/ECommerce.RestApi/ECommerce.db*
dotnet run --project src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj
```

---

## 🔧 Geliştirme Notları

### API Dokümantasyonu
Swagger UI hem Development hem Production ortamında açıktır:
```
http://localhost:5010/swagger
```

### Health Check
```
http://localhost:5010/health
```

### Loglama
- Console çıktısı (Serilog)
- Dosya logları: `src/Presentation/ECommerce.RestApi/Logs/log-YYYYMMDD.txt`

### Migration Oluşturma
```bash
cd src/Presentation/ECommerce.RestApi
dotnet ef migrations add <MigrationName> --project ../../Infrastructure/ECommerce.Infrastructure
```

---

## 📸 Ekran Görüntüleri

Projeye ait tüm ekran görüntüleri `ScreenShots/` klasöründe bulunmaktadır.

### Admin Dashboard

| Dosya | Açıklama |
|-------|----------|
| `Dsh.png` | Dashboard genel görünüm |
| `DshLogin.png` | Dashboard giriş ekranı |
| `DshRegister.png` | Dashboard kayıt ekranı |
| `DshPanel1.png` | Dashboard ana panel — KPI kartları |
| `DshPanel2.png` | Dashboard ana panel — Grafikler ve istatistikler |
| `DshNavbar.png` | Dashboard sol menü navigasyonu |
| `DshProducts.png` | Ürün yönetim listesi |
| `DshCategoryIndex.png` | Kategori yönetim listesi |
| `DshBrandIndex.png` | Marka yönetim listesi |
| `DshFeatureIndex.png` | Özellik (attribute) yönetimi |
| `DshOrder.png` | Sipariş yönetim ekranı |
| `DshCustomer.png` | Müşteri yönetim listesi |
| `DshCampaing.png` | Kampanya yönetimi |
| `DshCompany.png` | Şirket/tenant yönetimi |
| `DshReview.png` | Değerlendirme moderasyonu |
| `DshReturnRequest.png` | İade talepleri ekranı |
| `DshRequestIndex.png` | Genel talepler listesi |
| `DshNotofication.png` | Bildirim paneli |

### Müşteri Frontend

| Dosya | Açıklama |
|-------|----------|
| `FrntHome.png` | Frontend ana sayfa — banner, kampanyalar, ürün vitrini |
| `FrntProducts.png` | Frontend ürün listeleme sayfası |
| `FrntBskt.png` | Frontend alışveriş sepeti |
| `FrntFvrt.png` | Frontend favoriler / istek listesi |
| `FrntLgn.png` | Frontend giriş ekranı |
| `FrntRgstr.png` | Frontend kayıt ekranı |

---


## 📄 Lisans

Bu proje özel kullanım amaçlıdır.

---

<p align="center">
  <b>ECommerce Platform</b> — Multi-Tenant E-Ticaret Çözümü<br>
  ASP.NET Core 9 · Angular 21 · Clean Architecture
</p>