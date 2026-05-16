import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { ProductList } from './features/products/product-list/product-list';
import { ProductDetails } from './features/products/product-details/product-details';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { Cartcomponent } from './features/cartcomponent/cartcomponent';
import { Checkout } from './features/checkout/checkout';
import { OrderHistory } from './features/orders/order-history/order-history';
import { OrderTracking } from './features/orders/order-tracking/order-tracking';
import { AdminDashboard } from './features/admin/admin-dashboard/admin-dashboard';
import { AdminProducts } from './features/admin/admin-products/admin-products';
import { AdminCategories } from './features/admin/admin-categories/admin-categories';
import { AdminOrders } from './features/admin/admin-orders/admin-orders';
import { AdminUsers } from './features/admin/admin-users/admin-users';
import { guestGuard } from './core/guards/guest-guard';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin-guard';
import { WishlistComponent } from './features/wishlist/wishlist';

// products
export const routes: Routes = [
  { path: '',             component: Home },
  { path: 'products',     component: ProductList },
  { path: 'products/:id', component: ProductDetails },
// Auth
  { path: 'login',    component: Login,    canActivate: [guestGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },

  { path: 'cart',       component: Cartcomponent,    canActivate: [authGuard] },
  { path: 'checkout',   component: Checkout,          canActivate: [authGuard] },
  { path: 'wishlist',   component: WishlistComponent, canActivate: [authGuard] },
  { path: 'orders',     component: OrderHistory,      canActivate: [authGuard] },
  { path: 'orders/:id', component: OrderTracking,     canActivate: [authGuard] },
// admin
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: '',           component: AdminDashboard },
      { path: 'dashboard',  component: AdminDashboard },
      { path: 'products',   component: AdminProducts },
      { path: 'categories', component: AdminCategories },
      { path: 'orders',     component: AdminOrders },
      { path: 'users',      component: AdminUsers }
    ]
  },

// fallback
  { path: '**', redirectTo: '' }
];