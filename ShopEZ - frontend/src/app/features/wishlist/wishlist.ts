import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../core/services/wishlist.service';
import { CartService } from '../../core/services/cart.service';
import { ImageService } from '../../core/services/image.service';
import { WishlistItem } from '../../models/wishlist'; 

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css'
})
export class WishlistComponent implements OnInit {
  items: WishlistItem[] = [];

  constructor(
    public wishlistService: WishlistService,
    private cartService: CartService,
    public imageService: ImageService
  ) {}

  ngOnInit() {
    this.wishlistService.wishlist$.subscribe(w => this.items = w.items);
  }

  remove(productId: number) {
    this.wishlistService.remove(productId);
  }

  addToCart(item: WishlistItem) {
    this.cartService.addToCart(item.product, 1);
    this.wishlistService.remove(item.product.id);
  }
}