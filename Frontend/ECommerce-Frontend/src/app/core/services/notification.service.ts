import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export type NotificationType = 'OrderStatus' | 'Promotion' | 'System' | 'LowStock';

export interface Notification {
  id: number;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
  link?: string;
  data?: Record<string, unknown>;
}

export interface NotificationSummary {
  totalCount: number;
  unreadCount: number;
  recentNotifications: Notification[];
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = '/api/notification';

  // Reactive state for unread count
  unreadCount = signal(0);

  /**
   * Get all notifications for current user
   */
  getAll(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.apiUrl);
  }

  /**
   * Get unread notifications
   */
  getUnread(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}/unread`).pipe(
      tap((notifications) => this.unreadCount.set(notifications.length))
    );
  }

  /**
   * Get notification summary (for header badge)
   */
  getSummary(): Observable<NotificationSummary> {
    return this.http.get<NotificationSummary>(`${this.apiUrl}/summary`).pipe(
      tap((summary) => this.unreadCount.set(summary.unreadCount))
    );
  }

  /**
   * Mark a notification as read
   */
  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => this.unreadCount.update((count) => Math.max(0, count - 1)))
    );
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCount.set(0))
    );
  }

  /**
   * Delete a notification
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Subscribe to real-time notifications (WebSocket/SignalR)
   * This is a placeholder - implement based on your real-time strategy
   */
  subscribeToRealTime(): void {
    // TODO: Implement SignalR or WebSocket connection
    console.log('Real-time notifications not yet implemented');
  }

  /**
   * Get notification icon based on type
   */
  getIcon(type: NotificationType): string {
    switch (type) {
      case 'OrderStatus':
        return 'bi-box-seam';
      case 'Promotion':
        return 'bi-tag';
      case 'System':
        return 'bi-info-circle';
      case 'LowStock':
        return 'bi-exclamation-triangle';
      default:
        return 'bi-bell';
    }
  }

  /**
   * Get notification color based on type
   */
  getColor(type: NotificationType): string {
    switch (type) {
      case 'OrderStatus':
        return 'primary';
      case 'Promotion':
        return 'success';
      case 'System':
        return 'info';
      case 'LowStock':
        return 'warning';
      default:
        return 'secondary';
    }
  }
}
