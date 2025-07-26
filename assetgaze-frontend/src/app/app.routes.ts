// assetgaze-frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';
import { Transactions } from './transactions/transactions';
import { authGuard } from './auth/auth.guard'; // ⬅️ Import the new functional guard

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [authGuard] // 🔒 Use the functional guard
  },
  {
    path: 'transactions',
    component: Transactions,
    canActivate: [authGuard] // 🔒 Use the functional guard
  },
  // ... other routes
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '/dashboard' }
];
