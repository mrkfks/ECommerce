using System;

namespace ECommerce.Application.Exceptions
{
    /// <summary>
    /// İş kurallarına aykırı durumlarda fırlatılan özel exception.
    /// </summary>
    public class BusinessException : Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message, int statusCode = 400) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
