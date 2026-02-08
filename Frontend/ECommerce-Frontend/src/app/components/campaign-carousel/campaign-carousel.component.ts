import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CampaignDto {
  id: number;
  name: string;
  description?: string;
  bannerImageUrl?: string;
  discountPercent: number;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
  isCurrentlyActive: boolean;
  remainingDays: number;
  companyId: number;
  companyName: string;
  createdAt: Date;
}

@Component({
  selector: 'app-campaign-carousel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaign-carousel.component.html',
  styleUrls: ['./campaign-carousel.component.css']
})
export class CampaignCarouselComponent implements OnInit {
  private http = inject(HttpClient);
  
  campaigns: CampaignDto[] = [];
  isLoading = true;
  currentIndex = 0;

  ngOnInit(): void {
    this.loadActiveCampaigns();
  }

  loadActiveCampaigns(): void {
    // apiInterceptor adds environment.apiUrl (http://localhost:5010/api) prefix automatically
    // So we only need /campaigns/active, not /api/campaigns/active
    this.http.get<{success: boolean, data: CampaignDto[], message: string}>('/campaigns/active')
      .subscribe({
        next: (response) => {
          // Backend returns ApiResponse<CampaignDto[]> with {success, data, message}
          // Extract campaigns from data property
          this.campaigns = response.data || [];
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Kampanyalar yüklenemedi:', err);
          this.isLoading = false;
        }
      });
  }

  nextSlide(): void {
    if (this.campaigns.length > 0) {
      this.currentIndex = (this.currentIndex + 1) % this.campaigns.length;
    }
  }

  prevSlide(): void {
    if (this.campaigns.length > 0) {
      this.currentIndex = (this.currentIndex - 1 + this.campaigns.length) % this.campaigns.length;
    }
  }

  goToSlide(index: number): void {
    if (index >= 0 && index < this.campaigns.length) {
      this.currentIndex = index;
    }
  }

  getCurrentCampaign(): CampaignDto | null {
    return this.campaigns[this.currentIndex] || null;
  }

  getDiscountBadgeColor(discount: number): string {
    if (discount >= 50) return '#ff4444'; // Red for high discount
    if (discount >= 30) return '#ff8800'; // Orange for medium
    return '#0099cc'; // Blue for low
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('tr-TR');
  }
}
