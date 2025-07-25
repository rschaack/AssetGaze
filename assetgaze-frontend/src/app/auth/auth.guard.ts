// assetgaze-frontend/src/app/auth/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { map, take } from 'rxjs/operators'; // Removed 'switchMap' as it's no longer needed

export const AuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Simple approach: Check client-side token presence.
  // The ErrorInterceptor will handle redirection if a protected backend call returns 401.

  // Use the BehaviorSubject's current value for immediate check
  // and then subscribe to ensure it's up-to-date.
  return authService.isAuthenticated$.pipe(
    take(1), // Take the current value
    map(isAuthenticated => {
      if (isAuthenticated) {
        return true; // User is authenticated, allow access
      } else {
        // User is not authenticated, redirect to login page
        router.navigate(['/login']);
        return false;
      }
    })
  );
};
