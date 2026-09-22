const money = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })

/** Prices are a single store-defined currency, so the amount carries no symbol. */
export const price = (value: number) => money.format(value)

/** Volumes store 3 decimals; trailing zeroes read as noise on a shelf label. */
export const litres = (value: number) => `${Number(value.toFixed(3))} L`
