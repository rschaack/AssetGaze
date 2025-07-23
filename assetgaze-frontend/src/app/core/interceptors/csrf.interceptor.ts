// assetgaze-frontend/src/app/core/interceptors/csrf.interceptor.ts
import { Injectable, Injector } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpXsrfTokenExtractor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { AuthService } from '../../auth/auth.service'; // Import AuthService

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {

  constructor(private tokenExtractor: HttpXsrfTokenExtractor, private injector: Injector) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const xsrfToken = this.tokenExtractor.getToken();

    // --- LOGGING ---
    console.log('CsrfInterceptor: Extracted XSRF-TOKEN from cookie:', xsrfToken);
    // --- END LOGGING ---

    if (xsrfToken !== null && !request.headers.has('X-XSRF-TOKEN')) {
      request = request.clone({
        setHeaders: {
          'X-XSRF-TOKEN': xsrfToken
        },
        withCredentials: true
      });
      // --- LOGGING ---
      console.log('CsrfInterceptor: Added X-XSRF-TOKEN header to request:', xsrfToken);
      // --- END LOGGING ---
    } else if (request.method !== 'GET') {
      console.warn('CsrfInterceptor: CSRF token not found for a non-GET request. This might indicate an issue.');
    }

    return next.handle(request);
  }
}
