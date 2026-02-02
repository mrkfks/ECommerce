import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  { 
    path: 'home', 
    loadComponent: () => import('./pages/home/home').then(m => m.Home)
  },
  { 
    path: 'products/:categoryId', 
    loadComponent: () => import('./pages/category-products/category-products').then(m => m.CategoryProducts)
  },
  { 
    path: 'product/:productId', 
    loadComponent: () => import('./pages/product-detail/product-detail').then(m => m.ProductDetail)
  },
  { 
    path: 'cart', 
    loadComponent: () => import('./pages/cart/cart').then(m => m.Cart)
  },
  { 
    path: 'checkout', 
    loadComponent: () => import('./pages/checkout/checkout').then(m => m.Checkout),
    canActivate: [authGuard]
  },
  { 
    path: 'order/:orderId', 
    loadComponent: () => import('./pages/order-confirmation/order-confirmation').then(m => m.OrderConfirmation),
    canActivate: [authGuard]
  },
  { 
    path: 'orders', 
    loadComponent: () => import('./pages/order-history/order-history').then(m => m.OrderHistory),
    canActivate: [authGuard]
  },
  { 
    path: 'profile', 
    loadComponent: () => import('./pages/profile/profile').then(m => m.Profile),
    canActivate: [authGuard]
  },
  { 
    path: 'login', 
    loadComponent: () => import('./pages/login/login').then(m => m.Login)
  },
  { 
    path: 'register', 
    loadComponent: () => import('./pages/register/register').then(m => m.Register)
  },
  { 
    path: 'error', 
    loadComponent: () => import('./pages/error/server-error/server-error.component').then(m => m.ServerErrorComponent)
  },
  { 
    path: '404', 
    loadComponent: () => import('./pages/error/not-found/not-found.component').then(m => m.NotFoundComponent)
  },
  { 
    path: '**', 
    loadComponent: () => import('./pages/error/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];

