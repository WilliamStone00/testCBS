using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CMoney
{
    public class CMoneyMembersActivationAccount
    {
        public string Id { get; set; }
        public string CustomerId { get; set; } // e.g., "0011234567"
        public string BranchCode { get; set; } // e.g., "001"
        public string LoginId { get; set; } // e.g., "0014567"
        public string PhoneNumber { get; set; }
        public string PIN { get; set; } // 4-digit PIN
        public string BranchId { get; set; }
        public DateTime ActivationDate { get; set; }
        public bool IsActive { get; set; }
        public string DefaultPin { get; set; }
        public bool HasChangeDefaultPin { get; set; }
        public DateTime LastSubcriptionRenewalDate { get; set; }
        public bool IsSubcribed { get; set; }
        public decimal LastPaymentAmount { get; set; }
        public string DeactivationReason { get; set; }
        public string SecretQuestion { get; set; }
        public string SecretAnswer { get; set; }
        public string ActivatingBranchId { get; set; }
        public string ActivatingBranchCode { get; set; }
        public string ActivatingBranchName { get; set; }
        public string ActivatedBy { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public int FailedAttempts { get; set; }
        public string Option { get; set; }
        public string Reason { get; set; }

        public IndividualProfile Customer { get; set; }
        public AddCMoneyMemberActivationCommand AddCMoneyMemberActivationCommand { get; set; }
        public ChangeCMoneyMemberPinCommand ChangeCMoneyMemberPinCommand { get; set; }
        public DeactivateCMoneyMemberCommand DeactivateCMoneyMemberCommand { get; set; }
        public ManageSecretQuestionCommand ManageSecretQuestionCommand { get; set; }
        public ReactivateCMoneyMemberCommand ReactivateCMoneyMemberCommand { get; set; }
        public ResetCMoneyMemberPinCommand ResetCMoneyMemberPinCommand { get; set; }
        public ResetPinWithSecurityCommand ResetPinWithSecurityCommand { get; set; }
        public UpdateCMoneyMemberActivationCommand UpdateCMoneyMemberActivationCommand { get; set; }
        public CustomerDocumentRequest CustomerDocumentRequest { get; set; }
        public GenerateMemberActivationOTPCommand GenerateMemberActivationOTPCommand { get; set; }
        public PaginationMetadata PaginationMetadata { get; set; }
        public ChangePhoneNumberCommand ChangePhoneNumberCommand { get; set; }

        public CMoneyMembersActivationAccount()
        {
            AddCMoneyMemberActivationCommand=new AddCMoneyMemberActivationCommand();
            ChangeCMoneyMemberPinCommand=new ChangeCMoneyMemberPinCommand();
            DeactivateCMoneyMemberCommand=new DeactivateCMoneyMemberCommand();
            ManageSecretQuestionCommand=new ManageSecretQuestionCommand();
            ReactivateCMoneyMemberCommand=new ReactivateCMoneyMemberCommand();
            ResetCMoneyMemberPinCommand=new ResetCMoneyMemberPinCommand();
            ResetPinWithSecurityCommand=new ResetPinWithSecurityCommand();
            UpdateCMoneyMemberActivationCommand=new UpdateCMoneyMemberActivationCommand();
            GenerateMemberActivationOTPCommand=new GenerateMemberActivationOTPCommand();
            CustomerDocumentRequest=new CustomerDocumentRequest();
            PaginationMetadata=new PaginationMetadata();
            Customer =new IndividualProfile();
            ChangePhoneNumberCommand=new ChangePhoneNumberCommand();
        }
    }
    public class ChangePhoneNumberCommand
    {
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "The provided Phone Number is not valid.")]
        public string OldPhoneNumber { get; set; }
        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "The provided Phone Number is not valid.")]
        public string NewPhoneNumber { get; set; }
        public string OTP { get; set; }
        [Required(ErrorMessage = "Member prefered language is required.")]
        public string Language { get; set; }
        [Required(ErrorMessage = "A Reason for changing phone number is required.")]
        [MaxLength(250, ErrorMessage = "The Reason cannot exceed 250 characters.")]
        public string Reason { get; set; }
    }
    public class GenerateMemberActivationOTPCommand
    {
        [Required(ErrorMessage = "Customer ID is required.")]
        [StringLength(25, ErrorMessage = "Customer ID must not exceed 25 characters.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "The provided Phone Number is not valid.")]
        public string PhoneNumber { get; set; }
    }

    public class GetCMoneyMemberActivationsQuery
    {
        /// <summary>
        /// Determines if the query should filter by branch.
        /// </summary>
        public bool ByBranch { get; set; }

        /// <summary>
        /// Determines if the query should filter by user.
        /// </summary>
        public bool ByUser { get; set; }

        /// <summary>
        /// Determines if the query should filter by date.
        /// </summary>
        public bool ByDate { get; set; }

        /// <summary>
        /// Filter by start date of activation.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Filter by end date of activation.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// A parameter string to represent either branch ID or user ID.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Parameter string cannot exceed 50 characters.")]
        public string ParameterString { get; set; }

        /// <summary>
        /// Determines if the query should filter by active members.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Determines if the query should filter by deactivated members.
        /// </summary>
        public bool IsDeactivated { get; set; }
    }

    public class AddCMoneyMemberActivationCommand
    {
        [Required(ErrorMessage = "The Customer ID is required.")]
        public string CustomerId { get; set; }

        public string BranchCode { get; set; }
        [Required(ErrorMessage = "Member prefered language is required.")]
        public string Language { get; set; }

        public string LoginId { get; set; }

        [Required(ErrorMessage = "The Phone Number is required.")]
        [Phone(ErrorMessage = "The Phone Number format is invalid.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "The Branch ID is required.")]
        public string BranchId { get; set; }
        [Required(ErrorMessage = "OTP is required.")]
        [StringLength(4, ErrorMessage = "OTP must not exceed 4 characters.")]
        public string OTP { get; set; }
    }

    public class ChangeCMoneyMemberPinCommand
    {
        [Required(ErrorMessage = "The Customer ID is required.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "The Old PIN is required.")]
        [RegularExpression(@"\d{4}", ErrorMessage = "The Old PIN must be exactly 4 numeric digits.")]
        public string OldPin { get; set; }

        [Required(ErrorMessage = "The New PIN is required.")]
        [RegularExpression(@"\d{4}", ErrorMessage = "The New PIN must be exactly 4 numeric digits.")]
        public string NewPin { get; set; }
    }

    public class DeactivateCMoneyMemberCommand
    {
        [Required(ErrorMessage = "The Customer ID is required.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "A Reason for deactivation is required.")]
        [MaxLength(250, ErrorMessage = "The Reason cannot exceed 250 characters.")]
        public string Reason { get; set; }
    }

    public class ManageSecretQuestionCommand
    {
        [Required(ErrorMessage = "The Login ID is required.")]
        public string LoginId { get; set; }

        [Required(ErrorMessage = "The Secret Question is required.")]
        [MaxLength(250, ErrorMessage = "The Secret Question cannot exceed 250 characters.")]
        public string SecretQuestion { get; set; }

        [Required(ErrorMessage = "The Secret Answer is required.")]
        [MaxLength(250, ErrorMessage = "The Secret Answer cannot exceed 250 characters.")]
        public string SecretAnswer { get; set; }
    }

    public class ReactivateCMoneyMemberCommand
    {
        [Required(ErrorMessage = "The Customer ID is required.")]
        public string CustomerId { get; set; }
    }

    public class ResetCMoneyMemberPinCommand
    {
        [Required(ErrorMessage = "The Customer ID is required.")]
        public string CustomerId { get; set; }
    }

    public class ResetPinWithSecurityCommand
    {
        [Required(ErrorMessage = "The Login ID is required.")]
        public string LoginId { get; set; }

        [Required(ErrorMessage = "The Secret Question is required.")]
        [MaxLength(250, ErrorMessage = "The Secret Question cannot exceed 250 characters.")]
        public string SecretQuestion { get; set; }

        [Required(ErrorMessage = "The Secret Answer is required.")]
        [MaxLength(250, ErrorMessage = "The Secret Answer cannot exceed 250 characters.")]
        public string SecretAnswer { get; set; }
    }

    public class UpdateCMoneyMemberActivationCommand
    {
        [Required(ErrorMessage = "The Activation ID is required.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "The Phone Number is required.")]
        [Phone(ErrorMessage = "The Phone Number format is invalid.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "The Active status must be specified.")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "The Subscription status must be specified.")]
        public bool IsSubcribed { get; set; }
    }

    public class GetCMoneyMemberActivationsDatatableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string BranchId { get; set; }
        public bool ByBranch { get; set; }
        public bool ByUser { get; set; }
        public bool ByDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeactivated { get; set; }
        public string CustomerId { get; set; }
        public string LoginId { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public bool? HasChangeDefaultPin { get; set; }
        public bool? IsSubscribed { get; set; }
        public int? FailedAttemptsMin { get; set; }
        public int? FailedAttemptsMax { get; set; }
        public DateTime? LastPaymentStartDate { get; set; }
        public DateTime? LastPaymentEndDate { get; set; }
        public string ActivatedBy { get; set; }
    }
    public class CMoneyMembersActivationAccountDto
    {
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string ActivatedBy { get; set; }
        public string BranchCode { get; set; }
        public DateTime ActivationDate { get; set; }
        public bool IsActive { get; set; }
        public string LoginId { get; set; }
    }

}
