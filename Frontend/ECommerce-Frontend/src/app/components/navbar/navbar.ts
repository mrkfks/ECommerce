import { Component, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Category } from '../../core/models';
import { AuthService, CartService, CategoryService, DesignService } from '../../core/services';
import { LoggerService } from '../../core/services/logger.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  public designService = inject(DesignService);
  private platformId = inject(PLATFORM_ID);
  private logger = inject(LoggerService);

  cartItemCount = this.cartService.totalItems;
  isAuthenticated = this.authService.isAuthenticated;
  currentUser$ = this.authService.currentUser$;
  
  categories: Category[] = [];
  searchTerm: string = '';

  ngOnInit() {
    // SSR sırasında API istekleri yapma
    if (isPlatformBrowser(this.platformId)) {
      this.loadCategories();
    }
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories = cats,
      error: (err) => this.logger.error('Navbar categories load error', err)
    });
  }

  onSearch(term: string) {
    if (term && term.trim().length > 0) {
      this.logger.debug('Arama terimi:', term);
      // yönlendirmeyi products/all üzerine yapıyoruz, böylece CategoryProducts bileşeni yüklenir
      this.router.navigate(['/products', 'all'], { queryParams: { search: term } });
    } else {
      this.logger.warn('Arama terimi boş veya geçersiz.');
    }
  }

  logout(): void {
    this.authService.logout();
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/no-image.svg';
  }
}
