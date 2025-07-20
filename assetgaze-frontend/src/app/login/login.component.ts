// assetgaze-frontend/src/app/login/login.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service'; // Import the new AuthService
import { Router } from '@angular/router'; // Import Router for navigation

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage: string | null = null; // To display error messages on the UI

  constructor(private authService: AuthService, private router: Router) { }

  /**
   * Handles the login form submission.
   * Calls the authentication service to log in the user.
   */
  onLogin(): void {
    this.errorMessage = null; // Clear previous errors
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        // Navigate to a protected route (e.g., dashboard) upon success
        this.router.navigate(['/dashboard']); // Make sure you have a '/dashboard' route defined
      },
      error: (err) => {
        console.error('Login failed in component:', err);
        this.errorMessage = err.message || 'Login failed. Please try again.'; // Display error on UI
      }
    });
  }
}
