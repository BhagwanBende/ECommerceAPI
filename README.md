\# 🛒 E-Commerce REST API

## 🚀 Live Demo

API Live on Azure: **[ecommerceapi-bhagwan-dkcbenbsdudvgwcx.centralindia-01.azurewebsites.net/swagger](https://ecommerceapi-bhagwan-dkcbenbsdudvgwcx.centralindia-01.azurewebsites.net)**


!\[.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)

!\[SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?logo=microsoftsqlserver)

!\[JWT](https://img.shields.io/badge/Auth-JWT-orange)

!\[Swagger](https://img.shields.io/badge/Docs-Swagger-green)



A production-ready \*\*RESTful Web API\*\* built with \*\*.NET 8\*\* for managing an e-commerce platform.



\## ✨ Features

\- 🔐 JWT Authentication \& Role-Based Authorization

\- 📦 Product Management (CRUD + Search + Pagination)

\- 🛍️ Order Management with Stock Control

\- 📊 Soft Delete for Products

\- 📖 Swagger UI Documentation

\- 🪵 Serilog Logging (Console + File)

\- ✅ Model Validation

\- 🛡️ Global Error Handling Middleware



\## 🛠️ Tech Stack

\- .NET 8 / ASP.NET Core Web API

\- Entity Framework Core 8

\- SQL Server

\- JWT Bearer Authentication

\- BCrypt Password Hashing

\- Swagger / Swashbuckle

\- Serilog



\## 🚀 Getting Started



\### Prerequisites

\- .NET 8 SDK

\- SQL Server



\### 1. Clone the repository

git clone https://github.com/YOUR\_USERNAME/ECommerceAPI.git

cd ECommerceAPI



\### 2. Update Connection String

appsettings.json मध्ये SQL Server connection string update कर



\### 3. Run Migrations

dotnet ef migrations add InitialCreate

dotnet ef database update



\### 4. Run the application

dotnet run



\### 5. Open Swagger UI

https://localhost:7107



\## 🔑 API Endpoints



\### Auth

| Method | Endpoint | Description |

|--------|----------|-------------|

| POST | /api/auth/register | Register new user |

| POST | /api/auth/login | Login \& get JWT token |



\### Products

| Method | Endpoint | Description |

|--------|----------|-------------|

| GET | /api/products | List all products |

| GET | /api/products/{id} | Get product by ID |

| POST | /api/products | Create product (Admin) |

| PUT | /api/products/{id} | Update product (Admin) |

| DELETE | /api/products/{id} | Delete product (Admin) |



\### Orders

| Method | Endpoint | Description |

|--------|----------|-------------|

| GET | /api/orders | Get orders |

| GET | /api/orders/{id} | Get order by ID |

| POST | /api/orders | Place new order |

| PATCH | /api/orders/{id}/status | Update status (Admin) |

| DELETE | /api/orders/{id} | Cancel order |



\## 👤 Default Admin

Email: admin@ecommerce.com

Password: Admin@123



\## 📜 License

MIT License

