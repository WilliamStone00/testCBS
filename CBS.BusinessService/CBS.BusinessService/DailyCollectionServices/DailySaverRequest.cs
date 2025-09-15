using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CBS.BusinessService.DailyCollectionServices
{
    public class DailySaverRequest
    {
        public string AccountNumber { get;   set; }
        public string FirstName { get;   set; }
        public string Username { get;   set; }
        public string BranchCode { get;   set; }
        public string DailySaverId { get;   set; }
        public string BranchName { get;   set; }
        public bool IsNewCustomer { get;   set; }
        public string BankCode { get;   set; }
        public string AccountId { get;   set; }
        public decimal Amount { get;   set; }
        public string BranchId { get;   set; }
        public bool HasBeenProcessed { get;   set; }
 
        public string Id { get; set; }

        
        public string CreatedBy { get; set; }
 
        public string ModifiedBy { get; set; }

 
        public string CollectorId { get; set; }
        public string ChartOfAccountId { get;   set; }
    }
}