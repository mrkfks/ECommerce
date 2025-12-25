# Gelişmiş Ürün Yönetim Sistemi

Bu proje için kapsamlı bir ürün yönetim sistemi oluşturulmuştur. Aşağıdaki özellikler eklenmiştir:

## 🎯 Yeni Entity'ler

### 1. **Model Entity**
- Markaya bağlı model yapısı (ör: Samsung Galaxy S23, iPhone 15 Pro)
- Her model bir markaya (Brand) ait olmalıdır
- Dosya: `ECommerce.Domain/Entities/Model.cs`

### 2. **ProductSpecification Entity**
- Dinamik ürün özellikleri (Key-Value yapısı)
- Örnek: Ekran Boyutu: 6.7", RAM: 8GB, İşlemci: Snapdragon 8 Gen 2
- Tenant izolasyonu (CompanyId)
- Dosya: `ECommerce.Domain/Entities/ProductSpecification.cs`

### 3. **ProductAttribute Entity**
- Varyant öznitelikleri (ör: Renk, Beden, Bellek Kapasitesi)
- Dosya: `ECommerce.Domain/Entities/Attribute.cs`

### 4. **AttributeValue Entity**
- Öznitelik değerleri (ör: Kırmızı, Mavi, S, M, L, XL)
- Renk için hex code desteği
- Dosya: `ECommerce.Domain/Entities/AttributeValue.cs`

### 5. **ProductVariant Entity**
- Ürün varyantları ile fiyat ve stok yönetimi
- SKU (Stock Keeping Unit) sistemi
- PriceAdjustment ile ana ürün fiyatına ek/indirim
- Dosya: `ECommerce.Domain/Entities/ProductVariant.cs`

### 6. **ProductVariantAttribute Entity**
- Varyant ile öznitelik değeri arasındaki ilişki (Many-to-Many)
- Örnek: Varyant #1 -> Renk: Kırmızı, Beden: L
- Dosya: `ECommerce.Domain/Entities/ProductVariantAttribute.cs`

## 📊 Entity Güncellemeleri

### Category Entity
- ✅ **Hiyerarşik Yapı**: `ParentCategoryId` ile alt kategori desteği
- ✅ Self-referencing ilişki
- ✅ `DisplayOrder` alanı eklendi

### Product Entity
- ✅ **ModelId** eklendi (opsiyonel)
- ✅ **Specifications** koleksiyonu
- ✅ **Variants** koleksiyonu  
- ✅ **Sku** alanı
- ✅ Rich Domain Model metotları:
  - `AddSpecification()`
  - `RemoveSpecification()`
  - `SetModel()`
  - `UpdateSku()`

### Brand Entity
- ✅ **Models** koleksiyonu eklendi

## 🔧 EF Core Configurations

Tüm entity'ler için ayrı configuration dosyaları oluşturuldu:

- `ModelConfiguration.cs`
- `ProductSpecificationConfiguration.cs`
- `AttributeConfiguration.cs`
- `AttributeValueConfiguration.cs`
- `ProductVariantConfiguration.cs`
- `ProductVariantAttributeConfiguration.cs`

### Güncellenen Configurations
- `CategoryConfiguration.cs` - Hiyerarşik yapı eklendi
- `ProductConfiguration.cs` - Model ilişkisi ve Sku alanı eklendi

## 🛡️ Multi-Tenancy & Audit

### Global Query Filters
- `Product`, `ProductSpecification`, `ProductVariant` için CompanyId filtresi
- Tenant izolasyonu otomatik çalışıyor

### Audit Fields
- `CreatedAt`, `UpdatedAt` otomatik yönetiliyor
- `SaveChangesAsync` override ile audit alanları set ediliyor

## 🎨 Rich Domain Model Özellikleri

### Encapsulation
- Tüm property'ler `private set` ile korunuyor
- Public setter'lar kaldırıldı

### Behavior-Driven
- İş mantığı metotlarla kapsüllenmiş
- Validation işlemleri domain katmanında

### Examples:
```csharp
// Product
product.AddSpecification("Ekran Boyutu", "6.7 inch");
product.SetModel(modelId);
product.UpdateStock(100);

// ProductVariant
variant.UpdateStock(50);
variant.DecreaseStock(5);
var finalPrice = variant.GetFinalPrice(basePrice);

// Category
category.SetParentCategory(parentId);
```

## 📋 Migration Oluşturma

```bash
# Infrastructure projesinde
cd src/Infrastructure/ECommerce.Infrastructure

# Migration oluştur
dotnet ef migrations add AdvancedProductManagement --startup-project ../../Presentation/ECommerce.RestApi

# Database'i güncelle
dotnet ef database update --startup-project ../../Presentation/ECommerce.RestApi
```

## 🎯 Kullanım Senaryoları

### Senaryo 1: Hiyerarşik Kategoriler
```
Elektronik (Ana Kategori)
├── Telefon
│   ├── Android Telefonlar
│   └── iPhone
├── Bilgisayar
│   ├── Dizüstü
│   └── Masaüstü
```

### Senaryo 2: Marka-Model-Ürün Hiyerarşisi
```
Apple (Brand)
├── iPhone 15 Pro (Model)
│   ├── iPhone 15 Pro - 128GB - Siyah (Variant)
│   ├── iPhone 15 Pro - 256GB - Mavi (Variant)
│   └── iPhone 15 Pro - 512GB - Beyaz (Variant)
```

### Senaryo 3: Dinamik Özellikler
```
Product: Samsung Galaxy S23
Specifications:
  - Ekran Boyutu: 6.1"
  - İşlemci: Snapdragon 8 Gen 2
  - RAM: 8GB
  - Kamera: 50MP + 12MP + 10MP
```

### Senaryo 4: Varyant Yönetimi
```
T-Shirt - Beyaz
Attributes:
  - Renk: Beyaz (#FFFFFF)
  - Beden: M
  
SKU: TSH-WHT-M
Price Adjustment: +0.00
Stock: 50
```

## 🚀 Sonraki Adımlar

1. **DTOs Oluşturma**
   - ModelDto, ProductSpecificationDto
   - AttributeDto, AttributeValueDto
   - ProductVariantDto

2. **Services/Repositories**
   - IModelService, ModelService
   - IAttributeService, AttributeService
   - IProductVariantService, ProductVariantService

3. **API Controllers**
   - ModelController
   - AttributeController
   - ProductVariantController

4. **Admin Panel**
   - Model CRUD sayfaları
   - Attribute & AttributeValue yönetimi
   - Variant yönetimi UI
   - Kategori hiyerarşi ağacı görünümü

5. **Seed Data**
   - Örnek kategoriler (hiyerarşik)
   - Örnek markalar ve modeller
   - Örnek attributes (Renk, Beden, vb.)

## 📚 Mimari Prensipler

✅ **Clean Architecture** - Domain, Application, Infrastructure katmanları ayrık  
✅ **DDD (Domain-Driven Design)** - Rich domain model  
✅ **Repository Pattern** - Veri erişim soyutlaması  
✅ **CQRS** - Command/Query ayrımı hazır  
✅ **Multi-Tenancy** - CompanyId ile tenant izolasyonu  
✅ **Audit Trail** - CreatedAt, UpdatedAt otomatik  

## 🔒 Güvenlik

- Global Query Filter ile tenant izolasyonu
- Private setters ile encapsulation
- Domain validations
- Foreign key constraints
- Unique constraints

Sistem hazır! Migration çalıştırıldığında tüm yeni tablolar oluşturulacak.
