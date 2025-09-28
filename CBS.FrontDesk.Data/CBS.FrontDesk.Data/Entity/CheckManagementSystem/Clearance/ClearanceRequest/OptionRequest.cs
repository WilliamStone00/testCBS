using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest
{
    public class OptionRequest
    {
        public string ChequeClearanceId { get; set; }
        public bool IsNotfromMFI { get; set; }
        [Required(ErrorMessage = "Branch is required")]
        public string BranchId { get; set; } = null;

        [Required(ErrorMessage = "CheckBook ID is required")]
        public string CheckBookNumber { get; set; } = null ;

        [Required(ErrorMessage = "Page number is required")]
        public string CheckBookPageNumber { get; set; } = null;
        public string AccountNumber { get; set; }

        public bool DepositToAccount { get; set; }
        public bool SameDayProcessing { get; set; }
        public string NationalIdNumber { get; set; }
        public string ChequeBranchId { get; set; }
        public string ReceivingBranchId { get; set; }
        public string MemberReference { get; set; }
        public DateTime NationalIdCreationDate { get; set; }
        public DateTime NationalIdExpirationDate { get; set; }
        public DateTime ChequeCreationDate { get; set; }
        public DateTime ChequeExpirationDate { get; set; }
        public decimal ChequeAmount { get; set; }
        public string Status { get; set; }
        //public string DepositToAccount { get; set; }
        public string FrontShotImagePath { get; set; } = null;
    }
}
