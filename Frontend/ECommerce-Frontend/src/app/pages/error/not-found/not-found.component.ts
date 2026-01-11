import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="error-page">
      <div class="error-content">
        <h1 class="error-code">404</h1>
        <h2 class="error-title">Sayfa Bulunamadı</h2>
        <p class="error-message">
          Aradığınız sayfa mevcut değil veya taşınmış olabilir.
        </p>
        <div class="error-actions">
          <a routerLink="/" class="btn btn-primary btn-lg">
            <i class="bi bi-house-door me-2"></i>
            Ana Sayfaya Dön
          </a>
          <button class="btn btn-outline-secondary btn-lg" (click)="goBack()">
            <i class="bi bi-arrow-left me-2"></i>
            Geri Dön
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .error-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 2rem;
    }

    .error-content {
      text-align: center;
      color: white;
      max-width: 600px;
    }

    .error-code {
      font-size: 10rem;
      font-weight: 700;
      margin: 0;
      line-height: 1;
      text-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    }

    .error-title {
      font-size: 2.5rem;
      margin: 1rem 0;
      font-weight: 600;
    }

    .error-message {
      font-size: 1.2rem;
      margin: 1.5rem 0 2.5rem;
      opacity: 0.9;
    }

    .error-actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .btn {
      display: inline-flex;
      align-items: center;
    }
  `]
})
export class NotFoundComponent {
  goBack(): void {
    window.history.back();
  }
}
