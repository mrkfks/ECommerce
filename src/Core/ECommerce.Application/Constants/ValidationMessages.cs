namespace ECommerce.Application.Constants;

/// <summary>
/// Validation hata mesajlarını merkezi olarak yönetir.
/// </summary>
public static class ValidationMessages
{
    // Generic Validation Messages
    public const string Required = "{PropertyName} alanı zorunludur";
    public const string MaxLength = "{PropertyName} maximum {MaxLength} karakter olabilir";
    public const string MinLength = "{PropertyName} minimum {MinLength} karakter olmalıdır";
    public const string EmailInvalid = "Geçerli bir e-posta adresi giriniz";
    public const string GreaterThanZero = "{PropertyName} sıfırdan büyük olmalıdır";
    public const string GreaterThanOrEqualToZero = "{PropertyName} sıfırdan büyük veya eşit olmalıdır";

    // Product Validation
    public const string ProductNameRequired = "Ürün adı boş olamaz";
    public const string ProductNameMaxLength = "Ürün adı maximum 255 karakter olabilir";
    public const string ProductDescriptionMaxLength = "Ürün açıklaması maximum 1000 karakter olabilir";
    public const string ProductPriceRequired = "Ürün fiyatı boş olamaz";
    public const string ProductPricePositive = "Ürün fiyatı sıfırdan büyük olmalıdır";
    public const string ProductStockQuantityRequired = "Stok miktarı boş olamaz";
    public const string ProductStockQuantityNonNegative = "Stok miktarı negatif olamaz";
    public const string ProductCategoryRequired = "Kategori seçilmelidir";
    public const string ProductBrandRequired = "Marka seçilmelidir";

    // Category Validation
    public const string CategoryNameRequired = "Kategori adı boş olamaz";
    public const string CategoryNameMaxLength = "Kategori adı maximum 200 karakter olabilir";
    public const string CategoryDescriptionRequired = "Kategori açıklaması boş olamaz";
    public const string CategoryDescriptionMaxLength = "Kategori açıklaması maximum 500 karakter olabilir";

    // Brand Validation
    public const string BrandNameRequired = "Marka adı boş olamaz";
    public const string BrandNameMaxLength = "Marka adı maximum 200 karakter olabilir";

    // User Validation
    public const string UserEmailRequired = "E-posta adresi boş olamaz";
    public const string UserEmailInvalid = "Geçerli bir e-posta adresi giriniz";
    public const string UserPasswordRequired = "Şifre boş olamaz";
    public const string UserPasswordMinLength = "Şifre minimum 8 karakter olmalıdır";
    public const string UserPasswordComplexity = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir";
    public const string UserUsernameRequired = "Kullanıcı adı boş olamaz";
    public const string UserUsernameMaxLength = "Kullanıcı adı maximum 50 karakter olabilir";
    public const string UserFullNameRequired = "Ad Soyad boş olamaz";
    public const string UserFullNameMaxLength = "Ad Soyad maximum 200 karakter olabilir";
    public const string UserPhoneInvalid = "Geçerli bir telefon numarası giriniz";

    // Order Validation
    public const string OrderCustomerRequired = "Müşteri seçilmelidir";
    public const string OrderItemsRequired = "Sipariş en az bir ürün içermelidir";
    public const string OrderTotalAmountPositive = "Sipariş tutarı sıfırdan büyük olmalıdır";
    public const string OrderAddressRequired = "Teslimat adresi boş olamaz";

    // Review Validation
    public const string ReviewRatingRequired = "Puan verilmelidir";
    public const string ReviewRatingRange = "Puan 1 ile 5 arasında olmalıdır";
    public const string ReviewCommentMaxLength = "Yorum maximum 1000 karakter olabilir";

    // Customer Validation
    public const string CustomerFirstNameRequired = "Ad boş olamaz";
    public const string CustomerFirstNameMaxLength = "Ad maximum 100 karakter olabilir";
    public const string CustomerLastNameRequired = "Soyad boş olamaz";
    public const string CustomerLastNameMaxLength = "Soyad maximum 100 karakter olabilir";
    public const string CustomerEmailRequired = "E-posta adresi boş olamaz";
    public const string CustomerPhoneRequired = "Telefon numarası boş olamaz";

    // Company Validation
    public const string CompanyNameRequired = "Şirket adı boş olamaz";
    public const string CompanyNameMaxLength = "Şirket adı maximum 200 karakter olabilir";
    public const string CompanyEmailRequired = "Şirket e-posta adresi boş olamaz";
    public const string CompanyAddressRequired = "Şirket adresi boş olamaz";
}
