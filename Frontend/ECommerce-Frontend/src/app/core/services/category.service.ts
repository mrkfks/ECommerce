import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Category, CategoryCreateRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly basePath = '/category';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Category[]> {
    return this.http.get<ApiResponse<Category[]>>(this.basePath).pipe(
      map(response => response.data || [])
    );
  }

  getById(id: number): Observable<Category> {
    return this.http.get<ApiResponse<Category>>(`${this.basePath}/${id}`).pipe(
      map(response => response.data)
    );
  }

  create(category: CategoryCreateRequest): Observable<Category> {
    return this.http.post<ApiResponse<Category>>(this.basePath, category).pipe(
      map(response => response.data)
    );
  }

  update(id: number, category: Category): Observable<Category> {
    return this.http.put<ApiResponse<Category>>(`${this.basePath}/${id}`, category).pipe(
      map(response => response.data)
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.basePath}/${id}`);
  }
}
