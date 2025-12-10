using System;

namespace ECommerce.Application.Exceptions
{
    /// <summary>
    /// İstenen kaynak bulunamadığında fırlatılan özel exception.
    /// </summary>
    public class NotFoundException : Exception
    {
        public int StatusCode { get; }

        public NotFoundException(string message, int statusCode = 404)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
