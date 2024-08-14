using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class NotificationServices
    {
        private readonly AccountingServices _AccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly BranchServices branchServices;
        private readonly UserManagementServices _userServices;
    }
}
