using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data
{
    public class Account
    {
        public string AccountNumberNetwork { get; set; } = "";

        public string AccountNumberCU { get; set; } = "";
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountTypeId { get; set; }
        public string BeginningBalance { get; set; }
        public string DebitBalance { get; set; }
        public string CreditBalance { get; set; }
        public string LastBalance { get; set; }
        public string CurrentBalance { get; set; }
        public string ChartOfAccountId { get; set; }
        public string AccountCategoryId { get; set; }
        public string AccountOwnerId { get; set; }
        public string BookingDirection { get; set; }
        public bool CanBeNegative { get; set; }
        public bool IsBalanceSheetAccount { get; set; }
        public string Account7 { get; set; }
        public string Account6 { get; set; }
        public string Account5 { get; set; }
        public string Account4 { get; set; }
        public string Account3 { get; set; }
        public string Account2 { get; set; }
    }


    public class AccountDto
    {
        //public string BranchName { get; set; }
        //public string BranchLocation { get; set; }

        //public string Capital { get; set; }
        //public string ImmatriculationNumber { get; set; }
        //public string WebSite { get; set; }
        //public string BranchTelephone { get; set; }
        //public string HeadOfficeTelePhone { get; set; }
        //public string Name { get; set; }
        //public string Location { get; set; }
        //public string Address { get; set; }
        //public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string DebitBalance { get; set; }
        public string CreditBalance { get; set; }
        public string CurrentBalance { get; set; }
 
    }

    public class UploadAccountCommand  
    {
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public List<AccountModel> AccountModelList { get; set; }
    }
    public class AccountModel
    {
        public string AccountNumberNetwork { get; set; } = "";

        public string AccountNumberCU { get; set; } = "";
        public string ChartofAccount { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BranchCode { get; set; }

        public decimal BeginningBalance { get; set; }
     
        public decimal CurrentBalance { get; set; }
  
        public string CreatedDate { get;  set; }

        public AccountModel()
        {
                
        }
        public AccountModel(string AccNumber, string AccName, string chartofaccount, string DateCreated, decimal beginningBalance , decimal currentBalance, string branchCode)
        {
           
            AccountNumber = AccNumber;  
            AccountName = AccName;
            BeginningBalance= beginningBalance;
            CurrentBalance = currentBalance;
  CreatedDate= DateCreated;
            ChartofAccount = chartofaccount;
            BranchCode= branchCode;
        }

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
