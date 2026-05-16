import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';

export interface PaymentResult {
  success: boolean;
  transactionId?: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {

  processPayment(amount: number, method: string): Observable<PaymentResult> {
    const txId = `TXN-${Date.now()}`;
    return of({
      success: true,
      transactionId: txId,
      message: `Payment of ₹${amount.toFixed(2)} processed via ${method}`
    }).pipe(delay(1000));
  }

  getSupportedMethods(): string[] {
    return ['CASH_ON_DELIVERY', 'UPI'];
  }
}