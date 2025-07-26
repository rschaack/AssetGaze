import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TransactionService } from '../transaction.service';
import { Transaction } from '../transaction.interface';
import { CreateTransactionRequest, UpdateTransactionRequest } from '../transaction-requests.interface';
import { Header } from '../../header/header';

@Component({
  selector: 'app-transaction-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Header],
  templateUrl: './transaction-detail.html',
})
export class TransactionDetail implements OnInit {
private route = inject(ActivatedRoute);
private router = inject(Router);
private transactionService = inject(TransactionService);

// Use a model that matches the backend DTOs
transaction: Partial<Transaction> = {
  transactionType: 'Buy', // Default value
  taxWrapper: 'ISA' // Default value
};
isEditMode = false;
pageTitle = 'Add New Transaction';

ngOnInit(): void {
  const transactionId = this.route.snapshot.paramMap.get('id');
  const accountId = this.route.snapshot.paramMap.get('accountId');

  if (transactionId) { // We are in EDIT mode
    this.isEditMode = true;
    this.pageTitle = 'Edit Transaction';
    this.transactionService.getById(transactionId).subscribe(data => {
      this.transaction = data;
    });
  } else if (accountId) { // We are in CREATE mode
    this.isEditMode = false;
    this.pageTitle = 'Add New Transaction';
    this.transaction.accountId = accountId; // Pre-fill the account ID from the route
  }
}

onSubmit(): void {
  if (this.isEditMode) {
    // We assert that 'this.transaction' is a valid 'UpdateTransactionRequest'
    this.transactionService.update(this.transaction.id!, this.transaction as UpdateTransactionRequest).subscribe(() => {
      this.router.navigate(['/transactions']);
    });
  } else {
    // We do the same for the create request
    this.transactionService.create(this.transaction as CreateTransactionRequest).subscribe(() => {
      this.router.navigate(['/transactions']);
    });
  }
}
}
