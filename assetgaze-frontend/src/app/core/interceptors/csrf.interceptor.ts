// assetgaze-frontend/src/app/core/interceptors/csrf.interceptor.ts
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpXsrfTokenExtractor // To extract the CSRF token from the cookie
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {

  constructor(private tokenExtractor: HttpXsrfTokenExtractor) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Only add the X-XSRF-TOKEN header to non-GET requests
    // Angular's HttpXsrfTokenExtractor automatically looks for a cookie named XSRF-TOKEN by default.
    // It also expects the header name to be X-XSRF-TOKEN by default.
    // These defaults match ASP.NET Core's Anti-Forgery configuration.

    const xsrfToken = this.tokenExtractor.getToken();

    if (xsrfToken !== null && !request.headers.has('X-XSRF-TOKEN')) {
      // Clone the request and add the X-XSRF-TOKEN header
      request = request.clone({
        setHeaders: {
          'X-XSRF-TOKEN': xsrfToken
        },
        withCredentials: true // Ensure cookies are sent with this request
      });
    } else if (request.method !== 'GET') {
      // For non-GET requests, if no XSRF token is found, it's a potential issue.
      // In a real app, you might want to log this or handle it more robustly.
      console.warn('CSRF token not found for a non-GET request. This might indicate an issue.');
      // You might also choose to throw an error or redirect to login.
    }

    return next.handle(request);
  }
}
