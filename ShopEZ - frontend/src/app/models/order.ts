import { Product } from "./product";

export type OrderStatus = 'PENDING' | 'CONFIRMED' | 'SHIPPED' | 'DELIVERED' | 'CANCELLED';
export type PaymentMethod = 'CASH_ON_DELIVERY' | 'UPI' | 'CARD';
export type PaymentStatus = 'PENDING' | 'PAID' | 'FAILED';
 
export interface OrderItem {
  id?: number;
  product: Product;
  quantity: number;
  price: number;
}
 
export interface Address {
  id?: number;
  fullName: string;
  phone: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  pincode: string;
  isDefault?: boolean;
}
 
export interface Order {
  id: number;
  orderNumber: string;
  items: OrderItem[];
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  shippingAddress: Address;
  subtotal: number;
  shippingCost: number;
  total: number;
  createdAt: string;
  updatedAt: string;
  estimatedDelivery?: string;
}
 
export interface PlaceOrderRequest {
  items: { productId: number; quantity: number; price: number }[];
  shippingAddress: Address;
  paymentMethod: PaymentMethod;
}