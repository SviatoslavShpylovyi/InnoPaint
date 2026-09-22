import type { Paint } from '../api/types'
import { contrastInk, swatchOf } from '../lib/color'
import { litres, price } from '../lib/format'

interface Props {
  paint: Paint
  inOrder: number
  busy: boolean
  onAdd: (paint: Paint) => void
}

export function PaintChip({ paint, inOrder, busy, onAdd }: Props) {
  const hex = swatchOf(paint.color)
  const soldOut = paint.stockQuantity === 0
  const maxed = inOrder >= paint.stockQuantity

  return (
    <article className={soldOut ? 'chip out' : 'chip'}>
      <div className="chip-color" style={{ background: hex, color: contrastInk(hex) }}>
        <span className="swatch-name">{paint.color}</span>
        <span className="swatch-vol">{litres(paint.volumeLiters)}</span>
      </div>

      <div className="chip-body">
        <h3 className="chip-name">{paint.name}</h3>
        <p className="chip-brand">{paint.brand}</p>
        <p className="chip-spec">
          <span>{paint.finish}</span>
          <span>{paint.surface}</span>
          <span>{paint.isExterior ? 'Exterior' : 'Interior'}</span>
        </p>

        <div className="chip-foot">
          <div>
            <div className="chip-price">{price(paint.price)}</div>
            <div className={soldOut ? 'stock none' : 'stock'}>
              {soldOut ? 'Out of stock' : `${paint.stockQuantity} in stock`}
            </div>
          </div>

          <button
            type="button"
            className={inOrder > 0 ? 'add in' : 'add'}
            disabled={soldOut || maxed || busy}
            onClick={() => onAdd(paint)}
          >
            {orderLabel(inOrder, soldOut, maxed)}
          </button>
        </div>
      </div>
    </article>
  )
}

function orderLabel(inOrder: number, soldOut: boolean, maxed: boolean): string {
  if (soldOut) return 'Sold out'
  if (maxed) return `All ${inOrder} added`
  if (inOrder > 0) return `Add another (${inOrder})`
  return 'Add to order'
}
