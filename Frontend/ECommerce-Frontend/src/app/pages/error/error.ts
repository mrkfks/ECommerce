import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-error',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="error-container">
      <div class="error-content">
        <div class="error-code">⚠️</div>
        <h1 class="error-title">{{ title }}</h1>
        <p class="error-message">{{ message }}</p>
        
        <div class="error-actions">
          <button class="btn btn-primary" (click)="refresh()">
            <span>🔄 Sayfayı Yenile</span>
          </button>
          <a routerLink="/" class="btn btn-secondary">
            <span>🏠 Ana Sayfaya Dön</span>
          </a>
        </div>

        <div class="error-details">
          <p class="error-subtext">
            Sunucu şu anda erişilebilir değil. Lütfen internet bağlantınızı kontrol edin ve tekrar deneyin.
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
        'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
        sans-serif;
      padding: 20px;
    }

    .error-content {
      background: white;
      border-radius: 16px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      padding: 60px 40px;
      text-align: center;
      max-width: 600px;
      animation: slideUp 0.6s ease-out;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .error-code {
      font-size: 80px;
      margin-bottom: 20px;
      display: block;
    }

    .error-title {
      font-size: 48px;
      font-weight: 700;
      color: #2d3748;
      margin: 0 0 15px 0;
      line-height: 1.2;
    }

    .error-message {
      font-size: 18px;
      color: #718096;
      margin: 0 0 40px 0;
      line-height: 1.6;
    }

    .error-actions {
      display: flex;
      gap: 15px;
      justify-content: center;
      margin-bottom: 40px;
      flex-wrap: wrap;
    }

    .btn {
      padding: 14px 32px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 8px;
      transition: all 0.3s ease;
      min-width: 200px;
      justify-content: center;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .btn-primary:hover {
      transform: translateY(-2px);
      box-shadow: 0 10px 20px rgba(102, 126, 234, 0.4);
    }

    .btn-primary:active {
      transform: translateY(0);
    }

    .btn-secondary {
      background: #f0f4ff;
      color: #667eea;
      border: 2px solid #667eea;
    }

    .btn-secondary:hover {
      background: #667eea;
      color: white;
      transform: translateY(-2px);
    }

    .btn-secondary:active {
      transform: translateY(0);
    }

    .error-details {
      border-top: 1px solid #e2e8f0;
      padding-top: 30px;
    }

    .error-subtext {
      font-size: 14px;
      color: #a0aec0;
      margin: 0;
      line-height: 1.6;
    }

    @media (max-width: 768px) {
      .error-content {
        padding: 40px 20px;
      }

      .error-title {
        font-size: 36px;
      }

      .error-message {
        font-size: 16px;
        margin-bottom: 30px;
      }

      .error-code {
        font-size: 60px;
      }

      .btn {
        min-width: 160px;
        padding: 12px 24px;
        font-size: 14px;
      }

      .error-actions {
        flex-direction: column;
      }
    }
  `]
})
export class ErrorPage {
  @Input() title = 'Sayfa Bulunamadı';
  @Input() message = 'Sunucuya bağlanılamıyor';

  refresh() {
    window.location.reload();
  }
}
