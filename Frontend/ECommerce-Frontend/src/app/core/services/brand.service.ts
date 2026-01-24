import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Brand {
  id: number;
  name: string;
  description?: string;
  logoUrl?: string;
  productCount: number;
}

export interface Model {
  id: number;
  name: string;
  brandId: number;
  brandName?: string;
  productCount: number;
}

@Injectable({
  providedIn: 'root',
})
export class BrandService {
  private http = inject(HttpClient);
  private apiUrl = '/api/brand';

  /**
   * Get all brands
   */
  getAll(): Observable<Brand[]> {
    return this.http.get<Brand[]>(this.apiUrl);
  }

  /**
   * Get a brand by ID
   */
  getById(id: number): Observable<Brand> {
    return this.http.get<Brand>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get models by brand
   */
  getModels(brandId: number): Observable<Model[]> {
    return this.http.get<Model[]>(`/api/model/brand/${brandId}`);
  }
}
