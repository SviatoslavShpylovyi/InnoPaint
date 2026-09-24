import type {
  AssistantReply, BasketView, Paint, PaintLookups, PaintPage, PaintQuery, PaintRequest,
} from './types'

/**
 * Thrown for any non-2xx response. The API answers errors with Problem Details
 * (400 invalid input, 404 missing, 409 stock conflict), so `status` and the
 * server's own message are carried through to the UI rather than a generic one.
 */
export class ApiError extends Error {
  readonly status: number
  readonly errors?: Record<string, string[]>

  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

async function toError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails = {}
  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // A body that isn't JSON leaves the status line as the only detail.
  }
  // Validation failures from [ApiController] put the useful text in `errors`;
  // StoreException puts it in `detail`.
  const fieldMessage = problem.errors && Object.values(problem.errors).flat()[0]
  const message = problem.detail ?? fieldMessage ?? problem.title ?? response.statusText
  return new ApiError(response.status, message, problem.errors)
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: init?.body ? { 'Content-Type': 'application/json' } : undefined,
  })
  if (!response.ok) throw await toError(response)
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

function queryString(query: PaintQuery): string {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === null || value === '') continue
    params.set(key, String(value))
  }
  const q = params.toString()
  return q ? `?${q}` : ''
}

export const api = {
  searchPaints: (query: PaintQuery = {}) =>
    request<PaintPage>(`/api/paints${queryString(query)}`),

  lookups: () => request<PaintLookups>('/api/paints/lookups'),

  getPaint: (id: string) => request<Paint>(`/api/paints/${id}`),

  createPaint: (paint: PaintRequest) =>
    request<Paint>('/api/paints', { method: 'POST', body: JSON.stringify(paint) }),

  updatePaint: (id: string, paint: PaintRequest) =>
    request<Paint>(`/api/paints/${id}`, { method: 'PUT', body: JSON.stringify(paint) }),

  deletePaint: (id: string) =>
    request<void>(`/api/paints/${id}`, { method: 'DELETE' }),

  createBasket: () => request<BasketView>('/api/baskets', { method: 'POST' }),

  getBasket: (id: string) => request<BasketView>(`/api/baskets/${id}`),

  /** PUT replaces the quantity outright; it does not add to it. */
  setItem: (basketId: string, paintId: string, quantity: number) =>
    request<BasketView>(`/api/baskets/${basketId}/items/${paintId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantity }),
    }),

  removeItem: (basketId: string, paintId: string) =>
    request<void>(`/api/baskets/${basketId}/items/${paintId}`, { method: 'DELETE' }),

  clearBasket: (basketId: string) =>
    request<void>(`/api/baskets/${basketId}/items`, { method: 'DELETE' }),

  deleteBasket: (basketId: string) =>
    request<void>(`/api/baskets/${basketId}`, { method: 'DELETE' }),

  /**
   * Omit `conversationId` to start a conversation. The server keeps history for
   * 30 minutes; a stale id answers 404 and the caller must start over.
   */
  sendAssistantMessage: (message: string, conversationId?: string) =>
    request<AssistantReply>('/api/assistant/messages', {
      method: 'POST',
      body: JSON.stringify({ message, conversationId }),
    }),
}
