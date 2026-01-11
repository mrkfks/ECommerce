# E-Commerce Project

## Overview

This represents a modern, enterprise-grade E-Commerce solution built with **Clean Architecture** principles. It features a high-performance .NET 9 REST API, a robust MVC Admin Panel, and a responsive Angular 20 storefront.

## Technology Stack

### Backend (.NET Core 9)
*   **Architecture**: Clean Architecture (Core, Application, Infrastructure, Presentation)
*   **API**: REST API with JWT Authentication & Role-Based Authorization
*   **Data Access**: Entity Framework Core 9 (Sqlite/PostgreSQL ready)
*   **Validation**: FluentValidation
*   **Mapping**: AutoMapper
*   **Logging**: Serilog
*   **CQRS**: MediatR (Command Query Responsibility Segregation)
*   **Documentation**: Swagger/OpenAPI

### Frontend (Angular 20)
*   **Framework**: Angular 20 (Standalone Components)
*   **State Management**: RxJS, Signals
*   **Styling**: Modern CSS / SCSS
*   **Features**: Lazy Loading, Auth Guards, Interceptors (Auth, Error, Loading)

### Admin Panel (MVC)
*   **Framework**: ASP.NET Core MVC 9
*   **Integration**: Consumes Backend API via typed ApiServices
*   **Components**: Rich Text Editor, Dynamic Dashboard

## Project Structure

```
src/
├── Core/
│   ├── ECommerce.Application   # Interfaces, DTOs, CQRS Features, Mappers
│   └── ECommerce.Domain        # Entities, Enums, Domain Interfaces
├── Infrastructure/
│   └── ECommerce.Infrastructure # EF Core, Repositories, Services (Email, File, etc.)
├── Presentation/
│   └── ECommerce.RestApi       # API Controllers, Middleware, Filters
AdminPanel/
└── Dashboard.Web               # MVC Admin Interface
Frontend/
└── ECommerce-Frontend          # Angular Storefront
```

## Setup Instructions

### Prerequisites
*   .NET 9.0 SDK
*   Node.js (v18+) & NPM
*   Angular CLI (`npm install -g @angular/cli`)

### Backend Setup
1.  Navigate to the solution folder.
2.  Update connection strings in `src/Presentation/ECommerce.RestApi/appsettings.json`.
3.  Apply migrations:
    ```bash
    dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure --startup-project src/Presentation/ECommerce.RestApi
    ```
4.  Run the API:
    ```bash
    dotnet run --project src/Presentation/ECommerce.RestApi
    ```
    API will run at `http://localhost:5027` (or configured port).

### Frontend Setup
1.  Navigate to `Frontend/ECommerce-Frontend`.
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Start the development server:
    ```bash
    npm start
    ```
    Access via `http://localhost:4200`.

### Admin Panel Setup
1.  Navigate to `AdminPanel/Dashboard.Web`.
2.  Run the application:
    ```bash
    dotnet run
    ```

## Key Modules & Features

*   **Authentication**: Secure JWT token-based auth for API, Session-based for Admin.
*   **Role Management**: SuperAdmin, CompanyAdmin, User, Customer roles.
*   **Product Management**: Full CRUD with Pagination, Filtering, and Image Upload.
*   **Cart System**: Persistent database-backed cart for Users and Guests (Session-based).
*   **Order System**: Order creation, status tracking, and history.
*   **Pagination**: Standardized pagination across listing endpoints (`PaginatedResult<T>`).

## API Endpoints (Partial List)

### Auth
*   `POST /api/auth/login`: Authenticate user
*   `POST /api/auth/register`: Register new user/company
*   `POST /api/auth/refresh`: Refresh JWT token

### Products
*   `GET /api/product`: Get paginated products (`?pageNumber=1&pageSize=10`)
*   `GET /api/product/{id}`: Get product details
*   `POST /api/product`: Create product
*   `GET /api/product/search`: Search products

### Cart
*   `GET /api/cart`: Get current cart
*   `POST /api/cart/items`: Add item to cart
*   `PUT /api/cart/items/{id}`: Update item quantity
*   `DELETE /api/cart/items/{id}`: Remove item
*   `POST /api/cart/merge`: Merge guest cart with user cart

### Orders
*   `POST /api/order`: Create order
*   `GET /api/order/my-orders`: Get current user's orders

## Architecture Standards
*   **Standardized Responses**: All API responses follow `{"success": true, "message": "...", "data": ...}`.
*   **Global Exception Handling**: Centralized middleware catches and formats all exceptions.
*   **Tenant Isolation**: Built-in Query Filters ensure data isolation per Company.

---
*Maintained by Antigravity Architect Agent*
