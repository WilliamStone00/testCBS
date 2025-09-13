using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanCommitee
{
    public class LoanCommiteeValidationHistory
    {

        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public string UserId { get; set; }
        [Required]
        public string Comment { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public LoanApplication LoanApplication { get; set; }
        public LoanCommiteeMember LoanCommiteeMember { get; set; }

    }

}
