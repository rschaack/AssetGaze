// assetgaze-frontend/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // Added HttpHeaders
import { Observable, tap, catchError, throwError, of, map, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

interface LoginResponse {
  token: string; // Backend now returns the JWT directly
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'https://localhost:5002/api/auth/login';
  private statusUrl = 'https://localhost:5002/api/auth/status';
  private logoutUrl = 'https://localhost:5002/api/auth/logout';

  // BehaviorSubject to hold and emit the current authentication status
  // Initialized based on presence of token in localStorage
  private _isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this._isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    // No initial backend check on app load for simplicity, relies on localStorage
    // If token exists, assume authenticated until proven otherwise by a 401
  }

  /**
   * Checks if a token exists in localStorage.
   * @returns boolean - true if token exists, false otherwise.
   */
  private hasToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  /**
   * Public method to update the internal authentication state.
   */
  public setAuthenticatedState(state: boolean): void {
    this._isAuthenticatedSubject.next(state);
  }

  /**
   * Sends login credentials to the backend.
   * Stores the received JWT in localStorage.
   * @param credentials An object containing email and password.
   * @returns An Observable of the LoginResponse containing the JWT.
   */
  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const payload = {
      email: credentials.email,
      password: credentials.password
    };

    // No withCredentials needed as no cookies are involved from frontend
    return this.http.post<LoginResponse>(this.loginUrl, payload).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('authToken', response.token); // Store token in localStorage
          console.log('AuthService: Login successful. Token stored in localStorage.');
          this.setAuthenticatedState(true); // Update state
        }
      }),
      catchError(error => {
        console.error('AuthService: Login failed:', error);
        localStorage.removeItem('authToken'); // Clear any stale token
        this.setAuthenticatedState(false); // Update state
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

  /**
   * Retrieves the authentication token from localStorage.
   * @returns The authentication token string, or null if not found.
   */
  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  /**
   * Checks if the user is currently authenticated based on the token in localStorage.
   * This is a client-side check. For definitive validation, a backend call is needed.
   * @returns boolean - true if authenticated, false otherwise.
   */
  isAuthenticated(): boolean {
    return this.hasToken();
  }

  /**
   * Logs out the user by clearing the token from localStorage and navigating.
   * No backend call needed to clear token as it's not a cookie.
   */
  logout(): void {
    localStorage.removeItem('authToken'); // Clear token from localStorage
    this.setAuthenticatedState(false); // Update state
    console.log('AuthService: Logout successful. Token removed from localStorage.');
    this.router.navigate(['/login']);
    // Optional: If backend has a logout endpoint that clears server-side session, call it here.
    // this.http.post(this.logoutUrl, {}).subscribe();
  }
}
