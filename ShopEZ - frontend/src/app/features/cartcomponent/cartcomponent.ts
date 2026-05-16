import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Rating } from '../../shared/components/rating/rating';
import { AlertMessages } from '../../shared/components/alert-messages/alert-messages';
import { Cart } from '../../models/cart';
import { Subject, takeUntil } from 'rxjs';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-cartcomponent',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, Rating, AlertMessages],
  templateUrl: './cartcomponent.html',
  styleUrl: './cartcomponent.css',
})
export class Cartcomponent implements OnInit, OnDestroy {
  cart: Cart = { items: [], totalItems: 0, totalPrice: 0 };
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' | 'warning' = 'success';
  private destroy$ = new Subject<void>();
 
  constructor(
    private cartService: CartService,
    private router: Router
  ) {}
 
  ngOnInit(): void {
    this.cartService.cart$
      .pipe(takeUntil(this.destroy$))
      .subscribe(cart => this.cart = cart);
  }
 
  updateQty(productId: number, qty: number): void {
    // quantity must be >= 1
    if (qty < 1) return;
    this.cartService.updateQuantity(productId, qty);
  }
 
  removeItem(productId: number): void {
    this.cartService.removeFromCart(productId);
    this.showAlert('Item removed from cart.', 'info');
  }
 
  saveForLater(productId: number): void {
    // Move to wishlist
    const item = this.cart.items.find(i => i.product.id === productId);
    if (item) {
      this.cartService.removeFromCart(productId);
      this.showAlert('Item saved for later.', 'info');
    }
  }
 
  proceedToCheckout(): void {
    if (this.cart.items.length === 0) return;
    this.router.navigate(['/checkout']);
  }
 
  onImgError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://via.placeholder.com/100x100?text=No+Image';
  }
 
  private showAlert(msg: string, type: typeof this.alertType): void {
    this.alertMessage = msg;
    this.alertType = type;
    setTimeout(() => this.alertMessage = '', 2500);
  }
 
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}