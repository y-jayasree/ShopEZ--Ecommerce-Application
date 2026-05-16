import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment'; 
import { AdminStats, AdminUser } from '../../models/admin'; 
import { Order } from '../../models/order';
import { ApiResponse } from '../../models/ApiResponse'; 

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<ApiResponse<AdminUser[]>>(`${this.api}/auth/users`).pipe(
      map(res => res.data ?? [])
    );
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.api}/orders/all-orders`).pipe(
      map(res => res.data ?? [])
    );
  }

  updateOrderStatus(orderId: number, status: string): Observable<any> {
    return this.http.patch(`${this.api}/orders/${orderId}/status`, { status });
  }

  getStats(): Observable<AdminStats> {
    return this.http.get<ApiResponse<AdminStats>>(`${this.api}/auth/users`).pipe(
      map(res => ({
        totalUsers:    res.count ?? 0,
        totalProducts: 0,
        totalOrders:   0,
        totalRevenue:  0
      }))
    );
  }
}