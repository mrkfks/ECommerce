import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Category } from '../../core/models';
import { AuthService, CartService, CategoryService, DesignService } from '../../core/services';

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

  cartItemCount = this.cartService.totalItems;
  isAuthenticated = this.authService.isAuthenticated;
  currentUser$ = this.authService.currentUser$;
  
  categories: Category[] = [];
  searchTerm: string = '';

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories = cats,
      error: (err) => console.error('Navbar categories load error', err)
    });
  }

  onSearch(term: string) {
    if (term && term.trim().length > 0) {
      this.router.navigate(['/products'], { queryParams: { search: term } });
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
