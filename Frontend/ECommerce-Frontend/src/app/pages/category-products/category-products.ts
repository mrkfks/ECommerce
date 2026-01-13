import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CategoryService, CartService } from '../../core/services';
import { Product, Category } from '../../core/models';
import { ProductCard } from '../../components/product-card/product-card';

@Component({
  selector: 'app-category-products',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ProductCard],
  templateUrl: './category-products.html',
  styleUrl: './category-products.css',
})
export class CategoryProducts implements OnInit {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private cartService = inject(CartService);

  category: Category | null = null;
  products: Product[] = [];
  filteredProducts: Product[] = [];
  isLoading = true;
  error: string | null = null;

  // Filtreler
  sortBy = 'default';
  priceRange = { min: 0, max: 10000 };
  searchTerm = '';

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const categoryId = params['categoryId'];
      if (categoryId === 'all') {
        this.loadAllProducts();
      } else {
        this.loadCategory(+categoryId);
        this.loadProducts(+categoryId);
      }
    });
  }

  loadCategory(id: number): void {
    this.categoryService.getById(id).subscribe({
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
    this.productService.getByCategory(categoryId).subscribe({
      next: (products) => {
        this.products = products.map(p => this.mapProduct(p));
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
        this.products = response.items.map(p => this.mapProduct(p));
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
    this.products = [
      { id: 1, name: 'Kablosuz Kulaklık', description: 'ANC özellikli', price: 1299.99, imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400', categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 50, rating: 4.5, reviewCount: 128, inStock: true, createdAt: new Date() },
      { id: 2, name: 'Akıllı Saat', description: 'GPS ve sağlık takibi', price: 2499.99, imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400', categoryId: 1, brandId: 1, companyId: 1, stockQuantity: 30, rating: 4.8, reviewCount: 256, inStock: true, createdAt: new Date() },
      { id: 3, name: 'Spor Ayakkabı', description: 'Hafif ve konforlu', price: 899.99, originalPrice: 1199.99, imageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400', categoryId: 2, brandId: 2, companyId: 1, stockQuantity: 100, rating: 4.3, reviewCount: 89, discount: 25, inStock: true, createdAt: new Date() },
      { id: 4, name: 'Laptop Stand', description: 'Ergonomik tasarım', price: 349.99, imageUrl: 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400', categoryId: 1, brandId: 3, companyId: 1, stockQuantity: 75, rating: 4.6, reviewCount: 67, isNew: true, inStock: true, createdAt: new Date() }
    ];
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.products];

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
    this.cartService.addToCart(product.id);
  }

  onAddToWishlist(product: Product): void {
    console.log('Favorilere eklendi:', product.name);
  }
}
