import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../../models/product'; 
import { WishlistItem,WishlistState } from '../../models/wishlist'; 

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly KEY = 'shopez_wishlist';

  private wishlistSubject = new BehaviorSubject<WishlistState>(this.load());
  wishlist$ = this.wishlistSubject.asObservable();

  isInWishlist(productId: number): boolean {
    return this.wishlistSubject.value.items.some(i => i.product.id === productId);
  }

  toggleWishlist(product: Product): void {
    this.toggle(product);
  }

  toggle(product: Product): void {
    const current = { ...this.wishlistSubject.value };
    const idx = current.items.findIndex(i => i.product.id === product.id);

    if (idx > -1) {
      current.items = current.items.filter((_: WishlistItem, i: number) => i !== idx);
    } else {
      current.items = [
        ...current.items,
        { id: Date.now(), product, addedAt: new Date().toISOString() }
      ];
    }

    this.save(current);
  }

  remove(productId: number): void {
    const current = { ...this.wishlistSubject.value };
    current.items = current.items.filter((i: WishlistItem) => i.product.id !== productId);
    this.save(current);
  }

  getItems(): WishlistItem[] {
    return this.wishlistSubject.value.items;
  }

  clear(): void {
    this.save({ items: [] });
  }

  private save(state: WishlistState): void {
    this.wishlistSubject.next(state);
    localStorage.setItem(this.KEY, JSON.stringify(state));
  }

  private load(): WishlistState {
    try {
      const stored = localStorage.getItem(this.KEY);
      return stored ? JSON.parse(stored) : { items: [] };
    } catch {
      return { items: [] };
    }
  }
}