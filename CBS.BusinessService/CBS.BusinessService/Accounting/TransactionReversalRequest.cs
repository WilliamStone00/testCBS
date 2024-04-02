using CBS.BusinessService.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class TransactionReversalRequest 
    {


        public string RequestMessage { get; set; }
        [TransactionReferenceValidator]
        public string ReferenceNumber { get; set; }


    }

    public class TransactionReversalDetailRequestDto
    {
        public string RequestMessage { get; set; }
        public string Id { get; set; }
        public string ReferenceId { get; set; } // opening balance reference id
        public string IssuedBy { get; set; }
        public DateTime IssuedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedMessage { get; set; }
        public string Status { get; set; }
    }

    public class TransactionReversalRequestApproval  //: IRequest<ServiceResponse<TransactionReversalRequestDataDto>>
    {

        public string Id { get; set; }
        public string ApprovedMessage { get; set; }

        public bool IsApproved { get; set; }
        public string Status { get; set; }

        public TransactionReversalRequestApproval ConvertToTransactionReversalRequestApproval()
        {
             return new TransactionReversalRequestApproval { Id = this.Id, ApprovedMessage = this.ApprovedMessage, IsApproved = this.IsApproved};
        }
    }
}
