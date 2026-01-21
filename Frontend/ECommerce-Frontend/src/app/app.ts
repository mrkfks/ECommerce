import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from './components/footer/footer';
import { Navbar } from './components/navbar/navbar';
import { DesignService } from './core/services';
import { LoadingSpinnerComponent } from './shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, Footer, LoadingSpinnerComponent],
  template: `
    <div class="d-flex flex-column min-vh-100">
      <app-navbar></app-navbar>
      <main class="flex-grow-1">
        <router-outlet></router-outlet>
      </main>
      <app-footer></app-footer>
      <app-loading-spinner></app-loading-spinner>
    </div>
  `
})
export class App implements OnInit {
  private designService = inject(DesignService);

  ngOnInit() {
    this.designService.loadSettings();
  }
}

