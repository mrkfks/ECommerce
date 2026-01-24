import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CustomerMessage {
  id: number;
  subject: string;
  message: string;
  createdAt: Date;
  isRead: boolean;
  isResolved: boolean;
  response?: string;
  respondedAt?: Date;
}

export interface CustomerMessageCreate {
  subject: string;
  message: string;
  orderId?: number; // Optional link to an order
}

@Injectable({
  providedIn: 'root',
})
export class CustomerMessageService {
  private http = inject(HttpClient);
  private apiUrl = '/api/customermessage';

  /**
   * Get all messages for current customer
   */
  getMyMessages(): Observable<CustomerMessage[]> {
    return this.http.get<CustomerMessage[]>(`${this.apiUrl}/my`);
  }

  /**
   * Get a specific message by ID
   */
  getById(id: number): Observable<CustomerMessage> {
    return this.http.get<CustomerMessage>(`${this.apiUrl}/${id}`);
  }

  /**
   * Send a new customer support message
   */
  send(message: CustomerMessageCreate): Observable<CustomerMessage> {
    return this.http.post<CustomerMessage>(this.apiUrl, message);
  }

  /**
   * Get unread message count
   */
  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`);
  }

  /**
   * Mark a message as read
   */
  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {});
  }
}
