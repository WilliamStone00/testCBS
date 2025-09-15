using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;

namespace CBS.BusinessService.DailyCollectionServices
{

    public class DailySaverUpload
    {
        public List<DailySaverRequest> DailySaverList { get; set; }
        public string BranchId { get; set; }
        public string CollectorId { get; set; }
        public string CollectorName { get; set; }
        public string AccountId { get; set; }
        public string CashDifferenceAccountId { get; set; }
        public string ProductId { get; set; }
    }
    public class PostingCommand  
    {
        public string CorrespondingAccountId { get; set; }
        public string BranchId { get; set; }
        public string Id { get; set; }


    }
    public class DailySaverUploadTempResult
    {



        public string BranchName { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalExpectedAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public string CollectorGL { get; set; }
        public decimal GLAccountBalance { get; set; }
        public string CollectorName { get; set; }
        public string AccountId { get; set; }
        public string BranchId { get; set; }
        public string ProductId { get; set; }
        public string Id { get; set; }
        public decimal TotalAmount { get; set; }

        internal PostingCommand ToPostingCommand()
        {
            return new PostingCommand
            {
                Id= Id,
                BranchId= BranchId,
                CorrespondingAccountId= AccountId
            };
        }
    }
    public class AddDailySaverMinCommand
    {


        public string DailySaverId { get; set; }
        public bool IsNewCustomer { get; set; }
        public string FirstName { get; set; }

        public string BankCode { get; set; }//
        public string BranchName { get; set; }//
        public string BranchCode { get; set; }
    }

}