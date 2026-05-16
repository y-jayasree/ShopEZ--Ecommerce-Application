import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Cart, CartItem } from '../../models/cart';
import { Product } from '../../models/product';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly CART_KEY = 'shopez_cart';

  private cartSubject = new BehaviorSubject<Cart>(this.loadLocalCart());
  cart$ = this.cartSubject.asObservable();

  //  Getters 
  getCart(): Cart {
    return this.cartSubject.value;
  }

  getItemCount(): number {
    return this.cartSubject.value.totalItems;
  }

  //  Add to cart 
  addToCart(product: Product, quantity = 1): void {
    const cart = { ...this.cartSubject.value };
    const existingIndex = cart.items.findIndex(i => i.product.id === product.id);

    if (existingIndex > -1) {
      cart.items = cart.items.map((item, idx) =>
        idx === existingIndex
          ? { ...item, quantity: item.quantity + quantity }
          : item
      );
    } else {
      cart.items = [...cart.items, { product, quantity }];
    }

    this.updateCartState(cart);
  }

  //  Remove from cart 
  removeFromCart(productId: number): void {
    const cart = { ...this.cartSubject.value };
    cart.items = cart.items.filter(i => i.product.id !== productId);
    this.updateCartState(cart);
  }

  //  Update quantity 
  updateQuantity(productId: number, quantity: number): void {
    if (quantity < 1) {
      this.removeFromCart(productId);
      return;
    }
    const cart = { ...this.cartSubject.value };
    cart.items = cart.items.map(i =>
      i.product.id === productId ? { ...i, quantity } : i
    );
    this.updateCartState(cart);
  }

  //  Clear cart 
  clearCart(): void {
    const emptyCart: Cart = { items: [], totalItems: 0, totalPrice: 0 };
    this.cartSubject.next(emptyCart);
    localStorage.removeItem(this.CART_KEY);
  }

  //  Check if product in cart 
  isInCart(productId: number): boolean {
    return this.cartSubject.value.items.some(i => i.product.id === productId);
  }

  //  Private helpers 
  private updateCartState(cart: Cart): void {
    cart.totalItems = cart.items.reduce((sum, i) => sum + i.quantity, 0);
    cart.totalPrice = cart.items.reduce((sum, i) => sum + i.product.price * i.quantity, 0);
    this.cartSubject.next({ ...cart });
    localStorage.setItem(this.CART_KEY, JSON.stringify(cart));
  }

  private loadLocalCart(): Cart {
    try {
      const stored = localStorage.getItem(this.CART_KEY);
      return stored ? JSON.parse(stored) : { items: [], totalItems: 0, totalPrice: 0 };
    } catch {
      return { items: [], totalItems: 0, totalPrice: 0 };
    }
  }
}