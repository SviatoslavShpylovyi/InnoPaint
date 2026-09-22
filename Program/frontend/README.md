# Inno Paint trade counter

React front end for the Innowise paint store API. Vite + React 19 + TypeScript,
no UI framework. The ASP.NET Core API owns all data; this app only reads and
writes through it.

## Run

The API must be running first, since every request is proxied to it.

```sh
dotnet run --project ../src/Innowise.Api --launch-profile http   # http://localhost:5247
npm run dev                                                      # http://localhost:5173
```

`vite.config.ts` proxies `/api` and `/health` to port 5247, so the browser stays
on one origin and the API needs no CORS policy.

```sh
npm run build   # tsc -b, then a static bundle in dist/
npm run lint    # oxlint
```

## Layout

| Path | Holds |
| --- | --- |
| `src/api/types.ts` | Mirrors `StoreContracts.cs` and `Paint.cs`, camelCased |
| `src/api/client.ts` | Every endpoint, plus `ApiError` carrying the Problem Details status and message |
| `src/hooks/useBasket.ts` | The one basket for this browser; its id lives in localStorage |
| `src/lib/color.ts` | Turns a `color` name into a swatch hex, hashing unknown names |
| `src/components/` | Fan deck, filter rail, swatch chips, order panel |

## Notes

- The catalog stores `color` as free text, so swatches are derived in
  `lib/color.ts`. Add entries to `NAMED` there as the catalog grows; anything
  missing falls back to a stable hashed hue rather than a wrong colour.
- `PUT /api/baskets/{id}/items/{paintId}` replaces a quantity rather than adding
  to it, so the add button sends the current quantity plus one.
- Baskets do not reserve stock. A 409 from the API surfaces as an inline message
  rather than a thrown-away error.
- There is no session. If the stored basket is gone server-side, the app opens a
  fresh one instead of showing a 404.
