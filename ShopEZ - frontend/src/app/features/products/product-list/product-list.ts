import { Component, OnDestroy, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { PagedResponse, Product, ProductFilter } from '../../../models/product';
import { Subject, takeUntil } from 'rxjs';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Rating } from '../../../shared/components/rating/rating';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { AlertMessages } from '../../../shared/components/alert-messages/alert-messages';
import { FilterSidebar } from '../filter-sidebar/filter-sidebar';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, Rating, LoadingSpinner, AlertMessages, FilterSidebar],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css',
})
export class ProductList implements OnInit, OnDestroy {
  allProducts: Product[] = [];  
  products: Product[] = [];    

  isLoading = true;
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' | 'warning' = 'success';

  // currentFilter holds ALL active filters in one place.
  currentFilter: ProductFilter = { page: 0, size: 20 };

  keyword = '';
  totalProducts = 0;
  totalPages = 0;
  currentPage = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private route: ActivatedRoute,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    this.keyword = params['keyword'] || '';
    const urlCategoryId = params['category'] ? +params['category'] : undefined;
    this.currentFilter = {
      ...this.currentFilter,
      keyword:    this.keyword || undefined,
      categoryId: urlCategoryId,
      page:       0
    };

    // Load all products ONCE from backend
    this.loadAllProducts();

    // Watch for subsequent URL param changes 
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.keyword = params['keyword'] || '';
      const urlCategoryId = params['category'] ? +params['category'] : undefined;
      this.currentFilter = {
        ...this.currentFilter,
        keyword:    this.keyword || undefined,
        categoryId: urlCategoryId,
        page:       0
      };
      // Only re-filter if products already loaded 
      if (!this.isLoading && this.allProducts.length > 0) {
        this.applyFilters();
      }
    });
  }

  loadAllProducts(): void {
    this.isLoading    = true;
    this.totalProducts = 0;   //  reset 0 products doesn't flash while loading

    // safety timeout — if backend hangs for 10s, stop spinner so page isn't frozen
    const timeoutId = setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.showAlert('Server is taking too long to respond. Please try again.', 'danger');
      }
    }, 10000);

    this.productService.getProducts({}).subscribe({
      next: (res: PagedResponse<Product>) => {
        clearTimeout(timeoutId);
        this.zone.run(() => {
          this.allProducts = res.content ?? [];
          console.log('[loadAllProducts] got', this.allProducts.length, 'products, setting isLoading=false');
          this.isLoading = false;
          this.applyFilters();
          this.cdr.detectChanges();
        });
      },
      error: () => {
        clearTimeout(timeoutId);
        this.zone.run(() => {
          this.isLoading = false;
          this.showAlert('Could not load products. Is your backend running?', 'danger');
          this.cdr.detectChanges();
        });
      }
    });
  }

  applyFilters(): void {
    if (this.isLoading) return;
    console.log('[applyFilters] allProducts:', this.allProducts.length, 'isLoading:', this.isLoading, 'filter:', JSON.stringify(this.currentFilter));

    let filtered = [...this.allProducts];

    // Category filter
    if (this.currentFilter.categoryId) {
      filtered = filtered.filter(p => p.categoryId === this.currentFilter.categoryId);
    }

    // Keyword search
    if (this.currentFilter.keyword) {
      const kw = this.currentFilter.keyword.toLowerCase();
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(kw) ||
        p.description.toLowerCase().includes(kw)
      );
    }

    // Price filters
    if (this.currentFilter.minPrice !== undefined) {
      filtered = filtered.filter(p => p.price >= this.currentFilter.minPrice!);
    }
    if (this.currentFilter.maxPrice !== undefined) {
      filtered = filtered.filter(p => p.price <= this.currentFilter.maxPrice!);
    }

    // Rating filter
    if (this.currentFilter.minRating !== undefined) {
      filtered = filtered.filter(p => p.rating >= this.currentFilter.minRating!);
    }

    // Sorting
    if (this.currentFilter.sortBy === 'price_asc')  filtered.sort((a, b) => a.price - b.price);
    if (this.currentFilter.sortBy === 'price_desc') filtered.sort((a, b) => b.price - a.price);
    if (this.currentFilter.sortBy === 'rating')     filtered.sort((a, b) => b.rating - a.rating);

    this.products      = filtered;
    this.totalProducts = filtered.length;
    this.totalPages    = 1;
    this.currentPage   = 0;
  }

  // Called when the sidebar emits a filter change
  onFilterChange(filter: ProductFilter): void {
    this.currentFilter = {
      ...this.currentFilter,
      ...filter,
      categoryId: filter.categoryId,
      page: 0
    };
    this.applyFilters();
  }

  addToCart(product: Product): void {
    if (product.stock === 0) return;
    this.cartService.addToCart(product);
    this.showAlert(`"${product.name}" added to cart!`, 'success');
  }

  toggleWishlist(product: Product, event: Event): void {
    event.preventDefault();
    this.wishlistService.toggleWishlist(product);
  }

  isInCart(productId: number): boolean     { return this.cartService.isInCart(productId); }
  isWishlisted(productId: number): boolean { return this.wishlistService.isInWishlist(productId); }

  getDiscount(product: Product): number {
    if (!product.originalPrice || product.originalPrice <= product.price) return 0;
    return Math.round((1 - product.price / product.originalPrice) * 100);
  }

  getImageUrl(product: Product): string {
    if (product.imageUrl && product.imageUrl.trim() !== '') return product.imageUrl;
    return `https://placehold.co/200x200/f8f9fa/999?text=${encodeURIComponent(product.name.slice(0, 10))}`;
  }

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (!img.dataset['fallback']) {
      img.dataset['fallback'] = 'true';
      img.src = 'https://placehold.co/200x200/f8f9fa/999?text=ShopEZ';
    }
  }

  goToPage(page: number): void {
    if (page < 0 || page >= this.totalPages) return;
    this.currentFilter = { ...this.currentFilter, page };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    for (let i = 0; i < this.totalPages; i++) pages.push(i);
    return pages;
  }

  private showAlert(message: string, type: typeof this.alertType): void {
    this.alertMessage = message;
    this.alertType    = type;
    setTimeout(() => this.alertMessage = '', 4000);
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}