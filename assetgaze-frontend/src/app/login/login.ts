// assetgaze-frontend/src/app/login/login.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html', // Updated: points to login.html
  styleUrls: ['./login.css'] // Updated: points to login.css
})
export class Login { // <--- CLASS NAME CHANGED TO 'Login'
  email = '';
  password = '';
  errorMessage: string | null = null;

  constructor(private authService: AuthService, private router: Router) { }

  onLogin(): void {
    this.errorMessage = null;
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error('Login failed in component:', err);
        this.errorMessage = err.message || 'Login failed. Please try again.';
      }
    });
  }
}
