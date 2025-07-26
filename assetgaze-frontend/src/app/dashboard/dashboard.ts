// assetgaze-frontend/src/app/dashboard/dashboard.ts (example snippet)
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http'; // Import HttpClient
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink // <--- ADDED: Include RouterLink in imports
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit { // Implement OnInit
  protectedDataMessage: string = '';

  constructor(private http: HttpClient) {} // Inject HttpClient

  ngOnInit(): void {
    this.testProtectedPost(); // Call test method on init
  }

  testProtectedPost(): void {
    const testPayload = { item: 'Test Item', value: 123 };
    this.http.post('https://localhost:5002/api/auth/protected-data', testPayload, { withCredentials: true })
      .subscribe({
        next: (response: any) => {
          this.protectedDataMessage = response.message;
          console.log('Protected POST successful:', response);
        },
        error: (err) => {
          this.protectedDataMessage = 'Protected POST failed: ' + (err.error?.message || err.message);
          console.error('Protected POST failed:', err);
        }
      });
  }
}
