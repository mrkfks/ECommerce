import { Component, OnInit, inject, OnDestroy, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CartService, WishlistService, ReviewService, Review, ReviewCreate } from '../../core/services';
import { CompanyContextService } from '../../core/services/company-context.service';
import { AuthService } from '../../core/services/auth.service';
import { Product } from '../../core/models';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit, OnDestroy {
    onImgError(event: Event) {
      (event.target as HTMLImageElement).src = 'assets/images/no-image.svg';
    }

    onImageLoad(event: Event): void {
    }

  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);
  private reviewService = inject(ReviewService);
  private authService = inject(AuthService);
  private companyContext = inject(CompanyContextService);
  private platformId = inject(PLATFORM_ID);

  product: Product | null = null;
  relatedProducts: Product[] = [];
  reviews: Review[] = [];
  isLoading = true;
  isLoadingReviews = false;
  isSubmittingReview = false;
  error: string | null = null;
  quantity = 1;
  selectedImageIndex = 0;
  
  // Yorum formu
  reviewForm = {
    rating: 5,
    comment: ''
  };
  isLoggedIn = false;

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Kullanıcı giriş durumunu kontrol et
    this.isLoggedIn = this.authService.isAuthenticated();
    
    // SSR sırasında API istekleri yapma
    if (isPlatformBrowser(this.platformId)) {
      this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
        const productId = +params['productId'];
        this.loadProduct(productId);
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProduct(id: number): void {
    this.isLoading = true;
    this.productService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (product) => {
        this.product = this.productService.mapProduct(product);

        // Don't set company context here - it should be set before navigation
        // to avoid ExpressionChangedAfterItHasBeenCheckedError

        this.selectedImageIndex = 0;
        this.isLoading = false;
        this.loadRelatedProducts();
        this.loadReviews(id);
      },
      error: () => {
        this.error = 'Ürün bulunamadı.';
        this.isLoading = false;
      }
    });
  }

  private loadRelatedProducts(): void {
    if (this.product?.categoryId) {
      this.productService.getByCategory(this.product.categoryId).pipe(takeUntil(this.destroy$)).subscribe({
        next: (products) => {
          this.relatedProducts = products
            .filter(p => p.id !== this.product?.id && p.isActive)
            .slice(0, 4)
            .map(p => this.productService.mapProduct(p));
        },
        error: () => {
          // Ignore
        }
      });
    }
  }

  addToCart(): void {
    if (this.product) {
      this.cartService.addToCart(this.product.id, this.quantity).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => alert(`${this.product!.name} sepete eklendi!`),
        error: (err) => console.error('Sepete eklenirken hata:', err)
      });
    }
  }

  addToWishlist(): void {
    if (!this.product) return;
    this.wishlistService.addToWishlist(this.product.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        alert(`${this.product!.name} favorilere eklendi!`);
      },
      error: (err) => {
        alert('Favorilere eklenirken bir hata oluştu.');
      }
    });
  }

  incrementQuantity(): void {
    if (this.product && this.quantity < this.product.stockQuantity) {
      this.quantity++;
    }
  }

  decrementQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  nextImage(): void {
    if (this.product?.images && this.product.images.length > 1) {
      this.selectedImageIndex = (this.selectedImageIndex + 1) % this.product.images.length;
    }
  }

  previousImage(): void {
    if (this.product?.images && this.product.images.length > 1) {
      this.selectedImageIndex = this.selectedImageIndex === 0 
        ? this.product.images.length - 1 
        : this.selectedImageIndex - 1;
    }
  }

  selectImage(index: number): void {
    this.selectedImageIndex = index;
  }

  get discountPercentage(): number {
    if (this.product?.originalPrice && this.product.originalPrice > this.product.price) {
      return Math.round((1 - this.product.price / this.product.originalPrice) * 100);
    }
    return 0;
  }

  // ========== YORUM FONKSİYONLARI ==========

  loadReviews(productId: number): void {
    this.isLoadingReviews = true;
    this.reviewService.getByProduct(productId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (reviews) => {
        this.reviews = reviews;
        this.isLoadingReviews = false;
      },
      error: (err) => {
        this.reviews = [];
        this.isLoadingReviews = false;
      }
    });
  }

  setRating(rating: number): void {
    this.reviewForm.rating = rating;
  }

  submitReview(): void {
    if (!this.product || !this.isLoggedIn) return;
    if (!this.reviewForm.comment.trim()) {
      alert('Lütfen bir yorum yazın.');
      return;
    }

    this.isSubmittingReview = true;
    const reviewData: ReviewCreate = {
      productId: this.product.id,
      rating: this.reviewForm.rating,
      comment: this.reviewForm.comment
    };

    this.reviewService.create(reviewData).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        alert('Yorumunuz başarıyla gönderildi!');
        this.reviewForm = { rating: 5, comment: '' };
        this.isSubmittingReview = false;
        this.loadReviews(this.product!.id);
      },
      error: (err) => {
        alert('Yorum gönderilirken bir hata oluştu. Lütfen tekrar deneyin.');
        this.isSubmittingReview = false;
      }
    });
  }

  getStarArray(rating: number): number[] {
    return [1, 2, 3, 4, 5].map(star => star <= rating ? 1 : 0);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('tr-TR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
