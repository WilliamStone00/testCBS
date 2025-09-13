using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DashBoards
{
    public class DashboardData
    {
        public LoanStatistics LoanStatistics { get; set; }
        public DailyOperationStatistics DailyOperationStatistics { get; set; }
        public List<OrdinaryAccountStatistics> OrdinaryAccountStatistics { get; set; }
        public MemberStatistics MemberStatistics { get; set; }


        public AccountingBookingStatistics AccountingBookingStatistics { get; set; }
    }
}
