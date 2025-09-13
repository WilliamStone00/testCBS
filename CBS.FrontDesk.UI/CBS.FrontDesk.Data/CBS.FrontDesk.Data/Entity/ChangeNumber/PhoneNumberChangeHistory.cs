using CBS.FrontDesk.Data.Entity.CustomerManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ChangeNumber
{
    public class PhoneNumberChangeHistory
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string MemberName { get; set; }
        public string OldPhoneNumber { get; set; }
        public string NewPhoneNumber { get; set; }
        public string RequestBy { get; set; }
        public string ApprovedBy { get; set; }
        public string RequestUserId { get; set; }
        public string ApprovalUserId { get; set; }
        public DateTime DateOfRequest { get; set; }
        public DateTime ApproveDate { get; set; }
        public string RequestComment { get; set; }
        public string ApprovalComment { get; set; }
        public string Status { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
    }

    public class ApprovePhoneNumberRequestCommand
    {
        [Required(ErrorMessage = "Phone number change history ID is required.")]
        public string PhoneNumberChangeHistoryId { get; set; }

        [Required(ErrorMessage = "Approval comment is required.")]
        [StringLength(250, ErrorMessage = "Approval comment cannot exceed 250 characters.")]
        public string ApprovalComment { get; set; }

        [Required(ErrorMessage = "Approval status must be specified.")]
        public bool Approved { get; set; }
    }



    public class ChangePhoneNumberRequestCommand
    {
        [Required(ErrorMessage = "Member Reference is required.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Old phone number is required.")]
        public string OldPhoneNumber { get; set; }

        [Required(ErrorMessage = "New phone number is required.")]
        [DifferentPhoneNumber] // Custom validation to ensure different phone numbers
        public string NewPhoneNumber { get; set; }

        [Required(ErrorMessage = "Request comment is required.")]
        [StringLength(250, ErrorMessage = "Request comment cannot exceed 250 characters.")]
        public string RequestComment { get; set; }

        [Required(ErrorMessage = "OTP code is required.")]
        [Range(0000, 999999, ErrorMessage = "OTP code must be a 4-digit number.")]
        public int OtpCode { get; set; }
    }


    public class ChangeNumberCarrier
    {
        public PhoneNumberChangeHistory PhoneNumberChangeHistory { get; set; }
        public List<PhoneNumberChangeHistory> PhoneNumberChangeHistories { get; set; }
        public ChangePhoneNumberRequestCommand ChangePhoneNumberRequestCommand { get; set; }
        public ApprovePhoneNumberRequestCommand ApprovePhoneNumberRequestCommand { get; set; }
        public IndividualProfile Customer { get; set; }
        public string Option { get; set; }
        public ChangeNumberCarrier()
        {
            PhoneNumberChangeHistory=new PhoneNumberChangeHistory();
            PhoneNumberChangeHistories=new List<PhoneNumberChangeHistory>();
            ChangePhoneNumberRequestCommand=new ChangePhoneNumberRequestCommand();
            ApprovePhoneNumberRequestCommand=new ApprovePhoneNumberRequestCommand();
            Customer=new IndividualProfile();
        }
    }


    public class DifferentPhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ChangePhoneNumberRequestCommand)validationContext.ObjectInstance;

            if (model == null || string.IsNullOrEmpty(model.OldPhoneNumber) || string.IsNullOrEmpty(model.NewPhoneNumber))
            {
                return ValidationResult.Success;
            }

            // Normalize phone numbers (remove +237 or 237 prefix if present)
            string oldNumber = NormalizePhoneNumber(model.OldPhoneNumber);
            string newNumber = NormalizePhoneNumber(model.NewPhoneNumber);

            if (oldNumber == newNumber)
            {
                return new ValidationResult("New phone number must be different from the old phone number.");
            }

            return ValidationResult.Success;
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            // Remove country code +237 or 237 if present
            return Regex.Replace(phoneNumber, @"^(\+?237)", "").Trim();
        }
    }

}
