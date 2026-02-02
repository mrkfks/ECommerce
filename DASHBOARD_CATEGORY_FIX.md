# 🔧 Dashboard Kategori Ekleme Sorunu - Çözüm Raporu

**Tarih:** 30 Ocak 2026, 23:45  
**Sorun:** Dashboard'dan kategori eklenemiyor  
**Durum:** ✅ ÇÖZÜLDÜ

---

## 🐛 Sorun Tespiti

### Hata Mesajı
```
[ApiService] CreateAsync<CategoryDto> exception: An invalid request URI was provided. 
Either the request URI must be an absolute URI or BaseAddress must be set.
```

### Kök Neden
Generic `IApiService<T>` servisleri için HttpClient'a `BaseAddress` ayarlanmamıştı.

---

## ✅ Uygulanan Çözüm

### 1. HttpClient BaseAddress Yapılandırması

Her generic API service için HttpClient'a BaseAddress eklendi:

```csharp
// Her DTO tipi için ayrı HttpClient kaydı
builder.Services.AddHttpClient<IApiService<CategoryViewModel>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); // ✅ BaseAddress eklendi
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddTransient<IApiService<CategoryViewModel>>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient(nameof(IApiService<CategoryViewModel>));
    return new ApiService<CategoryViewModel>(httpClient);
});
```

### 2. Tüm DTO Tipleri İçin Kayıt

Aşağıdaki DTO tipleri için API service kayıtları eklendi:

- ✅ CategoryViewModel
- ✅ AppBrandDto (ECommerce.Application.DTOs.BrandDto)
- ✅ ModelDto
- ✅ AppGlobalAttributeDto
- ✅ ProductViewModel
- ✅ DashBrandDto (Dashboard.Web.Models.BrandDto)
- ✅ DashCompanyDto
- ✅ RequestDto
- ✅ DashOrderDto
- ✅ DashCustomerDto
- ✅ AppCompanyDto
- ✅ CategoryDto
- ✅ DashProductDto
- ✅ DashCampaignDto
- ✅ BannerViewModel

### 3. Namespace Çakışmaları Çözüldü

Aynı isimde farklı namespace'lerde DTO'lar vardı. Alias'lar eklendi:

```csharp
// Aliases for ambiguous types
using AppBrandDto = ECommerce.Application.DTOs.BrandDto;
using AppCompanyDto = ECommerce.Application.DTOs.CompanyDto;
using AppCustomerDto = ECommerce.Application.DTOs.CustomerDto;
using AppOrderDto = ECommerce.Application.DTOs.OrderDto;
using AppProductDto = ECommerce.Application.DTOs.ProductDto;
using AppCampaignDto = ECommerce.Application.DTOs.CampaignDto;
using AppGlobalAttributeDto = ECommerce.Application.DTOs.GlobalAttributeDto;
using DashBrandDto = Dashboard.Web.Models.BrandDto;
using DashCompanyDto = Dashboard.Web.Models.CompanyDto;
using DashCustomerDto = Dashboard.Web.Models.CustomerDto;
using DashOrderDto = Dashboard.Web.Models.OrderDto;
using DashProductDto = Dashboard.Web.Models.ProductDto;
using DashCampaignDto = Dashboard.Web.Models.CampaignDto;
```

---

## 📝 Değiştirilen Dosyalar

1. **`AdminPanel/Dashboard.Web/Program.cs`**
   - HttpClient BaseAddress yapılandırması eklendi
   - Tüm generic API service kayıtları eklendi
   - Namespace alias'ları eklendi

---

## 🧪 Test

### Önce (Hatalı)
```
1. Dashboard'a giriş yap
2. Kategori Yönetimi'ne git
3. "Yeni Ana Kategori" butonuna tıkla
4. Form doldur ve "Kaydet"e tıkla
❌ Sonuç: "Kategori eklenirken hata oluştu"
```

### Sonra (Düzeltilmiş)
```
1. Dashboard'a giriş yap
2. Kategori Yönetimi'ne git
3. "Yeni Ana Kategori" butonuna tıkla
4. Form doldur ve "Kaydet"e tıkla
✅ Sonuç: Kategori başarıyla eklendi
```

---

## 🎯 Sonuç

**Sorun Çözüldü:** ✅

Dashboard'dan kategori ekleme artık çalışıyor. Sorun, generic API service'leri için HttpClient'ın BaseAddress'inin ayarlanmamış olmasıydı.

---

**Düzelten:** Antigravity AI  
**Tarih:** 30 Ocak 2026, 23:45
