import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Campaign {
  id: number;
  name: string;
  description?: string;
  discountType: 'Percentage' | 'Fixed';
  discountValue: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  code?: string;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
  usageLimit?: number;
  usageCount: number;
  imageUrl?: string;
  companyId?: number;
}

export interface CampaignValidation {
  isValid: boolean;
  message: string;
  discountAmount?: number;
  campaign?: Campaign;
}

@Injectable({
  providedIn: 'root',
})
export class CampaignService {
  private http = inject(HttpClient);
  private apiUrl = '/campaigns';

  /**
   * Get all active campaigns for display
   */
  getActive(): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.apiUrl}/active`);
  }

  /**
   * Get a specific campaign by ID
   */
  getById(id: number): Observable<Campaign> {
    return this.http.get<Campaign>(`${this.apiUrl}/${id}`);
  }

  /**
   * Validate and apply a campaign code
   */
  validateCode(code: string, orderTotal: number): Observable<CampaignValidation> {
    return this.http.post<CampaignValidation>(`${this.apiUrl}/validate`, {
      code,
      orderTotal,
    });
  }

  /**
   * Calculate discount for a campaign
   */
  calculateDiscount(campaignId: number, orderTotal: number): Observable<{ discountAmount: number; finalTotal: number }> {
    return this.http.get<{ discountAmount: number; finalTotal: number }>(
      `${this.apiUrl}/${campaignId}/calculate?orderTotal=${orderTotal}`
    );
  }

  /**
   * Get campaign banner for homepage display
   */
  getBannerCampaigns(): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.apiUrl}/banners`);
  }
}
