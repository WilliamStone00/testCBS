using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{

    public class GetTellerOpenningAndClossingQuery
    {
        public bool ByBracnch { get; set; }
        [RequiredIfByBranch]
        public string BranchId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date From")]
        public string DateFrom { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date To")]
        public string DateTo { get; set; }
    }




    public class GetTillStatusQuery : IValidatableObject
    {
        public bool ByBranch { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date From")]
        public string DateFrom { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date To")]
        public string DateTo { get; set; }

        public bool ByTeller { get; set; }

        public string BranchId { get; set; }

        public string TellerId { get; set; }
        public string QueryParameter { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return PerformValidation();
        }

        private IEnumerable<ValidationResult> PerformValidation()
        {
            var validationResults = new List<ValidationResult>();



            // Validate DateFrom and DateTo are equal
            DateTime fromDate;
            DateTime toDate;

            if (DateTime.TryParse(DateFrom, out fromDate) && DateTime.TryParse(DateTo, out toDate))
            {
                if (fromDate != toDate)
                {
                    validationResults.Add(new ValidationResult(
                        "The DateFrom must be equal to DateTo.",
                        new[] { nameof(DateFrom), nameof(DateTo) }));
                }
            }
            else
            {
                validationResults.Add(new ValidationResult(
                    "Invalid date format for DateFrom or DateTo.",
                    new[] { nameof(DateFrom), nameof(DateTo) }));
            }

            // Validate BranchId and TellerId based on ByBranch and ByTeller
            if (ByBranch)
            {
                if (string.IsNullOrEmpty(BranchId))
                {
                    validationResults.Add(new ValidationResult(
                        "Branch is required when ByBranch is selected.",
                        new[] { nameof(BranchId) }));
                }
            }
            else if (ByTeller)
            {
                if (string.IsNullOrEmpty(TellerId))
                {
                    validationResults.Add(new ValidationResult(
                        "Till is required when Till is selected.",
                        new[] { nameof(TellerId) }));
                }
            }

            return validationResults;
        }

    }



    public class RequiredIfByBranchAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var query = (GetTellerOpenningAndClossingQuery)validationContext.ObjectInstance;
            if (query.ByBracnch && string.IsNullOrWhiteSpace(query.BranchId))
            {
                return new ValidationResult("BranchId is required when ByBracnch is true.");
            }
            return ValidationResult.Success;
        }
    }


    public class SubTellerProvisioningDto
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public bool IsCashReplenished { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime openedDate { get; set; }
        public DateTime clossedDate { get; set; }
        public decimal openOfDayAmount { get; set; }
        public decimal cashAtHand { get; set; }
        public decimal endOfDayAmount { get; set; }
        public decimal accountBalance { get; set; }
        public decimal tellerAccountBalance { get; set; }
        public decimal lastOPerationAmount { get; set; }
        public string lastOperationType { get; set; }
        public decimal previouseBalance { get; set; }
        public string lastUserID { get; set; }
        public string subTellerComment { get; set; }
        public string primaryTellerID { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string startOfDayCurrencyNoteId { get; set; }
        public string clossedStatus { get; set; }
        public string primaryTellerComment { get; set; }
        public string primaryTellerConfirmationStatus { get; set; }
        public Teller teller { get; set; }
    }
    public class PrimaryTellerProvisioningDto
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public string ReferenceId { get; set; }
        public string CloseOfDayReferenceId { get; set; }
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public DateTime? OpenedDate { get; set; }
        public string PrimaryTellerId { get; set; }
        public DateTime? ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public decimal CashReplenishmentAmount { get; set; } = 0;
        public string ReplenishmentReferenceNumber { get; set; }
        public bool IsCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public string DailyTellerId { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string LastUserID { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public Teller teller { get; set; }
    }
    public class EndOfDaySubTellerCommand
    {
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        [Required]
        public string comment { get; set; }
        [Required]
        public int cashAtHand { get; set; }
        public string operationDate { get; set; }
        public string tellerProvisioningId { get; set; }
        public string openOfDayAmount { get; set; }
    }
    public class EndOfDayBySubTellerIDCommand
    {
        [Required]
        public string comment { get; set; }
        [Required]
        public string primaryTellerConfirmationStatus { get; set; }
        public string subTellerProvioningHistoryID { get; set; }

    }

    public class PrimaryTellerProvisioningHistory
    {
        public string id { get; set; }
        public string tellerId { get; set; }
        public string userIdInChargeOfThisTeller { get; set; }
        public string provisionedBy { get; set; }
        public DateTime openedDate { get; set; }
        public DateTime clossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal openOfDayAmount { get; set; } = 0;
        public decimal cashAtHand { get; set; } = 0;
        public decimal endOfDayAmount { get; set; } = 0;
        public decimal accountBalance { get; set; } = 0;
        public decimal previouseBalance { get; set; }
        public string lastUserID { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string startOfDayCurrencyNoteId { get; set; }
        public string accountantComment { get; set; }
        public string accountantUserID { get; set; }
        public string clossedStatus { get; set; }
        public string primaryTellerComment { get; set; }
        public string accountantConfirmationStatus { get; set; }
        public string accountantCloseOfDayComment { get; set; }
    }
    public class EndOfDayPrimaryTellerCommand
    {
        [Required]
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        [Required]
        public string comment { get; set; }
        [Required]
        public int cashAtHand { get; set; }
        public string primaryTellerProvioningHistoryID { get; set; }
        public string clossedStatus { get; set; }


    }
    public class EndOfDayAccountantCommand
    {
        [Required]
        public string comment { get; set; }
        [Required]
        public decimal amountRecieved { get; set; }
        public string primaryTellerProvioningHistoryID { get; set; }
        [Required]
        public string eodClosedStatus { get; set; }
        [Required]
        public string accountantConfirmationStatus { get; set; }
    }
    public class EndOfTheDay
    {
        public CloseOfDayRequest CloseOfDayRequest { get; set; } = new CloseOfDayRequest();
        public EndOfDayAccountantCommand EndOfDayAccountantCommand { get; set; } = new EndOfDayAccountantCommand();
        public EndOfDayBySubTellerIDCommand EndOfDayBySubTellerIDCommand { get; set; } = new EndOfDayBySubTellerIDCommand();
        public EndOfDayPrimaryTellerCommand EndOfDayPrimaryTellerCommand { get; set; } = new EndOfDayPrimaryTellerCommand();
        public EndOfDaySubTellerCommand EndOfDaySubTellerCommand { get; set; } = new EndOfDaySubTellerCommand();
        public List<PrimaryTellerProvisioningDto> PrimaryTellerProvisioningHistories { get; set; } = new List<PrimaryTellerProvisioningDto>();
        public PrimaryTellerProvisioningDto PrimaryTellerProvisioningHistory { get; set; } = new PrimaryTellerProvisioningDto();
        public List<SubTellerProvisioningDto> SubTellerProvioningHistories { get; set; } = new List<SubTellerProvisioningDto>();
        public SubTellerProvisioningDto SubTellerProvioningHistory { get; set; } = new SubTellerProvisioningDto();
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
        public string Option { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public decimal CashAtHand { get; set; }
        public DateTime? AccountingDay { get; set; }
        public Teller Teller { get; set; }
    }
    public class GetDailyTellerByUserIdQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier of the TempAccount to be retrieved.
        /// </summary>
        public string UserId { get; set; }
        public string TellerType { get; set; }
    }
    public class DailyTeller
    {
        public string Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ProvisionedBy { get; set; }
        [Required]
        public string TellerId { get; set; }
        [Required]
        public string OperationType { get; set; }//Cash, NoneCash
        public string UserBranchId { get; set; }
        public bool Status { get; set; }
        public bool IsPrimary { get; set; }
        [Required]
        public string BranchId { get; set; }
        [Required]
        public decimal MaximumWithdrawalAmount { get; set; }
        [Required]
        public decimal MaximumCeilin { get; set; }
        public Teller Teller { get; set; }
        [ValidateObject]
        public GetAllTellerOperationsQuery GetAllTellerOperationsQuery { get; set; } = new GetAllTellerOperationsQuery();
        [ValidateObject]
        public GetTellerOpenningAndClossingQuery GetTellerOpenningAndClossingQuery { get; set; } = new GetTellerOpenningAndClossingQuery();
        [ValidateObject]
        public GetTillStatusQuery GetTillStatusQuery { get; set; } = new GetTillStatusQuery();

        public Branch Branch { get; set; }
        public List<Branch> Branches { get; set; }
        public List<PrimaryTellerProvisioningHistory> PrimaryTellerProvisioningHistories { get; set; }
        public List<TellerProvioningHistory> SubTellerProvioningHistories { get; set; }

    }
    public class QueryParamWithDates
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string BranchId { get; set; }
    }
    public class TellerOperationGL
    {
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string TransactionRef { get; set; }
        public string DailyReferences { get; set; }
        public decimal BalanceBF { get; set; }
        public string MemberId { get; set; }
        public string Description { get; set; }
        public string MemberAccountNumber { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Naration { get; set; }
        public string MemberName { get; set; }
        public decimal Balance { get; set; }
        public string TellerID { get; set; }
        public DateTime Date { get; set; }
        public DateTime EntryDate { get; set; }
        public string AccountType { get; set; }
        public string CashierName { get; set; }
        public string TellerName { get; set; }
        public string TransactionReference { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }

    }

    public class ExportTellerGL
    {
        public DateTime Date { get; set; }
        public string Naration { get; set; }
        public decimal BalanceBF { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string BranchName { get; set; }
        public string TellerName { get; set; }
        public string UserName { get; set; }
        public string DailyReferences { get; set; }
        public string MemberId { get; set; }
        public string Description { get; set; }
        public string MemberAccountNumber { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string HeadOffice { get; set; }
        public string BranchCode { get; set; }
        public string BranchTel { get; set; }
        public string BranchAddress { get; set; }
        public string LogoUrl { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal Amount { get; set; }
        public string TransactionRef { get; set; }
        public string MemberName { get; set; }
        public string TellerID { get; set; }
        public DateTime EntryDate { get; set; }
        public string AccountType { get; set; }
        public string CashierName { get; set; }
        public string TransactionReference { get; set; }
        public string BranchId { get; set; }
    }


    public class GetAllTellerOperationsQuery
    {
        [Required(ErrorMessage = "Operation type is required.")]
        [RegularExpression("^(CashOperations|NoneCashOperations|All)$", ErrorMessage = "Operation must be 'CashOperations', 'NoneCashOperation' Or 'All'.")]
        public string QueryString { get; set; }

        [RequiredIf("IsByTeller", ErrorMessage = "Teller is required if 'IsByTeller' is true.")]
        [StringLength(50, ErrorMessage = "Teller cannot exceed 50 characters.")]
        public string TellerId { get; set; }

        [RequiredIf("IsByDate", ErrorMessage = "DateFrom is required if 'IsByDate' is true.")]
        [DataType(DataType.Date, ErrorMessage = "DateFrom must be a valid date.")]
        public string DateFrom { get; set; }

        [RequiredIf("IsByDate", ErrorMessage = "DateTo is required if 'IsByDate' is true.")]
        [DataType(DataType.Date, ErrorMessage = "DateTo must be a valid date.")]
        [DateGreaterThan("DateFrom", ErrorMessage = "DateTo must be greater than DateFrom.")]
        public string DateTo { get; set; }

        [RequiredIf("IsByBranch", ErrorMessage = "Branch is required if 'IsByBranch' is true.")]
        [StringLength(50, ErrorMessage = "Branch cannot exceed 50 characters.")]
        public string BranchId { get; set; }

        public bool IsByTeller { get; set; }
        public bool IsByBranch { get; set; }
        public bool IsByDate { get; set; }
        public bool IsPDF { get; set; }
        public GetAllTellerOperationsQuery()
        {
            IsByDate = true;
            QueryString = "CashOperations";
        }
    }

    public class CashReplenishmentSubTeller
    {
        public string Id { get; set; }
        [Required]
        public decimal RequestedAmount { get; set; }
        [Required]
        public decimal ConfirmedAmount { get; set; }
        public string RequesterUserId { get; set; }
        public string RequesterName { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByUserId { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime InitializeDate { get; set; }
        [Required]
        public string ApprovedComment { get; set; }
        [Required]
        public string Requetcomment { get; set; }
        public string ApprovedStatus { get; set; }//Pending,Approved, Rejected
        public string TellerId { get; set; }
        public string BranchId { get; set; }
        public bool Approved { get; set; }
        public Branch Branch { get; set; }
        public string TransactionReference { get; set; }
        public CurrencyNotes CurrencyNotes { get; set; } = new CurrencyNotes();

        public CashReplenishmentSubTeller()
        {
            Approved = true;
            ApprovedComment = "Approved: Cash replenishment authorized to maintain optimal cash levels for customer service and operational efficiency.";
            Requetcomment = "Cash replenishment needed to meet current customer demand and ensure seamless operations at the teller station.";
        }
    }
    public class CashReplenishmentPrimaryTeller
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public bool Status { get; set; }
        public string BranchId { get; set; }
        [Required(ErrorMessage = "Amount in hand is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal RequestedAmount { get; set; }
        [Required]
        public decimal ConfirmedAmount { get; set; }
        public string RequesterUserId { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByUserId { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime InitializeDate { get; set; }
        [Required]
        public string ApprovedComment { get; set; }
        [Required]
        public string Requetcomment { get; set; }
        [Required]
        public string ApprovedStatus { get; set; }//Pending,Approved, Rejected
        public string TransactionReference { get; set; }
        public string AccountingPostingReference { get; set; }

        public string RequesterName { get; set; }
        public Teller Teller { get; set; } = new Teller();
        public Branch Branch { get; set; } = new Branch();
        public CurrencyNotes CurrencyNote { get; set; } = new CurrencyNotes();
        public CashReplenishmentPrimaryTeller()
        {
            Status = true;
            ApprovedComment = "Approved: Cash replenishment authorized to maintain optimal cash levels for customer service and operational efficiency.";
            Requetcomment = "Cash replenishment needed to meet current customer demand and ensure seamless operations at the teller station.";
        }

    }
    public class GetAllMobileMoneyCashTopupQuery
    {
        public string QueryParameter { get; set; }
        public bool ByBranch { get; set; }
        public string BranchId { get; set; }
    }
    public class MobileMoneyCashTopup
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string OperatorType { get; set; }//MTN Or Orange
        public string SourceType { get; set; }//GAV, OtherTransfer, HeadOffice, Wirering, M2Float
        public DateTime RequestDate { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string RequestNote { get; set; }
        public string RequestInitiatedBy { get; set; }
        public DateTime RequestApprovalDate { get; set; }
        public string RequestApprovedBy { get; set; }
        public string RequestApprovalStatus { get; set; }
        public string RequestApprovalNote { get; set; }
        public string RequestReference { get; set; }
        public string MobileMoneyTransactionId { get; set; }
        public string MobileMoneyMemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string TellerId { get; set; }
        public Branch Branch { get; set; }
        public string BranchName { get; set; }
        public string StrRequestDate { get; set; }
        public string StrRequestApprovalDate { get; set; }
        public Teller Teller { get; set; }
        public ValidateMobileMoneyCashTopup ValidateMobileMoneyCashTopup { get; set; }
    }

    public class AddMobileMoneyCashTopup
    {
        [Required(ErrorMessage = "Please enter the requested amount.")]
        [Range(1000, double.MaxValue, ErrorMessage = "The amount must be at least 1000 and be a positive value.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Operator Type is required.")]
        [RegularExpression(@"^(MobileMoneyORANGE|MobileMoneyMTN)$", ErrorMessage = "Operator Type must be either 'MTN' or 'Orange'.")]
        public string OperatorType { get; set; } // MTN or Orange

        [Required(ErrorMessage = "Trannsfer type is required.")]
        [RegularExpression(@"^(AuxillaryToBranch|HeadOfficeToBranch|BranchToBranch|BranchToHeadOffice)$", ErrorMessage = "Transfer type must be one of the following: AuxillaryToBranch, HeadOfficeToBranch, BranchToBranch, BranchToHeadOffice.")]
        public string SourceType { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        public string BranchId { get; set; }

        [StringLength(500, ErrorMessage = "Request Note cannot exceed 500 characters.")]
        public string RequestNote { get; set; }

        [StringLength(20, ErrorMessage = "Mobile Money Or Orange Money Transaction Reference cannot exceed 20 characters.")]
        [Required(ErrorMessage = "Mobile Money Or Orange Money Transaction Reference is required.")]
        public string MobileMoneyTransactionId { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "Account Number must be between 10 and 20 characters.")]
        public string AccountNumber { get; set; }
    }


    public class ValidateMobileMoneyCashTopup
    {
        [Required(ErrorMessage = "Id is required.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Request Approval Status is required.")]
        [RegularExpression(@"^(Pending|Approved|Rejected)$", ErrorMessage = "Request Approval Status must be 'Pending', 'Approved', or 'Rejected'.")]
        public string RequestApprovalStatus { get; set; }

        [StringLength(200, ErrorMessage = "Request Approval Note cannot exceed 200 characters.")]
        public string RequestApprovalNote { get; set; }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public RequiredIfAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the property being used as the condition
            PropertyInfo property = validationContext.ObjectType.GetProperty(_propertyName);
            if (property == null)
                return new ValidationResult($"Unknown property: {_propertyName}");

            // Get the value of the conditional property
            var propertyValue = property.GetValue(validationContext.ObjectInstance);

            // Check if the conditional property is true, and if so, validate the value
            if (propertyValue is bool boolValue && boolValue && value == null)
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");

            return ValidationResult.Success;
        }
    }


    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = value as DateTime?;

            // Get the property to compare
            PropertyInfo comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonProperty == null)
                return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

            // Check if current value is greater than the comparison value
            if (currentValue != null && comparisonValue != null && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be greater than {_comparisonProperty}.");
            }

            return ValidationResult.Success;
        }
    }


    public class ValidateObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If the value is null, skip validation (this attribute doesn't enforce required)
            if (value == null)
                return ValidationResult.Success;

            // Validate the object by its properties
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(value, new ValidationContext(value), validationResults, validateAllProperties: true);

            if (isValid)
                return ValidationResult.Success;

            // Collect all error messages and return a combined result
            var compositeErrorMessage = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            return new ValidationResult(compositeErrorMessage);
        }
    }

}
