import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ProductPrice {
  originalPrice: number;
  discountedPrice: number;
  discountPercentage: number;
  savingAmount: number;
}

@Component({
  selector: 'app-campaign-price-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaign-price-display.component.html',
  styleUrls: ['./campaign-price-display.component.css']
})
export class CampaignPriceDisplayComponent {
  @Input() originalPrice: number = 0;
  @Input() discountedPrice: number = 0;
  @Input() productName: string = '';
  @Input() campaignName: string = '';

  get priceData(): ProductPrice {
    const savingAmount = this.originalPrice - this.discountedPrice;
    const discountPercentage = this.originalPrice > 0 
      ? Math.round((savingAmount / this.originalPrice) * 100) 
      : 0;

    return {
      originalPrice: this.originalPrice,
      discountedPrice: this.discountedPrice,
      discountPercentage,
      savingAmount
    };
  }

  getPriceDisplayClass(): string {
    const discount = this.priceData.discountPercentage;
    if (discount >= 50) return 'hot-deal';
    if (discount >= 30) return 'great-deal';
    if (discount >= 10) return 'good-deal';
    return 'normal-deal';
  }

  formatPrice(price: number): string {
    return price.toFixed(2).replace('.', ',');
  }
}
