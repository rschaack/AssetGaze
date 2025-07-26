// assetgaze-frontend/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of, map, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { Transaction } from '../shared/interfaces/transaction.interface'; // <--- NEW: Import shared Transaction interface

interface LoginResponse {
  token: string;
}

// Removed: Duplicate Transaction interface definition from here


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'https://localhost:5002/api/auth/login';
  private statusUrl = 'https://localhost:5002/api/auth/status';
  private logoutUrl = 'https://localhost:5002/api/auth/logout';
  private transactionsUrl = 'https://localhost:5002/api/transactions/user';

  private _isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this._isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  private hasToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  public setAuthenticatedState(state: boolean): void {
    this._isAuthenticatedSubject.next(state);
  }

  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const payload = {
      email: credentials.email,
      password: credentials.password
    };

    return this.http.post<LoginResponse>(this.loginUrl, payload).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('authToken', response.token);
          console.log('AuthService: Login successful. Token stored in localStorage.');
          this.setAuthenticatedState(true);
        }
      }),
      catchError(error => {
        console.error('AuthService: Login failed:', error);
        localStorage.removeItem('authToken');
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

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return this.hasToken();
  }

  logout(): void {
    localStorage.removeItem('authToken');
    this.setAuthenticatedState(false);
    console.log('AuthService: Logout successful. Token removed from localStorage.');
    this.router.navigate(['/login']);
  }

  getAllUsersTransactions(): Observable<Transaction[]> {
    const token = this.getToken();
    if (!token) {
      this.router.navigate(['/login']);
      return throwError(() => new Error('No authentication token found.'));
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get<Transaction[]>(this.transactionsUrl, { headers }).pipe(
      catchError(error => {
        console.error('AuthService: Failed to fetch transactions:', error);
        return throwError(() => new Error('Failed to fetch transactions.'));
      })
    );
  }
}
