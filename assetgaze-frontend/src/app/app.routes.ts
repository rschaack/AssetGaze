// assetgaze-frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { Login } from './login/login'; // Import your new login component
import { Dashboard } from './dashboard/dashboard';
import { AuthGuard } from './auth/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'dashboard', component: Dashboard, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/login', pathMatch: 'full' }, // Redirects to /login when the app starts
  // Add other routes for your application here (e.g., dashboard, home)
  // { path: 'dashboard', component: DashboardComponent },
  // { path: '**', redirectTo: '/login' } // Wildcard route for any unmatched paths
];
