// assetgaze-frontend/src/app/transactions/transactions.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';
import { Observable, catchError, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { RouterLink } from '@angular/router';
import { Header } from '../header/header';
import { Transaction } from './transaction.interface'; // <--- NEW: Import shared Transaction interface

// Removed: Duplicate Transaction interface definition from here

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    Header
  ],
  templateUrl: './transactions.html',
  styleUrls: ['./transactions.css']
})
export class Transactions implements OnInit {
  transactions$: Observable<Transaction[]> | undefined;
  loading = true;
  error: string | null = null;

  constructor(private authService: AuthService) { }

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading = true;
    this.error = null;
    this.transactions$ = this.authService.getAllUsersTransactions().pipe(
      tap(() => this.loading = false),
      catchError(err => {
        this.loading = false;
        this.error = err.message || 'Failed to load transactions.';
        console.error('Transactions Component: Error loading transactions', err);
        return of([]);
      })
    );
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}
