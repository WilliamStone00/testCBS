using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data
{
    public class Account
    {
        public string AccountNumberNetwok { get; set; } = "";

        public string AccountNumberCU { get; set; } = "";
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountTypeId { get; set; }
        public string BeginningBalance { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public string LastBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string ChartOfAccountManagementPositionId { get; set; }
        public string AccountCategoryId { get; set; }
        public string AccountOwnerId { get; set; }
        public string AccountCounterPartId { get; set; }
        public string BookingDirection { get; set; }
        public bool CanBeNegative { get; set; }
        public bool IsBalanceSheetAccount { get; set; }
        public string Account7 { get; set; }
        public string Account6 { get; set; }
        public string Account5 { get; set; }
        public string Account4 { get; set; }
        public string Account3 { get; set; }
        public string Account2 { get; set; }
        public string BranchCode { get; set; }
        public string LiaisonId { get; set; }
        public string AccountNumberManagementPosition { get; set; }
        public string TempData { get; set; }
        public bool IsNormalCreation { get; set; }
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
        public List<AccountModelX> AccountModelList { get; set; }

        public string BranchId { get; set; }
   
    }
    public class UploadAccount
    {
  
        public string BranchId { get; set; }
        public List<AccountModelX> AccountModelList { get; set; }
        public bool IsHarmonizationActivated { get; set; }
    }
    public class AccountModelX
    {
        public string AccountNumberNetwork { get; set; } = "";

        public string AccountNumberCU { get; set; } = "";
        public string ChartofAccount { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        public string BookingDirection { get; set; }
        public decimal BeginningDebitBalance { get; set; }

        public decimal BeginningCreditBalance { get; set; }
        public decimal MovementDebitBalance { get; set; }
        public decimal MovementCreditBalance { get; set; }
        public decimal EndBalanceDebit { get; set; }
        public decimal EndBalanceCredit { get; set; }

public string CreatedDate { get; set; }

        public AccountModelX()
        {
                
        }
        public AccountModelX(string AccNumber, string AccName, string chartofaccount, string DateCreated, decimal beginningBalanceDr, decimal beginningBalanceCr,   decimal movementCreditBalance, decimal movementDebitBalance, decimal endingBalanceCr, decimal endingBalanceDr)
        {

            AccountNumber = AccNumber;
            AccountName = AccName;
            BeginningDebitBalance = beginningBalanceDr;
            BeginningCreditBalance = beginningBalanceCr;
            CreatedDate = DateCreated;
            ChartofAccount = chartofaccount;
            //BranchCode = branchCode;
            BookingDirection = beginningBalanceCr == 0 ? "D" : "C";
            EndBalanceCredit = endingBalanceCr;
            EndBalanceDebit = endingBalanceDr;
            MovementDebitBalance = movementDebitBalance;
            MovementCreditBalance = movementCreditBalance;
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
