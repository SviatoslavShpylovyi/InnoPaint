import { contrastInk, swatchOf } from '../lib/color'

interface Props {
  colors: string[]
  selected?: string
  onSelect: (color: string | undefined) => void
}

/**
 * The catalog's real colours, laid out like the leaves of a fan deck. Picking
 * one is the colour filter, so the band does a job rather than decorating.
 */
export function FanDeck({ colors, selected, onSelect }: Props) {
  if (colors.length === 0) return null

  return (
    <div className="deck" role="group" aria-label="Filter by colour">
      {colors.map((color) => {
        const hex = swatchOf(color)
        const active = selected === color
        return (
          <button
            key={color}
            type="button"
            className="deck-leaf"
            style={{ background: hex, color: contrastInk(hex) }}
            aria-pressed={active}
            onClick={() => onSelect(active ? undefined : color)}
          >
            {color}
          </button>
        )
      })}
    </div>
  )
}
