using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Reporting
{
    public class Reporting
    {    
        
            public string ReportCategory { get; set; } // Category, ChequeBook, ChequeBookRequest, CounterCheque, Transaction, All
            public string BranchId { get; set; }
            public string MemberReference { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }

            // Additional optional parameters
            public string CategoryType { get; set; }
            public string ChequeBookStatus { get; set; }
            public string RequestStatus { get; set; }
            public string ChequeType { get; set; }
            public string TransactionType { get; set; }
            public string ChequeNumber { get; set; }
            public string AmountRange { get; set; }
            public string ReportFormat { get; set; }
        
}
}
