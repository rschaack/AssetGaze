import { Component, OnInit, OnDestroy } from '@angular/core'; // Added OnInit, OnDestroy
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { Observable, Subscription } from 'rxjs'; // Added Observable, Subscription

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class Header implements OnInit, OnDestroy { // Implemented OnInit, OnDestroy
  isAuthenticated$: Observable<boolean>; // Expose auth state to template
  private authSubscription: Subscription | undefined; // To manage subscription

  constructor(private authService: AuthService) {
    this.isAuthenticated$ = this.authService.isAuthenticated$; // Assign observable
  }

  ngOnInit(): void {
    // Optional: You can subscribe here if you need side effects based on auth state
    // this.authSubscription = this.isAuthenticated$.subscribe(isAuth => {
    //   console.log('Header: Auth state changed to', isAuth);
    // });
  }

  ngOnDestroy(): void {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  /**
   * Handles the logout action from the header.
   * Calls the AuthService to clear the token and navigate to the login page.
   */
  onLogout(): void {
    this.authService.logout();
  }
}
