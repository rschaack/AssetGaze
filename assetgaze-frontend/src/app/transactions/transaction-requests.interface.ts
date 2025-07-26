// This should mirror your backend DTOs
export interface CreateTransactionRequest {
  transactionType: string; // Enum from C# maps to string in TS
  brokerId: string; // Guid from C# maps to string in TS
  brokerDealReference?: string;
  accountId: string; // Guid from C# maps to string in TS
  taxWrapper: string; // Enum from C# maps to string in TS
  isin: string;
  transactionDate: string; // DateTime from C# maps to string in TS
  quantity: number; // decimal from C# maps to number in TS
  nativePrice: number; // decimal from C# maps to number in TS
  localPrice: number; // decimal from C# maps to number in TS
  consideration: number; // decimal from C# maps to number in TS
  brokerCharge?: number; // Optional decimal from C# maps to number in TS
  stampDuty?: number; // Optional decimal from C# maps to number in TS
  fxCharge?: number; // Optional decimal from C# maps to number in TS
  accruedInterest?: number; // Optional decimal from C# maps to number in TS
}

export interface UpdateTransactionRequest extends CreateTransactionRequest {
  // Update might have slightly different fields, but often they are the same
}
