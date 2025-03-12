using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    public class Remittance
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountId { get; set; }
        public string SourceBranchCode { get; set; }
        public string SourceBranchId { get; set; }
        public string SourceTellerId { get; set; }
        public string SourceBranchName { get; set; }
        public string SenderSecreteCode { get; set; }
        public string SenderName { get; set; }
        public string SenderCNI { get; set; }
        public string SourceTellerName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverCNI { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverLanguage { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceivingBranchCode { get; set; }
        public string ReceivingBranchId { get; set; }
        public string ReceivingBranchName { get; set; }
        public string ReceivingTellerName { get; set; }
        public string ReceivingTellerId { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public decimal SourceBranchCommision { get; set; }
        public decimal RecivingBranchCommision { get; set; }
        public string Status { get; set; }
        public string ApprovalComment { get; set; }
        public string ApprovedBy { get; set; }
        public string InitiatedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime InitiationDate { get; set; }
        public DateTime? DateOfCashOut { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime? DatePaidToCashDesk { get; set; }
        public string TransactionReference { get; set; }
        public string SenderNote { get; set; }
        public string RemittanceType { get; set; }
        public string ChargeType { get; set; }
        public decimal InitailAmount { get; set; }

        public string TransferType { get; set; } //Incoming & Out_Going Transfer

        public string TransfterSource { get; set; }//International_Remittance Or Local_Remittance
        public DateTime? InternationalTransfterDate { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsAutoVerifyReceiver { get; set; }
        public bool IsAutoVerifySender { get; set; }
        public bool IsManualVerification { get; set; }
        public string SenderCountry { get; set; }
        public string ReceiverCountry { get; set; }
        public string CapturedReceiverCNI { get; set; }
        public string CapturedReceiverName { get; set; }
        public string CapturedReceiverCNIDateOfIssue { get; set; }
        public string CapturedReceiverCNIDateOfExpiration { get; set; }
        public string CapturedReceiverCNIPlcaceOfIssue { get; set; }
        public string CapturedOTP { get; set; }
        public string CapturedSenderName { get; set; }
        public string CapturedSenderPhoneNumber { get; set; }
        public string CapturedReceiverPhoneNumber { get; set; }
        public string CapturedSenderSecretCode { get; set; }
        public string CapturedSenderAddress { get; set; }
        public string CapturedReceiverAddress { get; set; }
        public decimal CapturedRemittanceAmount { get; set; }
        public DateTime? CapturedRemittanceDate { get; set; }
    }

    public class RemittanceChargeDto
    {
        public string FeeName { get; set; }
        public decimal Charge { get; set; }
        public decimal Amount { get; set; }
        public decimal PercentageValue { get; set; }
        public string RemittanceType { get; set; }
        public string FeeType { get; set; }
        public string GlobalStatus { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal InitailAmount { get; set; }
        public string ChargeType { get; set; }


    }
    public class AddRemittanceCommand:IValidatableObject
    {
        [Required(ErrorMessage = "The Account Number field is mandatory and cannot be empty.")]
        [StringLength(20, MinimumLength = 15, ErrorMessage = "The Account Number must be between 15 and 20 characters in length.")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "The sender country is required")]
        public string SenderCountry { get; set; }
        [Required(ErrorMessage = "The receiver country is required")]
        public string ReceiverCountry { get; set; }

        public string SourceBranchCode { get; set; }

        [Required(ErrorMessage = "The Source Branch field is mandatory and must be provided.")]
        public string SourceBranchId { get; set; }

        public string SourceBranchName { get; set; }

        [StringLength(10, MinimumLength = 4, ErrorMessage = "The Sender Secret Code must be between 4 and 10 characters in length.")]
        public string SenderSecreteCode { get; set; }

        [Required(ErrorMessage = "The Sender Name field is mandatory and cannot be empty.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "The Sender Name must be between 3 and 30 characters in length.")]
        public string SenderName { get; set; }

        [Required(ErrorMessage = "The Sender CNI field is mandatory and cannot be empty.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "The Sender CNI must be between 9 and 20 characters in length.")]
        public string SenderCNI { get; set; }

        [Required(ErrorMessage = "The Sender Phone Number field is mandatory and cannot be empty.")]
        [Phone(ErrorMessage = "The Sender Phone Number provided is not in a valid format.")]
        public string SenderPhoneNumber { get; set; }

        [Required(ErrorMessage = "The Sender Address field is mandatory and cannot be empty.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "The Sender Address must be between 5 and 50 characters in length.")]
        public string SenderAddress { get; set; }

        [Required(ErrorMessage = "The Receiver Name field is mandatory and cannot be empty.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "The Receiver Name must be between 3 and 30 characters in length.")]
        public string ReceiverName { get; set; }

        [StringLength(20, MinimumLength = 5, ErrorMessage = "The Receiver CNI must be between 5 and 20 characters in length.")]
        public string ReceiverCNI { get; set; }

        [Required(ErrorMessage = "The Receiver Address field is mandatory and cannot be empty.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "The Receiver Address must be between 5 and 50 characters in length.")]
        public string ReceiverAddress { get; set; }

        [Required(ErrorMessage = "The Receiver Phone Number field is mandatory and cannot be empty.")]
        [Phone(ErrorMessage = "The Receiver Phone Number provided is not in a valid format.")]
        public string ReceiverPhoneNumber { get; set; }

        [Required(ErrorMessage = "The Amount field is mandatory and must be provided.")]
        [Range(1, double.MaxValue, ErrorMessage = "The Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Charge mandatory and must be provided.")]
        [Range(0, double.MaxValue, ErrorMessage = "The Fee cannot be a negative value.")]
        public decimal Fee { get; set; }

        [Required(ErrorMessage = "The Remittance Type field is mandatory and cannot be empty.")]
        public string RemittanceType { get; set; }

        [StringLength(100, MinimumLength = 10, ErrorMessage = "The Sender Note must be between 10 and 100 characters in length.")]
        public string SenderNote { get; set; }
        [Required(ErrorMessage = "Receiver prefered languagee is required.")]
        public string ReceiverLanguage { get; set; }
        public decimal InitailAmount { get; set; }
        [Required(ErrorMessage = "Charge Type is required.")]
        [RegularExpression("Inclussive|Exclussive", ErrorMessage = "Charge Type must be either 'Inclussive' or 'Exclussive'.")]
        [DefaultValue("Exclussive")]
        public string ChargeType { get; set; } = "Exclussive";
        public bool SendSMSTotReceiver { get; set; }

        [Required(ErrorMessage = "Date of Issue is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Date of Issue format.")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of Issue must be in the format yyyy-MM-dd.")]
        public string DateOfIssue { get; set; }

        [Required(ErrorMessage = "Expiration Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Expiration Date format.")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Expiration Date must be in the format yyyy-MM-dd.")]
        public string ExpirationDate { get; set; }



        [Required(ErrorMessage = "Place of Issue is required.")]
        [StringLength(100, ErrorMessage = "Place of Issue must not exceed 100 characters.")]
        public string PlaceOfIssue { get; set; }
        [Required(ErrorMessage = "Transfer Type is required")]
        [Display(Name = "Transfer Type")]
        public string TransferType { get; set; } = "Local"; // Default value set to Local

        [Required(ErrorMessage = "Transfer source is required")]
        [Display(Name = "Transfer Source")]
        public string TransferSource { get; set; } // International_Remittance or Local_Remittance

        [Display(Name = "External Reference")]
        public string ExternalReference { get; set; }

        [Display(Name = "Date of International Remittance")]
        //[DataType(DataType.Date, ErrorMessage = "Invalid Expiration Date format.")]
        //[RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of International Remittance must be in the format yyyy-MM-dd.")]
        public DateTime? InternationalTransfterDate { get; set; }

        // Custom Validation Logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TransferSource == "International_Remittance")
            {
                // Check if External Reference is provided
                if (string.IsNullOrWhiteSpace(ExternalReference))
                {
                    yield return new ValidationResult(
                        "External Reference is required for International Remittance.",
                        new[] { nameof(ExternalReference) }
                    );
                }

                // Check if International Transfer Date is provided
                if (!InternationalTransfterDate.HasValue)
                {
                    yield return new ValidationResult(
                        "Date of International Remittance is required.",
                        new[] { nameof(InternationalTransfterDate) }
                    );
                }
            }
        }

    }
    public class TempOTPDto
    {
        public string Otp { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Url { get; set; }

        public string Id { get; set; }
        public bool IsVerify { get; set; } = true;
    }
    public class GenerateRemittanceOTPCommand 
    {
        public string RemittanceReference { get; set; }
        public string ReceiverPhoneNumber { get; set; }
    }
    public class ValidationOfRemittanceCommand
    {
        [Required(ErrorMessage = "The Id field is mandatory and cannot be empty.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "The Status field is mandatory and cannot be empty.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Approval comment is mandatory and cannot be empty.")]
        public string ApprovalComment { get; set; }
        public Remittance Remittance { get; set; }
    }
    public class GetRemittanceChargeQuery
    {
        public string RemittanceType { get; set; }
        public decimal Amount { get; set; }
        public string SenderAccountNumber { get; set; }
        public string ChargeType { get; set; }
        public string TransfterType { get; set; }
    }
    public class GetAllRemittanceWildQuery
    {
       
        public string QueryString { get; set; }
        public string QueryValue { get; set; }
        public bool Approved { get; set; }
    }
    public class GetAllRemittanceQuery
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DataTableOptions DataTableOptions { get; set; }
        public bool ByDateRange { get; set; }
        public bool Approved { get; set; }
        public string QueryParameter { get; set; } //Can be By SourceBranchId,ReceivingBranchId or All
        public string QueryValue { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; }
    }
    public class GetRemittanceAccountByTypeQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier of the Transaction to be retrieved.
        /// </summary>
        public string AccountType { get; set; }
        public string BranchId { get; set; }
    }
}
