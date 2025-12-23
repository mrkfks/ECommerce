namespace ECommerce.Application.Constants;

/// <summary>
/// Uygulama genelinde kullanılan sabit değerleri merkezi olarak yönetir.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Veritabanı sabitler
    /// </summary>
    public static class Database
    {
        public const string DefaultConnectionStringName = "DefaultConnection";
        public const int CommandTimeout = 30;
        public const int MaxRetryCount = 3;
    }

    /// <summary>
    /// JWT Token sabitler
    /// </summary>
    public static class Jwt
    {
        public const string Secret = "JwtSettings:Secret";
        public const string Issuer = "JwtSettings:Issuer";
        public const string Audience = "JwtSettings:Audience";
        public const string ExpiryInMinutes = "JwtSettings:ExpiryInMinutes";
        public const int DefaultExpiryMinutes = 60;
        public const string ClaimTypeUserId = "user_id";
        public const string ClaimTypeEmail = "email";
        public const string ClaimTypeUsername = "username";
        public const string ClaimTypeCompanyId = "tenant_id";
        public const string ClaimTypeRole = "role";
    }

    /// <summary>
    /// Sayfalama sabitler
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    /// <summary>
    /// Dosya yükleme sabitler
    /// </summary>
    public static class FileUpload
    {
        public const int MaxFileSizeMB = 5;
        public const long MaxFileSizeBytes = MaxFileSizeMB * 1024 * 1024;
        
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        
        public const string ImageUploadPath = "uploads/images/";
        public const string DocumentUploadPath = "uploads/documents/";
    }

    /// <summary>
    /// Önbellekleme sabitler
    /// </summary>
    public static class Cache
    {
        public const int DefaultCacheExpirationMinutes = 30;
        public const string ProductCacheKey = "products";
        public const string CategoryCacheKey = "categories";
        public const string BrandCacheKey = "brands";
    }

    /// <summary>
    /// Email sabitler
    /// </summary>
    public static class Email
    {
        public const string SenderEmail = "EmailSettings:SenderEmail";
        public const string SenderName = "EmailSettings:SenderName";
        public const string SmtpServer = "EmailSettings:SmtpServer";
        public const string SmtpPort = "EmailSettings:SmtpPort";
        public const string SmtpUsername = "EmailSettings:SmtpUsername";
        public const string SmtpPassword = "EmailSettings:SmtpPassword";
        public const string EnableSSL = "EmailSettings:EnableSSL";
    }

    /// <summary>
    /// Sipariş durumları
    /// </summary>
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
    }

    /// <summary>
    /// Roller
    /// </summary>
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string CompanyAdmin = "CompanyAdmin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";
        public const string Customer = "Customer";
    }

    /// <summary>
    /// Policy isimleri
    /// </summary>
    public static class Policies
    {
        public const string RequireSuperAdmin = "RequireSuperAdmin";
        public const string RequireCompanyAdmin = "RequireCompanyAdmin";
        public const string RequireManager = "RequireManager";
        public const string RequireStaff = "RequireStaff";
        public const string RequireCustomer = "RequireCustomer";
    }

    /// <summary>
    /// Regex pattern'ları
    /// </summary>
    public static class RegexPatterns
    {
        public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string Phone = @"^(\+90|0)?[0-9]{10}$";
        public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        public const string Username = @"^[a-zA-Z0-9_]{3,20}$";
    }

    /// <summary>
    /// Tarih formatları
    /// </summary>
    public static class DateFormats
    {
        public const string ShortDate = "dd.MM.yyyy";
        public const string LongDate = "dd MMMM yyyy";
        public const string ShortDateTime = "dd.MM.yyyy HH:mm";
        public const string LongDateTime = "dd MMMM yyyy HH:mm:ss";
        public const string Iso8601 = "yyyy-MM-ddTHH:mm:ssZ";
    }

    /// <summary>
    /// Para birimi sabitler
    /// </summary>
    public static class Currency
    {
        public const string DefaultCurrency = "TRY";
        public const string CurrencySymbol = "₺";
        public const string USDCurrency = "USD";
        public const string EURCurrency = "EUR";
    }

    /// <summary>
    /// API sabitleri
    /// </summary>
    public static class Api
    {
        public const string Version = "v1";
        public const string BaseRoute = "api";
        public const int DefaultTimeout = 30000; // milliseconds
        public const string ContentTypeJson = "application/json";
        public const string ContentTypeFormData = "multipart/form-data";
    }

    /// <summary>
    /// Logging sabitler
    /// </summary>
    public static class Logging
    {
        public const string ApplicationName = "ECommerce.API";
        public const string ErrorLogPath = "Logs/errors.txt";
        public const string InfoLogPath = "Logs/info.txt";
        public const string DebugLogPath = "Logs/debug.txt";
    }
}
