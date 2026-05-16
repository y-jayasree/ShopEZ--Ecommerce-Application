import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, Address } from '../../models/order';
import { PagedResponse } from '../../models/product';

//  Shape the backend returns for order item 
interface BackendOrderItem {
  productId:    number;
  productName:  string;
  quantity:     number;
  price:        number;
  subtotal:     number;
  imageUrl?:    string;   
  productImage?: string;  // alternate field name
}

//  Shape the backend returns for order 
interface BackendOrder {
  orderId:     number;
  userId:      number;
  orderDate:   string;
  totalAmount: number;
  items:       BackendOrderItem[];
}

interface BackendOrderResponse {
  message: string;
  data:    BackendOrder;
}

interface BackendOrderListResponse {
  message: string;
  count:   number;
  data:    BackendOrder[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl     = environment.apiUrl;
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  //  Map backend order - frontend Order 

  private resolveOrderImage(url: string): string {
    if (!url || url.trim() === '') return 'https://placehold.co/80x80/f8f9fa/999?text=?';
    if (url.startsWith('http')) return url;
    return `${this.apiBaseUrl}${url}`;
  }

  private mapOrder(bo: any): Order {
    const items = (bo.items ?? []).map((i: any) => ({
      product: {
        id:          i.productId   ?? i.ProductId   ?? 0,
        name:        i.productName ?? i.ProductName ?? '',
        description: '',
        price:       i.price       ?? i.Price       ?? 0,
        imageUrl:    this.resolveOrderImage(i.imageUrl ?? i.productImage ?? i.ImageUrl ?? ''),
        stock:       0,
        category:    { id: 0, name: '' },
        rating:      0,
        reviewCount: 0,
        isActive:    true
      },
      quantity: i.quantity,
      price:    i.price,
      subtotal: i.subtotal ?? i.price * i.quantity
    }));

    return {
      id:           bo.orderId,
      orderNumber:  `ORD-${String(bo.orderId).padStart(5, '0')}`,
      items,
      status:        'PENDING',
      paymentMethod: 'CASH_ON_DELIVERY',
      paymentStatus: 'PENDING',
      shippingAddress: {
        fullName:     '',
        phone:        '',
        addressLine1: '',
        city:         '',
        state:        '',
        pincode:      ''
      },
      subtotal:   bo.totalAmount,
      shippingCost: 0,
      total:      bo.totalAmount,
      createdAt:  bo.orderDate,
      updatedAt:  bo.orderDate
    };
  }

  //Accept any payload and forward to backend.
  placeOrder(request: any): Observable<Order> {
    return this.http
      .post<BackendOrderResponse>(`${this.apiUrl}/orders`, request)
      .pipe(map(res => this.mapOrder(res.data)));
  }

  //  Customer own orders 
  getMyOrders(page = 0, size = 10): Observable<PagedResponse<Order>> {
    return this.http
      .get<any>(`${this.apiUrl}/orders/my-orders`)
      .pipe(
        map(res => {
          let rawList: any[] = [];
          if (Array.isArray(res))             rawList = res;
          else if (Array.isArray(res?.data))   rawList = res.data;
          else if (Array.isArray(res?.items))  rawList = res.items;
          else if (Array.isArray(res?.orders)) rawList = res.orders;
          const orders = rawList.map(o => this.mapOrder(o));
          return {
            content:       orders,
            totalElements: orders.length,
            totalPages:    1,
            size:          orders.length,
            number:        0
          } as PagedResponse<Order>;
        })
      );
  }

  //  Get single order by ID 
  getOrderById(id: number): Observable<Order> {
    return this.http
      .get<BackendOrderResponse>(`${this.apiUrl}/orders/${id}`)
      .pipe(map(res => this.mapOrder(res.data)));
  }

  //cancel order 
  cancelOrder(id: number): Observable<Order> {
    return this.getOrderById(id);
  }

  //  Address management 
  getAddresses(): Observable<Address[]>                        { return of([]); }
  addAddress(address: Address): Observable<Address>            { return of({ ...address, id: Date.now() }); }
  updateAddress(id: number, address: Address): Observable<Address> { return of({ ...address, id }); }
  deleteAddress(id: number): Observable<void>                  { return of(undefined as void); }
  setDefaultAddress(id: number): Observable<Address>           { return of({ id } as Address); }

  //  Admin: Get all orders 
  getAllOrders(page = 0, size = 20): Observable<PagedResponse<Order>> {
    return this.http
      .get<BackendOrderListResponse>(`${this.apiUrl}/orders/all-orders`)
      .pipe(
        map(res => {
          const orders = (res.data ?? []).map(o => this.mapOrder(o));
          return {
            content:       orders,
            totalElements: orders.length,
            totalPages:    1,
            size:          orders.length,
            number:        0
          } as PagedResponse<Order>;
        })
      );
  }

  // updateOrderStatus
  updateOrderStatus(id: number, status: string): Observable<Order> {
    return this.getOrderById(id);
  }
}