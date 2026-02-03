import { Injectable, computed, signal, Signal, PLATFORM_ID, inject, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Product, ApiResponse } from '../models';
import { isPlatformBrowser } from '@angular/common';
import { CompanyContextService } from './company-context.service';

export interface WishlistItem {
  id: number;
  productId: number;
  productName: string;
  productImage: string;
  price: number;
  companyId: number;
  addedAt: Date;
}

export interface Wishlist {
  id: number;
  items: WishlistItem[];
  totalItems: number;
}

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private readonly basePath = '/api/wishlist';
  private readonly platformId = inject(PLATFORM_ID);
  private companyContext = inject(CompanyContextService);

  private wishlistSubject = new BehaviorSubject<Wishlist | null>(null);
  public wishlist$ = this.wishlistSubject.asObservable();

  readonly wishlist = signal<Wishlist | null>(null);
  readonly items = computed(() => this.wishlist()?.items ?? []);
  readonly totalItems = computed(() => this.wishlist()?.items.length ?? 0);

  constructor(private http: HttpClient) {
    // Reload wishlist whenever company ID changes
    if (isPlatformBrowser(this.platformId)) {
      effect(() => {
        const companyId = this.companyContext.companyId();
        if (companyId) {
          this.loadWishlist();
        }
      });
    }
  }

  private getSessionId(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return 'server-session';
    }

    let sessionId = localStorage.getItem('wishlist_session_id');
    if (!sessionId) {
      sessionId = crypto.randomUUID();
      localStorage.setItem('wishlist_session_id', sessionId);
    }
    return sessionId;
  }

  private getCompanyIdHeader(): { [key: string]: string } {
    const companyId = this.companyContext.companyId() || 1; // Fallback to company 1
    return { 'X-Company-Id': companyId.toString() };
  }

  loadWishlist(): void {
    const sessionId = this.getSessionId();
    this.http.get<ApiResponse<Wishlist>>(
      `${this.basePath}?sessionId=${sessionId}`,
      { headers: this.getCompanyIdHeader() }
    ).subscribe({
      next: (response) => {
        const wishlist = response.data;
        this.wishlistSubject.next(wishlist);
        this.wishlist.set(wishlist);
      },
      error: (err) => console.error('Failed to load wishlist', err)
    });
  }

  addToWishlist(productId: number): Observable<any> {
    const sessionId = this.getSessionId();
    return this.http.post(
      `${this.basePath}/items?sessionId=${sessionId}`,
      { productId },
      { headers: this.getCompanyIdHeader() }
    ).pipe(
      tap(() => this.loadWishlist())
    );
  }

  removeFromWishlist(itemId: number): Observable<any> {
    return this.http.delete(`${this.basePath}/items/${itemId}`, {
      headers: this.getCompanyIdHeader()
    }).pipe(
      tap(() => this.loadWishlist())
    );
  }

  clearWishlist(): Observable<any> {
    const sessionId = this.getSessionId();
    return this.http.delete(`${this.basePath}?sessionId=${sessionId}`, {
      headers: this.getCompanyIdHeader()
    }).pipe(
      tap(() => this.loadWishlist())
    );
  }

  isInWishlist(productId: number): boolean {
    return this.items().some(item => item.productId === productId);
  }
}
