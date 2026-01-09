import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Product, ProductCreateRequest, ProductUpdateRequest, PaginatedResponse, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly basePath = '/product';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(this.basePath).pipe(
      map(response => response.data || [])
    );
  }

  getById(id: number): Observable<Product> {
    return this.http.get<ApiResponse<Product>>(`${this.basePath}/${id}`).pipe(
      map(response => response.data)
    );
  }

  getByCategory(categoryId: number): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${this.basePath}/category/${categoryId}`).pipe(
      map(response => response.data || [])
    );
  }

  search(searchTerm: string): Observable<Product[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<ApiResponse<Product[]>>(`${this.basePath}/search`, { params }).pipe(
      map(response => response.data || [])
    );
  }

  create(product: ProductCreateRequest): Observable<Product> {
    return this.http.post<ApiResponse<Product>>(this.basePath, product).pipe(
      map(response => response.data)
    );
  }

  update(id: number, product: ProductUpdateRequest): Observable<Product> {
    return this.http.put<ApiResponse<Product>>(`${this.basePath}/${id}`, product).pipe(
      map(response => response.data)
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.basePath}/${id}`);
  }

  getFeatured(): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${this.basePath}/featured`).pipe(
      map(response => response.data || [])
    );
  }

  getNewArrivals(): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${this.basePath}/new`).pipe(
      map(response => response.data || [])
    );
  }

  getBestSellers(): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${this.basePath}/bestsellers`).pipe(
      map(response => response.data || [])
    );
  }
}
