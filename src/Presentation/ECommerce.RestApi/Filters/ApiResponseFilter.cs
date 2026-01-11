using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.RestApi.Filters
{
    /// <summary>
    /// Tüm ObjectResult çıktılarının standart ApiResponseDto formatına sarılmasını sağlar.
    /// FileResult, EmptyResult, StatusCodeResult gibi durumları bozmaz.
    /// Eğer değer zaten ApiResponseDto ise dokunmaz.
    /// </summary>
    public class ApiResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                if (IsAlreadyWrapped(objectResult.Value))
                {
                    await next();
                    return;
                }

                // ProblemDetails / ValidationProblemDetails mesajlarını yakala
                var message = ExtractMessage(objectResult.Value);
                var success = IsSuccessStatusCode(objectResult.StatusCode);
                var wrapped = new ApiResponseDto<object>
                {
                    Success = success,
                    Message = message,
                    Data = success ? objectResult.Value : null
                };

                context.Result = new ObjectResult(wrapped)
                {
                    StatusCode = objectResult.StatusCode ?? (success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest),
                    DeclaredType = wrapped.GetType()
                };
            }

            await next();
        }

        private static bool IsSuccessStatusCode(int? statusCode)
        {
            var code = statusCode ?? StatusCodes.Status200OK;
            return code >= (int)HttpStatusCode.OK && code < (int)HttpStatusCode.BadRequest;
        }

        private static bool IsAlreadyWrapped(object? value)
        {
            if (value is null) return false;
            var type = value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponseDto<>))
            {
                return true;
            }
            return false;
        }

        private static string ExtractMessage(object? value)
        {
            switch (value)
            {
                case ValidationProblemDetails vpd:
                    return string.Join("; ", vpd.Errors.SelectMany(e => e.Value));
                case ProblemDetails pd:
                    return pd.Detail ?? pd.Title ?? "";
                default:
                    return string.Empty;
            }
        }
    }
}
