import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { Product } from '../../models/product';
import { Category } from '../../models/category';
import { Rating } from '../../shared/components/rating/rating';
import { LoadingSpinner } from '../../shared/components/loading-spinner/loading-spinner';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, Rating, LoadingSpinner],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {

  featuredProducts: Product[] = [];
  categories: Category[] = [];
  apiNote = environment.apiUrl;

  isLoadingFeatured = true;
  isLoadingCategories = true;

  defaultCategories: Category[] = [
    { id: 1, name: 'Electronics' },
    { id: 2, name: 'Fashion' },
    { id: 3, name: 'Home & Kitchen' },
    { id: 4, name: 'Sports' },
    { id: 5, name: 'Books' },
    { id: 6, name: 'Beauty' },
    { id: 8, name: 'Toys' },
    { id: 7, name: 'Grocery' },
  ];

  categoryIcons: Record<string, string> = {
    'Electronics': 'bi-laptop',
    'Fashion': 'bi-bag',
    'Home & Kitchen': 'bi-house',
    'Sports': 'bi-trophy',
    'Books': 'bi-book',
    'Beauty': 'bi-stars',
    'Toys': 'bi-controller',
    'Grocery': 'bi-basket',
  };

  categoryColors: Record<string, string> = {
    'Electronics': '#0dcaf0',
    'Fashion': '#fd7e14',
    'Home & Kitchen': '#20c997',
    'Sports': '#0d6efd',
    'Books': '#6f42c1',
    'Beauty': '#e83e8c',
    'Toys': '#ffc107',
    'Grocery': '#198754',
  };

  constructor(
    private productService: ProductService,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {

    this.productService.getCategories().subscribe({
      next: cats => {
        this.categories = cats.length > 0 ? cats.slice(0, 8) : this.defaultCategories;
        this.isLoadingCategories = false;
      },
      error: () => {
        this.categories = this.defaultCategories;
        this.isLoadingCategories = false;
      }
    });

    const timeoutId = setTimeout(() => {
      if (this.isLoadingFeatured) {
        this.isLoadingFeatured = false;
      }
    }, 8000);

    this.productService.getProducts({ page: 0, size: 10 }).subscribe({
      next: res => {
        clearTimeout(timeoutId);

        this.zone.run(() => {
          this.featuredProducts = res.content ?? [];
          this.isLoadingFeatured = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        clearTimeout(timeoutId);

        this.zone.run(() => {
          this.isLoadingFeatured = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  getCategoryIcon(name: string): string {
    return this.categoryIcons[name] || 'bi-tag';
  }

  getCategoryColor(name: string): string {
    return this.categoryColors[name] || '#6c757d';
  }

  getDiscount(p: Product): number {
    if (!p.originalPrice || p.originalPrice <= p.price) return 0;

    return Math.round((1 - p.price / p.originalPrice) * 100);
  }

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'https://placehold.co/200x200/f8f9fa/999?text=ShopEZ';
  }
}