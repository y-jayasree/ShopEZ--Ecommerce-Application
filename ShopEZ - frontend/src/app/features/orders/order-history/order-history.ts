import { CommonModule } from '@angular/common';
import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { AlertMessages } from '../../../shared/components/alert-messages/alert-messages';
import { Order } from '../../../models/order';
import { OrderService } from '../../../core/services/order.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, LoadingSpinner, AlertMessages],
  templateUrl: './order-history.html',
  styleUrl: './order-history.css',
})
export class OrderHistory implements OnInit {
  orders: Order[] = [];
  isLoading = false;
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' | 'warning' = 'success';
  currentPage = 0;
  totalPages = 1;
  justPlaced = false;

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.justPlaced = this.route.snapshot.queryParams['placed'] === 'true';
    this.loadPage(0);
  }

  loadPage(page: number): void {
    if (page < 0) return;
    this.isLoading = true;
    this.orderService.getMyOrders(page, 10).subscribe({
      next: res => {
        this.zone.run(() => {
          this.orders = res.content ?? [];
          this.totalPages = res.totalPages || 1;
          this.currentPage = res.number ?? 0;
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.zone.run(() => {
          this.showAlert('Failed to load orders.', 'danger');
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  cancelOrder(order: Order): void {
    if (!confirm('Are you sure you want to cancel this order?')) return;
    this.zone.run(() => {
      const idx = this.orders.findIndex(o => o.id === order.id);
      if (idx > -1) {
        this.orders[idx] = { ...this.orders[idx], status: 'CANCELLED' };
        this.orders = [...this.orders];
      }
      this.showAlert('Order cancellation requested. Our team will process it shortly.', 'success');
      this.cdr.detectChanges();
    });
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      PENDING:   'Order Placed',
      CONFIRMED: 'Order Confirmed',
      SHIPPED:   'Shipped',
      DELIVERED: 'Delivered',
      CANCELLED: 'Cancelled'
    };
    return map[status] || status;
  }

  get pageArray(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i);
  }

  onImgError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://placehold.co/80x80/f8f9fa/999?text=?';
  }

  private showAlert(msg: string, type: typeof this.alertType): void {
    this.alertMessage = msg;
    this.alertType = type;
    setTimeout(() => this.alertMessage = '', 4000);
  }
}