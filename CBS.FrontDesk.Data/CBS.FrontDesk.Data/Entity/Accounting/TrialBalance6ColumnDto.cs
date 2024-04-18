using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
 
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class TrialBalance6ColumnDto
    {
        public string entityId { get; set; }
        public string entityType { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public string branchName { get; set; }
        public object branchLocation { get; set; }
        public string branchAddress { get; set; }
        public string capital { get; set; }
        public string immatriculationNumber { get; set; }
        public string webSite { get; set; }
        public string branchTelephone { get; set; }
        public string headOfficeTelePhone { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string address { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string beginningDebitBalance { get; set; }
        public string beginningCreditBalance { get; set; }
        public string debitBalance { get; set; }
        public string creditBalance { get; set; }
        public string endDebitBalance { get; set; }
        public string endCreditBalance { get; set; }
        public object totalBeginningDebitBalance { get; set; }
        public object totalBeginningCreditBalance { get; set; }
        public object totalDebitBalance { get; set; }
        public object totalCreditBalance { get; set; }
        public object totalEndDebitBalance { get; set; }
        public object totalEndCreditBalance { get; set; }
        public string cartegory { get; set; }
    }

    public class TrialBalance6ColumnDtoServiceResponse
    {
        public List<TrialBalance6ColumnDto> data { get; set; }
        public List<object> errors { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }



}
