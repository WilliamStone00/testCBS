using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DepositLimit
    {
        public string id { get; set; }
        public string productId { get; set; }
        public string depositType { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double feePercentage { get; set; }
    }

    public class Saving
    {
        public string id { get; set; }
        public string name { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double yearlyInterestRate { get; set; }
        public string interestCalculationFrequency { get; set; }
        public string postingFrequency { get; set; }
        public List<DepositLimit> depositLimits { get; set; }
        public List<object> withdrawalLimits { get; set; }
        public List<TransferLimit> transferLimits { get; set; }
        public double entryFee { get; set; }
        public double reopeningFee { get; set; }
        public double closingFee { get; set; }
        public double managementFee { get; set; }
        public string managementFeeFrequency { get; set; }
    }

    public class TransferLimit
    {
        public string id { get; set; }
        public string productId { get; set; }
        public string transferType { get; set; }
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double feePercentage { get; set; }
    }


}
