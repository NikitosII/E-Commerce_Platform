# E-Commerce Platform

An educational ASP.NET Core 9 REST API with Clean Architecture, SOLID principles, custom middleware, action filters, Redis caching, and repository / unit-of-work patterns backed by PostgreSQL. Using dependency injection (DI) to manage cross-cutting issues such as logging, authentication, exception handling, and background job processing.

---

## Technologies

| Layer | Technology |
|---|---|
| Web API | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL  |
| Cache | Redis |
| Docs | Swagger / Swashbuckle 7 |
| Testing | xUnit + Moq |

---

## Solution Structure

```
ECommercePlatform/
├── src/
│   ├── ECommerce.Domain/          # Domain layer (entities, enums, repository & UoW interfaces)
│   ├── ECommerce.Common/          # Common utilities and helpers
│   ├── ECommerce.Application/     # Application layer (business logic)
│   ├── ECommerce.Infrastructure/  # Infrastructure layer (data access, services)
│   └── ECommerce.Web/             # Controllers, middleware, action filters, Program.cs
└── tests/
    ├── ECommerce.Application.Tests/
    └── ECommerce.Infrastructure.Tests/
```

### Dependency Graph

```
Domain  ←  Common  ←  Application  ←  Infrastructure  ←  Web
```

Every outer layer depends only on layers to its left. The **Web** layer wires everything together through DI.

---

### Entities

| Entity | Key Fields |
|---|---|
| `Product` | `Name`, `Description`, `Price`, `StockQuantity`, `ImageUrl`, `CategoryId` |
| `Category` | `Name`, `Description`, `ParentCategoryId` (self-referencing tree) |
| `Customer` | `FirstName`, `LastName`, `Email`, `Address` |
| `Order` | `CustomerId`, `Status` (enum), `TotalAmount`, `ShippingAddress`, `Notes` |
| `OrderItem` | `OrderId`, `ProductId`, `Quantity`, `UnitPrice` |
| `Payment` | `OrderId`, `Amount`, `Status`, `TransactionId`, `Provider` |

### Order Status Flow

```
Pending → Processing → Shipped → Delivered
       ↘                       ↗
               Cancelled 
```

Status cannot be updated once an order is `Delivered` or `Cancelled`. Marking as `Delivered` requires an associated `Payment` record.

---

## Middleware Pipeline

Every request passes through three middleware components before reaching a controller:

| Middleware | Responsibility |
|---|---|
| `ErrorHandlingMiddleware` | Catches all unhandled exceptions; maps them to a consistent JSON error envelope with the correct HTTP status code. In **Development**, the full stack trace is included in a `detail` field. |
| `RequestLoggingMiddleware` | Logs HTTP method, path, status code, and duration. Reads the incoming `X-Correlation-ID` header or generates a new GUID; propagates it to the response and to the log scope. |
| `ApiVersionMiddleware` | Appends `X-Api-Version: 1.0` to every response. |

---

## Global Action Filters

Applied to every controller action via `AddControllers(options => ...)`:

| Filter | Responsibility |
|---|---|
| `ValidateModelFilter` | Returns `400 Bad Request` with the full `ModelState` error dictionary before the action runs — no manual `ModelState.IsValid` checks needed in controllers. |
| `LogActionFilter` | Logs the controller name, action name, and correlation ID at the start and end of each action execution. |

---

## Caching

`ICacheService` is backed by Redis (`IDistributedCache`) with JSON serialisation. All cache entries use a **30-minute TTL** by default.

| Service | Cached operation | Cache key pattern | 
|---|---|---|---|
| `ProductService` | `GetProductByIdAsync` | `product:<id>` | 
| `ProductService` | `SearchProductsAsync` | `search:<sha256(term)>` | 
| `CategoryService` | `GetCategoryByIdAsync` | `category:<id>` | 
| `OrderService` | `GetOrderByIdAsync` | `order:<id>` | 

---

## Pagination

`GET /api/products` and `GET /api/orders` accept query parameters:

| Parameter | Default | Description |
|---|---|---|
| `page` | `1` | Page number (minimum 1) |
| `pageSize` | `10` | Items per page (clamped to 1–100) |

Response shape:

```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

## Prerequisites

See [USAGE.md](USAGE.md) for full instructions.
