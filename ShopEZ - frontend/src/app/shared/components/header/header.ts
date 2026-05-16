import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { Router, RouterModule } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { User } from '../../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit, OnDestroy {
  currentUser: User | null = null;
  isAdmin = false;
  cartCount = 0;
  searchQuery = '';
  suggestions: string[] = [];
  showSuggestions = false;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        this.isAdmin = user?.role === 'ADMIN';
      });

    this.cartService.cart$
      .pipe(takeUntil(this.destroy$))
      .subscribe(cart => { this.cartCount = cart.totalItems; });

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(query => {
      if (query.length > 1) {
        this.productService.searchSuggestions(query).subscribe({
          next: s => { this.suggestions = s.slice(0, 6); this.showSuggestions = true; },
          error: () => this.suggestions = []
        });
      } else {
        this.suggestions = [];
        this.showSuggestions = false;
      }
    });
  }

  onSearchChange(query: string): void { this.searchSubject.next(query); }

  performSearch(): void {
    this.showSuggestions = false;
    if (this.searchQuery.trim()) {
      this.router.navigate(['/products'], { queryParams: { keyword: this.searchQuery.trim() } });
    }
  }

  selectSuggestion(suggestion: string): void {
    this.searchQuery = suggestion;
    this.showSuggestions = false;
    this.performSearch();
  }

  hideDropdown(): void {
    // Small delay so mousedown on suggestion fires first
    setTimeout(() => this.showSuggestions = false, 200);
  }

  logout(): void { this.authService.logout(); }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}