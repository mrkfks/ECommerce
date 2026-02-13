import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, shareReplay } from 'rxjs';
import { Category, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly basePath = '/categories';
  private allCategoriesCache$: Observable<Category[]> | null = null;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Category[]> {
    if (!this.allCategoriesCache$) {
      this.allCategoriesCache$ = this.http.get<Category[] | ApiResponse<Category[]>>(this.basePath).pipe(
        map((response: any) => {
          if (Array.isArray(response)) return response;
          if (response?.data?.data && Array.isArray(response.data.data)) return response.data.data;
          if (response?.data && Array.isArray(response.data)) return response.data;
          return [];
        }),
        shareReplay(1)
      );
    }
    return this.allCategoriesCache$;
  }

  getById(id: number): Observable<Category> {
    return this.http.get<Category | ApiResponse<Category>>(`${this.basePath}/${id}`).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  create(category: any): Observable<Category> {
    return this.http.post<Category | ApiResponse<Category>>(this.basePath, category).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  update(id: number, category: Category): Observable<Category> {
    return this.http.put<Category | ApiResponse<Category>>(`${this.basePath}/${id}`, category).pipe(
      map((response: any) => ('data' in response ? response.data : response))
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.basePath}/${id}`);
  }
}
