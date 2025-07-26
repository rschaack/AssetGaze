// assetgaze-frontend/src/app/auth/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * A functional route guard that checks if a user is authenticated.
 * If the user is authenticated, it allows access to the route.
 * Otherwise, it redirects the user to the /login page.
 */
export const authGuard: CanActivateFn = (route, state) => {
  // Inject the AuthService and Router directly into the function
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true; // ✅ User is logged in, so allow access
  }

  // 🛑 User is not logged in, redirect to the login page
  router.navigate(['/login']);
  return false;
};
