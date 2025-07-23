// assetgaze-frontend/src/app/core/interceptors/error.interceptor.ts
import { Injectable, Injector } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service'; // Import AuthService

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private injector: Injector) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          const authService = this.injector.get(AuthService);
          console.warn('401 Unauthorized response caught by ErrorInterceptor. Redirecting to login.');
          // Call the public setter method on AuthService
          authService.setAuthenticatedState(false);
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          console.warn('403 Forbidden response caught by ErrorInterceptor.');
        }
        return throwError(() => error);
      })
    );
  }
}
