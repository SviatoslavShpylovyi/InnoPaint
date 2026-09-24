import { useCallback, useEffect, useMemo, useState } from 'react'
import { ApiError, api } from './api/client'
import type { Paint, PaintLookups, PaintPage, PaintQuery, SortBy } from './api/types'
import { FanDeck } from './components/FanDeck'
import { Filters } from './components/Filters'
import { Home } from './components/Home'
import { OrderPanel } from './components/OrderPanel'
import { PaintChip } from './components/PaintChip'
import { PaletteAssistant } from './components/PaletteAssistant'
import { useBasket } from './hooks/useBasket'

const PAGE_SIZE = 24
const INITIAL: PaintQuery = { sortBy: 'name', page: 1, pageSize: PAGE_SIZE }

const SORTS: { value: SortBy; label: string }[] = [
  { value: 'name', label: 'Name' },
  { value: 'brand', label: 'Brand' },
  { value: 'price', label: 'Price' },
  { value: 'volume', label: 'Volume' },
]

export default function App() {
  const [view, setView] = useState<'home' | 'catalog'>('home')
  const [query, setQuery] = useState<PaintQuery>(INITIAL)
  const [results, setResults] = useState<PaintPage | null>(null)
  const [lookups, setLookups] = useState<PaintLookups | null>(null)
  const [searchError, setSearchError] = useState<string | null>(null)
  const basket = useBasket()

  useEffect(() => {
    api.lookups().then(setLookups).catch(() => setLookups(null))
  }, [])

  // Typing in the search box should not fire a request per keystroke.
  useEffect(() => {
    const timer = setTimeout(() => {
      api
        .searchPaints(query)
        .then((page) => {
          setResults(page)
          setSearchError(null)
        })
        .catch((cause: unknown) => {
          setSearchError(
            cause instanceof ApiError
              ? cause.message
              : 'The catalog is not responding. Check the API is running on port 5247.',
          )
        })
    }, 250)
    return () => clearTimeout(timer)
  }, [query])

  // Any change to a filter invalidates the current page number.
  const patch = useCallback((change: Partial<PaintQuery>) => {
    setQuery((current) => ({ ...current, page: 1, ...change }))
  }, [])

  const quantities = useMemo(() => {
    const map = new Map<string, number>()
    for (const line of basket.basket?.items ?? []) map.set(line.paint.id, line.quantity)
    return map
  }, [basket.basket])

  const addOne = (paint: Paint) =>
    basket.setQuantity(paint.id, (quantities.get(paint.id) ?? 0) + 1)

  const total = results?.totalCount ?? 0
  const lastPage = Math.max(1, Math.ceil(total / PAGE_SIZE))
  const page = query.page ?? 1

  return (
    <>
      <header className="masthead">
        <h1 className="wordmark">
          InnoPaint <span>trade counter</span>
        </h1>
        <div className="masthead-side">
          <p className="count">
            {lookups ? `${lookups.colors.length} colours, ${lookups.brands.length} brands` : ' '}
          </p>
          <button
            type="button"
            className="btn btn--small"
            onClick={() => setView(view === 'home' ? 'catalog' : 'home')}
          >
            {view === 'home' ? 'Catalogue' : 'Home'}
          </button>
        </div>
      </header>

      {view === 'home' && (
        <Home
          colors={lookups?.colors ?? []}
          brandCount={lookups?.brands.length ?? 0}
          onEnter={() => setView('catalog')}
        />
      )}

      {view === 'catalog' && (
        <>
          <FanDeck
            colors={lookups?.colors ?? []}
            selected={query.color}
            onSelect={(color) => patch({ color })}
          />

          {(searchError || basket.error) && (
            <p className="alert" role="status">
              <span>{searchError ?? basket.error}</span>
              {basket.error && (
                <button type="button" className="btn btn--small" onClick={basket.dismissError}>
                  Dismiss
                </button>
              )}
            </p>
          )}

          <main className="shell">
            <Filters
              query={query}
              lookups={lookups}
              onChange={patch}
              onReset={() => setQuery(INITIAL)}
            />

            <section aria-label="Paints">
              <div className="wall-head">
                <p className="tally">
                  {results ? `${total} ${total === 1 ? 'paint' : 'paints'}` : 'Loading the catalog'}
                </p>
                <div className="sorts">
                  {SORTS.map(({ value, label }) => {
                    const active = query.sortBy === value
                    return (
                      <button
                        key={value}
                        type="button"
                        className="btn btn--small"
                        aria-pressed={active}
                        onClick={() =>
                          patch(
                            active
                              ? { descending: !query.descending }
                              : { sortBy: value, descending: false },
                          )
                        }
                      >
                        {label}
                        {active && (query.descending ? ' ↓' : ' ↑')}
                      </button>
                    )
                  })}
                </div>
              </div>

              <div className="wall">
                {results?.items.map((paint) => (
                  <PaintChip
                    key={paint.id}
                    paint={paint}
                    inOrder={quantities.get(paint.id) ?? 0}
                    busy={basket.pendingPaintId === paint.id}
                    onAdd={addOne}
                  />
                ))}
                {results?.items.length === 0 && (
                  <p className="wall-empty">
                    No paint matches those filters. Widen the price range or clear a filter.
                  </p>
                )}
              </div>

              {lastPage > 1 && (
                <nav className="pager" aria-label="Pages">
                  <button
                    type="button"
                    className="btn btn--small"
                    disabled={page <= 1}
                    onClick={() => setQuery((q) => ({ ...q, page: page - 1 }))}
                  >
                    Previous
                  </button>
                  <span>
                    Page {page} of {lastPage}
                  </span>
                  <button
                    type="button"
                    className="btn btn--small"
                    disabled={page >= lastPage}
                    onClick={() => setQuery((q) => ({ ...q, page: page + 1 }))}
                  >
                    Next
                  </button>
                </nav>
              )}
            </section>

            <OrderPanel
              basket={basket.basket}
              pendingPaintId={basket.pendingPaintId}
              onSetQuantity={basket.setQuantity}
              onRemove={basket.remove}
              onClear={basket.clear}
            />
          </main>

          <PaletteAssistant
            onAdd={addOne}
            orderBusy={basket.pendingPaintId !== null}
            quantities={quantities}
          />
        </>
      )}
    </>
  )
}
