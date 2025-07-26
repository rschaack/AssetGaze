// assetgaze-frontend/src/app/dashboard/dashboard.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // Added HttpHeaders
import { AuthService } from '../auth/auth.service';
import { Subscription } from 'rxjs';
import { RouterLink } from '@angular/router';
import { Header } from '../header/header'; // Import the new Header component

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    Header // Include Header component in imports
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit, OnDestroy {
  protectedDataMessage: string = 'Loading protected data...';
  private authSubscription: Subscription | undefined;

  constructor(private http: HttpClient, private authService: AuthService) {}

  ngOnInit(): void {
    // For simple auth, check localStorage directly or use isAuthenticated() synchronous check
    if (this.authService.isAuthenticated()) { // Check if token exists in localStorage
      this.testProtectedPost();
    } else {
      this.protectedDataMessage = 'Not authenticated to view data.';
      // The AuthGuard should have redirected, but this is a fallback for UI.
    }
  }

  ngOnDestroy(): void {
    // If authSubscription is still used for other dashboard-specific logic, keep it.
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  testProtectedPost(): void {
    const token = this.authService.getToken(); // Get the token from AuthService
    if (!token) {
      this.protectedDataMessage = 'No token found for protected POST.';
      console.warn('Dashboard: No token found for protected POST.');
      // The ErrorInterceptor should redirect on 401, but this handles the no-token case.
      return;
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}` // <--- CRITICAL: Attach the JWT token
    });

    const testPayload = { item: 'Test Item', value: 123 };
    // Pass the headers to the request options
    this.http.post('https://localhost:5002/api/auth/protected-data', testPayload, { headers: headers }).subscribe({
      next: (response: any) => {
        this.protectedDataMessage = 'Protected POST successful: ' + response.message;
        console.log('Protected POST successful:', response);
      },
      error: (err) => {
        this.protectedDataMessage = 'Protected POST failed: ' + (err.error?.message || err.message);
        console.error('Protected POST failed:', err);
        // ErrorInterceptor handles 401 redirection, so this might not be reached for 401.
      }
    });
  }
}
