import { Component, EventEmitter, Input, OnInit, Output, ChangeDetectorRef } from '@angular/core';
import { ProductFilter } from '../../../models/product';
import { Category } from '../../../models/category';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-filter-sidebar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './filter-sidebar.html',
  styleUrl: './filter-sidebar.css',
})
export class FilterSidebar implements OnInit {

  @Input() initialFilter: ProductFilter = {};
  @Output() filterChange = new EventEmitter<ProductFilter>();

  // categories list and selectedCategoryId
  categories: Category[] = [];
  selectedCategoryId?: number;

  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  sortBy: ProductFilter['sortBy'] = 'popularity';

  pricePresets = [
    { label: 'Under ₹500',  min: 0,     max: 500    },
    { label: '₹500–₹2K',   min: 500,   max: 2000   },
    { label: '₹2K–₹10K',   min: 2000,  max: 10000  },
    { label: 'Above ₹10K', min: 10000, max: 999999 }
  ];

  constructor(private productService: ProductService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    if (this.initialFilter.minPrice   !== undefined) this.minPrice          = this.initialFilter.minPrice;
    if (this.initialFilter.maxPrice   !== undefined) this.maxPrice          = this.initialFilter.maxPrice;
    if (this.initialFilter.minRating  !== undefined) this.minRating         = this.initialFilter.minRating;
    if (this.initialFilter.sortBy)                   this.sortBy            = this.initialFilter.sortBy;
    if (this.initialFilter.categoryId !== undefined) this.selectedCategoryId = this.initialFilter.categoryId;

    // load categories from backend so the dropdown is populated
    this.productService.getCategories().subscribe({
      next: cats => { this.categories = cats; this.cdr.detectChanges(); },
      error: () => console.error('[FilterSidebar] Failed to load categories')
    });
  }

  applyFilters(): void {
    this.filterChange.emit({
      categoryId: this.selectedCategoryId,
      minPrice:   this.minPrice,
      maxPrice:   this.maxPrice,
      minRating:  this.minRating,
      sortBy:     this.sortBy
    });
  }

  setPricePreset(preset: { min: number; max: number }): void {
    this.minPrice = preset.min;
    this.maxPrice = preset.max === 999999 ? undefined : preset.max;
    this.applyFilters();
  }

  clearFilters(): void {
    this.selectedCategoryId = undefined;
    this.minPrice           = undefined;
    this.maxPrice           = undefined;
    this.minRating          = undefined;
    this.sortBy             = 'popularity';
    this.applyFilters();
  }

  getStars(count: number): number[] {
    return Array(count).fill(0);
  }
}