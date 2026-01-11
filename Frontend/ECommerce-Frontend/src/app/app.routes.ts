import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { CategoryProducts } from './pages/category-products/category-products';
import { ProductDetail } from './pages/product-detail/product-detail';
import { Cart } from './pages/cart/cart';
import { Checkout } from './pages/checkout/checkout';
import { OrderConfirmation } from './pages/order-confirmation/order-confirmation';
import { OrderHistory } from './pages/order-history/order-history';
import { Profile } from './pages/profile/profile';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { NotFoundComponent } from './pages/error/not-found/not-found.component';
import { ServerErrorComponent } from './pages/error/server-error/server-error.component';


export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  { path: 'home', component: Home },
  { path: 'products/:categoryId', component: CategoryProducts },
  { path: 'product/:productId', component: ProductDetail },
  { path: 'cart', component: Cart },
  { path: 'checkout', component: Checkout },
  { path: 'order/:orderId', component: OrderConfirmation },

  { path: 'orders', component: OrderHistory },
  { path: 'profile', component: Profile },

  { path: 'login', component: Login },
  { path: 'register', component: Register },

  { path: 'error', component: ServerErrorComponent },
  { path: '404', component: NotFoundComponent },
  { path: '**', component: NotFoundComponent }
];

