// assetgaze-frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component'; // Import your new login component

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' }, // Redirects to /login when the app starts
  // Add other routes for your application here (e.g., dashboard, home)
  // { path: 'dashboard', component: DashboardComponent },
  // { path: '**', redirectTo: '/login' } // Wildcard route for any unmatched paths
];
