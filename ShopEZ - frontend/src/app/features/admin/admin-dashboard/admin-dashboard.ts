import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map, shareReplay } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface DashboardStats {
  totalUsers: number;
  totalProducts: number;
  totalOrders: number;
  totalRevenue: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  stats: DashboardStats = { totalUsers: 0, totalProducts: 0, totalOrders: 0, totalRevenue: 0 };
  loading = true;
  constructor(private http: HttpClient) {}

  ngOnInit() {
    const api = environment.apiUrl;

    const users$ = this.http.get<any>(`${api}/auth/users`).pipe(
      map(res => res?.count ?? (Array.isArray(res?.data) ? res.data.length : 0)),
      catchError(() => of(0))
    );

    const products$ = this.http.get<any>(`${api}/products?pageSize=1`).pipe(
      map(res => res?.count ?? res?.data?.totalCount ?? 0),
      catchError(() => of(0))
    );

    //  count and revenue calculations.
    const ordersRaw$ = this.http.get<any>(`${api}/orders/all-orders`).pipe(
      shareReplay(1),
      catchError(() => of({ count: 0, data: [] }))
    );

    const orders$ = ordersRaw$.pipe(
      map(res => res?.count ?? (Array.isArray(res?.data) ? res.data.length : 0))
    );

    const revenue$ = ordersRaw$.pipe(
      map(res => {
        const list: any[] = Array.isArray(res?.data) ? res.data : [];
        return list.reduce((sum: number, o: any) => sum + (o.totalAmount ?? 0), 0);
      })
    );

    forkJoin({ users: users$, products: products$, orders: orders$, revenue: revenue$ }).subscribe({
      next: r => {
        this.stats = {
          totalUsers:    r.users,
          totalProducts: r.products,
          totalOrders:   r.orders,
          totalRevenue:  r.revenue
        };
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}