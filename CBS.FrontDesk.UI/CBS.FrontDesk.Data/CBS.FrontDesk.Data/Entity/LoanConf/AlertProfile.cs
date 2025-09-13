using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{

    public class AlertProfile
    {
        public string id { get; set; }
        public string name { get; set; }
        public string msisdn { get; set; }
        public bool sendSMS { get; set; }
        public bool sendEmail { get; set; }
        public bool isSupperAdmin { get; set; }
        public string language { get; set; }
        public bool activeStatus { get; set; }
        public string serviceId { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string organizationId { get; set; }

        
    }

}
