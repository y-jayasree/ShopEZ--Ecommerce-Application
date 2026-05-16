import { Product } from './product';

export interface WishlistItem {
  id: number;
  product: Product;
  addedAt: string;
}

export interface WishlistState {
  items: WishlistItem[];
}