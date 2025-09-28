using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeCertification
{
    public class ChequeCertificationDto
    {
        public string ChequeCertificationID { get; set; }
        public string MemberReference { get; set; }
        public string BranchId { get; set; }
        public string ChequeBookId { get; set; }
        public string ChequeleafId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CertificationStatus { get; set; }
        public string Description { get; set; }
      
    }
}
