import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Order, OrderCreateRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly basePath = '/order';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Order[]> {
    return this.http.get<ApiResponse<Order[]>>(this.basePath).pipe(
      map(response => response.data || [])
    );
  }

  getById(id: number): Observable<Order> {
    return this.http.get<ApiResponse<Order>>(`${this.basePath}/${id}`).pipe(
      map(response => response.data)
    );
  }

  getByCustomer(customerId: number): Observable<Order[]> {
    return this.http.get<ApiResponse<Order[]>>(`${this.basePath}/customer/${customerId}`).pipe(
      map(response => response.data || [])
    );
  }

  getMyOrders(): Observable<Order[]> {
    return this.http.get<ApiResponse<Order[]>>(`${this.basePath}/my-orders`).pipe(
      map(response => response.data || [])
    );
  }

  create(order: OrderCreateRequest): Observable<Order> {
    return this.http.post<ApiResponse<Order>>(this.basePath, order).pipe(
      map(response => response.data)
    );
  }

  updateStatus(id: number, status: number): Observable<Order> {
    return this.http.patch<ApiResponse<Order>>(`${this.basePath}/${id}/status`, { status }).pipe(
      map(response => response.data)
    );
  }

  cancel(id: number): Observable<void> {
    return this.http.post<void>(`${this.basePath}/${id}/cancel`, {});
  }
}
