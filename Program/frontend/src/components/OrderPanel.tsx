import type { BasketView } from '../api/types'
import { swatchOf } from '../lib/color'
import { litres, price } from '../lib/format'

interface Props {
  basket: BasketView | null
  pendingPaintId: string | null
  onSetQuantity: (paintId: string, quantity: number) => void
  onRemove: (paintId: string) => void
  onClear: () => void
}

export function OrderPanel({ basket, pendingPaintId, onSetQuantity, onRemove, onClear }: Props) {
  const lines = basket?.items ?? []

  return (
    <section className="order" aria-label="Your order">
      <div className="order-head">
        <h2>Your order</h2>
        {lines.length > 0 && (
          <button type="button" className="btn btn--ghost btn--small" onClick={onClear}>
            Empty it
          </button>
        )}
      </div>

      {lines.length === 0 ? (
        <p className="empty">Pick a colour from the wall and it lands here.</p>
      ) : (
        <ul className="lines">
          {lines.map(({ paint, quantity, lineTotal }) => {
            const busy = pendingPaintId === paint.id
            return (
              <li className="line" key={paint.id}>
                <span className="line-swatch" style={{ background: swatchOf(paint.color) }} />
                <div>
                  <div className="line-name">{paint.name}</div>
                  <div className="line-meta">
                    {paint.brand}, {litres(paint.volumeLiters)}
                  </div>

                  <div className="line-controls">
                    <button
                      type="button"
                      className="btn btn--icon"
                      aria-label={`One fewer ${paint.name}`}
                      disabled={busy}
                      onClick={() =>
                        quantity === 1 ? onRemove(paint.id) : onSetQuantity(paint.id, quantity - 1)
                      }
                    >
                      &minus;
                    </button>
                    <span className="qty">{quantity}</span>
                    <button
                      type="button"
                      className="btn btn--icon btn--accent"
                      aria-label={`One more ${paint.name}`}
                      disabled={busy || quantity >= paint.stockQuantity}
                      onClick={() => onSetQuantity(paint.id, quantity + 1)}
                    >
                      +
                    </button>
                    <span className="line-total">{price(lineTotal)}</span>
                  </div>
                </div>
              </li>
            )
          })}
        </ul>
      )}

      <div className="order-foot">
        <div className="total">
          <span>
            {basket?.itemCount ?? 0} {basket?.itemCount === 1 ? 'tin' : 'tins'}
          </span>
          <span className="total-value">{price(basket?.total ?? 0)}</span>
        </div>
        <p className="note">Before tax and delivery. Stock is not held for you.</p>
      </div>
    </section>
  )
}
