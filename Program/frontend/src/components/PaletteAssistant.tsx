import { useEffect, useRef, useState } from 'react'
import { ApiError, api } from '../api/client'
import type { Paint, PaletteItem } from '../api/types'
import { contrastInk, swatchOf } from '../lib/color'
import { litres, price } from '../lib/format'

interface Props {
  onAdd: (paint: Paint) => Promise<void> | void
  orderBusy: boolean
  quantities: Map<string, number>
}

interface Turn {
  role: 'user' | 'assistant'
  text: string
}

const OPENER = 'Tell me the room, the surface and the mood. I will pick colours from the shelf.'

export function PaletteAssistant({ onAdd, orderBusy, quantities }: Props) {
  const [open, setOpen] = useState(false)
  const [input, setInput] = useState('')
  const [conversationId, setConversationId] = useState<string>()
  const [turns, setTurns] = useState<Turn[]>([])
  const [palette, setPalette] = useState<PaletteItem[]>([])
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const log = useRef<HTMLDivElement>(null)
  const box = useRef<HTMLTextAreaElement>(null)

  // A new turn or a fresh palette both push the interesting part out of view.
  useEffect(() => {
    if (log.current) log.current.scrollTop = log.current.scrollHeight
  }, [turns, palette, busy])

  useEffect(() => {
    if (open) box.current?.focus()
  }, [open])

  async function send() {
    const text = input.trim()
    if (!text || busy) return

    setBusy(true)
    setError(null)
    // An old recommendation must not stay clickable while it is being revised.
    setPalette([])
    setTurns((current) => [...current, { role: 'user', text }])
    setInput('')

    try {
      const reply = await api.sendAssistantMessage(text, conversationId)
      setConversationId(reply.conversationId)
      setTurns((current) => [...current, { role: 'assistant', text: reply.message }])
      setPalette(reply.palette)
    } catch (cause) {
      if (cause instanceof ApiError) {
        setError(cause.message)
        // An expired conversation can never be resumed; the next send starts one.
        if (cause.status === 404) setConversationId(undefined)
      } else {
        setError('The assistant is not responding. Check the API is running on port 5247.')
      }
    } finally {
      setBusy(false)
    }
  }

  function reset() {
    setConversationId(undefined)
    setTurns([])
    setPalette([])
    setError(null)
    setInput('')
    box.current?.focus()
  }

  if (!open) {
    return (
      <button
        type="button"
        className="btn btn--accent assistant-launcher"
        aria-expanded={false}
        aria-controls="assistant-panel"
        onClick={() => setOpen(true)}
      >
        Colour help
      </button>
    )
  }

  return (
    <section
      id="assistant-panel"
      className="assistant"
      aria-label="Palette assistant"
      onKeyDown={(event) => {
        if (event.key === 'Escape') setOpen(false)
      }}
    >
      <header className="assistant-head">
        <h2>Colour help</h2>
        <div className="assistant-head-side">
          <button
            type="button"
            className="btn btn--ghost btn--small"
            disabled={busy || turns.length === 0}
            onClick={reset}
          >
            New chat
          </button>
          <button
            type="button"
            className="btn btn--icon"
            aria-label="Close palette assistant"
            onClick={() => setOpen(false)}
          >
            ×
          </button>
        </div>
      </header>

      <div className="assistant-log" ref={log} aria-live="polite">
        {turns.length === 0 && <p className="assistant-msg assistant-msg--bot">{OPENER}</p>}

        {turns.map((turn, index) => (
          <p
            key={index}
            className={
              turn.role === 'user'
                ? 'assistant-msg assistant-msg--user'
                : 'assistant-msg assistant-msg--bot'
            }
          >
            {turn.text}
          </p>
        ))}

        {busy && (
          <p className="assistant-msg assistant-msg--bot assistant-msg--wait">Mixing…</p>
        )}

        {palette.length > 0 && (
          <div className="assistant-palette">
            {palette.map(({ paint, role, reason }) => (
              <Pick
                key={paint.id}
                paint={paint}
                role={role}
                reason={reason}
                inOrder={quantities.get(paint.id) ?? 0}
                disabled={busy || orderBusy}
                onAdd={onAdd}
              />
            ))}
          </div>
        )}
      </div>

      {error && (
        <p className="assistant-error" role="alert">
          {error}
        </p>
      )}

      <form
        className="assistant-form"
        onSubmit={(event) => {
          event.preventDefault()
          void send()
        }}
      >
        <label className="assistant-label" htmlFor="assistant-message">
          Describe the job
        </label>
        <textarea
          id="assistant-message"
          ref={box}
          className="assistant-box"
          rows={2}
          maxLength={2000}
          value={input}
          disabled={busy}
          placeholder="Warm neutrals with a green accent, interior plaster…"
          onChange={(event) => setInput(event.target.value)}
          onKeyDown={(event) => {
            // Enter sends; the widget is too small to be a text editor.
            if (event.key === 'Enter' && !event.shiftKey) {
              event.preventDefault()
              void send()
            }
          }}
        />
        <button
          type="submit"
          className="btn btn--accent btn--small assistant-send"
          disabled={busy || input.trim() === ''}
        >
          {busy ? 'Mixing…' : 'Send'}
        </button>
      </form>
    </section>
  )
}

interface PickProps {
  paint: Paint
  role: string
  reason: string
  inOrder: number
  disabled: boolean
  onAdd: (paint: Paint) => Promise<void> | void
}

function Pick({ paint, role, reason, inOrder, disabled, onAdd }: PickProps) {
  const hex = swatchOf(paint.color)
  const maxed = inOrder >= paint.stockQuantity

  return (
    <article className="assistant-pick">
      <div
        className="assistant-swatch"
        style={{ background: hex, color: contrastInk(hex) }}
        aria-hidden="true"
      />
      <div className="assistant-pick-body">
        <p className="assistant-role">{role}</p>
        <h3 className="assistant-pick-name">{paint.name}</h3>
        <p className="assistant-reason">{reason}</p>
        <p className="assistant-pick-meta">
          {paint.brand} · {litres(paint.volumeLiters)} · {price(paint.price)}
          {inOrder > 0 && ` · ${inOrder} in order`}
        </p>
        <button
          type="button"
          className="btn btn--small btn--violet"
          disabled={disabled || maxed}
          onClick={() => void onAdd(paint)}
        >
          {maxed ? `All ${inOrder} added` : 'Add one tin'}
        </button>
      </div>
    </article>
  )
}
