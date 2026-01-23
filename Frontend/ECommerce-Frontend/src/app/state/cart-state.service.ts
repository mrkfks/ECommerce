import { computed, Injectable, signal } from '@angular/core';

export interface CartItem {
  productId: number;
  name: string;
  price: number;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartStateService {
  private readonly _items = signal<CartItem[]>([]);

  readonly items = this._items.asReadonly();
  readonly totalCount = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));
  readonly totalPrice = computed(() => this._items().reduce((sum, i) => sum + i.price * i.quantity, 0));

  addToCart(item: CartItem) {
    const items = this._items();
    const idx = items.findIndex(i => i.productId === item.productId);
    if (idx > -1) {
      items[idx].quantity += item.quantity;
      this._items.set([...items]);
    } else {
      this._items.set([...items, item]);
    }
  }

  removeFromCart(productId: number) {
    this._items.set(this._items().filter(i => i.productId !== productId));
  }

  clearCart() {
    this._items.set([]);
  }
}
