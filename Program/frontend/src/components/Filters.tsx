import type { PaintLookups, PaintQuery } from '../api/types'
import { litres, price } from '../lib/format'
import { RangeSlider } from './RangeSlider'

interface Props {
  query: PaintQuery
  lookups: PaintLookups | null
  onChange: (patch: Partial<PaintQuery>) => void
  onReset: () => void
}

/** Exact, case-insensitive filters the API exposes as dropdowns. */
const DROPDOWNS = [
  { key: 'brand', label: 'Brand', source: 'brands' },
  { key: 'finish', label: 'Finish', source: 'finishes' },
  { key: 'surface', label: 'Surface', source: 'surfaces' },
] as const

/**
 * Slider ends. The catalog endpoint reports no numeric bounds, so these are
 * shop-floor limits: a thumb at either end drops the filter entirely, which
 * keeps stock outside the range reachable.
 */
const PRICE = { min: 0, max: 200, step: 1 }
const VOLUME = { min: 0, max: 20, step: 0.5 }

export function Filters({ query, lookups, onChange, onReset }: Props) {
  return (
    <aside className="rail">
      <div className="field">
        <label htmlFor="search">Search</label>
        <input
          id="search"
          type="search"
          placeholder="white wall"
          value={query.search ?? ''}
          onChange={(event) => onChange({ search: event.target.value })}
        />
      </div>

      {DROPDOWNS.map(({ key, label, source }) => (
        <div className="field" key={key}>
          <label htmlFor={key}>{label}</label>
          <select
            id={key}
            value={query[key] ?? ''}
            onChange={(event) => onChange({ [key]: event.target.value || undefined })}
          >
            <option value="">Any</option>
            {lookups?.[source].map((value) => (
              <option key={value} value={value}>
                {value}
              </option>
            ))}
          </select>
        </div>
      ))}

      <RangeSlider
        label="Price"
        min={PRICE.min}
        max={PRICE.max}
        step={PRICE.step}
        low={query.minPrice}
        high={query.maxPrice}
        format={price}
        onChange={(minPrice, maxPrice) => onChange({ minPrice, maxPrice })}
      />

      <RangeSlider
        label="Volume"
        min={VOLUME.min}
        max={VOLUME.max}
        step={VOLUME.step}
        low={query.minVolumeLiters}
        high={query.maxVolumeLiters}
        format={litres}
        onChange={(minVolumeLiters, maxVolumeLiters) =>
          onChange({ minVolumeLiters, maxVolumeLiters })
        }
      />

      <div className="toggles">
        <label className="toggle">
          <input
            type="checkbox"
            checked={query.inStock === true}
            onChange={(event) => onChange({ inStock: event.target.checked || undefined })}
          />
          <span className="switch" aria-hidden="true" />
          In stock only
        </label>
        <label className="toggle">
          <input
            type="checkbox"
            checked={query.isExterior === true}
            onChange={(event) => onChange({ isExterior: event.target.checked || undefined })}
          />
          <span className="switch" aria-hidden="true" />
          Exterior use
        </label>
      </div>

      <button type="button" className="btn btn--ghost" onClick={onReset}>
        Clear filters
      </button>
    </aside>
  )
}
