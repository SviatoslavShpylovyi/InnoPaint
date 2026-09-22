// Mirrors src/Innowise.Application/StoreContracts.cs and Innowise.Domain/Paint.cs.
// ASP.NET Core serialises with camelCase, hence the casing here.

export interface Paint {
  id: string
  name: string
  brand: string
  description: string
  color: string
  finish: string
  surface: string
  isExterior: boolean
  volumeLiters: number
  price: number
  stockQuantity: number
}

export interface PaintPage {
  items: Paint[]
  totalCount: number
  page: number
  pageSize: number
}

export interface PaintLookups {
  brands: string[]
  colors: string[]
  finishes: string[]
  surfaces: string[]
}

export interface BasketLine {
  paint: Paint
  quantity: number
  lineTotal: number
}

export interface BasketView {
  id: string
  items: BasketLine[]
  itemCount: number
  total: number
}

export type SortBy = 'name' | 'brand' | 'price' | 'volume'

/** Query string for GET /api/paints. Every field is optional. */
export interface PaintQuery {
  search?: string
  brand?: string
  color?: string
  finish?: string
  surface?: string
  isExterior?: boolean
  inStock?: boolean
  minPrice?: number
  maxPrice?: number
  minVolumeLiters?: number
  maxVolumeLiters?: number
  sortBy?: SortBy
  descending?: boolean
  page?: number
  pageSize?: number
}

/** Body for POST/PUT /api/paints. */
export type PaintRequest = Omit<Paint, 'id'>
