import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart {
  private cartService = inject(CartService);

  items = this.cartService.items;
  totalItems = this.cartService.totalItems;
  totalPrice = this.cartService.totalPrice;

  updateQuantity(productId: number, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateQuantity(productId, quantity).subscribe({
      error: (err) => console.error('Failed to update quantity', err)
    });
  }

  removeItem(productId: number): void {
    this.cartService.removeFromCart(productId).subscribe({
      error: (err) => console.error('Failed to remove item', err)
    });
  }

  clearCart(): void {
    if (confirm('Sepeti temizlemek istediğinize emin misiniz?')) {
      this.cartService.clearCart().subscribe({
        error: (err) => console.error('Failed to clear cart', err)
      });
    }
  }

  get shippingCost(): number {
    return this.totalPrice() >= 500 ? 0 : 29.99;
  }

  get grandTotal(): number {
    return this.totalPrice() + this.shippingCost;
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/no-image.svg';
  }
}
