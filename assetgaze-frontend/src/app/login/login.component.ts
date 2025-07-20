// assetgaze-frontend/src/app/login/login.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // Import FormsModule for ngModel
import { CommonModule } from '@angular/common'; // Import CommonModule for directives like ngIf, ngFor

@Component({
  selector: 'app-login',
  standalone: true, // Mark as standalone component
  imports: [FormsModule, CommonModule], // Add FormsModule and CommonModule to imports for standalone components
  templateUrl: './login.component.html', // Points to the separate HTML file
  styleUrls: ['./login.component.css'] // Points to the separate CSS file
})
export class LoginComponent {
  username = '';
  password = '';

  /**
   * Handles the login form submission.
   * In a real application, this would involve calling an authentication service.
   */
  onLogin(): void {
    console.log('Login attempt:', { username: this.username, password: this.password });
    // TODO: Implement actual authentication logic here
    // For now, just a console log.
    if (this.username === 'user' && this.password === 'password') {
      // In a real app, you'd navigate to a dashboard or home page
      alert('Login successful!'); // Using alert for demo, replace with proper UI feedback
    } else {
      alert('Invalid username or password.'); // Using alert for demo, replace with proper UI feedback
    }
  }
}
