using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
   
    public class LoanApplication
    {
        public string id { get; set; }
        public string loanProductId { get; set; }
        public double amount { get; set; }
        public double interestRate { get; set; }
        public double disbursementFee { get; set; }
        public double followupFee { get; set; }
        public int numberOfInstallment { get; set; }
        public DateTime firstPreferenceDisburseDate { get; set; }
        public string installmentTypeId { get; set; }
        public string creditLineId { get; set; }
        public string customerId { get; set; }
        public string borrowerDescription { get; set; }
        public string economicActivityId { get; set; }
        public string accountNumber { get; set; }
        public int gracePeriod { get; set; }
        public double gracePeriodAmount { get; set; }
        public double insuranceFund { get; set; }
        public bool isGuarantee { get; set; }
        public string loanPurposeId { get; set; }
        public double scoreRiskAmount { get; set; }
        public double scoringAmount { get; set; }
        public string organizationId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
        public string approvalStatus { get; set; }
        public bool isApproved { get; set; }
        public bool isDisbursed { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedBy { get; set; }
        public DateTime deletedDate { get; set; }
        public DateTime applicationDate { get; set; }
        public DateTime approvalDate { get; set; }
        public DateTime disbursementDate { get; set; }
        public DateTime nextInstallmentDate { get; set; }
    }
}

