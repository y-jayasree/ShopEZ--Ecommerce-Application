import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AlertMessages } from '../../shared/components/alert-messages/alert-messages';
import { LoadingSpinner } from '../../shared/components/loading-spinner/loading-spinner';
import { Cart } from '../../models/cart';
import { Address, PaymentMethod } from '../../models/order';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { PaymentService } from '../../core/services/payment.service';
import { ImageService } from '../../core/services/image.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule,RouterModule,FormsModule,ReactiveFormsModule,AlertMessages,LoadingSpinner],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class Checkout implements OnInit {
  cart: Cart = { items: [], totalItems: 0, totalPrice: 0 };
  addressForm!: FormGroup;
  savedAddresses: Address[] = [];
  selectedAddressId: number | null = null;
  selectedAddress: Address | null = null;
  currentStep = 1;
  showAddressForm = false;

  // Payment
  selectedPayment: PaymentMethod | '' = '';
  paymentMethod: PaymentMethod = 'CASH_ON_DELIVERY';
  upiId = '';
  upiTouched = false;
  paymentTouched = false;

  promoCode = '';
  isProcessing = false;
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' | 'warning' = 'success';

  readonly indianStates: string[] = [
    'Andhra Pradesh', 'Arunachal Pradesh', 'Assam', 'Bihar', 'Chhattisgarh',
    'Goa', 'Gujarat', 'Haryana', 'Himachal Pradesh', 'Jharkhand', 'Karnataka',
    'Kerala', 'Madhya Pradesh', 'Maharashtra', 'Manipur', 'Meghalaya', 'Mizoram',
    'Nagaland', 'Odisha', 'Punjab', 'Rajasthan', 'Sikkim', 'Tamil Nadu',
    'Telangana', 'Tripura', 'Uttar Pradesh', 'Uttarakhand', 'West Bengal',
    'Andaman and Nicobar Islands', 'Chandigarh',
    'Delhi', 'Jammu and Kashmir', 'Ladakh', 'Lakshadweep', 'Puducherry'
  ];

  constructor(
    private fb:             FormBuilder,
    private cartService:    CartService,
    private orderService:   OrderService,
    private paymentService: PaymentService,
    private router:         Router,
    public  imageService:   ImageService
  ) {}

  ngOnInit(): void {
    this.cartService.cart$.subscribe(c => (this.cart = c));

    if (this.cart.items.length === 0) {
      this.router.navigate(['/cart']);
      return;
    }

    this.addressForm = this.fb.group({
      fullName:     ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      phone:        ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
      addressLine1: ['', [Validators.required, Validators.minLength(5)]],
      addressLine2: [''],
      city:         ['', [Validators.required, Validators.minLength(2)]],
      state:        ['', Validators.required],
      pincode:      ['', [Validators.required, Validators.pattern(/^[1-9][0-9]{5}$/)]],
      isDefault:    [false]
    });

    this.loadAddresses();
  }

  loadAddresses(): void {
    this.orderService.getAddresses().subscribe({
      next: addresses => {
        this.savedAddresses = addresses;
        const def = addresses.find(a => a.isDefault);
        if (def) this.selectAddress(def);
        if (addresses.length === 0) this.showAddressForm = true;
      },
      error: () => {
        this.showAddressForm = true;
      }
    });
  }

  selectAddress(addr: Address): void {
    this.selectedAddressId = addr.id ?? null;
    this.selectedAddress = addr;
    this.addressForm.patchValue(addr);
  }

  saveAddress(): void {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }
    const addr: Address = this.addressForm.value;
    this.selectedAddress = addr;
    this.showAddressForm = false;
  }

  goToStep(step: number): void {
    if (step === 2 && !this.selectedAddress && !this.showAddressForm) {
      this.showAlert('Please select or add a delivery address.', 'danger');
      return;
    }
    if (step === 2 && this.showAddressForm) {
      this.saveAddress();
      if (!this.selectedAddress) return;
    }
    this.currentStep = step;
  }

  goToPaymentStep3(): void {
    this.paymentTouched = true;
    if (!this.selectedPayment) {
      this.showAlert('Please select a payment method.', 'danger');
      return;
    }
    if (this.selectedPayment === 'UPI') {
      this.upiTouched = true;
      if (!this.isValidUpi()) {
        this.showAlert('Please enter a valid UPI ID.', 'danger');
        return;
      }
    }
    this.paymentMethod = this.selectedPayment as PaymentMethod;
    this.currentStep = 3;
  }

  isValidUpi(): boolean {
    // Valid UPI formats
    return /^[\w.\-]{3,}@[a-zA-Z]{3,}$/.test(this.upiId.trim());
  }

  get shippingCost(): number {
    return this.cart.totalPrice >= 499 ? 0 : 49;
  }

  get grandTotal(): number {
    return this.cart.totalPrice + this.shippingCost;
  }

  placeOrder(): void {
    if (!this.selectedAddress) {
      this.showAlert('Please select a delivery address.', 'danger');
      this.currentStep = 1;
      return;
    }

    this.isProcessing = true;

    const payload = {
      items: this.cart.items.map(i => ({
        productId:   i.product.id,
        productName: i.product.name,
        quantity:    i.quantity,
        unitPrice:   i.product.price,
        imageUrl:    i.product.imageUrl  
      })),
      shippingAddress: this.selectedAddress
        ? `${this.selectedAddress.addressLine1}, ${this.selectedAddress.addressLine2 ? this.selectedAddress.addressLine2 + ', ' : ''}${this.selectedAddress.city}, ${this.selectedAddress.state} - ${this.selectedAddress.pincode}`
        : '',
      paymentMethod: this.paymentMethod
    };

    this.orderService.placeOrder(payload as any).subscribe({
      next: () => {
        this.cartService.clearCart();
        this.isProcessing = false;
        this.router.navigate(['/orders'], { queryParams: { placed: 'true' } });
      },
      error: err => {
        this.isProcessing = false;
        this.showAlert(err?.error?.message || 'Order placement failed. Please try again.', 'danger');
      }
    });
  }

  applyPromo(): void {
    if (!this.promoCode) return;
    this.showAlert('Promo code applied (demo only).', 'info');
  }

  isAddrInvalid(field: string): boolean {
    const ctrl = this.addressForm.get(field);
    return !!(ctrl?.invalid && (ctrl.dirty || ctrl.touched));
  }

  private showAlert(msg: string, type: typeof this.alertType): void {
    this.alertMessage = msg;
    this.alertType = type;
    setTimeout(() => (this.alertMessage = ''), 5000);
  }
}