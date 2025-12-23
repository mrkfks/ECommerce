namespace ECommerce.Application.Constants;

/// <summary>
/// API rotalarını merkezi olarak yönetir.
/// </summary>
public static class ApiRoutes
{
    private const string ApiBase = "api";
    private const string ApiVersion = "v1";
    
    public static class Auth
    {
        private const string Base = $"{ApiBase}/auth";
        
        public const string Login = $"{Base}/login";
        public const string Register = $"{Base}/register";
        public const string Logout = $"{Base}/logout";
        public const string RefreshToken = $"{Base}/refresh-token";
        public const string ForgotPassword = $"{Base}/forgot-password";
        public const string ResetPassword = $"{Base}/reset-password";
        public const string ConfirmEmail = $"{Base}/confirm-email";
        public const string Me = $"{Base}/me";
    }

    public static class Products
    {
        private const string Base = $"{ApiBase}/products";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
        public const string UpdateStock = $"{Base}/{{id}}/stock";
        public const string GetByCategory = $"{Base}/category/{{categoryId}}";
        public const string GetByBrand = $"{Base}/brand/{{brandId}}";
        public const string Search = $"{Base}/search";
    }

    public static class Categories
    {
        private const string Base = $"{ApiBase}/categories";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
    }

    public static class Brands
    {
        private const string Base = $"{ApiBase}/brands";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
    }

    public static class Orders
    {
        private const string Base = $"{ApiBase}/orders";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Cancel = $"{Base}/{{id}}/cancel";
        public const string Complete = $"{Base}/{{id}}/complete";
        public const string Ship = $"{Base}/{{id}}/ship";
        public const string Deliver = $"{Base}/{{id}}/deliver";
        public const string GetByCustomer = $"{Base}/customer/{{customerId}}";
        public const string GetByStatus = $"{Base}/status/{{status}}";
    }

    public static class Customers
    {
        private const string Base = $"{ApiBase}/customers";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
        public const string GetOrders = $"{Base}/{{id}}/orders";
    }

    public static class Reviews
    {
        private const string Base = $"{ApiBase}/reviews";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string GetByProduct = $"{Base}/product/{{productId}}";
        public const string GetByCustomer = $"{Base}/customer/{{customerId}}";
    }

    public static class Users
    {
        private const string Base = $"{ApiBase}/users";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
        public const string ChangePassword = $"{Base}/{{id}}/change-password";
        public const string AssignRole = $"{Base}/{{id}}/roles";
        public const string UnassignRole = $"{Base}/{{id}}/roles/{{roleId}}";
    }

    public static class Roles
    {
        private const string Base = $"{ApiBase}/roles";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
    }

    public static class Companies
    {
        private const string Base = $"{ApiBase}/companies";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Approve = $"{Base}/{{id}}/approve";
        public const string Reject = $"{Base}/{{id}}/reject";
        public const string Activate = $"{Base}/{{id}}/activate";
        public const string Deactivate = $"{Base}/{{id}}/deactivate";
    }

    public static class Addresses
    {
        private const string Base = $"{ApiBase}/addresses";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string GetByCustomer = $"{Base}/customer/{{customerId}}";
    }

    public static class Dashboard
    {
        private const string Base = $"{ApiBase}/dashboard";
        
        public const string GetStats = $"{Base}/stats";
        public const string GetRecentOrders = $"{Base}/recent-orders";
        public const string GetTopProducts = $"{Base}/top-products";
        public const string GetRevenue = $"{Base}/revenue";
    }
}
