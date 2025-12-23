using System.Collections.Generic;

namespace ECommerce.Application.Exceptions;

/// <summary>
/// Tüm özel exception'lar için temel sınıf
/// </summary>
public class AppException : Exception
{
    public int? StatusCode { get; set; }

    public AppException(string message, int? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Doğrulama hatalarında fırlatılan exception (400)
/// </summary>
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; set; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null) 
        : base(message, 400) 
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}

/// <summary>
/// İstenen kaynak bulunamadığında fırlatılan exception (404)
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

/// <summary>
/// Yetkilendirme hatalarında fırlatılan exception (401)
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}

/// <summary>
/// Yasaklı işlemler için fırlatılan exception (403)
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}

/// <summary>
/// Kaynak çakışması durumunda fırlatılan exception (409)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

/// <summary>
/// İş kurallarına aykırı durumlarda fırlatılan exception (400)
/// </summary>
public class BusinessException : AppException
{
    public BusinessException(string message, int statusCode = 400) 
        : base(message, statusCode) { }
}

/// <summary>
/// Eşzamanlılık hatalarında fırlatılan exception
/// </summary>
public class ConcurrencyException : AppException
{
    public ConcurrencyException(string message) 
        : base(message, 409) { }
}
