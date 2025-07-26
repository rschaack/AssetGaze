// assetgaze-frontend/src/app/shared/interfaces/transaction.interface.ts

// Define the Transaction interface to match your backend DTO
// Ensure consistency with C# Guid (string in TS), DateTime (string in TS), decimal (number in TS).
export interface Transaction { // <--- Export it
  id: string; // Guid from C# maps to string in TS
  transactionType: string; // Enum from C# maps to string in TS
  brokerId: string; // Guid from C# maps to string in TS
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
  brokerDealReference?: string;
}
