/**
 * The catalog stores `color` as a free-text name, so the swatch has to be
 * derived from it. Known decorating names get a hand-picked hex; anything else
 * gets a stable hue from a hash, keeping a paint the same colour on every load.
 */
const NAMED: Record<string, string> = {
  white: '#F2EFE8', 'pure white': '#F7F5F0', 'brilliant white': '#FAF9F6',
  magnolia: '#F2E9D8', ivory: '#EFE6D2', cream: '#EDE0C4', linen: '#E8DFCE',
  beige: '#D9CBB2', taupe: '#B7A894', stone: '#C6BCA8', sand: '#D6C6A8',
  clay: '#B08968', terracotta: '#B85C38', rust: '#9C4A24', brick: '#8E3B2E',
  brown: '#6B4B37', chocolate: '#4A3225', mocha: '#7A6154',
  black: '#17171A', graphite: '#33363B', charcoal: '#3A3D42', slate: '#4B5561',
  grey: '#8C8F94', gray: '#8C8F94', 'light grey': '#C3C6CA', 'dark grey': '#55585D',
  silver: '#BFC3C7', dove: '#CFCDC6', pebble: '#B4B0A6',
  red: '#B3242B', crimson: '#8E1C2B', burgundy: '#6B2233', wine: '#5E2333',
  pink: '#DDA2AE', blush: '#E3BDB8', rose: '#C97B84', coral: '#D96C57',
  orange: '#D2701F', amber: '#C8871F', apricot: '#E0A46C',
  yellow: '#DFBE3E', mustard: '#C39A22', ochre: '#B98B2A', lemon: '#E7D66A',
  green: '#3E6B45', sage: '#9AA98C', olive: '#6E7143', forest: '#2C4A34',
  mint: '#AFCFBA', moss: '#69784C', emerald: '#1F6B4E', lime: '#9DBF4A',
  blue: '#2B5FA8', navy: '#1E2A47', teal: '#26686E', cyan: '#3E9BA8',
  sky: '#8FB8D8', duckegg: '#A9C6C4', 'duck egg': '#A9C6C4',
  cobalt: '#1F46A8', ultramarine: '#2B34C4', denim: '#3B5C82',
  purple: '#5B3C79', lilac: '#B8A7CC', lavender: '#A99BC1',
  plum: '#5B2E45', aubergine: '#432A3E',
  gold: '#B48A2E', bronze: '#8A6A3B', copper: '#A5623C',
}

/** Text colour that stays readable on `hex`, using the sRGB luminance rule. */
export function contrastInk(hex: string): string {
  const n = parseInt(hex.slice(1), 16)
  const channels = [(n >> 16) & 255, (n >> 8) & 255, n & 255].map((c) => {
    const s = c / 255
    return s <= 0.04045 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4
  })
  const luminance = 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2]
  return luminance > 0.45 ? '#191A17' : '#F6F5F2'
}

function hashHue(value: string): string {
  let hash = 0
  for (let i = 0; i < value.length; i++) hash = (hash * 31 + value.charCodeAt(i)) | 0
  const hue = Math.abs(hash) % 360
  const saturation = 24 + (Math.abs(hash >> 8) % 26)
  const lightness = 42 + (Math.abs(hash >> 16) % 26)
  return hslToHex(hue, saturation, lightness)
}

function hslToHex(h: number, s: number, l: number): string {
  const a = (s / 100) * Math.min(l / 100, 1 - l / 100)
  const channel = (n: number) => {
    const k = (n + h / 30) % 12
    const value = l / 100 - a * Math.max(-1, Math.min(k - 3, 9 - k, 1))
    return Math.round(value * 255).toString(16).padStart(2, '0')
  }
  return `#${channel(0)}${channel(8)}${channel(4)}`
}

export function swatchOf(colorName: string): string {
  const key = colorName.trim().toLowerCase()
  if (NAMED[key]) return NAMED[key]
  // "Soft Sage Green" should still land on sage rather than a random hue.
  const word = Object.keys(NAMED).find((name) => key.includes(name))
  return word ? NAMED[word] : hashHue(key)
}
