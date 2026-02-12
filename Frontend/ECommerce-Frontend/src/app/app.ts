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

