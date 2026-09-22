import { useCallback, useEffect, useRef, useState } from 'react'
import { ApiError, api } from '../api/client'
import type { BasketView } from '../api/types'

const STORAGE_KEY = 'innopaint.basketId'

/**
 * Owns the one basket this browser is working with. The API has no session, so
 * the id lives in localStorage; if the row is gone server-side a fresh basket
 * takes its place rather than leaving the page stuck on a 404.
 */
export function useBasket() {
  const [basket, setBasket] = useState<BasketView | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [pendingPaintId, setPendingPaintId] = useState<string | null>(null)
  const loading = useRef(false)

  const open = useCallback(async (): Promise<BasketView> => {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      try {
        return await api.getBasket(saved)
      } catch (cause) {
        if (!(cause instanceof ApiError) || cause.status !== 404) throw cause
      }
    }
    const created = await api.createBasket()
    localStorage.setItem(STORAGE_KEY, created.id)
    return created
  }, [])

  useEffect(() => {
    if (loading.current) return
    loading.current = true
    open()
      .then(setBasket)
      .catch((cause: unknown) => setError(describe(cause)))
  }, [open])

  /** Runs a basket call, recovering once if the basket vanished server-side. */
  const run = useCallback(
    async (paintId: string | null, action: (basketId: string) => Promise<BasketView | void>) => {
      setError(null)
      setPendingPaintId(paintId)
      try {
        const current = basket ?? (await open())
        const next = await action(current.id)
        setBasket(next ?? (await api.getBasket(current.id)))
      } catch (cause) {
        if (cause instanceof ApiError && cause.status === 404) {
          localStorage.removeItem(STORAGE_KEY)
          const fresh = await open().catch(() => null)
          if (fresh) setBasket(fresh)
        }
        setError(describe(cause))
      } finally {
        setPendingPaintId(null)
      }
    },
    [basket, open],
  )

  return {
    basket,
    error,
    pendingPaintId,
    dismissError: () => setError(null),
    /** Replaces the line quantity; the API's PUT does not accumulate. */
    setQuantity: (paintId: string, quantity: number) =>
      run(paintId, (id) => api.setItem(id, paintId, quantity)),
    remove: (paintId: string) => run(paintId, (id) => api.removeItem(id, paintId)),
    clear: () => run(null, (id) => api.clearBasket(id)),
  }
}

function describe(cause: unknown): string {
  if (cause instanceof ApiError) return cause.message
  return 'The store is not responding. Check the API is running on port 5247.'
}
