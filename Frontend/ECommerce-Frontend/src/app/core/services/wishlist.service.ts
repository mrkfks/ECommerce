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
  private readonly basePath = '/wishlist';
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
      // Generate a unique session ID for server-side rendering
      return 'server-' + Math.random().toString(36).substr(2, 9);
    }

    try {
      let sessionId = localStorage.getItem('wishlist_session_id');
      if (!sessionId) {
        // Use crypto.randomUUID() if available, otherwise generate a simpler UUID
        if (typeof crypto !== 'undefined' && crypto.randomUUID) {
          sessionId = crypto.randomUUID();
        } else {
          // Fallback UUID generation
          sessionId = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
          });
        }
        localStorage.setItem('wishlist_session_id', sessionId);
      }
      return sessionId!;
    } catch (error) {
      // Fallback for localStorage being unavailable
      return 'session-' + Math.random().toString(36).substr(2, 9);
    }
  }

  private getCompanyIdHeader(): { [key: string]: string } {
    const companyId = this.companyContext.companyId() || 1; // Fallback to company 1
    return { 'X-Company-Id': companyId.toString() };
  }

  loadWishlist(): void {
    const sessionId = this.getSessionId();
    this.http.get<any>(
      `${this.basePath}?sessionId=${sessionId}`,
      { headers: this.getCompanyIdHeader() }
    ).subscribe({
      next: (response) => {
        const wishlist = response.data;
        this.wishlistSubject.next(wishlist);
        this.wishlist.set(wishlist);
      },
      error: () => {}
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
