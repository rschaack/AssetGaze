// assetgaze-frontend/src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
// Removed: import { provideClientHydration } from '@angular/platform-browser';
// Removed: import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { routes } from './app.routes';
import { CsrfInterceptor } from './core/interceptors/csrf.interceptor'; // Import your CSRF interceptor

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), // Essential for routing
    provideHttpClient(withInterceptorsFromDi()), // Essential for HTTP client and interceptors

    // Provide the CSRF Interceptor
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CsrfInterceptor,
      multi: true // Essential if you plan to have multiple interceptors
    }
  ]
};
