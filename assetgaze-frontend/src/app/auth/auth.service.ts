// assetgaze-frontend/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';

interface LoginResponse {
  csrfToken: string; // Backend now returns the CSRF token in the body
  // Add other non-sensitive user data here if your backend sends it
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'https://localhost:5002/api/auth/login'; // Your C# backend login endpoint

  constructor(private http: HttpClient, private router: Router) { }

  /**
   * Sends login credentials to the backend.
   * The JWT (access_token) will be set in an HTTP-only cookie by the backend.
   * The CSRF token will be returned in the response body.
   * @param credentials An object containing email and password.
   * @returns An Observable of the LoginResponse containing the CSRF token.
   */
  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const payload = {
      email: credentials.email, // Ensure this matches your C# LoginRequest DTO
      password: credentials.password
    };

    // The 'withCredentials: true' option is crucial to send cookies (including HTTP-only ones)
    // with cross-origin requests.
    return this.http.post<LoginResponse>(this.loginUrl, payload, { withCredentials: true }).pipe(
      tap(response => {
        // We no longer store the JWT in localStorage.
        // The CSRF token is received here, but the HTTP Interceptor will handle its usage.
        console.log('Login successful. CSRF Token received:', response.csrfToken);
        // You might store other non-sensitive user data from the response if needed.
      }),
      catchError(error => {
        console.error('Login failed:', error);
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
   * Checks if the user is authenticated. With HTTP-only cookies, we can't directly
   * read the access token. This method now relies on a backend endpoint that
   * checks cookie validity, or a simple check for a 'logged in' state if you store it.
   * For now, it's a placeholder. A real implementation might hit a /api/auth/status endpoint.
   * @returns boolean - true if authenticated, false otherwise.
   */
  isAuthenticated(): boolean {
    // With HTTP-only cookies, we cannot directly read the 'authToken' from localStorage.
    // A robust solution would involve:
    // 1. Making a small, authorized request to a backend endpoint (e.g., /api/auth/status)
    //    that checks the cookie and returns a boolean or user info.
    // 2. Storing a simple 'loggedIn' flag in sessionStorage (less secure, but better than localStorage for JWT itself)
    //    that gets set on successful login and cleared on logout/refresh.
    // For now, we'll assume the presence of a CSRF token (if you want to use it for this check).
    // A more accurate check would be to try an authenticated API call.
    return true; // Placeholder: You'll need a real check here.
  }

  /**
   * Logs out the user. This will typically involve a backend call to clear the cookie.
   */
  logout(): void {
    // In a secure setup, you'd make a backend call to invalidate the session/cookie.
    // For now, we'll just navigate. The browser will eventually clear the cookie.
    // If you had a refresh token, you'd send it to the backend to revoke.
    console.log('Logging out...');
    this.router.navigate(['/login']);
  }

  // getToken() method is removed as we no longer access the token directly from JS.
}
