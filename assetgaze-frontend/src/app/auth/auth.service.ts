// assetgaze-frontend/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of, map, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

interface LoginResponse {
  csrfToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'https://localhost:5002/api/auth/login';
  private statusUrl = 'https://localhost:5002/api/auth/status';
  private logoutUrl = 'https://localhost:5002/api/auth/logout';

  private _isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this._isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.checkAuthStatusOnAppLoad();
  }

  private checkAuthStatusOnAppLoad(): void {
    this.http.get(this.statusUrl, { withCredentials: true }).pipe(
      map(() => true),
      catchError(() => of(false))
    ).subscribe(isAuthenticated => {
      this._isAuthenticatedSubject.next(isAuthenticated);
      if (!isAuthenticated && !this.router.url.includes('/login')) {
        this.router.navigate(['/login']);
      }
    });
  }

  public setAuthenticatedState(state: boolean): void {
    this._isAuthenticatedSubject.next(state);
  }

  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const payload = {
      email: credentials.email,
      password: credentials.password
    };

    return this.http.post<LoginResponse>(this.loginUrl, payload, { withCredentials: true }).pipe(
      tap(response => {
        // --- LOGGING ---
        console.log('AuthService: Login successful. CSRF Token received in response body:', response.csrfToken);
        // --- END LOGGING ---
        this.setAuthenticatedState(true);
      }),
      catchError(error => {
        console.error('AuthService: Login failed:', error);
        this.setAuthenticatedState(false);
        let errorMessage = 'An unknown error occurred during login.';
        if (error.status === 401) {
          errorMessage = 'Invalid email or password.';
        } else if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.message) {
          errorMessage = error.message;
        }
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  checkBackendAuthStatus(): Observable<boolean> {
    return this.http.get(this.statusUrl, { withCredentials: true }).pipe(
      map(() => {
        this.setAuthenticatedState(true);
        return true;
      }),
      catchError(() => {
        this.setAuthenticatedState(false);
        return of(false);
      })
    );
  }

  logout(): void {
    this.http.post(this.logoutUrl, {}, { withCredentials: true }).pipe(
      tap(() => {
        console.log('AuthService: Logout successful.');
        this.setAuthenticatedState(false);
        this.router.navigate(['/login']);
      }),
      catchError(error => {
        console.error('AuthService: Logout failed:', error);
        this.setAuthenticatedState(false);
        this.router.navigate(['/login']);
        return throwError(() => new Error('Logout failed.'));
      })
    ).subscribe();
  }
}
