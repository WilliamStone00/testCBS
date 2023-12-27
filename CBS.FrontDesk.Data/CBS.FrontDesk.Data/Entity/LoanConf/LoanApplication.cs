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
        public string scheduleTypeId { get; set; }
        public double amount { get; set; }
        public double interestRate { get; set; }
        public double disbursmentFee { get; set; }
        public double followupFee { get; set; }
        public int numberOfInstallment { get; set; }
        public DateTime firstPreferenceDisburseDate { get; set; }
        public DateTime applicationDate { get; set; }
        public DateTime approvalDate { get; set; }
        public DateTime disbursementDate { get; set; }
        public DateTime nextInstallmentDate { get; set; }
        public string installmentTypeID { get; set; }
        public string creditLineId { get; set; }
        public string customerid { get; set; }
        public string borrowerDescription { get; set; }
        public string economicActivityId { get; set; }
        public string accountNumber { get; set; }//For recoveries
        public int gracePeriod { get; set; }
        public double gracePeriodAmount { get; set; }
        public double insuranceFund { get; set; }
        public bool isGuarantee { get; set; }
        public string guarantyPackId { get; set; }
        public string documentPackId { get; set; }
        public string loanPurpose { get; set; }
        public double scoreRiskAmount { get; set; }
        public double scoringAmount { get; set; }
        public string organizationId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }    
        public string approvalStatus { get; set; }//Pending,Rejected,Understudies,Approved,Disbursed
        public bool isApproved { get; set; }
        public bool isDisbured { get; set; }
        public virtual ICollection<GurantiPack> GurantiPacks { get; set; }
        public virtual ICollection<LoanCollatera> Collateras { get; set; }
        public virtual ICollection<LoanGuarantor> Guarantors { get; set; }
        public virtual ICollection<LoanCommiteeValidation> CreditCommiteeValidations { get; set; }
        public virtual ICollection<DocumentPack> DocumentPacks { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
    }
}

