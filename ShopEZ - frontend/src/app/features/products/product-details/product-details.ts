import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { Product } from '../../../models/product';
import { Review } from '../../../models/review';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Rating } from '../../../shared/components/rating/rating';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { AlertMessages } from '../../../shared/components/alert-messages/alert-messages';

@Component({
  selector: 'app-product-details',
  standalone:true,
  imports: [CommonModule, RouterModule,FormsModule, ReactiveFormsModule,
    Rating, LoadingSpinner, AlertMessages],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails implements OnInit {
  product: Product | null = null;
  isLoading = true;
  selectedImage = '';
  quantity = 1;
  isWishlisted = false;
  reviews: Review[] = [];
  reviewsLoading = false;
  relatedProducts: Product[] = [];
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' | 'warning' = 'success';
  reviewSubmitting = false;
  reviewForm!: FormGroup;
 
  get currentUser() { return this.authService.getCurrentUser(); }
 
  get quantityOptions(): number[] {
    const max = Math.min(this.product?.stock || 10, 10);
    return Array.from({ length: max }, (_, i) => i + 1);
  }
 
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private fb: FormBuilder,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}
 
  ngOnInit(): void {
    this.reviewForm = this.fb.group({
      rating: [0, [Validators.required, Validators.min(1)]],
      title: ['', [Validators.required, Validators.maxLength(100)]],
      comment: ['', [Validators.required, Validators.minLength(10)]]
    });
 
    this.route.params.subscribe(params => {
      const id = +params['id'];
      this.loadProduct(id);
      this.loadReviews(id);
    });
  }
 
  loadProduct(id: number): void {
    this.isLoading = true;
    this.productService.getProductById(id).subscribe({
      next: product => {
        this.zone.run(() => {
          this.product = product;
          this.selectedImage = product.imageUrl || '';
          this.isWishlisted = this.wishlistService.isInWishlist(product.id);
          this.isLoading = false;
          this.cdr.detectChanges();
          this.loadRelated(id);
        });
      },
      error: () => {
        this.zone.run(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
          this.router.navigate(['/products']);
        });
      }
    });
  }
 
  loadReviews(productId: number): void {
    this.reviewsLoading = true;
    this.productService.getProductReviews(productId).subscribe({
      next: reviews => {
        this.reviews = reviews;
        this.reviewsLoading = false;
      },
      error: () => this.reviewsLoading = false
    });
  }
 
  loadRelated(productId: number): void {
    this.productService.getRelatedProducts(productId).subscribe({
      next: products => this.relatedProducts = products.slice(0, 5),
      error: () => {}
    });
  }
 
  addToCart(): void {
    if (!this.product) return;
    this.cartService.addToCart(this.product, this.quantity);
    this.showAlert(`Added ${this.quantity} × "${this.product.name}" to cart!`, 'success');
  }
 
  buyNow(): void {
    if (!this.product) return;
    this.cartService.addToCart(this.product, this.quantity);
    this.router.navigate(['/checkout']);
  }
 
  toggleWishlist(): void {
    if (!this.product) return;
    this.wishlistService.toggleWishlist(this.product);
    this.isWishlisted = !this.isWishlisted;
    this.showAlert(
      this.isWishlisted ? 'Added to Wishlist' : 'Removed from Wishlist',
      'info'
    );
  }
 
  submitReview(): void {
    if (!this.product || this.reviewForm.invalid) return;
    this.reviewSubmitting = true;
    this.productService.submitReview({
      productId: this.product.id,
      ...this.reviewForm.value
    }).subscribe({
      next: review => {
        this.zone.run(() => {
          this.reviews = [review, ...this.reviews];
          this.reviewForm.reset({ rating: 0, title: '', comment: '' });
          this.reviewSubmitting = false;
          this.showAlert('Review submitted! Thank you.', 'success');
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.zone.run(() => {
          this.reviewSubmitting = false;
          this.showAlert('Failed to submit review. Please try again.', 'danger');
          this.cdr.detectChanges();
        });
      }
    });
  }
 
  onImgError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://placehold.co/400x400/f8f9fa/999?text=No+Image';
  }
 
  private showAlert(message: string, type: typeof this.alertType): void {
    this.alertMessage = message;
    this.alertType = type;
    setTimeout(() => this.alertMessage = '', 3000);
  }
}