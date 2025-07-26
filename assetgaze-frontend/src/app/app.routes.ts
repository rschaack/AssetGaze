// assetgaze-frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';
import { Transactions } from './transactions/transactions';
import { authGuard } from './auth/auth.guard';
import {TransactionDetail} from './transactions/transaction-detail/transaction-detail'; // ⬅️ Import the new functional guard

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
  {
    path: 'transactions/new/:accountId', // ✅ Pass accountId for creating a new transaction
    component: TransactionDetail,
    canActivate: [authGuard]
  },
  {
    path: 'transactions/edit/:id', // ✅ Pass transactionId for editing
    component: TransactionDetail,
    canActivate: [authGuard]
  },
  // ... other routes
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '/dashboard' }
];
