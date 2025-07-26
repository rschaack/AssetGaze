// assetgaze-frontend/src/app/transactions/transactions.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // For ngIf, ngFor
import { AuthService } from '../auth/auth.service'; // Import AuthService
import { Observable, catchError, of } from 'rxjs'; // For Observable, catchError, of
import { tap } from 'rxjs/operators';
import {RouterLink} from '@angular/router';

// Define the Transaction interface to match your backend DTO
interface Transaction {
  id: string;
  transactionType: string;
  brokerId: string;
  accountId: string;
  taxWrapper: string;
  isin: string;
  transactionDate: string;
  quantity: number;
  nativePrice: number;
  localPrice: number;
  consideration: number;
  brokerCharge?: number;
  stampDuty?: number;
  fxCharge?: number;
  accruedInterest?: number;
  brokerDealReference?: string;
}

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './transactions.html',
  styleUrls: ['./transactions.css']
})
export class Transactions implements OnInit {
  transactions$: Observable<Transaction[]> | undefined; // Observable to hold the list of transactions
  loading = true;
  error: string | null = null;

  constructor(private authService: AuthService) { } // Inject AuthService

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading = true;
    this.error = null;
    this.transactions$ = this.authService.getAllUsersTransactions().pipe(
      tap(() => this.loading = false), // Set loading to false on success
      catchError(err => {
        this.loading = false;
        this.error = err.message || 'Failed to load transactions.';
        console.error('Transactions Component: Error loading transactions', err);
        return of([]); // Return an empty array on error to prevent breaking the stream
      })
    );
  }

  // Helper to format dates (optional, can be done with Angular DatePipe)
  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}
