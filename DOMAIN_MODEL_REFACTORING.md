# Rich Domain Model Refactoring

## Özet
Tüm Domain Entity'leri **Rich Domain Model** yaklaşımına göre refactor edilmiştir. Entity'ler artık:
- Private setters ile encapsulation sağlıyor
- Iş kurallarını kapsülleyen davranış metotları içeriyor
- Factory pattern (Create) ile kontrollü instance oluşturma sağlıyor
- Input validation yapıyor

## Refactor Edilen Entity'ler

### 1. **Product**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu eklendi
- ✅ `Update()` methodu (fiyat, ad, açıklama)
- ✅ `UpdateStock()` methodu
- ✅ `DecreaseStock()` methodu (stok kontrollü azaltma)
- ✅ `Activate()` / `Deactivate()` methodları
- ✅ Tüm metotlarda input validation

**İş Kuralları:**
- Ürün adı boş olamaz
- Fiyat > 0 olmalı
- Stok miktarı negatif olamaz
- Stok azaltırken yeterli miktar kontrolü

### 2. **Order**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `AddItem()` methodu (sadece Pending durumda)
- ✅ `RemoveItem()` methodu (sadece Pending durumda)
- ✅ `Confirm()`, `Ship()`, `Deliver()`, `Cancel()` state machine methodları
- ✅ `RecalculateTotal()` private methodu

**İş Kuralları (State Machine):**
- Pending → Confirmed → Shipped → Delivered
- Herhangi bir durumdan Cancelled'a geçiş (Delivered ve Cancelled hariç)
- Order item eklenebilmesi/çıkartılması sadece Pending durumda

### 3. **Customer**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu
- ✅ `LinkUser()` methodu

**İş Kuralları:**
- Ad, soyad, email, telefon boş olamaz
- Email formatı kontrolü (@)

### 4. **User**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `UpdateProfile()` methodu
- ✅ `UpdatePassword()` methodu
- ✅ `Activate()` / `Deactivate()` methodları

**İş Kuralları:**
- Kullanıcı adı boş olamaz
- Email formatı kontrolü
- Şifre boş olamaz

### 5. **Review**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu (rating ve comment)

**İş Kuralları:**
- Rating 1-5 arasında olmalı
- Yorum metni boş olamaz
- Yorum yapan adı boş olamaz

### 6. **OrderItem**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `UpdateQuantity()` methodu

**İş Kuralları:**
- Miktar > 0 olmalı
- Birim fiyat > 0 olmalı

### 7. **Address**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu

**İş Kuralları:**
- Sokak, şehir, il, posta kodu, ülke boş olamaz

### 8. **Category**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu
- ✅ `Activate()` / `Deactivate()` methodları

**İş Kuralları:**
- Kategori adı ve açıklaması boş olamaz

### 9. **Brand**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu
- ✅ `Activate()` / `Deactivate()` methodları

**İş Kuralları:**
- Marka adı ve açıklaması boş olamaz

### 10. **Company**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu
- ✅ `Approve()` / `Reject()` methodları
- ✅ `Activate()` / `Deactivate()` methodları

**İş Kuralları:**
- Şirket adı, adres, telefon, email, vergi numarası boş olamaz
- Email formatı kontrolü

### 11. **Request**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Approve()`, `Reject()`, `MarkInProgress()`, `Complete()` state machine methodları

**İş Kuralları (State Machine):**
- Pending → Approved → InProgress → Completed
- Reject ile Pending → Rejected
- Reddetme geri bildirimi boş olamaz
- InProgress'e sadece Approved'dan geçiş yapılabilir

### 12. **Role**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu

**İş Kuralları:**
- Rol adı boş olamaz

### 13. **UserRole**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu

**İş Kuralları:**
- User ID, Role ID geçerli olmalı
- Rol adı boş olamaz

### 14. **Banner**
- ✅ Setters private yapıldı
- ✅ `Create()` factory methodu
- ✅ `Update()` methodu

**İş Kuralları:**
- Title, ImageUrl, RedirectUrl boş olamaz

## Avantajlar

1. **Encapsulation**: Entity'ler kendi iş kurallarını kontrol eder
2. **Testability**: Business logic Entity içinde olduğundan, unit test yazması kolay
3. **Maintainability**: İş kuralları merkezi bir yerde
4. **Data Integrity**: Property'ler sadece validation yapan metotlar aracılığıyla değişir
5. **Self-documenting**: Entity'ler hangi state transitions'ine izin verdiklerini açıkça gösteriyor

## Application Layer İntegrasyonu

Application/Command handlers'ı Entity factory ve davranış metotlarını kullanmalı:

```csharp
// ❌ ESKİ YÖNTEM (Artık kullanılmayacak)
var product = new Product
{
    Name = command.Name,
    Price = command.Price,
    // ... vs
};

// ✅ YENİ YÖNTEM
var product = Product.Create(
    command.Name,
    command.Description,
    command.Price,
    command.CategoryId,
    command.BrandId,
    command.CompanyId,
    command.StockQuantity
);

// State değişimi için davranış metotları
product.DecreaseStock(10);
product.Activate();
```

## EF Core Configuration

Entity configuration'lar (`HasMaxLength`, `IsRequired` vb.) Entity Fluent API ile yapılacak:

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Name)
    .HasMaxLength(256)
    .IsRequired();

// Setters private olsa bile EF Core migration ve loading için izin verilir
modelBuilder.Entity<Product>()
    .Property(p => p.Version)
    .IsConcurrencyToken();
```

## Yapılacak Sonraki Adımlar

1. ✅ Entity refactoring tamamlandı
2. ⏳ Application handlers güncellenmesi (Create/Update commandları)
3. ⏳ Unit testler yazılması
4. ⏳ EF Core mapping configuration'lar review edilmesi
5. ⏳ API controllers'ın Entity factory'lerini çağırması sağlanması
