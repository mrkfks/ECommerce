import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loadingService.loading$ | async) {
      <div class="loading-overlay">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Yükleniyor...</span>
        </div>
      </div>
    }
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
    }

    .spinner-border {
      width: 3rem;
      height: 3rem;
      border-width: 0.3em;
    }
  `]
})
export class LoadingSpinnerComponent {
  constructor(public loadingService: LoadingService) {}
}
