import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../components/product-card/product-card';
import { CampaignCarouselComponent } from '../../components/campaign-carousel/campaign-carousel.component';
import { Category, Product } from '../../core/models';
import { BannerService, CartService, CategoryService, ProductService, WishlistService } from '../../core/services';
import { Subject, takeUntil } from 'rxjs';

interface Banner {
  id: number;
  title: string;
  subtitle: string;
  buttonText: string;
  buttonLink: string;
  imageUrl: string;
}

// ...

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCard, CampaignCarouselComponent],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);
  private bannerService = inject(BannerService);
  private platformId = inject(PLATFORM_ID);

  banners: Banner[] = [];
  categories: Category[] = [];
  featuredProducts: Product[] = [];
  newProducts: Product[] = [];
  bestSellers: Product[] = [];
  allProducts: Product[] = [];
  currentBannerIndex = 0;
  isLoading = true;
  error: string | null = null;

  private bannerInterval: any;
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // SSR sırasında API istekleri yapma
    if (isPlatformBrowser(this.platformId)) {
      this.loadBanners();
      this.loadCategories();
      this.loadProducts();
      this.loadAllProducts();
    }
  }

  ngOnDestroy(): void {
    if (this.bannerInterval) {
      clearInterval(this.bannerInterval);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBanners(): void {
    this.bannerService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.banners = response.data.map(b => ({
            id: b.id,
            title: b.title,
            subtitle: b.description || '',
            buttonText: 'Detayları İncele', // Default text as backend doesn't support button text yet
            buttonLink: b.link || '/',
            imageUrl: b.imageUrl
          }));
          
          // Banner yüklendikten sonra rotation'ı yeniden başlat
          if (this.bannerInterval) {
            clearInterval(this.bannerInterval);
          }
          this.startBannerRotation();
        }
      },
      error: (err) => console.error('Bannerlar yüklenemedi:', err)
    });
  }

  loadCategories(): void {
    this.categoryService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (categories) => {
        this.categories = categories.map(cat => ({
          ...cat,
          icon: this.getCategoryIcon(cat.name)
        }));
      },
      error: (err) => {
        // Fallback to mock data
        this.categories = [
          { id: 1, name: 'Elektronik', icon: 'bi-laptop', productCount: 245, createdAt: new Date() },
          { id: 2, name: 'Giyim', icon: 'bi-bag', productCount: 532, createdAt: new Date() },
          { id: 3, name: 'Ev & Yaşam', icon: 'bi-house', productCount: 189, createdAt: new Date() },
          { id: 4, name: 'Spor', icon: 'bi-bicycle', productCount: 156, createdAt: new Date() },
          { id: 5, name: 'Kitap', icon: 'bi-book', productCount: 423, createdAt: new Date() },
          { id: 6, name: 'Kozmetik', icon: 'bi-droplet', productCount: 312, createdAt: new Date() }
        ];
      }
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getAll(1, 10).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        // Check if response has data property (ApiResponse wrapper)
        const responseData = (response as any).items ? response : (response as any).data;
        
        let products: any[] = [];
        
        if (responseData && Array.isArray(responseData.items)) {
           products = responseData.items;
        } else if (Array.isArray(responseData)) {
           products = responseData;
        }

        if (!Array.isArray(products)) {
          this.error = 'Ürünler yüklenirken bir hata oluştu.';
          this.isLoading = false;
          return;
        }
        const mappedProducts = products.map(p => this.productService.mapProduct(p)).filter(p => p.isActive);
        this.featuredProducts = mappedProducts.slice(0, 3);
        this.newProducts = mappedProducts.filter(p => p.isNew).slice(0, 3);
        this.bestSellers = mappedProducts.slice(0, 3);
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Ürünler yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  loadAllProducts(): void {
    this.productService.getAll(1, 24).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        const responseData = (response as any).items ? response : (response as any).data;
        let products: any[] = [];
        if (responseData && Array.isArray(responseData.items)) {
          products = responseData.items;
        } else if (Array.isArray(responseData)) {
          products = responseData;
        }
        if (!Array.isArray(products)) {
          return;
        }
        const mapped = products.map(p => this.productService.mapProduct(p)).filter(p => p.isActive);
        this.allProducts = mapped.slice(0, 3); // Sadece ilk 3 ürünü al
      },
      error: (err) => {
        // fallback to bestSellers if available
        this.allProducts = this.bestSellers.slice(0, 3); // Sadece ilk 3 ürünü al
      }
    });
  }

  private getCategoryIcon(name: string): string {
    const icons: Record<string, string> = {
      'elektronik': 'bi-laptop',
      'giyim': 'bi-bag',
      'ev': 'bi-house',
      'spor': 'bi-bicycle',
      'kitap': 'bi-book',
      'kozmetik': 'bi-droplet'
    };
    const key = name.toLowerCase().split(' ')[0];
    return icons[key] || 'bi-grid';
  }

  startBannerRotation(): void {
    // Banner rotation sadece banner varsa başlar
    if (this.banners.length === 0) {
      return;
    }
    
    this.bannerInterval = setInterval(() => {
      if (this.banners.length > 0) {
        this.currentBannerIndex = (this.currentBannerIndex + 1) % this.banners.length;
      }
    }, 5000);
  }

  goToBanner(index: number): void {
    this.currentBannerIndex = index;
  }

  onAddToCart(product: Product): void {
    this.cartService.addToCart(product.id, 1).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => console.log('Sepete eklendi:', product.name),
      error: (err) => console.error('Sepete eklenemedi:', err)
    });
  }

  onAddToWishlist(product: Product): void {
    this.wishlistService.addToWishlist(product.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => console.log('Favorilere eklendi:', product.name),
      error: (err) => console.error('Favorilere eklenemedi:', err)
    });
  }
}
