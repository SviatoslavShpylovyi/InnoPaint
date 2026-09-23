import { contrastInk, swatchOf } from '../lib/color'

interface Props {
  colors: string[]
  brandCount: number
  onEnter: () => void
}

const PITCH = [
  { head: 'Real stock', body: 'Every tin on the wall is counted. Sold out says sold out.' },
  { head: 'One counter', body: 'Filter, sort and build the order on a single screen.' },
  { head: 'No account', body: 'Pick colours and go. Nothing to sign up for.' },
]

export function Home({ colors, brandCount, onEnter }: Props) {
  return (
    <main className="home">
      <section className="hero">
        <p className="hero-kicker">Trade counter</p>
        <h2 className="hero-title">InnoPaint</h2>
        <p className="slogan">Pick the colour. We have the tin.</p>
        <button type="button" className="btn btn--accent btn--big" onClick={onEnter}>
          Open the catalogue
        </button>
        <p className="hero-stat">
          {colors.length > 0
            ? `${colors.length} colours, ${brandCount} brands, on the shelf today`
            : 'Loading the shelf'}
        </p>
      </section>

      {colors.length > 0 && (
        <div className="hero-strip" aria-hidden="true">
          {colors.slice(0, 14).map((color) => {
            const hex = swatchOf(color)
            return (
              <span key={color} style={{ background: hex, color: contrastInk(hex) }}>
                {color}
              </span>
            )
          })}
        </div>
      )}

      <section className="pitch">
        {PITCH.map(({ head, body }) => (
          <article className="pitch-card" key={head}>
            <h3>{head}</h3>
            <p>{body}</p>
          </article>
        ))}
      </section>
    </main>
  )
}
