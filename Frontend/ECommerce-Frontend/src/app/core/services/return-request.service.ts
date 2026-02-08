import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface ReturnRequest {
  id?: number;
  orderId: number;
  orderItemId: number;
  productId: number;
  quantity: number;
  reason: string;
  comments?: string;
  productName?: string;
  customerName?: string;
  status?: number;
  statusText?: string;
  adminResponse?: string;
  requestDate?: string;
  resolutionDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReturnRequestService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5010/api/return-requests';

  getMyRequests(): Observable<ReturnRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/my-requests`).pipe(
      map(response => {
        // ApiResponseDto wrapper varsa data'yı çıkar
        if (response && response.data) {
          return Array.isArray(response.data) ? response.data : [response.data];
        }
        return Array.isArray(response) ? response : [];
      })
    );
  }

  getByOrderId(orderId: number): Observable<ReturnRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/order/${orderId}`).pipe(
      map(response => {
        // ApiResponseDto wrapper varsa data'yı çıkar
        if (response && response.data) {
          return Array.isArray(response.data) ? response.data : [response.data];
        }
        return Array.isArray(response) ? response : [];
      })
    );
  }

  createReturnRequest(returnRequest: ReturnRequest): Observable<any> {
    return this.http.post(this.apiUrl, returnRequest);
  }
}
