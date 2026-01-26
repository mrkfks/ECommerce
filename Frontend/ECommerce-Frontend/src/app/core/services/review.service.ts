import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Review {
  id: number;
  productId: number;
  productName?: string;
  customerId: number;
  customerName?: string;
  rating: number;
  title?: string;
  comment: string;
  isApproved: boolean;
  createdAt: Date;
}

export interface ReviewCreate {
  productId: number;
  rating: number;
  title?: string;
  comment: string;
}

export interface ProductReviewSummary {
  productId: number;
  averageRating: number;
  totalReviews: number;
  ratingDistribution: { [key: number]: number };
}

@Injectable({
  providedIn: 'root',
})
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = '/reviews';

  /**
   * Get all reviews for a product
   */
  getByProduct(productId: number): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/product/${productId}`);
  }

  /**
   * Get review summary for a product (average rating, distribution)
   */
  getProductSummary(productId: number): Observable<ProductReviewSummary> {
    return this.http.get<ProductReviewSummary>(`${this.apiUrl}/product/${productId}/summary`);
  }

  /**
   * Get reviews by current customer
   */
  getMyReviews(): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/my`);
  }

  /**
   * Create a new review
   */
  create(review: ReviewCreate): Observable<Review> {
    return this.http.post<Review>(this.apiUrl, review);
  }

  /**
   * Update an existing review
   */
  update(id: number, review: ReviewCreate): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, review);
  }

  /**
   * Delete a review
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Check if current customer can review a product (has purchased it)
   */
  canReview(productId: number): Observable<{ canReview: boolean; hasPurchased: boolean; hasReviewed: boolean }> {
    return this.http.get<{ canReview: boolean; hasPurchased: boolean; hasReviewed: boolean }>(
      `${this.apiUrl}/can-review/${productId}`
    );
  }
}
