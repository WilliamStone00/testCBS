using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data
{
    public class Account
    {
        public string AccountNumberCamCCUL { get; set; } = "";

        public string AccountNumberAffiliate { get; set; } = "";
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountTypeId { get; set; }
        public string ChartOfAccountId { get; set; }
        public string AccountCategoryId { get; set; }
        public string AccountOwnerId { get; set; }
        public string BookingDirection { get; set; }
        public bool CanBeNegative { get; set; }
        public bool IsBalanceSheetAccount { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class AccountHoDto
    {
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string AccountTypeId { get; set; }
        public string ChartOfAccountDetails { get; set; }
        public string AccountOwnerId { get; set; }
    }


    public class AccountInfo
    {
        public string AccountNumberCamCCUL { get; set; } = "";

        public string AccountNumberAffiliate { get; set; } = "";
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountTypeId { get; set; }
        public string ChartOfAccountId { get; set; }
        public string AccountOwnerId { get; set; }
        public string BookingDirection { get; set; }
      
    }
}
