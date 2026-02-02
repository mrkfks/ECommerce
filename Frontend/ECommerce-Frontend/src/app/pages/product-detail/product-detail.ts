import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, CartService, ImageUrlService } from '../../core/services';
import { CompanyContextService } from '../../core/services/company-context.service';
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
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private companyContext = inject(CompanyContextService);
  private imageUrlService = inject(ImageUrlService);

  product: Product | null = null;
  relatedProducts: Product[] = [];
  isLoading = true;
  error: string | null = null;
  quantity = 1;
  selectedImageIndex = 0;

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const productId = +params['productId'];
      this.loadProduct(productId);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProduct(id: number): void {
    this.isLoading = true;
    this.productService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (product) => {
        console.log('API Response:', product);
        console.log('Images:', product.images);
        this.product = this.mapProduct(product);
        console.log('Mapped Product:', this.product);

        // Set company context from product if not already set
        if (product.companyId && !this.companyContext.getCompanyId()) {
          this.companyContext.setCompanyId(product.companyId);
        }

        setTimeout(() => {
          this.selectedImageIndex = 0;
        });

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
      imageUrl: this.imageUrlService.normalize(apiProduct.imageUrl),
      images: this.imageUrlService.normalizeImages(apiProduct.images || []),
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
      isActive: apiProduct.isActive || false,
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
      images: [
        {
          id: 1,
          productId: id,
          imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&h=400&fit=crop',
          order: 0,
          isPrimary: true
        },
        {
          id: 2,
          productId: id,
          imageUrl: 'https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600&h=400&fit=crop',
          order: 1,
          isPrimary: false
        },
        {
          id: 3,
          productId: id,
          imageUrl: 'https://images.unsplash.com/photo-1487215078519-e21cc028cb29?w=600&h=400&fit=crop',
          order: 2,
          isPrimary: false
        }
      ],
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
      isActive: true,
      inStock: true,
      createdAt: new Date()
    };
    
    // Set company context from mock product
    if (this.product.companyId && !this.companyContext.getCompanyId()) {
      this.companyContext.setCompanyId(this.product.companyId);
    }
  }

  private loadRelatedProducts(): void {
    if (this.product?.categoryId) {
      this.productService.getByCategory(this.product.categoryId).pipe(takeUntil(this.destroy$)).subscribe({
        next: (products) => {
          this.relatedProducts = products
            .filter(p => p.id !== this.product?.id && p.isActive)
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
      this.cartService.addToCart(this.product.id, this.quantity).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => alert(`${this.product!.name} sepete eklendi!`),
        error: (err) => console.error('Sepete eklenirken hata:', err)
      });
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
}
