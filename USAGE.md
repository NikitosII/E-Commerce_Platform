# Usage Guide

Step-by-step setup, configuration, and API reference for the E-Commerce Platform.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Apply Database Migrations](#apply-database-migrations)
- [Run the Application](#run-the-application)
- [Run Tests](#run-tests)
- [API Reference](#api-reference)
- [Error Responses](#error-responses)
- [Caching](#caching)
- [Database Migrations](#database-migrations)

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+
- Redis 7+ (local) **or** a Redis Cloud account
- EF Core CLI tools:

```bash
dotnet tool install --global dotnet-ef
```

---

## Configuration

Edit [src/ECommerce.Web/appsettings.json](src/ECommerce.Web/appsettings.json).

### Option A — Local Redis

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=yourpassword",
    "Redis": "localhost:6379"
  }
}
```

### Option B — Redis Cloud

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=yourpassword",
    "Redis": "redis-xxxxx.cloud.redislabs.com:12345,password=YOUR_PASSWORD,ssl=true,abortConnect=false"
  }
}
```

Get the endpoint and password from your database page at [cloud.redis.io](https://cloud.redis.io/#/databases).

### Email (optional)

Order confirmation emails are sent after a successful order. If SMTP is not configured, the failure is logged as a warning and the order is still returned successfully.

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "you@gmail.com",
    "Password": "app-password",
    "FromAddress": "noreply@yourdomain.com"
  }
}
```

### Quick start with Docker 

```bash
docker run -d --name postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -e POSTGRES_DB=ecommerce \
  -p 5432:5432 postgres:16

docker run -d --name redis -p 6379:6379 redis:7
```

---

## Apply Database Migrations

Run once from the solution root:

```bash
dotnet ef database update \
  --project src/ECommerce.Infrastructure \
  --startup-project src/ECommerce.Web
```

---

## Run the Application

```bash
dotnet run --project src/ECommerce.Web
```

The API starts on `https://localhost:5001` / `http://localhost:5000`.
Swagger UI is available at `https://localhost:5001/swagger`.

---

## Run Tests

```bash
# all projects
dotnet test

# specific project
dotnet test tests/ECommerce.Application.Tests
dotnet test tests/ECommerce.Infrastructure.Tests
```

---

## API Reference

### Response headers

Every response includes:

| Header | Example | Source |
|---|---|---|
| `X-Correlation-ID` | `3fa85f64-5717-4562-b3fc-2c963f66afa6` | `RequestLoggingMiddleware` (generated or propagated from request) |
| `X-Api-Version` | `1.0` | `ApiVersionMiddleware` |

Pass your own correlation ID to trace a request through logs:

```bash
curl https://localhost:5001/api/products \
  -H "X-Correlation-ID: my-trace-id-123"
```

<details>
<summary><strong>Products</strong> (click to expand)</summary>

#### GET /api/products

Returns a paged list of active (non-deleted) products.

Query parameters: `page` (default `1`), `pageSize` (default `10`, max `100`).

```bash
GET /api/products?page=1&pageSize=10
```

```json
{
  "items": [
    {
      "id": "eac6b891-e8cb-4e3d-a48d-9a95773b066f",
      "name": "Wireless Mouse",
      "description": "Ergonomic 2.4GHz wireless mouse",
      "price": 29.99,
      "stockQuantity": 150,
      "imageUrl": "https://example.com/mouse.jpg",
      "categoryId": "a1b2c3d4-0000-0000-0000-000000000001",
      "categoryName": "Peripherals"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### GET /api/products/{id}

Returns a single product by GUID. Result is cached for 30 minutes under `product:<id>`.

Returns `404` if not found or soft-deleted.

#### GET /api/products/search?term=

Searches product name and description. Results are cached under `search:<sha256(term)>`.

```bash
GET /api/products/search?term=wireless
```

#### GET /api/products/in-stock

Returns all products with `stockQuantity > 0`.

#### GET /api/products/category/{categoryId}

Returns all active products belonging to a category.

#### POST /api/products

Creates a product. The `categoryId` must reference an existing category.

```json
{
  "name": "Wireless Mouse",
  "description": "Ergonomic 2.4GHz wireless mouse",
  "price": 29.99,
  "stockQuantity": 150,
  "imageUrl": "https://example.com/mouse.jpg",
  "categoryId": "a1b2c3d4-0000-0000-0000-000000000001"
}
```

Returns `201 Created` with the product body and a `Location` header.

#### PUT /api/products/{id}

Full update. All fields required.

```json
{
  "name": "Wireless Mouse Pro",
  "description": "Updated description",
  "price": 39.99,
  "stockQuantity": 200,
  "imageUrl": "https://example.com/mouse-pro.jpg",
  "categoryId": "a1b2c3d4-0000-0000-0000-000000000001"
}
```

Cache entry for this product is invalidated on success.

#### DELETE /api/products/{id}

Soft-deletes the product (`IsDeleted = true`). Returns `204 No Content`. Cache entry is invalidated.

</details>

<details>
<summary><strong>Categories</strong> (click to expand)</summary>

#### GET /api/categories

Returns all root categories (no parent) with their sub-categories embedded.

```json
[
  {
    "id": "a1b2c3d4-0000-0000-0000-000000000001",
    "name": "Electronics",
    "description": "Electronic devices and accessories",
    "parentCategoryId": null,
    "subCategories": [
      {
        "id": "b2c3d4e5-0000-0000-0000-000000000002",
        "name": "Peripherals",
        "description": null,
        "parentCategoryId": "a1b2c3d4-0000-0000-0000-000000000001",
        "subCategories": []
      }
    ]
  }
]
```

#### GET /api/categories/{id}

Returns a category by ID with sub-categories. Cached for 30 minutes. Returns `404` if not found.

#### GET /api/categories/{id}/subcategories

Returns the direct children of a category.

#### POST /api/categories

`parentCategoryId` is optional — omit or set to `null` for a root category.

```json
{
  "name": "Peripherals",
  "description": "Keyboards, mice, and other peripherals",
  "parentCategoryId": "a1b2c3d4-0000-0000-0000-000000000001"
}
```

Returns `201 Created`.

#### DELETE /api/categories/{id}

Soft-deletes the category. Returns `204 No Content`. Cache entry is invalidated.

</details>

<details>
<summary><strong>Orders</strong> (click to expand)</summary>

#### GET /api/orders

Paged list of all orders including customer info and line items.

Query parameters: `page`, `pageSize`.

```json
{
  "items": [
    {
      "id": "f1a2b3c4-0000-0000-0000-000000000001",
      "customerId": "d8a16fad-a331-4c99-9325-9fa6412c0339",
      "customerName": "Jane Doe",
      "status": "Pending",
      "totalAmount": 59.98,
      "shippingAddress": "123 Main St, City, Country",
      "createdAt": "2026-03-09T18:00:00Z",
      "items": [
        {
          "productId": "eac6b891-e8cb-4e3d-a48d-9a95773b066f",
          "productName": "Wireless Mouse",
          "quantity": 2,
          "unitPrice": 29.99
        }
      ]
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### GET /api/orders/{id}

Returns a single order with full item and customer details. Cached for 30 minutes.

#### GET /api/orders/customer/{customerId}

Returns all orders for a given customer.

#### GET /api/orders/by-status/{status}

Filters orders by status. Valid values: `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`.

```bash
GET /api/orders/by-status/Pending
```

#### POST /api/orders

Places a new order. The service atomically:

1. Validates the customer exists
2. Validates each product exists and has sufficient stock
3. Deducts stock from each product
4. Calculates `totalAmount` from line items
5. Saves the order in a database transaction
6. Sends a confirmation email (failure is logged as a warning, not returned as an error)

```json
{
  "customerId": "d8a16fad-a331-4c99-9325-9fa6412c0339",
  "shippingAddress": "123 Main St, City, Country",
  "notes": "Leave at the door",
  "items": [
    {
      "productId": "eac6b891-e8cb-4e3d-a48d-9a95773b066f",
      "quantity": 2
    }
  ]
}
```

Returns `201 Created` with the full order. Returns `400` if stock is insufficient.

#### PATCH /api/orders/{id}/status

Updates the order status. Send the new status as a plain JSON string in the body.

```bash
PATCH /api/orders/f1a2b3c4-.../status
Content-Type: application/json

"Processing"
```

Rules enforced by the service:
- Cannot update an order that is already `Delivered` or `Cancelled`
- Cannot set status to `Delivered` unless a `Payment` record exists for the order

Cache entry is invalidated on success.

#### POST /api/orders/{id}/cancel

Cancels the order. Returns `204 No Content`.

</details>

<details>
<summary><strong>Customers</strong> (click to expand)</summary>

#### GET /api/customers/{id}

Returns a customer by ID. Returns `404` if not found or soft-deleted.

#### GET /api/customers/by-email?email=

Returns a customer by their unique email address.

```bash
GET /api/customers/by-email?email=jane@example.com
```

#### POST /api/customers

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "address": "123 Main St, City, Country"
}
```

Returns `201 Created`.

#### DELETE /api/customers/{id}

Soft-deletes the customer. Returns `204 No Content`.

</details>

---

## Error Responses

All errors return a consistent envelope:

```json
{
  "status": 404,
  "error": "Product 'eac6b891-...' was not found.",
  "timestamp": "2026-03-09T18:00:00Z"
}
```

In **Development** mode an additional `detail` field contains the full exception and stack trace.

| Status | Cause |
|---|---|
| `400` | Validation failure, insufficient stock, invalid state transition, DB constraint violation |
| `404` | Entity not found by ID or email |
| `401` | Unauthorized access |
| `500` | Unexpected server error |

---

## Caching

| Cache key | TTL | Populated by | 
|---|---|---|
| `product:<id>` | 30 min | `GET /api/products/{id}` |
| `search:<sha256(term)>` | 30 min | `GET /api/products/search` |
| `category:<id>` | 30 min | `GET /api/categories/{id}` |
| `order:<id>` | 30 min | `GET /api/orders/{id}` |

To flush all cache entries (local Redis only):

```bash
redis-cli FLUSHDB
```

---

## Database Migrations

### Add a migration after model changes

```bash
dotnet ef migrations add <MigrationName> \
  --project src/ECommerce.Infrastructure \
  --startup-project src/ECommerce.Web
```

### Apply all pending migrations

```bash
dotnet ef database update \
  --project src/ECommerce.Infrastructure \
  --startup-project src/ECommerce.Web
```

### Remove the last unapplied migration

```bash
dotnet ef migrations remove \
  --project src/ECommerce.Infrastructure \
  --startup-project src/ECommerce.Web
```

### Roll back to a specific migration

```bash
dotnet ef database update <MigrationName> \
  --project src/ECommerce.Infrastructure \
  --startup-project src/ECommerce.Web
```
