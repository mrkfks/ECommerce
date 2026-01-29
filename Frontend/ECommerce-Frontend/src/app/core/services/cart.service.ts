import { Injectable, computed, signal, Signal, PLATFORM_ID, inject, effect } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { Product, ApiResponse } from '../models';
import { isPlatformBrowser } from '@angular/common';
import { CompanyContextService } from './company-context.service';

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productImage: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  companyId: number;
}

export interface Cart {
  id: number;
  totalAmount: number;
  items: CartItem[];
  totalItems: number;
  totalPrice: number;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly basePath = '/cart';
  private readonly platformId = inject(PLATFORM_ID);
  private companyContext = inject(CompanyContextService);

  private cartSubject = new BehaviorSubject<Cart | null>(null);
  public cart$ = this.cartSubject.asObservable();

  // Signals
  readonly cart = toSignal(this.cart$, { initialValue: null });
  readonly items = computed(() => this.cart()?.items ?? []);
  readonly totalItems = computed(() => this.cart()?.items.reduce((acc, item) => acc + item.quantity, 0) ?? 0);
  readonly totalPrice = computed(() => this.cart()?.totalAmount ?? 0);

  constructor(private http: HttpClient) {
    // Reload cart whenever company ID changes
    if (isPlatformBrowser(this.platformId)) {
      effect(() => {
        const companyId = this.companyContext.companyId();
        if (companyId) {
          this.loadCart();
        }
      });
    }
  }

  private getSessionId(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return 'server-session';
    }

    let sessionId = localStorage.getItem('cart_session_id');
    if (!sessionId) {
      sessionId = crypto.randomUUID();
      localStorage.setItem('cart_session_id', sessionId);
    }
    return sessionId;
  }

  loadCart(): void {
    const sessionId = this.getSessionId();
    this.http.get<ApiResponse<Cart>>(`${this.basePath}?sessionId=${sessionId}`).pipe(
      map(response => response.data)
    ).subscribe({
      next: (cart) => {
        if (cart) {
          this.cartSubject.next(cart);
        }
      },
      error: (err) => console.error('Failed to load cart', err)
    });
  }

  addToCart(productId: number, quantity: number = 1): Observable<any> {
    const sessionId = this.getSessionId();
    const request: AddToCartRequest = { productId, quantity };
    return this.http.post(`${this.basePath}/items?sessionId=${sessionId}`, request).pipe(
      tap(() => this.loadCart())
    );
  }

  removeFromCart(itemId: number): Observable<any> {
    return this.http.delete(`${this.basePath}/items/${itemId}`).pipe(
      tap(() => this.loadCart())
    );
  }

  updateQuantity(itemId: number, quantity: number): Observable<any> {
    return this.http.put(`${this.basePath}/items/${itemId}`, { quantity }).pipe(
      tap(() => this.loadCart())
    );
  }

  clearCart(): Observable<any> {
    return this.http.delete(`${this.basePath}`).pipe(
      tap(() => this.loadCart())
    );
  }

  mergeCart(): Observable<any> {
    const sessionId = this.getSessionId();
    return this.http.post(`${this.basePath}/merge`, { sessionId }).pipe(
      tap(() => {
        // After merge, maybe clear session ID or just reload cart (which will now be user cart)
        this.loadCart();
      })
    );
  }
}
