import type { CSSProperties } from 'react'

interface Props {
  label: string
  min: number
  max: number
  step: number
  low?: number
  high?: number
  /** Renders one end of the range, e.g. price() or litres(). */
  format: (value: number) => string
  onChange: (low: number | undefined, high: number | undefined) => void
}

const clamp = (value: number, min: number, max: number) =>
  Math.min(max, Math.max(min, value))

/**
 * Two native range inputs stacked on one track. Native keeps the keyboard and
 * screen-reader behaviour; only the thumbs take pointer events, so the input
 * lying on top does not swallow drags meant for the other thumb.
 */
export function RangeSlider({ label, min, max, step, low, high, format, onChange }: Props) {
  const lo = clamp(low ?? min, min, max)
  const hi = clamp(high ?? max, min, max)
  const span = max - min || 1

  // A thumb parked at its end means "no bound", which the API expects as an
  // absent query parameter rather than the extreme value.
  const emit = (nextLo: number, nextHi: number) =>
    onChange(nextLo <= min ? undefined : nextLo, nextHi >= max ? undefined : nextHi)

  const track = {
    '--lo': `${((lo - min) / span) * 100}%`,
    '--hi': `${((hi - min) / span) * 100}%`,
  } as CSSProperties

  return (
    <div className="field">
      <div className="range-head">
        <label>{label}</label>
        <span className="range-readout">
          {format(lo)} &ndash; {format(hi)}
          {hi >= max ? '+' : ''}
        </span>
      </div>

      <div className="range" style={track}>
        <span className="range-track" aria-hidden="true" />
        <span className="range-fill" aria-hidden="true" />
        <input
          className="range-input range-input--lo"
          type="range"
          min={min}
          max={max}
          step={step}
          value={lo}
          aria-label={`${label}, lowest`}
          onChange={(event) => emit(Math.min(Number(event.target.value), hi), hi)}
        />
        <input
          className="range-input range-input--hi"
          type="range"
          min={min}
          max={max}
          step={step}
          value={hi}
          aria-label={`${label}, highest`}
          onChange={(event) => emit(lo, Math.max(Number(event.target.value), lo))}
        />
      </div>
    </div>
  )
}
