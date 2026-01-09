import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService, AuthService } from '../../core/services';
import { Order, OrderStatus } from '../../core/models';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-history.html',
  styleUrl: './order-history.css',
})
export class OrderHistory implements OnInit {
  private orderService = inject(OrderService);
  private authService = inject(AuthService);

  orders: Order[] = [];
  isLoading = true;
  selectedOrder: Order | null = null;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getMyOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.isLoading = false;
      },
      error: () => {
        this.loadMockOrders();
        this.isLoading = false;
      }
    });
  }

  private loadMockOrders(): void {
    this.orders = [
      {
        id: 1001,
        customerId: 1,
        customerName: 'Demo Kullanıcı',
        addressId: 1,
        companyId: 1,
        companyName: 'Demo Şirket',
        orderDate: new Date('2024-01-15'),
        items: [
          { id: 1, productId: 1, productName: 'Kablosuz Kulaklık', quantity: 1, unitPrice: 1299.99, totalPrice: 1299.99 },
          { id: 2, productId: 2, productName: 'Telefon Kılıfı', quantity: 2, unitPrice: 99.99, totalPrice: 199.98 }
        ],
        totalAmount: 1499.97,
        status: OrderStatus.Delivered,
        statusText: 'Teslim Edildi'
      },
      {
        id: 1002,
        customerId: 1,
        customerName: 'Demo Kullanıcı',
        addressId: 1,
        companyId: 1,
        companyName: 'Demo Şirket',
        orderDate: new Date('2024-01-20'),
        items: [
          { id: 3, productId: 3, productName: 'Akıllı Saat', quantity: 1, unitPrice: 2499.99, totalPrice: 2499.99 }
        ],
        totalAmount: 2499.99,
        status: OrderStatus.Shipped,
        statusText: 'Kargoda'
      },
      {
        id: 1003,
        customerId: 1,
        customerName: 'Demo Kullanıcı',
        addressId: 1,
        companyId: 1,
        companyName: 'Demo Şirket',
        orderDate: new Date('2024-01-22'),
        items: [
          { id: 4, productId: 4, productName: 'Laptop Stand', quantity: 1, unitPrice: 349.99, totalPrice: 349.99 }
        ],
        totalAmount: 349.99,
        status: OrderStatus.Processing,
        statusText: 'Hazırlanıyor'
      }
    ];
  }

  getStatusBadgeClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'bg-warning text-dark';
      case OrderStatus.Processing: return 'bg-info';
      case OrderStatus.Shipped: return 'bg-primary';
      case OrderStatus.Delivered: return 'bg-success';
      case OrderStatus.Cancelled: return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getStatusText(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'Beklemede';
      case OrderStatus.Processing: return 'Hazırlanıyor';
      case OrderStatus.Shipped: return 'Kargoda';
      case OrderStatus.Delivered: return 'Teslim Edildi';
      case OrderStatus.Cancelled: return 'İptal Edildi';
      default: return 'Bilinmiyor';
    }
  }

  viewOrderDetails(order: Order): void {
    this.selectedOrder = this.selectedOrder?.id === order.id ? null : order;
  }
}
