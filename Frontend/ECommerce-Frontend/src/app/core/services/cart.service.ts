import { Injectable, signal, computed } from '@angular/core';
import { Cart, CartItem, Product } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly CART_KEY = 'shopping_cart';

  private _cart = signal<Cart>(this.loadCartFromStorage());

  // Public signals
  public cart = this._cart.asReadonly();
  public totalItems = computed(() => this._cart().totalItems);
  public totalPrice = computed(() => this._cart().totalPrice);
  public items = computed(() => this._cart().items);

  constructor() {}

  addToCart(product: Product, quantity: number = 1): void {
    const cart = this._cart();
    const existingItemIndex = cart.items.findIndex(item => item.product.id === product.id);

    let updatedItems: CartItem[];

    if (existingItemIndex > -1) {
      // Ürün zaten sepette, miktarı güncelle
      updatedItems = cart.items.map((item, index) => {
        if (index === existingItemIndex) {
          return { ...item, quantity: item.quantity + quantity };
        }
        return item;
      });
    } else {
      // Yeni ürün ekle
      updatedItems = [...cart.items, { product, quantity }];
    }

    this.updateCart(updatedItems);
  }

  removeFromCart(productId: number): void {
    const cart = this._cart();
    const updatedItems = cart.items.filter(item => item.product.id !== productId);
    this.updateCart(updatedItems);
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }

    const cart = this._cart();
    const updatedItems = cart.items.map(item => {
      if (item.product.id === productId) {
        return { ...item, quantity };
      }
      return item;
    });

    this.updateCart(updatedItems);
  }

  clearCart(): void {
    this.updateCart([]);
  }

  getItemQuantity(productId: number): number {
    const item = this._cart().items.find(item => item.product.id === productId);
    return item?.quantity || 0;
  }

  isInCart(productId: number): boolean {
    return this._cart().items.some(item => item.product.id === productId);
  }

  private updateCart(items: CartItem[]): void {
    const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
    const totalPrice = items.reduce((sum, item) => sum + (item.product.price * item.quantity), 0);

    const cart: Cart = { items, totalItems, totalPrice };
    this._cart.set(cart);
    this.saveCartToStorage(cart);
  }

  private loadCartFromStorage(): Cart {
    if (typeof window !== 'undefined') {
      const cartJson = localStorage.getItem(this.CART_KEY);
      if (cartJson) {
        try {
          return JSON.parse(cartJson);
        } catch {
          // Invalid JSON
        }
      }
    }
    return { items: [], totalItems: 0, totalPrice: 0 };
  }

  private saveCartToStorage(cart: Cart): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.CART_KEY, JSON.stringify(cart));
    }
  }
}
