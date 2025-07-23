// assetgaze-frontend/src/app/auth/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { map, take, switchMap } from 'rxjs/operators'; // Added 'switchMap'

export const AuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Hybrid approach:
  // 1. Check client-side state first for quick UX (isAuthenticated$)
  // 2. If client-side state is true, perform a definitive backend check (checkBackendAuthStatus())
  // 3. If client-side state is false, or backend check fails, redirect to login.

  return authService.isAuthenticated$.pipe(
    take(1), // Take the current client-side state
    switchMap(isClientAuthenticated => {
      if (isClientAuthenticated) {
        // If client-side state is true, perform a definitive backend check
        return authService.checkBackendAuthStatus().pipe(
          map(isBackendAuthenticated => {
            if (isBackendAuthenticated) {
              return true; // Backend confirms, allow access
            } else {
              // Backend denies (e.g., token expired), redirect to login
              router.navigate(['/login']);
              return false;
            }
          })
        );
      } else {
        // If client-side state is false, directly attempt backend check (or just redirect)
        // For security, it's safer to always do a backend check if client-side is false
        // to avoid race conditions or stale client state.
        return authService.checkBackendAuthStatus().pipe(
          map(isBackendAuthenticated => {
            if (isBackendAuthenticated) {
              // This case means client-side was false but backend was true (e.g., app load race condition)
              return true;
            } else {
              router.navigate(['/login']);
              return false;
            }
          })
        );
      }
    })
  );
};
