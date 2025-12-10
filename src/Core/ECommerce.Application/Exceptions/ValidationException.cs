using System;
using System.Collections.Generic;

namespace ECommerce.Application.Exceptions
{
    /// <summary>
    /// Doğrulama hatalarında fırlatılan özel exception.
    /// </summary>
    public class ValidationException : Exception
    {
        public int StatusCode { get; }
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message, IDictionary<string, string[]>? errors = null, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
