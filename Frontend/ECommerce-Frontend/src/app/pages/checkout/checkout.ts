import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService, CartItem } from '../../core/services/cart.service';
import { OrderService, AuthService } from '../../core/services';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout implements OnInit, OnDestroy {
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

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Subscribe to cart changes
    this.cartService.cart$.pipe(takeUntil(this.destroy$)).subscribe(cart => {
      this.cartItems = cart ? cart.items : [];
      // Cart boşsa yönlendirme yapılabilir ancak ilk yüklemede boş gelebilir, dikkatli olunmalı.
      // if (cart && cart.items.length === 0) { ... }
    });

    // Giriş yapmış kullanıcı bilgilerini al
    const user = this.authService.currentUserValue;
    if (user) {
      this.shippingInfo.firstName = user.firstName;
      this.shippingInfo.lastName = user.lastName;
      this.shippingInfo.email = user.email;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get subtotal(): number {
    return this.cartItems.reduce((sum, item) => sum + item.totalPrice, 0);
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
      this.shippingInfo.city &&
      this.shippingInfo.district &&
      this.shippingInfo.postalCode
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

    const user = this.authService.currentUserValue;
    if (!user) {
      alert('Lütfen önce giriş yapın');
      this.router.navigate(['/login']);
      return;
    }

    this.isLoading = true;

    // Şirket ID'sini ilk üründen al (Varsayım: Tek satıcı veya marketplace yapısında sipariş bazında ayrıştırma)
    // Eğer cartItems boşsa 1 varsayıyoruz ama sepette ürün yoksa zaten buraya gelinmemeli.
    const companyId = this.cartItems.length > 0 ? this.cartItems[0].companyId : 1;

    // Order Request Hazırla
    const orderRequest = {
      customerId: user.id,
      addressId: 0, // Yeni adres
      companyId: companyId,
      items: this.cartItems.map(item => ({
        productId: item.productId,
        quantity: item.quantity
      })),
      shippingAddress: {
        customerId: user.id,
        street: this.shippingInfo.address,
        city: this.shippingInfo.city,
        state: this.shippingInfo.district,
        zipCode: this.shippingInfo.postalCode,
        country: 'Turkey'
      },
      cardNumber: this.paymentInfo.cardNumber || undefined,
      cardExpiry: this.paymentInfo.expiryDate || undefined,
      cardCvv: this.paymentInfo.cvv || undefined
    };

    this.orderService.create(orderRequest).pipe(takeUntil(this.destroy$)).subscribe({
      next: (order) => {
        this.isLoading = false;
        this.cartService.clearCart();

        // API Response yapısına göre ID alımı
        // @ts-ignore
        const orderId = order.id || (order as any).data?.id;

        alert('Siparişiniz başarıyla alındı!');
        this.router.navigate(['/order-history']);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Sipariş hatası:', err);
        // Hata mesajını daha kullanıcı dostu göster
        const msg = err.error?.message || err.message || 'Bilinmeyen bir hata oluştu';
        alert('Sipariş oluşturulurken bir hata oluştu: ' + msg);
      }
    });
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/no-image.svg';
  }
}
