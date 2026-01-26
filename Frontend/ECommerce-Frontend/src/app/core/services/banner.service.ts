import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models';

export interface Banner {
  id: number;
  title: string;
  description?: string;
  imageUrl: string;
  link?: string;
  order: number;
  isActive: boolean;
  companyId: number;
}

@Injectable({
  providedIn: 'root'
})
export class BannerService {
  private readonly baseUrl = '/banners'; // Assumes base API URL is handled by interceptor

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Banner[]>> {
    return this.http.get<ApiResponse<Banner[]>>(this.baseUrl);
  }
}
