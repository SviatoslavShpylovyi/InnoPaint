import type { PaintLookups, PaintQuery } from '../api/types'

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

export function Filters({ query, lookups, onChange, onReset }: Props) {
  const number = (value: string) => (value === '' ? undefined : Number(value))

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

      <div className="field">
        <label htmlFor="minPrice">Price</label>
        <div className="pair">
          <input
            id="minPrice"
            type="number"
            min={0}
            step="0.01"
            placeholder="from"
            value={query.minPrice ?? ''}
            onChange={(event) => onChange({ minPrice: number(event.target.value) })}
          />
          <input
            type="number"
            min={0}
            step="0.01"
            placeholder="to"
            aria-label="Maximum price"
            value={query.maxPrice ?? ''}
            onChange={(event) => onChange({ maxPrice: number(event.target.value) })}
          />
        </div>
      </div>

      <div className="field">
        <label htmlFor="minVolume">Volume in litres</label>
        <div className="pair">
          <input
            id="minVolume"
            type="number"
            min={0.01}
            step="0.001"
            placeholder="from"
            value={query.minVolumeLiters ?? ''}
            onChange={(event) => onChange({ minVolumeLiters: number(event.target.value) })}
          />
          <input
            type="number"
            min={0.01}
            step="0.001"
            placeholder="to"
            aria-label="Maximum volume"
            value={query.maxVolumeLiters ?? ''}
            onChange={(event) => onChange({ maxVolumeLiters: number(event.target.value) })}
          />
        </div>
      </div>

      <div className="toggles">
        <label className="toggle">
          <input
            type="checkbox"
            checked={query.inStock === true}
            onChange={(event) => onChange({ inStock: event.target.checked || undefined })}
          />
          In stock only
        </label>
        <label className="toggle">
          <input
            type="checkbox"
            checked={query.isExterior === true}
            onChange={(event) => onChange({ isExterior: event.target.checked || undefined })}
          />
          Exterior use
        </label>
      </div>

      <button type="button" className="link" onClick={onReset}>
        Clear filters
      </button>
    </aside>
  )
}
