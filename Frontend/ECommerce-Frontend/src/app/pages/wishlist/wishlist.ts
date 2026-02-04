import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { WishlistService, WishlistItem, CartService, ImageUrlService } from '../../core/services';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css',
})
export class Wishlist implements OnInit, OnDestroy {
  private wishlistService = inject(WishlistService);
  private cartService = inject(CartService);
  private imageUrlService = inject(ImageUrlService);
  
  wishlistItems: WishlistItem[] = [];
  isLoading = false;
  isEmpty = false;
  totalValue = 0;

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadWishlist();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadWishlist(): void {
    this.isLoading = true;
    // Initially load the wishlist first
    this.wishlistService.loadWishlist();
    
    // Then subscribe to changes
    this.wishlistService.wishlist$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (wishlist) => {
        console.log('Wishlist page received wishlist:', wishlist);
        if (wishlist && wishlist.items) {
          this.wishlistItems = wishlist.items;
          this.isEmpty = this.wishlistItems.length === 0;
          this.calculateTotalValue();
        } else {
          this.isEmpty = true;
          this.wishlistItems = [];
          this.totalValue = 0;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error in wishlist subscription:', err);
        this.isEmpty = true;
        this.isLoading = false;
        this.totalValue = 0;
      }
    });
  }

  calculateTotalValue(): void {
    this.totalValue = this.wishlistItems.reduce((sum, item) => sum + item.price, 0);
  }

  removeFromWishlist(itemId: number): void {
    this.wishlistService.removeFromWishlist(itemId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.loadWishlist();
      },
      error: (err) => {
        console.error('Favorilerden kaldırma hatası:', err);
      }
    });
  }

  addAllToCart(): void {
    if (this.wishlistItems.length === 0) return;
    this.wishlistItems.forEach(item => {
      this.cartService.addToCart(item.productId, 1).pipe(takeUntil(this.destroy$)).subscribe({
        error: (err) => console.error('Sepete ekleme hatası:', err)
      });
    });
  }

  addToCart(item: WishlistItem): void {
    this.cartService.addToCart(item.productId, 1).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        console.log('Sepete eklendi:', item.productName);
      },
      error: (err) => {
        console.error('Sepete ekleme hatası:', err);
      }
    });
  }

  getImageUrl(item: WishlistItem): string {
    return this.imageUrlService.normalize(item.productImage);
  }

  clearWishlist(): void {
    if (confirm('Tüm favori ürünleri kaldırmak istediğinizden emin misiniz?')) {
      this.wishlistService.clearWishlist().pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.loadWishlist();
        },
        error: (err) => {
          console.error('Favorileri temizleme hatası:', err);
        }
      });
    }
  }
}
