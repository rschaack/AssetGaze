// assetgaze-frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';
import { Transactions } from './transactions/transactions'; // Import the new Transactions component
import { AuthGuard } from './auth/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [AuthGuard]
  },
  {
    path: 'transactions', // New route for transactions
    component: Transactions,
    canActivate: [AuthGuard] // Protect the transactions route
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  // Add other routes for your application here
];
