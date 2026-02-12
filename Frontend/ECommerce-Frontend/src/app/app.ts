import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from './components/footer/footer';
import { Navbar } from './components/navbar/navbar';
import { DesignService } from './core/services';
import { LoadingSpinnerComponent } from './shared/components/loading-spinner/loading-spinner.component';
import { CommonModule } from '@angular/common';
import { ErrorPage } from './pages/error/error';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, Footer, LoadingSpinnerComponent, CommonModule, ErrorPage],
  template: `
    <div *ngIf="errorMessage" class="app-shell">
      <app-error 
        [title]="'Sayfa Bulunamadı'" 
        [message]="errorMessage">
      </app-error>
    </div>
    <div *ngIf="!errorMessage" class="app-shell">
      <app-navbar></app-navbar>
      <main class="page-container">
        <router-outlet></router-outlet>
      </main>

      <section class="features-section py-5 bg-light">
        <div class="container">
          <div class="row g-4">
            <div class="col-md-3">
              <div class="feature-item text-center">
                <div class="feature-icon mb-3">
                  <i class="bi bi-truck fs-1 text-primary"></i>
                </div>
                <h6 class="fw-bold">Hızlı Teslimat</h6>
                <p class="text-muted small mb-0">1-3 iş günü içinde kapınızda</p>
              </div>
            </div>
            <div class="col-md-3">
              <div class="feature-item text-center">
                <div class="feature-icon mb-3">
                  <i class="bi bi-shield-check fs-1 text-primary"></i>
                </div>
                <h6 class="fw-bold">Güvenli Ödeme</h6>
                <p class="text-muted small mb-0">256-bit SSL sertifikası</p>
              </div>
            </div>
            <div class="col-md-3">
              <div class="feature-item text-center">
                <div class="feature-icon mb-3">
                  <i class="bi bi-arrow-counterclockwise fs-1 text-primary"></i>
                </div>
                <h6 class="fw-bold">Kolay İade</h6>
                <p class="text-muted small mb-0">14 gün içinde ücretsiz iade</p>
              </div>
            </div>
            <div class="col-md-3">
              <div class="feature-item text-center">
                <div class="feature-icon mb-3">
                  <i class="bi bi-headset fs-1 text-primary"></i>
                </div>
                <h6 class="fw-bold">7/24 Destek</h6>
                <p class="text-muted small mb-0">Her zaman yanınızdayız</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <app-footer></app-footer>
      <app-loading-spinner></app-loading-spinner>
    </div>
  `
})
export class App implements OnInit {
  private designService = inject(DesignService);
  loadError = this.designService.loadError;

  ngOnInit() {
    this.designService.loadSettings();
  }

  get errorMessage() {
    return this.loadError();
  }
}

