import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';

export interface AdminOrder {
  orderId: number;
  userId: number;
  orderNumber: string;
  orderDate: string;
  totalAmount: number;
  status: string;
  shippingAddress?: string;
  paymentMethod: string;
  items: any[];
}

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-orders.html',
  styleUrl: './admin-orders.css'
})
export class AdminOrders implements OnInit {
  orders: AdminOrder[] = [];
  loading = true;
  error = '';
  successMessage = '';
  updateError = '';

  readonly statuses = ['PENDING', 'CONFIRMED', 'SHIPPED', 'DELIVERED', 'CANCELLED'];

  constructor(
    private adminService: AdminService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading = true;
    this.adminService.getAllOrders().subscribe({
      next: (data: any[]) => {
        this.orders = data.map(o => ({
          orderId:         o.orderId       ?? o.id ?? 0,
          userId:          o.userId        ?? 0,
          orderNumber:     o.orderNumber   ?? ('#' + (o.orderId ?? o.id ?? 0)),
          orderDate:       o.orderDate     ?? o.createdAt ?? '',
          totalAmount:     o.totalAmount   ?? o.total ?? 0,
          status:          o.status        ?? 'PENDING',
          shippingAddress: o.shippingAddress,
          paymentMethod:   o.paymentMethod ?? '',
          items:           o.items         ?? []
        }));
        this.loading = false;
      },
      error: () => { this.error = 'Failed to load orders'; this.loading = false; }
    });
  }

  updateStatus(orderId: number, status: string) {
    if (!status) return;
    this.updateError = '';
    this.adminService.updateOrderStatus(orderId, status).subscribe({
      next: () => {
        this.successMessage = `Order status updated to ${status}`;
        setTimeout(() => this.successMessage = '', 3000);
        this.loadOrders();
      },
      error: (err) => {
        this.updateError = err?.error?.message ?? 'Failed to update status. Please try again.';
        setTimeout(() => this.updateError = '', 4000);
      }
    });
  }

  goToDashboard() {
    this.router.navigate(['/admin']);
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      PENDING:   'badge-yellow',
      CONFIRMED: 'badge-blue',
      SHIPPED:   'badge-purple',
      DELIVERED: 'badge-green',
      CANCELLED: 'badge-red'
    };
    return map[status] ?? 'badge-gray';
  }
}