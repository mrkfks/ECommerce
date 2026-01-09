import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../components/product-card/product-card';
import { ProductService, CategoryService, CartService } from '../../core/services';
import { Product, Category } from '../../core/models';

interface Banner {
  id: number;
  title: string;
  subtitle: string;
  buttonText: string;
  buttonLink: string;
  imageUrl: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private cartService = inject(CartService);

  banners: Banner[] = [];
  categories: Category[] = [];
  featuredProducts: Product[] = [];
  newProducts: Product[] = [];
  bestSellers: Product[] = [];
  currentBannerIndex = 0;
  isLoading = true;
  error: string | null = null;
  
  private bannerInterval: any;

  ngOnInit(): void {
    this.loadBanners();
    this.loadCategories();
    this.loadProducts();
    this.startBannerRotation();
  }

  ngOnDestroy(): void {
    if (this.bannerInterval) {
      clearInterval(this.bannerInterval);
    }
  }

  loadBanners(): void {
    this.banners = [
      {
        id: 1,
        title: 'Kış Sezonu İndirimleri',
        subtitle: 'Seçili ürünlerde %50\'ye varan indirimler',
        buttonText: 'Alışverişe Başla',
        buttonLink: '/products/1',
        imageUrl: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1200&h=400&fit=crop'
      },
      {
        id: 2,
        title: 'Yeni Sezon Ürünleri',
        subtitle: '2026 koleksiyonu şimdi satışta',
        buttonText: 'Keşfet',
        buttonLink: '/products/2',
        imageUrl: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200&h=400&fit=crop'
      }
    ];
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
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
    this.productService.getAll().subscribe({
      next: (products) => {
        // API'den gelen ürünleri dönüştür
        const mappedProducts = products.map(p => this.mapProduct(p));
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

  private mapProduct(apiProduct: any): Product {
    return {
      id: apiProduct.id,
      name: apiProduct.name,
      description: apiProduct.description || '',
      price: apiProduct.price,
      originalPrice: apiProduct.originalPrice,
      imageUrl: apiProduct.imageUrl || 'https://via.placeholder.com/400x300',
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
      inStock: apiProduct.stockQuantity > 0,
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
        rating: 4.5, reviewCount: 128, isNew: true, discount: 20, inStock: true, createdAt: new Date()
      },
      {
        id: 2,
        name: 'Akıllı Saat Ultra',
        description: 'GPS, kalp atışı sensörü, su geçirmez',
        price: 2499.99,
        imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=300&fit=crop',
        categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 30,
        rating: 4.8, reviewCount: 256, inStock: true, createdAt: new Date()
      },
      {
        id: 3,
        name: 'Spor Ayakkabı',
        description: 'Hafif ve konforlu, koşu için ideal',
        price: 899.99,
        originalPrice: 1199.99,
        imageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=300&fit=crop',
        categoryId: 2, brandId: 2, companyId: 1, stockQuantity: 100,
        rating: 4.3, reviewCount: 89, discount: 25, inStock: true, createdAt: new Date()
      },
      {
        id: 4,
        name: 'Laptop Stand',
        description: 'Ergonomik tasarım, alüminyum gövde',
        price: 349.99,
        imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400&h=300&fit=crop',
        categoryId: 1, brandId: 3, companyId: 1, stockQuantity: 75,
        rating: 4.6, reviewCount: 67, isNew: true, inStock: true, createdAt: new Date()
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
    this.bannerInterval = setInterval(() => {
      this.currentBannerIndex = (this.currentBannerIndex + 1) % this.banners.length;
    }, 5000);
  }

  goToBanner(index: number): void {
    this.currentBannerIndex = index;
  }

  onAddToCart(product: Product): void {
    this.cartService.addToCart(product);
    console.log('Sepete eklendi:', product.name);
  }

  onAddToWishlist(product: Product): void {
    console.log('Favorilere eklendi:', product.name);
    // TODO: Wishlist service ile entegre edilecek
  }
}
