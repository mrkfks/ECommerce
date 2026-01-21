import { Routes } from '@angular/router';
import { Cart } from './pages/cart/cart';
import { CategoryProducts } from './pages/category-products/category-products';
import { Checkout } from './pages/checkout/checkout';
import { NotFoundComponent } from './pages/error/not-found/not-found.component';
import { ServerErrorComponent } from './pages/error/server-error/server-error.component';
import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { OrderConfirmation } from './pages/order-confirmation/order-confirmation';
import { OrderHistory } from './pages/order-history/order-history';
import { ProductDetail } from './pages/product-detail/product-detail';
import { Profile } from './pages/profile/profile';
import { Register } from './pages/register/register';


import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  { path: 'home', component: Home },
  { path: 'products/:categoryId', component: CategoryProducts },
  { path: 'product/:productId', component: ProductDetail },
  { path: 'cart', component: Cart },
  { path: 'checkout', component: Checkout, canActivate: [authGuard] },
  { path: 'order/:orderId', component: OrderConfirmation, canActivate: [authGuard] },

  { path: 'orders', component: OrderHistory, canActivate: [authGuard] },
  { path: 'profile', component: Profile, canActivate: [authGuard] },

  { path: 'login', component: Login },
  { path: 'register', component: Register },

  { path: 'error', component: ServerErrorComponent },
  { path: '404', component: NotFoundComponent },
  { path: '**', component: NotFoundComponent }
];

