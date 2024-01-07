using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;

namespace CBS.BusinessService.CustomerManagement
{
    public interface IIndividualProfileServices
    {
        Task<ExecutionMessages> Create(IndividualProfile model);
        Task<Aggregrate> GetAggregates();
        Task<ExecutionMessages> Delete(string id);
        Task<ExecutionMessages> UpdateProfile(IndividualCustomerProfile model);
        Task<ExecutionMessages> UpdateBankInfo(IndividualCustomerProfile model);
        Task<ExecutionMessages> ActivateDeactivate(IndividualCustomerProfile model);
        Task<IndividualCustomerProfile> GetCustomerProfile(string id);
        Task<IndividualCustomerProfile> GetCustomer(string id);
        Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions);
        
    }
}