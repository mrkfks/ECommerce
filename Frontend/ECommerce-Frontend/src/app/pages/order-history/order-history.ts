import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Order, OrderStatus } from '../../core/models';
import { AuthService, OrderService } from '../../core/services';

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
      error: (err) => {
        console.error('Siparişler yüklenemedi:', err);
        // Error handling - leave empty or show toast
        this.orders = [];
        this.isLoading = false;
      }
    });
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
