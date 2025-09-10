using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public interface IBranchServices
    {
        Task<ExecutionMessages> Create(Branch model);
        Task<ExecutionMessages> Delete(string id);
        Task<Branch> GetBranch(string id);
        Task<Branch> GetBranchByBankID(string bankid);
        Task<IEnumerable<Branch>> GetBranches();
        Task<ExecutionMessages> Update(Branch model);
        Task<ExecutionMessages> UploadBranchLogo(CustomerDocumentRequest attachedToLoan);
    }
}