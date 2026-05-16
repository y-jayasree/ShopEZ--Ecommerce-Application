import { CommonModule } from '@angular/common';
import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { Order, OrderStatus } from '../../../models/order';
import { OrderService } from '../../../core/services/order.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule, RouterLink,FormsModule, LoadingSpinner],
  templateUrl: './order-tracking.html',
  styleUrl: './order-tracking.css',
})
export class OrderTracking implements OnInit {
  order: Order | null = null;
  isLoading = true;
 
  timelineSteps = [
    { status: 'PENDING'  as OrderStatus, label: 'Order Placed',    icon: 'bi-bag-check',     description: 'We received your order' },
    { status: 'CONFIRMED'as OrderStatus, label: 'Order Confirmed', icon: 'bi-check-circle',  description: 'Your order is confirmed' },
    { status: 'SHIPPED'  as OrderStatus, label: 'Shipped',         icon: 'bi-truck',          description: 'Your package is on the way' },
    { status: 'DELIVERED'as OrderStatus, label: 'Delivered',       icon: 'bi-house-check',    description: 'Package delivered to you' }
  ];
 
  private statusOrder: OrderStatus[] = ['PENDING','CONFIRMED','SHIPPED','DELIVERED'];
 
  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}
 
  ngOnInit(): void {
    const id = +this.route.snapshot.params['id'];
    this.orderService.getOrderById(id).subscribe({
      next: order => {
        this.zone.run(() => {
          this.order = order;
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.zone.run(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }
 
  isStepCompleted(status: OrderStatus): boolean {
    if (!this.order) return false;
    if (this.order.status === 'CANCELLED') return false;
    return this.statusOrder.indexOf(this.order.status) >= this.statusOrder.indexOf(status);
  }
 
  statusLabel(status: string): string {
    const map: Record<string, string> = {
      PENDING: 'Order Placed', CONFIRMED: 'Confirmed',
      SHIPPED: 'Shipped', DELIVERED: 'Delivered', CANCELLED: 'Cancelled'
    };
    return map[status] || status;
  }
 
  paymentLabel(method: string): string {
    const map: Record<string, string> = {
      CASH_ON_DELIVERY: 'Cash on Delivery', UPI: 'UPI', CARD: 'Card'
    };
    return map[method] || method;
  }
 
  onImgError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://placehold.co/80x80/f8f9fa/999?text=?';
  }
}