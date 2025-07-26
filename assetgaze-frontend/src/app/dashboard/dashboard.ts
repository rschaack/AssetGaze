// assetgaze-frontend/src/app/dashboard/dashboard.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Header } from '../header/header';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    Header
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  protectedDataMessage: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.testProtectedPost();
  }

  testProtectedPost(): void {
    const testPayload = { item: 'Test Item', value: 123 };
    // The AuthInterceptor will now automatically add the Authorization header
    this.http.post('https://localhost:5002/api/auth/protected-data', testPayload)
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
