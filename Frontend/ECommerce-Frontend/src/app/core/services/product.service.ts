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
    return this.http.get<Product[] | ApiResponse<Product[]>>(this.basePath).pipe(
      map((response: any) => {
        if (Array.isArray(response)) return response;
        if (response?.data?.data && Array.isArray(response.data.data)) return response.data.data;
        if (response?.data && Array.isArray(response.data)) return response.data;
        if (response?.items && Array.isArray(response.items)) return response.items;
        if (response?.products && Array.isArray(response.products)) return response.products;
        console.warn('Unexpected product API response format:', response);
        return [];
      })
    );
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product | ApiResponse<Product>>(`${this.basePath}/${id}`).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  getByCategory(categoryId: number): Observable<Product[]> {
    return this.http.get<Product[] | ApiResponse<Product[]>>(`${this.basePath}/category/${categoryId}`).pipe(
      map((response: any) => (Array.isArray(response) ? response : (response?.data || [])))
    );
  }

  search(searchTerm: string): Observable<Product[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Product[] | ApiResponse<Product[]>>(`${this.basePath}/search`, { params }).pipe(
      map((response: any) => (Array.isArray(response) ? response : (response?.data || [])))
    );
  }

  create(product: ProductCreateRequest): Observable<Product> {
    return this.http.post<Product | ApiResponse<Product>>(this.basePath, product).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  update(id: number, product: ProductUpdateRequest): Observable<Product> {
    return this.http.put<Product | ApiResponse<Product>>(`${this.basePath}/${id}`, product).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.basePath}/${id}`);
  }

  getFeatured(): Observable<Product[]> {
    return this.http.get<Product[] | ApiResponse<Product[]>>(`${this.basePath}/featured`).pipe(
      map((response: any) => (Array.isArray(response) ? response : (response?.data || [])))
    );
  }

  getNewArrivals(): Observable<Product[]> {
    return this.http.get<Product[] | ApiResponse<Product[]>>(`${this.basePath}/new`).pipe(
      map((response: any) => (Array.isArray(response) ? response : (response?.data || [])))
    );
  }

  getBestSellers(): Observable<Product[]> {
    return this.http.get<Product[] | ApiResponse<Product[]>>(`${this.basePath}/bestsellers`).pipe(
      map((response: any) => (Array.isArray(response) ? response : (response?.data || [])))
    );
  }
}
