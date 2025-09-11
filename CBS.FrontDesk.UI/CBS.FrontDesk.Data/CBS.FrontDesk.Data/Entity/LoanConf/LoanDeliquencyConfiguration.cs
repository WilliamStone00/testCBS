using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanDeliquencyConfiguration
    {
        public string Id { get; set; }
        [Required]
        public int DaysFrom { get; set; }
        [Required]

        public int DaysTo { get; set; }
        [Required]

        public string Status { get; set; }//Normal,Bad loan, Due loan, Over due loan, unracoverable loan, Write off loan
        [Required]

        public string Name { get; set; }// Par 0-30, Par 31-60, Par 61-90, Par 91-120
        public bool SendSMStoClient { get; set; }
        public bool SendMail { get; set; }
        public bool SendSMS { get; set; }
        public bool ApplyFine { get; set; }
        public bool AffectScoring { get; set; }
        public bool ReportToCreditOffice { get; set; }

    }


}
