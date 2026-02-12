import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../components/product-card/product-card';
import { CampaignCarouselComponent } from '../../components/campaign-carousel/campaign-carousel.component';
import { Category, Product } from '../../core/models';
import { BannerService, CartService, CategoryService, ProductService, ImageUrlService, WishlistService } from '../../core/services';
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
  private imageUrlService = inject(ImageUrlService);
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
        console.error('Kategoriler yüklenemedi:', err);
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
          console.error('Products is not an array:', products);
          this.error = 'Ürünler yüklenirken bir hata oluştu.';
          this.isLoading = false;
          return;
        }
        const mappedProducts = products.map(p => this.mapProduct(p)).filter(p => p.isActive);
        this.featuredProducts = mappedProducts.slice(0, 4);
        this.newProducts = mappedProducts.filter(p => p.isNew).slice(0, 4);
        this.bestSellers = mappedProducts.slice(0, 4);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ürünler yüklenemedi:', err);
        this.error = 'Ürünler yüklenirken bir hata oluştu.';
        this.loadMockProducts();
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
          console.error('All products is not an array:', products);
          return;
        }
        const mapped = products.map(p => this.mapProduct(p)).filter(p => p.isActive);
        this.allProducts = mapped;
      },
      error: (err) => {
        console.error('Tüm ürünler yüklenemedi:', err);
        // fallback to bestSellers if available
        this.allProducts = this.bestSellers.slice();
      }
    });
  }

  private mapProduct(apiProduct: any): Product {
    return {
      id: apiProduct.id,
      name: apiProduct.name,
      description: apiProduct.description || '',
      price: apiProduct.price,
      originalPrice: apiProduct.originalPrice,
      imageUrl: this.imageUrlService.normalize(apiProduct.imageUrl),
      images: this.imageUrlService.normalizeImages(apiProduct.images || []),
      categoryId: apiProduct.categoryId,
      categoryName: apiProduct.categoryName,
      brandId: apiProduct.brandId,
      brandName: apiProduct.brandName,
      companyId: apiProduct.companyId,
      stockQuantity: apiProduct.stockQuantity || 0,
      rating: apiProduct.rating || 0,
      reviewCount: apiProduct.reviewCount || 0,
      isNew: apiProduct.isNew || false,
      discount: apiProduct.discount,
      isActive: apiProduct.isActive || false,
      inStock: (apiProduct.stockQuantity || 0) > 0,
      createdAt: new Date(apiProduct.createdAt)
    };
  }

  private loadMockProducts(): void {
    const sampleProducts: Product[] = [
      {
        id: 1,
        name: 'Kablosuz Kulaklık Pro',
        description: 'Aktif gürültü engelleme, 30 saat pil ömrü',
        price: 1299.99,
        originalPrice: 1599.99,
        imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=300&fit=crop',
        categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 50,
        rating: 4.5, reviewCount: 128, isNew: true, discount: 20, isActive: true, inStock: true, createdAt: new Date()
      },
      {
        id: 2,
        name: 'Akıllı Saat Ultra',
        description: 'GPS, kalp atışı sensörü, su geçirmez',
        price: 2499.99,
        imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=300&fit=crop',
        categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 30,
        rating: 4.8, reviewCount: 256, isActive: true, inStock: true, createdAt: new Date()
      },
      {
        id: 3,
        name: 'Spor Ayakkabı',
        description: 'Hafif ve konforlu, koşu için ideal',
        price: 899.99,
        originalPrice: 1199.99,
        imageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=300&fit=crop',
        categoryId: 2, brandId: 2, companyId: 1, stockQuantity: 100,
        rating: 4.3, reviewCount: 89, discount: 25, isActive: true, inStock: true, createdAt: new Date()
      },
      {
        id: 4,
        name: 'Laptop Stand',
        description: 'Ergonomik tasarım, alüminyum gövde',
        price: 349.99,
        imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400&h=300&fit=crop',
        categoryId: 1, brandId: 3, companyId: 1, stockQuantity: 75,
        rating: 4.6, reviewCount: 67, isNew: true, isActive: true, inStock: true, createdAt: new Date()
      }
    ];

    this.featuredProducts = sampleProducts;
    this.newProducts = sampleProducts.filter(p => p.isNew);
    this.bestSellers = sampleProducts;
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
