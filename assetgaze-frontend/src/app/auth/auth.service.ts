// assetgaze-frontend/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root' // Makes the service a singleton and available throughout the app
})
export class AuthService {
  // Replace with the actual URL of your C# backend's login endpoint
  private loginUrl = 'https://localhost:5002/api/auth/login'; // Example: Adjust port and path as needed

  constructor(private http: HttpClient, private router: Router) { }

  /**
   * Sends login credentials to the backend and handles the response.
   * @param credentials An object containing email and password.
   * @returns An Observable of the login response (e.g., { token: string }).
   */
  login(credentials: { email: string; password: string }): Observable<{ token: string }> {

    const payload = {
      email: credentials.email, // Assuming your C# DTO expects 'Email'
      password: credentials.password
    };

    console.log('Sending login request with payload:', payload);

    return this.http.post<{ token: string }>(this.loginUrl, credentials).pipe(
      tap(response => {
        // Assuming your backend returns a token upon successful login
        if (response && response.token) {
          localStorage.setItem('authToken', response.token); // Store the token securely
          // You might also want to store user details if returned
          console.log('Login successful, token stored:', response.token);
        }
      }),
      catchError(error => {
        console.error('Login failed:', error);
        // Depending on your backend's error structure, you might extract a specific message
        let errorMessage = 'An unknown error occurred during login.';
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.status === 401) {
          errorMessage = 'Invalid email or password.';
        } else if (error.message) {
          errorMessage = error.message;
        }
        return throwError(() => new Error(errorMessage)); // Re-throw with a user-friendly message
      })
    );
  }

  /**
   * Checks if the user is currently authenticated by verifying the presence of a token.
   * @returns boolean - true if authenticated, false otherwise.
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  }

  /**
   * Logs out the user by removing the authentication token and navigating to the login page.
   */
  logout(): void {
    localStorage.removeItem('authToken'); // Remove the stored token
    // You might also clear other user-related data from storage
    this.router.navigate(['/login']); // Navigate back to the login page
  }

  /**
   * Retrieves the authentication token from local storage.
   * @returns The authentication token string, or null if not found.
   */
  getToken(): string | null {
    return localStorage.getItem('authToken');
  }
}
