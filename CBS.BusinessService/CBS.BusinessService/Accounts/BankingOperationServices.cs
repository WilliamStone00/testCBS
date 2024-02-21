using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounts
{
    public  class BankingOperationServices
    {
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly AccountingServices _accountingServices;
        public BankingOperationServices()
        {
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();

        }


    }
}
