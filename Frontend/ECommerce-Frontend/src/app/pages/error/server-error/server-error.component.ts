import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="error-page">
      <div class="error-content">
        <h1 class="error-code">500</h1>
        <h2 class="error-title">Sunucu Hatası</h2>
        <p class="error-message">
          Üzgünüz, bir sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.
        </p>
        @if (errorDetails) {
          <div class="alert alert-danger">
            <small>{{ errorDetails }}</small>
          </div>
        }
        <div class="error-actions">
          <a routerLink="/" class="btn btn-primary btn-lg">
            <i class="bi bi-house-door me-2"></i>
            Ana Sayfaya Dön
          </a>
          <button class="btn btn-outline-secondary btn-lg" (click)="reload()">
            <i class="bi bi-arrow-clockwise me-2"></i>
            Sayfayı Yenile
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
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
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

    .alert {
      margin: 1.5rem 0;
      background: rgba(255, 255, 255, 0.2);
      border: 1px solid rgba(255, 255, 255, 0.3);
      color: white;
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
export class ServerErrorComponent implements OnInit {
  errorDetails?: string;

  constructor(private router: Router) {
    const navigation = this.router.getCurrentNavigation();
    this.errorDetails = navigation?.extras?.state?.['error'];
  }

  ngOnInit(): void {}

  reload(): void {
    window.location.reload();
  }
}
