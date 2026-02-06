import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LetDirective } from '../../shared/directives/let.directive';

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
  imports: [CommonModule, LetDirective],
  templateUrl: './campaign-carousel.component.html',
  styleUrls: ['./campaign-carousel.component.css']
})
export class CampaignCarouselComponent implements OnInit {
  private http = inject(HttpClient);
  
  campaigns: CampaignDto[] = [];
  isLoading = true;
  currentIndex = 0;
  private apiBaseUrl = '/api';

  ngOnInit(): void {
    this.loadActiveCampaigns();
  }

  loadActiveCampaigns(): void {
    this.http.get<CampaignDto[]>(`${this.apiBaseUrl}/campaigns/active`)
      .subscribe({
        next: (data) => {
          this.campaigns = data;
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
