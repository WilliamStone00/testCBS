using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque
{
	public class CounterCheques
	{
		[Required]
		public string AccountNumber { get; set; }
		[Required]
		public decimal Amount { get; set; }
		public string BranchId { get; set; }
		public string CustomerId { get; set; }
		// For displaying in the list
		public string CheckLeafId { get; set; }
		public string Id { get; set; }
		public string CheckNumber { get; set; }
		public DateTime? IssuedOn { get; set; }
		public string IssuedBy { get; set; }
		public string Status { get; set; }
		public string Name { get; set; }
		public string Catergoryid { get; set; }
		public string ClientName { get; set; }
		public int NumberOfPages { get; set; }
		public string ChequeBookId { get; set; }
		

}

	// DTO for the modal action form
	public class CounterChequeActionDto
	{
		[Required]
		public string CounterChequeId { get; set; }
		[Required]
		public string Motive { get; set; }
		public string Action { get; set; } // "Review", "Validate", or "Reject"
	}


	public class CounterChequeQuery
	{
		public CounterChequeQuery()
		{
			DataTableOptions = new DataTableOptions();
		}

		public DataTableOptions DataTableOptions { get; set; }


        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        public string CheckLeafId { get; set; }
        public string CheckNumber { get; set; }

        public DateTime IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string Status { get; set; }

        public string ClientName { get; set; }
        public string BranchName { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

public class CounterChequeDto
    {
        public string Id { get; set; }              
        public string CustomerId { get; set; }      
        public decimal Amount { get; set; }         
        public string AccountNumber { get; set; }   
        public string BranchId { get; set; }        
        public string CheckLeafId { get; set; }     
        public string CheckNumber { get; set; }    
        public DateTime IssuedOn { get; set; }      
        public string IssuedBy { get; set; }        
        public string Status { get; set; }        
        public string CustomerName { get; set; }        
        public string BranchName { get; set; }        
    }

	public class CustomerData
	{
		public CustomerDto CustomerDto { get; set; }
		public List<CheckBookData> CheckBooks { get; set; }
	}

	public class CustomerDto
	{
		public string CustomerId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string LegalForm { get; set; }
		public string CustomerType { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string PlaceOfBirth { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Gender { get; set; }
		public bool Active { get; set; }
	}

	public class CheckBookData
	{
		public CheckBook CheckBook { get; set; }
		public Account Account { get; set; }
		public List<CheckBookLeaf> CheckBookLeaves { get; set; }
	}

	public class CheckBook
	{
		public string Id { get; set; }
		public string CustomerId { get; set; }
		public string AccountId { get; set; }
		public string CustomerName { get; set; }
		public string AccountNumber { get; set; }
		public string BranchId { get; set; }
		public string BankId { get; set; }
		public string BranchName { get; set; }
		public string BranchCode { get; set; }
		public int NumberOfLeaves { get; set; }
		public int RemainingLeaves { get; set; }
		public int UsedLeaves { get; set; }
		public decimal Balance { get; set; }
		public int StartSerialNumber { get; set; }
		public int EndSerialNumber { get; set; }
		public int CurrentSerialNumber { get; set; }
		public int Current { get; set; }
		public string Status { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? IssuedDate { get; set; }
		public string IssuedBy { get; set; }
		public DateTime LastUpdatedDate { get; set; }
		public DateTime? ExpiratryDate { get; set; }
		public string LastUpdatedBy { get; set; }
		public string BlockedBy { get; set; }
		public bool IsBlocked { get; set; }
		public string ApproveBy { get; set; }
		public string BlockReasons { get; set; }
		public DateTime? BlockedDate { get; set; }
		public string CheckBookCategoryId { get; set; }
		public string CategoryName { get; set; }
		public CheckBookCategory CheckBookCategory { get; set; }
		public bool IsReissued { get; set; }
		public string ReplacementCheckBookId { get; set; }
		public bool NotifyOnClearance { get; set; }
		public bool NotifyOnPayment { get; set; }
		public bool NotifyOnAnyTransaction { get; set; }
		public bool IsPrinted { get; set; }
		public bool IsIssued { get; set; }
		public bool IsActive { get; set; }
		public string StatusDescription { get; set; }
		public List<CheckBookLeaf> Leaves { get; set; }
	}

	public class CheckBookCategory
	{
		public string Id { get; set; }
		public string BankId { get; set; }
		public string BranchId { get; set; }
		public string CheckBookId { get; set; }
		public string Name { get; set; }
		public bool IsCentralised { get; set; }
		public int NumberOfCheckBooks { get; set; }
		public int NumberOfPages { get; set; }
		public int ValidityPeriodInMonths { get; set; }
		public int IssuanceLimitPerCustomerType { get; set; }
		public int? MaxIssuancePerYear { get; set; }
		public decimal BasePrice { get; set; }
		public decimal IssuanceFee { get; set; }
		public decimal? RenewalFee { get; set; }
		public decimal? MaxTransactionAmount { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime ModifiedDate { get; set; }
		public string ModifiedBy { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime DeletedDate { get; set; }
		public string DeletedBy { get; set; }
	}

	public class CheckBookLeaf
	{
		public string Id { get; set; }
		public string CheckBookId { get; set; }
		public string CounterCheckId { get; set; }
		public int SerialNumber { get; set; }
		public string Status { get; set; }
		public DateTime? IssuedDate { get; set; }
		public string IssuedTo { get; set; }
		public string TransactionId { get; set; }
		public DateTime? ClearedDate { get; set; }
		public DateTime? CancelledDate { get; set; }
		public string CancelledReason { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? LastUpdatedDate { get; set; }
		public string LastUpdatedBy { get; set; }
		public string Transaction { get; set; }
	}

	public class Account
	{
		public string Id { get; set; }
		public string ProductId { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string AccountId { get; set; }
		public bool IsRemoveAccount { get; set; }
		public string AccountNumber { get; set; }
		public decimal Balance { get; set; }
		public decimal PreviousBalance { get; set; }
		public string Status { get; set; }
		public string TellerId { get; set; }
		public string BranchCode { get; set; }
		public string EncryptedBalance { get; set; }
		public decimal InterestGenerated { get; set; }
		public decimal LastInterestPosted { get; set; }
		public decimal BlockedAmount { get; set; }
		public string BlockedId { get; set; }
		public string ProfileType { get; set; }
		public DateTime DateBlocked { get; set; }
		public DateTime DateReleased { get; set; }
		public string ReasonOfBlocked { get; set; }
		public DateTime DateOfLastOperation { get; set; }
		public DateTime LastInterestCalculatedDate { get; set; }
		public string AccountName { get; set; }
		public string LastOperation { get; set; }
		public string AccountType { get; set; }
		public bool IsTellerAccount { get; set; }
		public string OpenningOfDayStatus { get; set; }
		public DateTime OpenningOfDayDate { get; set; }
		public string OpenningOfDayReference { get; set; }
		public decimal OpeningBalance { get; set; }
		public DateTime DateOfOpeningBalance { get; set; }
		public decimal LastOperationAmount { get; set; }
		public string BankId { get; set; }
		public string BranchId { get; set; }
	}

    public class CustomerCheckBookStatisticsDto
    {
        // ===============================
        // CUSTOMER
        // ===============================
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PrimaryAccountNumber { get; set; }

        // ===============================
        // CHECKBOOK COUNTS
        // ===============================
        public int TotalCheckBooks { get; set; }
        public int ActiveCheckBooks { get; set; }
        public int BlockedCheckBooks { get; set; }
        public int ExpiredCheckBooks { get; set; }
        public int ReissuedCheckBooks { get; set; }
        public int LostCheckBooks { get; set; }

        // ===============================
        // LEAVES / CHEQUES
        // ===============================
        public int TotalLeaves { get; set; }
        public int TotalUsedLeaves { get; set; }
        public int TotalRemainingLeaves { get; set; }
        public int CancelledLeaves { get; set; }
        public int StaleLeaves { get; set; }

        public double GlobalUsageRate { get; set; }          // %
        public double RemainingUsageRate { get; set; }       // %

        // ===============================
        // FINANCIAL STATISTICS
        // ===============================
        public decimal TotalBalance { get; set; }
        public decimal AverageBalance { get; set; }

        public decimal TotalAmountIssued { get; set; }
        public decimal TotalAmountCleared { get; set; }
        public decimal TotalAmountPending { get; set; }
        public decimal TotalAmountRejected { get; set; }

        public decimal HighestChequeAmount { get; set; }
        public decimal AverageChequeAmount { get; set; }

        // ===============================
        // CLEARANCE & TIMING
        // ===============================
        public int PendingClearances { get; set; }
        public int ClearedWithin24Hours { get; set; }
        public int ClearedWithin72Hours { get; set; }
        public int DelayedClearances { get; set; } 

		// ===============================
		// CLEARANCE BREAKDOWN (HOURS)
		// ===============================

        public double CounterChecks { get; set; }
        public double OnBehalfOf { get; set; }
        public double ByOwner { get; set; }

        // ===============================
        // CLEARANCE BREAKDOWN (COUNTS WITHIN 24H)
        // ===============================
        public int CounterChecksWithin24h { get; set; }
        public int OnBehalfOfWithin24h { get; set; }
        public int ByOwnerWithin24h { get; set; }


        // =========================
        // BOTTOM SECTION (TOTALS)
        // =========================

        /// <summary>
        /// Total average clearance time (hours)
        /// </summary>
        public double TotalAverageClearanceHours =>
                CounterChecks + OnBehalfOf + ByOwner;

            /// <summary>
            /// Total checks cleared within 24 hours
            /// </summary>
            public int TotalClearedWithin24h =>
                CounterChecksWithin24h + OnBehalfOfWithin24h + ByOwnerWithin24h;
        
        // ===============================
        // LIFE CYCLE
        // ===============================
        public DateTime? FirstIssuedDate { get; set; }
        public DateTime? LastIssuedDate { get; set; }
        public DateTime? LastChequeIssuedDate { get; set; }
        public DateTime? LastChequeClearedDate { get; set; }

        public int ExpiringSoonCheckBooks { get; set; }
        public bool NeedsRenewal { get; set; }

        // ===============================
        // SECURITY & RISK
        // ===============================
        public bool HasBlockedCheckBooks { get; set; }
        public bool HasBouncedCheques { get; set; }
        public int BouncedChequesCount { get; set; }

        public int StopPaymentRequests { get; set; }
        public int FraudFlaggedCheques { get; set; }

        public string RiskLevel { get; set; } // Low | Medium | High

        // ===============================
        // SERVICE & OPERATIONS
        // ===============================
        public int EmergencyClearanceRequests { get; set; }
        public int ApprovedFastTrackRequests { get; set; }
        public decimal FastTrackFeesPaid { get; set; }

        // ===============================
        // METADATA
        // ===============================
        public DateTime GeneratedAt { get; set; }

        public List<CustomerChequeAccountDto> ChequeAccounts { get; set; }

    }
    public enum ChequeOperationType
    {
        CounterCheque,
        OnBehalfOf,
        ByOwner
    }

    public class CustomerChequeAccountDto
    {
        public string AccountId { get; set; }
        public string AccountType { get; set; }        // Savings, Current, Loan
        public string AccountNumber { get; set; }
        public string Chequebook { get; set; }

        public ChequeOperationType OperationType { get; set; }

        public bool IsActive { get; set; }
        public decimal BalanceAmount { get; set; }
    }


}
