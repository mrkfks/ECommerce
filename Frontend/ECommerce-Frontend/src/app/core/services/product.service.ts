import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Product, PaginatedResponse, ApiResponse, ProductCampaign } from '../models';
import { environment } from '../../../environments/environment';
import { ImageUrlService } from './image-url.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly basePath = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient, private imageUrlService: ImageUrlService) { }

  mapProduct(apiProduct: any): Product {
    return {
      id: apiProduct.id,
      name: apiProduct.name,
      description: apiProduct.description || '',
      price: apiProduct.price,
      originalPrice: apiProduct.originalPrice,
      imageUrl: this.imageUrlService.normalize(apiProduct.imageUrl),
      images: this.imageUrlService.normalizeImages(apiProduct.images || []),
      categoryId: apiProduct.categoryId,
      categoryName: apiProduct.categoryName,
      brandId: apiProduct.brandId,
      brandName: apiProduct.brandName,
      companyId: apiProduct.companyId,
      stockQuantity: apiProduct.stockQuantity || 0,
      rating: apiProduct.rating || 0,
      reviewCount: apiProduct.reviewCount || 0,
      isNew: apiProduct.isNew || false,
      discount: apiProduct.discount,
      isActive: apiProduct.isActive || false,
      inStock: (apiProduct.stockQuantity || 0) > 0,
      createdAt: new Date(apiProduct.createdAt)
    };
  }

  getAll(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<Product>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<PaginatedResponse<Product>>>(this.basePath, { params }).pipe(
      map(response => response.data!)
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

  create(product: any): Observable<Product> {
    return this.http.post<Product | ApiResponse<Product>>(this.basePath, product).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  update(id: number, product: any): Observable<Product> {
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

  getActiveCampaign(productId: number): Observable<ProductCampaign> {
    return this.http.get<ApiResponse<ProductCampaign>>(`${this.basePath}/${productId}/active-campaign`).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }
}
