using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashInfusionRequest : Request
    {
        [Required]
        public string CurrentOperation { get; set; }

        public Request GetRequest()
        {
            return new Request
            {
                RequestedAmount = this.RequestedAmount,
                Requetcomment = this.Requetcomment,
            };
        }

    }
    public class  Request
    {
        [Required]
        public decimal RequestedAmount { get; set; }
        [Required]
        public string Requetcomment { get; set; }
       

    }
    public class Approval
    {
        public string id { get; set; }
        public decimal requestedAmount { get; set; }
        [Required]
        public decimal confirmedAmount { get; set; }
        [Required]
        public string approvedComment { get; set; }
        public string requetcomment { get; set; }
        [Required]
        public string approvedStatus { get; set; }
    }
    public class DetailsDto
    {
        public string id { get; set; }
        public decimal requestedAmount { get; set; }
        [Required]
        public decimal confirmedAmount { get; set; }
        public string requesterUserId { get; set; }
        public string approvedBy { get; set; }
        public DateTime RequestDate { get; set; }
        public string approvedByUserId { get; set; }
        public DateTime approvedDate { get; set; }
        [Required]
        public string approvedComment { get; set; }
        public string requetcomment { get; set; }
        [Required]
        public string approvedStatus { get; set; }

        public static DetailsDto SetDefault(CashInfusionRequest request)
        {
            return new DetailsDto
            {
                id = "Id not Set",
                requestedAmount = 0,
                requesterUserId = "User Id not Set",
                requetcomment = request.Requetcomment,
                approvedComment="No Comment",
                approvedBy = "Not Set",
                approvedByUserId = " Approved By User",
                approvedStatus = "Failed",
                approvedDate = DateTime.Now,
                confirmedAmount = 0

            };
        }
    }

    public class DisplayData
    {
        public string Text { get; set; }
        public string Value { get; set; }

    }
}
