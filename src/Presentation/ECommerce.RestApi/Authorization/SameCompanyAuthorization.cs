using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ECommerce.RestApi.Authorization
{
    public class SameCompanyRequirement : IAuthorizationRequirement { }

    public class SameCompanyAuthorizationHandler : AuthorizationHandler<SameCompanyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SameCompanyAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameCompanyRequirement requirement)
        {
            // SuperAdmin ise direkt yetkili
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            var userCompanyId = context.User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId))
            {
                return Task.CompletedTask;
            }

            // Route veya query'den companyId al
            string? routeCompanyId = null;
            if (httpContext.Request.RouteValues.TryGetValue("companyId", out var val) && val != null)
            {
                routeCompanyId = val.ToString();
            }
            else if (httpContext.Request.Query.TryGetValue("companyId", out var qval))
            {
                routeCompanyId = qval.ToString();
            }

            // Belirli bir şirket hedeflenmiyorsa politikayı geç (genel liste vb.)
            if (string.IsNullOrEmpty(routeCompanyId))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (routeCompanyId == userCompanyId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}