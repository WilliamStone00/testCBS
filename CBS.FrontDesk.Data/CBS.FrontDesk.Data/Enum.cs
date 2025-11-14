using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data
{

    public enum BulkOperationStatus
    {
        Pending,
        Accepted,
        Rejected,
        NoValidAccounts,
        BulkProcessInitiated,
        Executed,
        Failed,
        ProcessingApproval,
        Approved
    }

    public enum RemittanceTypes
    {
        WesternUnion,
        MoneyGram,
        Ria,
        OFX,
        MPesa,
        Payoneer,
        WorldRemit,
        TrustSoftCredit
    }
    public enum EntryStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum AdjustmentType
    {
        NameAdjustment,
        MemberReferenceAdjustment,
        MemberActiveStatusAdjustment,
        MemberActivationAdjustment,
        MemberMembershipStatusAdjustment,
        MemberCategoryAdjustment,
        AccountBalanceAdjustment,
        AccountStatusAdjustment,
    }

    public enum Status { Approved, Successful, Failed, Pending, Reviewed, Rejected, Completed, ProcessingApproval }


}
