import { Component, OnInit, OnDestroy, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CategoryService, CartService, ImageUrlService, WishlistService } from '../../core/services';
import { Product, Category } from '../../core/models';
import { ProductCard } from '../../components/product-card/product-card';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-category-products',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ProductCard],
  templateUrl: './category-products.html',
  styleUrl: './category-products.css',
})
export class CategoryProducts implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);
  private imageUrlService = inject(ImageUrlService);
  private platformId = inject(PLATFORM_ID);

  category: Category | null = null;
  products: Product[] = [];
  filteredProducts: Product[] = [];
  isLoading = true;
  error: string | null = null;

  // Filtreler
  sortBy = 'default';
  priceRange = { min: 0, max: 10000 };
  searchTerm = '';

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // SSR sırasında API istekleri yapma
    if (isPlatformBrowser(this.platformId)) {
      this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
        const categoryId = params['categoryId'];
        if (categoryId === 'all') {
          this.loadAllProducts();
        } else {
          this.loadCategory(+categoryId);
          this.loadProducts(+categoryId);
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategory(id: number): void {
    this.categoryService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (category) => {
        this.category = category;
      },
      error: () => {
        this.category = { id, name: 'Kategori', createdAt: new Date() };
      }
    });
  }

  loadProducts(categoryId: number): void {
    this.isLoading = true;
    this.productService.getByCategory(categoryId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (products) => {
        this.products = products.filter(p => p.isActive).map(p => this.mapProduct(p));
        this.applyFilters();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ürünler yüklenemedi:', err);
        this.loadMockProducts();
        this.isLoading = false;
      }
    });
  }

  loadAllProducts(): void {
    this.category = { id: 0, name: 'Tüm Ürünler', createdAt: new Date() };
    this.isLoading = true;
    this.productService.getAll().subscribe({
      next: (response) => {
        this.products = response.items.filter(p => p.isActive).map(p => this.mapProduct(p));
        this.applyFilters();
        this.isLoading = false;
      },
      error: () => {
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
      inStock: apiProduct.stockQuantity > 0,
      createdAt: new Date(apiProduct.createdAt)
    };
  }

  private loadMockProducts(): void {
    this.products = [
      { id: 1, name: 'Kablosuz Kulaklık', description: 'ANC özellikli', price: 1299.99, imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400', categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 50, rating: 4.5, reviewCount: 128, isActive: true, inStock: true, createdAt: new Date() },
      { id: 2, name: 'Akıllı Saat', description: 'GPS ve sağlık takibi', price: 2499.99, imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400', categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 30, rating: 4.8, reviewCount: 256, isActive: true, inStock: true, createdAt: new Date() },
      { id: 3, name: 'Spor Ayakkabı', description: 'Hafif ve konforlu', price: 899.99, originalPrice: 1199.99, imageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400', categoryId: 2, brandId: 2, companyId: 1, stockQuantity: 100, rating: 4.3, reviewCount: 89, discount: 25, isActive: true, inStock: true, createdAt: new Date() },
      { id: 4, name: 'Laptop Stand', description: 'Ergonomik tasarım', price: 349.99, imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400', categoryId: 1, brandId: 3, companyId: 1, stockQuantity: 75, rating: 4.6, reviewCount: 67, isNew: true, isActive: true, inStock: true, createdAt: new Date() }
    ];
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.products];

    // Aktif olmayan ürünleri filtrele
    filtered = filtered.filter(p => p.isActive);

    // Arama filtresi
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(term) ||
        p.description.toLowerCase().includes(term)
      );
    }

    // Fiyat filtresi
    filtered = filtered.filter(p =>
      p.price >= this.priceRange.min && p.price <= this.priceRange.max
    );

    // Sıralama
    switch (this.sortBy) {
      case 'price-asc':
        filtered.sort((a, b) => a.price - b.price);
        break;
      case 'price-desc':
        filtered.sort((a, b) => b.price - a.price);
        break;
      case 'name-asc':
        filtered.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'rating':
        filtered.sort((a, b) => b.rating - a.rating);
        break;
    }

    this.filteredProducts = filtered;
  }

  onAddToCart(product: Product): void {
    this.cartService.addToCart(product.id).pipe(takeUntil(this.destroy$)).subscribe({
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
