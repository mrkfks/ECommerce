namespace ECommerce.Application.Constants;

/// <summary>
/// Hata mesajlarını merkezi olarak yönetir.
/// </summary>
public static class ErrorMessages
{
    // Generic Errors
    public const string NotFound = "{0} bulunamadı";
    public const string AlreadyExists = "{0} zaten mevcut";
    public const string InvalidOperation = "İşlem geçersiz";
    public const string UnauthorizedAccess = "Bu işlemi yapmak için yetkiniz yok";
    public const string InternalServerError = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz";
    public const string ValidationFailed = "Doğrulama başarısız oldu";

    // Product Errors
    public const string ProductNotFound = "Ürün bulunamadı";
    public const string ProductAlreadyExists = "Bu isimde bir ürün zaten mevcut";
    public const string ProductInsufficientStock = "Yeterli stok yok. Mevcut: {0}, İstenen: {1}";
    public const string ProductInactive = "Ürün aktif değil";
    public const string ProductCannotBeDeleted = "Ürün silinemez. Aktif siparişlerde kullanılıyor";

    // Category Errors
    public const string CategoryNotFound = "Kategori bulunamadı";
    public const string CategoryAlreadyExists = "Bu isimde bir kategori zaten mevcut";
    public const string CategoryHasProducts = "Kategori silinemez. Bu kategoriye ait ürünler mevcut";
    public const string CategoryInactive = "Kategori aktif değil";

    // Brand Errors
    public const string BrandNotFound = "Marka bulunamadı";
    public const string BrandAlreadyExists = "Bu isimde bir marka zaten mevcut";
    public const string BrandHasProducts = "Marka silinemez. Bu markaya ait ürünler mevcut";
    public const string BrandInactive = "Marka aktif değil";

    // User Errors
    public const string UserNotFound = "Kullanıcı bulunamadı";
    public const string UserAlreadyExists = "Bu e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut";
    public const string UserInactive = "Kullanıcı hesabı aktif değil";
    public const string UserInvalidCredentials = "E-posta adresi veya şifre hatalı";
    public const string UserPasswordMismatch = "Şifreler eşleşmiyor";
    public const string UserCannotBeDeleted = "Kullanıcı silinemez";
    public const string UserEmailAlreadyConfirmed = "E-posta adresi zaten onaylanmış";
    public const string UserEmailNotConfirmed = "E-posta adresi onaylanmamış";

    // Order Errors
    public const string OrderNotFound = "Sipariş bulunamadı";
    public const string OrderCannotBeCancelled = "Sipariş iptal edilemez. Durum: {0}";
    public const string OrderCannotBeModified = "Sipariş değiştirilemez. Durum: {0}";
    public const string OrderAlreadyCompleted = "Sipariş zaten tamamlanmış";
    public const string OrderEmptyCart = "Sepetinizde ürün bulunmamaktadır";

    // Review Errors
    public const string ReviewNotFound = "Yorum bulunamadı";
    public const string ReviewAlreadyExists = "Bu ürün için zaten yorum yaptınız";
    public const string ReviewNotOwner = "Bu yorumu sadece sahibi değiştirebilir";
    public const string ReviewProductNotPurchased = "Yorum yapabilmek için ürünü satın almış olmalısınız";

    // Customer Errors
    public const string CustomerNotFound = "Müşteri bulunamadı";
    public const string CustomerAlreadyExists = "Bu e-posta adresi ile kayıtlı bir müşteri zaten mevcut";
    public const string CustomerInactive = "Müşteri hesabı aktif değil";

    // Company Errors
    public const string CompanyNotFound = "Şirket bulunamadı";
    public const string CompanyAlreadyExists = "Bu isimde bir şirket zaten mevcut";
    public const string CompanyInactive = "Şirket hesabı aktif değil";
    public const string CompanyNotApproved = "Şirket hesabı henüz onaylanmamış";

    // Role Errors
    public const string RoleNotFound = "Rol bulunamadı";
    public const string RoleAlreadyExists = "Bu isimde bir rol zaten mevcut";
    public const string RoleCannotBeDeleted = "Rol silinemez. Bu role atanmış kullanıcılar mevcut";

    // Address Errors
    public const string AddressNotFound = "Adres bulunamadı";
    public const string AddressCannotBeDeleted = "Adres silinemez. Aktif siparişlerde kullanılıyor";

    // Authentication Errors
    public const string InvalidToken = "Geçersiz token";
    public const string ExpiredToken = "Token süresi dolmuş";
    public const string InvalidRefreshToken = "Geçersiz refresh token";
    public const string TokenGenerationFailed = "Token oluşturulamadı";

    // Authorization Errors
    public const string InsufficientPermissions = "Bu işlemi yapmak için yetkiniz yok";
    public const string TenantMismatch = "Tenant erişim hatası";
    public const string CompanyAccessDenied = "Bu şirkete erişim yetkiniz yok";

    // File Upload Errors
    public const string FileNotFound = "Dosya bulunamadı";
    public const string FileTypeNotSupported = "Desteklenmeyen dosya tipi";
    public const string FileSizeExceeded = "Dosya boyutu çok büyük. Maximum: {0} MB";
    public const string FileUploadFailed = "Dosya yüklenemedi";

    // Database Errors
    public const string DatabaseConnectionFailed = "Veritabanı bağlantısı kurulamadı";
    public const string DatabaseSaveFailed = "Veriler kaydedilemedi";
    public const string ConcurrencyConflict = "Kayıt başka bir kullanıcı tarafından değiştirilmiş. Lütfen sayfayı yenileyip tekrar deneyiniz";
}
