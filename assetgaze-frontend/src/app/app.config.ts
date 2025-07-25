// assetgaze-frontend/src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideClientHydration } from '@angular/platform-browser';

import { routes } from './app.routes';
// Removed: import { CsrfInterceptor } from './core/interceptors/csrf.interceptor'; // No longer needed
import { ErrorInterceptor } from './core/interceptors/error.interceptor'; // Still needed for 401 handling

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()), // Still needed for HttpClient and interceptors
    provideClientHydration(),

    // Removed: CsrfInterceptor provider
    // Provide the Error Interceptor (still needed)
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ]
};
