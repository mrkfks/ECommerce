import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CartService } from '../../core/services';
import { Product } from '../../core/models';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  product: Product | null = null;
  relatedProducts: Product[] = [];
  isLoading = true;
  error: string | null = null;
  quantity = 1;
  selectedImageIndex = 0;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const productId = +params['productId'];
      this.loadProduct(productId);
    });
  }

  loadProduct(id: number): void {
    this.isLoading = true;
    this.productService.getById(id).subscribe({
      next: (product) => {
        this.product = this.mapProduct(product);
        this.isLoading = false;
        this.loadRelatedProducts();
      },
      error: (err) => {
        console.error('Ürün yüklenemedi:', err);
        this.loadMockProduct(id);
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
      imageUrl: apiProduct.imageUrl || 'https://via.placeholder.com/600x400',
      categoryId: apiProduct.categoryId,
      categoryName: apiProduct.categoryName,
      brandId: apiProduct.brandId,
      brandName: apiProduct.brandName,
      companyId: apiProduct.companyId,
      stockQuantity: apiProduct.stockQuantity || 0,
      rating: apiProduct.rating || 4.5,
      reviewCount: apiProduct.reviewCount || 0,
      isNew: apiProduct.isNew || false,
      discount: apiProduct.discount,
      inStock: apiProduct.stockQuantity > 0,
      createdAt: new Date(apiProduct.createdAt)
    };
  }

  private loadMockProduct(id: number): void {
    this.product = {
      id: id,
      name: 'Kablosuz Kulaklık Pro',
      description: 'Yüksek kaliteli ses deneyimi sunan kablosuz kulaklık. Aktif gürültü engelleme teknolojisi ile dış sesleri bloke eder. 30 saate varan pil ömrü ile uzun süreli kullanım imkanı. Bluetooth 5.0 bağlantısı ile hızlı ve kararlı bağlantı.',
      price: 1299.99,
      originalPrice: 1599.99,
      imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&h=400&fit=crop',
      categoryId: 1,
      categoryName: 'Elektronik',
      brandId: 1,
      brandName: 'TechBrand',
      companyId: 1,
      stockQuantity: 50,
      rating: 4.5,
      reviewCount: 128,
      isNew: true,
      discount: 20,
      inStock: true,
      createdAt: new Date()
    };
  }

  private loadRelatedProducts(): void {
    if (this.product?.categoryId) {
      this.productService.getByCategory(this.product.categoryId).subscribe({
        next: (products) => {
          this.relatedProducts = products
            .filter(p => p.id !== this.product?.id)
            .slice(0, 4)
            .map(p => this.mapProduct(p));
        },
        error: () => {
          // Ignore
        }
      });
    }
  }

  addToCart(): void {
    if (this.product) {
      this.cartService.addToCart(this.product, this.quantity);
      alert(`${this.product.name} sepete eklendi!`);
    }
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

  get discountPercentage(): number {
    if (this.product?.originalPrice && this.product.originalPrice > this.product.price) {
      return Math.round((1 - this.product.price / this.product.originalPrice) * 100);
    }
    return 0;
  }
}
