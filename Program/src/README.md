# InnoPaint backend notes

ASP.NET Core Web API (.NET 10) with Clean Architecture. Commands below run from `Program/`.

## Structure

```text
src/
  Innowise.slnx
  BuildingBlocks/
    BuildingBlocks/               # Shared exception family, no ASP.NET dependency
    BuildingBlocks.AspNetCore/    # Exception handler, validation filter, DI extensions
  Innowise.Domain/          # Models/ - entities and business rules
  Innowise.Application/     # DTOs/, Interfaces/, Services/, Mappings/, Validators/, Exceptions/
  Innowise.Infrastructure/  # Data/, Repositories/, Migrations/
  Innowise.Api/             # HTTP controllers, middleware, dependency injection
```

Project dependencies point inward:

```text
Application -> Domain + BuildingBlocks
Infrastructure -> Application -> Domain
Api -> Application + Infrastructure + BuildingBlocks.AspNetCore
```

Domain has no project or package dependencies. Define use cases and their
required interfaces in Application; implement persistence and integrations in
Infrastructure. Register implementations in the API's `Program.cs`, which is
the composition root. Controllers should delegate business operations to
Application services.

The paint store uses scoped application services (`IPaintService`,
`IBasketService`) over one repository per aggregate (`IPaintRepository`,
`IBasketRepository`), backed by the MySQL database `Inno-Paint` (provider
`MySql.EntityFrameworkCore`). Check-then-write operations run in a transaction
and take a `SELECT ... FOR UPDATE` row lock on the paint, so stock invariants
hold across API instances. The catalog starts empty; paints and baskets
persist across restarts. No authentication is configured. Prices use a single,
store-defined currency, are stored with 2 decimal places (volumes with 3), and
totals exclude tax and shipping.

Request bodies and query strings are validated by FluentValidation validators in
`Innowise.Application/Validators/`, run by a global action filter. Domain errors
are typed exceptions derived from `AppException`; one `IExceptionHandler` turns
them into Problem Details.

## Run without Docker

Install the .NET 10 SDK and a local MySQL 8 server. The API reads the `ConnectionStrings:InnoPaint` setting, kept in .NET user secrets
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

## Business rules

1. A basket quantity must be between 1 and the paint's current stock. More than stock returns 409.
2. `PUT` on a basket item replaces its quantity; it does not add to it.
3. Baskets never reserve or decrement stock. Checkout is not implemented.
4. A paint update cannot drop stock below a quantity already in a basket (409).
5. Deleting a paint removes it from every basket first.
6. Basket totals are computed on every read from current catalog prices.

Invalid input returns 400, missing resources return 404, and stock conflicts
return 409, all as Problem Details. Successful deletes return 204.

## Assistant endpoint

| Method | Path | Behavior |
| --- | --- | --- |
| POST | `/api/assistant/messages` | Send a message to the palette assistant; needs `OpenAI:ApiKey` and `OpenAI:Model` |

Run the HTTP smoke checks against a running development instance:

```sh
python tests/store_smoke.py
```

For local HTTPS, trust the development certificate with
`dotnet dev-certs https --trust`, then use `--launch-profile https`.
In Azure, TLS ends at the Container Apps ingress; the API restores the original
scheme and client IP from `X-Forwarded-*` headers (`UseForwardedHeaders`).

Unhandled errors and empty error responses use ASP.NET Core Problem Details.
Configuration lives in `appsettings.json` and `appsettings.Development.json`;
keep secrets in environment variables or .NET user secrets.
