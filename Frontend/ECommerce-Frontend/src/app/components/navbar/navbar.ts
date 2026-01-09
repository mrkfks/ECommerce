import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CartService, AuthService } from '../../core/services';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  private cartService = inject(CartService);
  private authService = inject(AuthService);

  cartItemCount = this.cartService.totalItems;
  isAuthenticated = this.authService.isAuthenticated;
  currentUser$ = this.authService.currentUser$;

  logout(): void {
    this.authService.logout();
  }
}
