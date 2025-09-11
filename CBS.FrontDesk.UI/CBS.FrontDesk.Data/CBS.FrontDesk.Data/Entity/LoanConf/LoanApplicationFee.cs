using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanApplicationFee
    {
        public string Id { get; set; }
        public string FeeRangeId { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeLable { get; set; }
        public decimal AmountPaid { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCashDeskPayment { get; set; }
        public string LoanApplicationId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string Period { get; set; }
        public string TransactionReference { get; set; }
        public virtual FeeRange FeeRange { get; set; }
        public virtual LoanApplication LoanApplication { get; set; }
        public DateTime DateOfPayment { get; set; } = DateTime.MinValue;
        public string PaidBy { get; set; }
    }

}
