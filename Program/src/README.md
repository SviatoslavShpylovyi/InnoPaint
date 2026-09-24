# Innowise

ASP.NET Core Web API starter using Clean Architecture and .NET 10.

## Structure

```text
src/
  Innowise.slnx
  Innowise.Domain/          # Entities, value objects, business rules
  Innowise.Application/     # Use cases and interfaces for external services
  Innowise.Infrastructure/  # Persistence and external service implementations
  Innowise.Api/             # HTTP controllers, middleware, dependency injection
```

Project dependencies point inward:

```text
Application -> Domain
Infrastructure -> Application -> Domain
Api -> Application + Infrastructure
```

Domain has no project or package dependencies. Define use cases and their
required interfaces in Application; implement persistence and integrations in
Infrastructure. Register implementations in the API's `Program.cs`, which is
the composition root. Controllers should delegate business operations to
Application services.

The paint store uses a singleton application service and an EF Core repository
backed by the MySQL database `Inno-Paint` (provider `MySql.EntityFrameworkCore`).
The catalog starts empty; paints and baskets persist across restarts. No
authentication is configured. Prices use a single, store-defined currency, are
stored with 2 decimal places (volumes with 3), and totals exclude tax and shipping.

## Run

Install the .NET 10 SDK and a local MySQL 8 server, then run from the repository root.
The API reads the `ConnectionStrings:InnoPaint` setting, kept in .NET user secrets
for development:

```sh
dotnet user-secrets set "ConnectionStrings:InnoPaint" "Server=localhost;Port=3306;Database=Inno-Paint;User=root;Password=<password>" --project src/Innowise.Api
dotnet ef database update --project src/Innowise.Infrastructure --startup-project src/Innowise.Api
dotnet build src/Innowise.slnx
dotnet run --project src/Innowise.Api --launch-profile http
```

Visit `http://localhost:5247/health`; a running application returns `Healthy`.
The health check also verifies the database connection. `Innowise.Api.http` contains example store requests.

## Paint endpoints

| Method | Path | Behavior |
| --- | --- | --- |
| POST | `/api/paints` | Create paint; returns 201 and a Location header |
| GET | `/api/paints/{id}` | Full paint details, including description |
| PUT | `/api/paints/{id}` | Replace paint details |
| DELETE | `/api/paints/{id}` | Delete paint and remove it from all baskets |
| GET | `/api/paints` | Search, filter, sort, and paginate |
| GET | `/api/paints/lookups` | Distinct sorted brands, colors, finishes, and surfaces for dropdowns |

Search example: `/api/paints?search=white%20wall&finish=matte&inStock=true&maxPrice=50&sortBy=price&page=1&pageSize=20`.
Search is case-insensitive and requires every space-separated term to appear in
the name, brand, description, color, finish, or surface. Exact, case-insensitive
filters: `brand`, `color`, `finish`, `surface`. Additional filters: `isExterior`,
`inStock`, `minPrice`, `maxPrice`, `minVolumeLiters`, `maxVolumeLiters`.
`inStock=false` selects out-of-stock paints. Combine filters as needed.

`sortBy` accepts `name` (default), `brand`, `price`, or `volume`;
`descending=true` reverses it. `page` starts at 1; `pageSize` defaults to 20 and
is capped at 100. Responses contain `items`, `totalCount`, `page`, and `pageSize`.
Lookups cover the entire catalog.

## Basket endpoints

| Method | Path | Behavior |
| --- | --- | --- |
| POST | `/api/baskets` | Create an empty basket; retain the returned ID |
| GET | `/api/baskets/{id}` | Items with paint details, line totals, item count, and total |
| PUT | `/api/baskets/{id}/items/{paintId}` | Add or replace quantity using `{"quantity":2}` |
| DELETE | `/api/baskets/{id}/items/{paintId}` | Remove an item |
| DELETE | `/api/baskets/{id}/items` | Clear the basket |
| DELETE | `/api/baskets/{id}` | Delete the basket |

Quantities must be positive and cannot exceed paint stock. Baskets do not reserve
or decrement stock; checkout is not implemented. Totals use current catalog
prices. Stock updates below a quantity already in a basket return 409.
Invalid input returns 400, missing resources return 404, and insufficient stock
returns 409, using Problem Details. Successful deletes return 204.

Run the HTTP smoke checks against a running development instance:

```sh
python tests/store_smoke.py
```

For local HTTPS, trust the development certificate with
`dotnet dev-certs https --trust`, then use `--launch-profile https`.
HTTPS redirection and HSTS are enabled outside Development. Configure a valid
HTTPS endpoint or proxy HTTPS settings when deploying.

Unhandled errors and empty error responses use ASP.NET Core Problem Details.
Configuration lives in `appsettings.json` and `appsettings.Development.json`;
keep secrets in environment variables or .NET user secrets.
