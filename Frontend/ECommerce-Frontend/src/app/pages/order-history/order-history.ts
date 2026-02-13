import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Order, OrderStatus } from '../../core/models';
import { AuthService, OrderService } from '../../core/services';
import { ReturnRequestService } from '../../core/services/return-request.service';
import { Subject, takeUntil, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ReturnRequest } from '../../core/services/return-request.service';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './order-history.html',
  styleUrl: './order-history.css',
})
export class OrderHistory implements OnInit, OnDestroy {
  private orderService = inject(OrderService);
  private authService = inject(AuthService);
  private returnRequestService = inject(ReturnRequestService);

  orders: Order[] = [];
  returnRequests: Map<number, any[]> = new Map(); // Order ID -> Return Requests
  isLoading = true;
  selectedOrder: Order | null = null;
  showReturnForm = false;
  selectedOrderItem: any = null;
  returnReason = '';
  returnComments = '';
  isSubmittingReturn = false;

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadOrders();
    // Her 10 saniyede bir iade taleplerini güncelle
    interval(10000).pipe(
      switchMap(() => this.returnRequestService.getMyRequests()),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (requests) => {
        this.updateReturnRequestsMap(requests);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getMyOrders().pipe(takeUntil(this.destroy$)).subscribe({
      next: (orders) => {
        this.orders = orders;
        // Her order için iade taleplerini yükle
        orders.forEach(order => {
          this.loadReturnRequestsForOrder(order.id);
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.orders = [];
        this.isLoading = false;
      }
    });
  }

  loadReturnRequestsForOrder(orderId: number): void {
    this.returnRequestService.getByOrderId(orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (requests) => {
          if (requests && Array.isArray(requests)) {
            this.returnRequests.set(orderId, requests);
          }
        },
        error: (err) => {
          this.returnRequests.set(orderId, []);
        }
      });
  }

  updateReturnRequestsMap(allRequests: any[]): void {
    // Tüm talepleri order ID'lerine göre grupla
    const grouped: Map<number, any[]> = new Map();
    for (const order of this.orders) {
      const orderRequests = allRequests.filter(r => r.orderId === order.id);
      grouped.set(order.id, orderRequests);
    }
    this.returnRequests = grouped;
  }

  getReturnRequestsForOrder(orderId: number): any[] {
    return this.returnRequests.get(orderId) || [];
  }

  // Verilen order item için iade talep olup olmadığını kontrol et
  hasReturnRequest(orderId: number, itemId: number): boolean {
    const returnRequests = this.getReturnRequestsForOrder(orderId);
    return returnRequests.some(r => r.orderItemId === itemId);
  }

  // Verilen order item'ın iade talebini getir
  getReturnRequestForItem(orderId: number, itemId: number): any | null {
    const returnRequests = this.getReturnRequestsForOrder(orderId);
    return returnRequests.find(r => r.orderItemId === itemId) || null;
  }

  getReturnStatusBadge(status: number): { text: string; class: string } {
    switch (status) {
      case 0: return { text: 'Beklemede', class: 'bg-warning text-dark' };
      case 1: return { text: 'Onaylı', class: 'bg-success' };
      case 2: return { text: 'Reddedildi', class: 'bg-danger' };
      case 3: return { text: 'İşlemde', class: 'bg-info' };
      case 4: return { text: 'Tamamlandı', class: 'bg-secondary' };
      default: return { text: 'Bilinmiyor', class: 'bg-secondary' };
    }
  }

  getStatusBadgeClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'bg-warning text-dark';
      case OrderStatus.Processing: return 'bg-info';
      case OrderStatus.Shipped: return 'bg-primary';
      case OrderStatus.Delivered: return 'bg-success';
      case OrderStatus.Received: return 'bg-success';
      case OrderStatus.Completed: return 'bg-secondary';
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
      case OrderStatus.Received: return 'Teslim Alındı';
      case OrderStatus.Completed: return 'Tamamlandı';
      case OrderStatus.Cancelled: return 'İptal Edildi';
      default: return 'Bilinmiyor';
    }
  }

  viewOrderDetails(order: Order): void {
    this.selectedOrder = this.selectedOrder?.id === order.id ? null : order;
  }

  // İade talebi oluşturma
  openReturnForm(order: Order, item: any): void {
    this.selectedOrder = order;
    this.selectedOrderItem = item;
    this.showReturnForm = true;
  }

  closeReturnForm(): void {
    this.showReturnForm = false;
    this.returnReason = '';
    this.returnComments = '';
    this.isSubmittingReturn = false;
  }

  submitReturnRequest(orderId: number): void {
    if (!this.returnReason.trim()) {
      alert('Lütfen iade sebebini belirtiniz');
      return;
    }

    this.isSubmittingReturn = true;
    const returnRequest: any = {
      orderId: this.selectedOrder!.id,
      orderItemId: this.selectedOrderItem.id,
      productId: this.selectedOrderItem.productId,
      quantity: this.selectedOrderItem.quantity,
      reason: this.returnReason
    };

    // Comments sadece boş değilse ekle
    if (this.returnComments && this.returnComments.trim()) {
      returnRequest.comments = this.returnComments.trim();
    }
    this.returnRequestService.createReturnRequest(returnRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          alert('İade talebiniz başarıyla oluşturulmuştur');
          // İade taleplerini tekrar yükle
          this.loadReturnRequestsForOrder(this.selectedOrder!.id);
          this.closeReturnForm();
          this.isSubmittingReturn = false;
        },
        error: (err) => {
          alert('İade talebiniz oluşturulurken bir hata meydana geldi: ' + (err.error?.message || err.message));
          this.isSubmittingReturn = false;
        }
      });
  }

  checkPendingReturn(orderId: number, productId: number): boolean {
    const requests = this.returnRequests.get(orderId) || [];
    return requests.some(request => request.productId === productId && request.status === 'Pending');
  }

  refreshReturnRequests(orderId: number): void {
    this.loadReturnRequestsForOrder(orderId);
  }
}
