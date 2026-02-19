import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Product, ProductCampaign } from '../../core/models';
import { ProductService } from '../../core/services/product.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard implements OnInit {
  @Input() product!: Product;
  @Input() showCampaign: boolean = false;
  @Output() addToCart = new EventEmitter<Product>();
  @Output() addToWishlist = new EventEmitter<Product>();

  activeCampaign: ProductCampaign | null = null;
  loading = false;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    if (this.showCampaign) {
      this.loadActiveCampaign();
    }
  }

  loadActiveCampaign(): void {
    if (!this.product?.id) return;
    
    this.loading = true;
    this.productService.getActiveCampaign(this.product.id).subscribe({
      next: (campaign) => {
        this.activeCampaign = campaign;
        this.loading = false;
      },
      error: () => {
        this.activeCampaign = null;
        this.loading = false;
      }
    });
  }

  onImgError(event: Event) {
    (event.target as HTMLImageElement).src = 'assets/images/no-image.svg';
  }

  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.addToCart.emit(this.product);
  }

  onAddToWishlist(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.addToWishlist.emit(this.product);
  }

  getDisplayPrice(): number {
    return this.activeCampaign?.discountedPrice ?? this.product?.price ?? 0;
  }

  getOriginalPrice(): number | null {
    return this.activeCampaign?.originalPrice ?? null;
  }
}

