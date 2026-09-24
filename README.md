# InnoPaint

Demo project to show agentic programming in .NET.

Paint store backend: a paint catalog with search, plus shopping baskets. ASP.NET Core on .NET 10, MySQL through EF Core. No authentication, no checkout.

## Run

```sh
dotnet user-secrets set "ConnectionStrings:InnoPaint" "Server=localhost;Port=3306;Database=Inno-Paint;User=root;Password=<password>" --project Program/src/Innowise.Api
dotnet ef database update --project Program/src/Innowise.Infrastructure --startup-project Program/src/Innowise.Api
dotnet run --project Program/src/Innowise.Api --launch-profile http
```

- API: `http://localhost:5247`
- Health (includes DB check): `GET /health`
- Smoke tests (API must be running): `python Program/tests/store_smoke.py`

Then start the React front end in a second terminal. The API must be running
first, because Vite proxies `/api` to it.

```sh
cd Program/frontend
npm install   # first run only
npm run dev
```

- Store: `http://localhost:5173`

See `Program/frontend/README.md` for the front end on its own.

## Domain

**Paint**: a catalog item.

| Field | Rule |
| --- | --- |
| `name` | required, max 150 |
| `brand` | required, max 100 |
| `description` | required, max 4000 |
| `color`, `finish`, `surface` | required, max 80 each |
| `isExterior` | bool |
| `volumeLiters` | 0.01 to 1000 |
| `price` | 0.01 to 1000000 |
| `stockQuantity` | 0 to 1000000 |

Text fields are trimmed on save.

**Basket**: an ID plus a list of lines `(paintId, quantity)`. A basket stores no prices. A basket is a wish list, not an order.

### Business rules

1. A basket line quantity must be between 1 and the paint's current `stockQuantity`. More than stock returns 409.
2. Setting an item replaces its quantity. It does not add to the existing quantity.
3. Baskets never reserve or decrement stock. Two baskets can each hold all of the stock.
4. A paint update cannot drop `stockQuantity` below a quantity already in any basket. The API returns 409.
5. Deleting a paint removes it from every basket first, then deletes the paint.
6. Basket totals are computed on every read from current catalog prices. A price change updates every basket that holds that paint.
7. All writes, and all basket reads, run under one global lock in `PaintStoreService` (singleton). That lock keeps rules 1, 4 and 5 consistent.

## Endpoints

### Paints: `/api/paints`

| Method | Path | Does | Success |
| --- | --- | --- | --- |
| GET | `/api/paints` | Search, filter, sort, page | 200 |
| GET | `/api/paints/lookups` | Distinct brands, colors, finishes, surfaces (for dropdowns) | 200 |
| GET | `/api/paints/{id}` | One paint | 200 |
| POST | `/api/paints` | Create paint | 201 + `Location` |
| PUT | `/api/paints/{id}` | Replace all paint fields (rule 4 applies) | 200 |
| DELETE | `/api/paints/{id}` | Delete paint (rule 5 applies) | 204 |

Create/update body:

```json
{
  "name": "Wall White",
  "brand": "Acme",
  "description": "Washable interior paint",
  "color": "White",
  "finish": "Matte",
  "surface": "Wall",
  "isExterior": false,
  "volumeLiters": 2.5,
  "price": 29.99,
  "stockQuantity": 40
}
```

Search query parameters (all optional):

| Param | Meaning |
| --- | --- |
| `search` | Space-separated terms. Every term must appear (case-insensitive) in name, brand, description, color, finish or surface. |
| `brand`, `color`, `finish`, `surface` | Exact match, case-insensitive |
| `isExterior` | `true` / `false` |
| `inStock` | `true` = stock > 0, `false` = stock is 0 |
| `minPrice`, `maxPrice` | Inclusive range. Min greater than max returns 400. |
| `minVolumeLiters`, `maxVolumeLiters` | Inclusive range. Min greater than max returns 400. |
| `sortBy` | `name` (default), `brand`, `price`, `volume`. Ties are sorted by ID, so pages stay stable. |
| `descending` | `true` reverses the sort |
| `page` | From 1, default 1 |
| `pageSize` | 1 to 100, default 20 |

Example: `GET /api/paints?search=white wall&finish=matte&inStock=true&maxPrice=50&sortBy=price`

Response: `{ "items": [...], "totalCount": 42, "page": 1, "pageSize": 20 }`. `totalCount` is the count after filters, before paging.

### Baskets: `/api/baskets`

| Method | Path | Does | Success |
| --- | --- | --- | --- |
| POST | `/api/baskets` | Create empty basket. Keep the returned `id`. | 201 |
| GET | `/api/baskets/{id}` | Basket with lines and totals | 200 |
| PUT | `/api/baskets/{id}/items/{paintId}` | Add item or replace its quantity. Body: `{"quantity": 2}` | 200 (basket view) |
| DELETE | `/api/baskets/{id}/items/{paintId}` | Remove one item | 204 |
| DELETE | `/api/baskets/{id}/items` | Clear all items | 204 |
| DELETE | `/api/baskets/{id}` | Delete basket | 204 |

Basket view:

```json
{
  "id": "…",
  "items": [ { "paint": { /* full paint */ }, "quantity": 2, "lineTotal": 59.98 } ],
  "itemCount": 2,
  "total": 59.98
}
```

`itemCount` is the sum of quantities, not the number of lines. A `PUT` on an existing item moves that item to the end of the list.

## Errors

All errors use Problem Details (`application/problem+json`) with a `status` field.

| Status | When |
| --- | --- |
| 400 | Validation failed (field rules, bad query, min > max) |
| 404 | Paint, basket or basket item not found |
| 409 | Quantity exceeds stock, or stock update goes below a basket quantity |

## Code map

```text
Program/src/
  Innowise.Domain/Paint.cs                        Paint, Basket, BasketItem records
  Innowise.Application/StoreContracts.cs          DTOs, validation, IStoreRepository, StoreException
  Innowise.Application/PaintStoreService.cs       all business logic
  Innowise.Infrastructure/Persistence/            EF Core DbContext, EfStoreRepository, migrations
  Innowise.Api/Controllers/                       thin controllers
  Innowise.Api/StoreExceptionHandler.cs           StoreException to Problem Details
  Innowise.Api/Program.cs                         DI and pipeline

Program/frontend/
  src/api/                                        typed client and DTOs for the endpoints above
  src/hooks/useBasket.ts                          the browser's basket, id kept in localStorage
  src/components/                                 fan deck, filter rail, swatch chips, order panel
  vite.config.ts                                  dev proxy to port 5247

Program/tests/store_smoke.py                      HTTP integration checks
```
