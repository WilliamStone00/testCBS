using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    using CBS.FrontDesk.Data.Entity.LoanConf.PCMFStructure;
    using System.ComponentModel.DataAnnotations;

    public class LoanTerm
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Name in English is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string NameEn { get; set; }
        [Required(ErrorMessage = "Name in French is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string NameFr { get; set; }

        public bool IsActive { get; set; } = true;
        // ✅ NEW
        public LoanTermKind TermKind { get; set; }
        public virtual ICollection<PCMFLoanProduct> LoanProducts { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Minimum term in months is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum term in months must be at least 1.")]
        public int MinInMonth { get; set; }

        [Required(ErrorMessage = "Maximum term in months is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum term in months must be at least 1.")]
        [CustomValidation(typeof(LoanTerm), nameof(ValidateTermRange))]
        public int MaxInMonth { get; set; }
        /// <summary>
        /// Custom validation to ensure MinInMonth is less than or equal to MaxInMonth.
        /// </summary>
        public static ValidationResult ValidateTermRange(int maxInMonth, ValidationContext context)
        {
            var instance = context.ObjectInstance as LoanTerm;
            if (instance != null && instance.MinInMonth > maxInMonth)
            {
                return new ValidationResult("Minimum term in months cannot be greater than the maximum term.");
            }
            return ValidationResult.Success;
        }

        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public int ObjectState { get; set; }
        public bool IsDeleted { get; set; }
    }

    }
// public virtual ICollection<LoanProduct> LoanProducts { get; set; }