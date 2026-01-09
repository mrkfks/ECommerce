import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService, OrderService, AuthService } from '../../core/services';
import { CartItem } from '../../core/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout implements OnInit {
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartItems: CartItem[] = [];
  isLoading = false;
  currentStep = 1;

  // Teslimat Bilgileri
  shippingInfo = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    district: '',
    postalCode: '',
    notes: ''
  };

  // Ödeme Bilgileri
  paymentInfo = {
    method: 'credit-card',
    cardNumber: '',
    cardName: '',
    expiryDate: '',
    cvv: ''
  };

  ngOnInit(): void {
    this.cartItems = this.cartService.items();
    if (this.cartItems.length === 0) {
      this.router.navigate(['/cart']);
    }

    // Giriş yapmış kullanıcı bilgilerini al
    const user = this.authService.currentUserValue;
    if (user) {
      this.shippingInfo.firstName = user.firstName;
      this.shippingInfo.lastName = user.lastName;
      this.shippingInfo.email = user.email;
    }
  }

  get subtotal(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.product.price * item.quantity), 0);
  }

  get shippingCost(): number {
    return this.subtotal >= 500 ? 0 : 29.99;
  }

  get total(): number {
    return this.subtotal + this.shippingCost;
  }

  nextStep(): void {
    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  prevStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  isShippingValid(): boolean {
    return !!(
      this.shippingInfo.firstName &&
      this.shippingInfo.lastName &&
      this.shippingInfo.email &&
      this.shippingInfo.phone &&
      this.shippingInfo.address &&
      this.shippingInfo.city
    );
  }

  isPaymentValid(): boolean {
    if (this.paymentInfo.method === 'cash-on-delivery') {
      return true;
    }
    return !!(
      this.paymentInfo.cardNumber &&
      this.paymentInfo.cardName &&
      this.paymentInfo.expiryDate &&
      this.paymentInfo.cvv
    );
  }

  placeOrder(): void {
    if (!this.isShippingValid() || !this.isPaymentValid()) {
      return;
    }

    this.isLoading = true;

    // Gerçek API çağrısı yerine simülasyon
    setTimeout(() => {
      const orderId = Math.floor(Math.random() * 1000000);
      this.cartService.clearCart();
      this.isLoading = false;
      this.router.navigate(['/order', orderId]);
    }, 1500);
  }
}
