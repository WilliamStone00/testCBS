using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AuditTrailP
{
    public class AuditTrailServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public AuditTrailServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingBank, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<AuditTrailDto> GetAuditTrailAsync(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<AuditTrailDto>>(string.Format(APICallHelper.Get_AuditTrail, id));
                var auditTrail = new AuditTrailDto();
                if (cusResponseObject.IsSuccess && cusResponseObject.ApiResponseData != null)
                {
                    auditTrail = cusResponseObject.ApiResponseData.Data;
                    return auditTrail;
                }
                return auditTrail;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }

        }
        public async Task<CustomDataTable> GetDataTableAsync(GetAuditTrailsDataTableQuery auditTrailsDataTableQuery, string searchCriterial)
        {
            auditTrailsDataTableQuery.DataTableOptions.searchValue = searchCriterial;
            auditTrailsDataTableQuery.DataTableOptions.search = searchCriterial;
            auditTrailsDataTableQuery.DataTableOptions.sortColumnName = "Timestamp";

            // Make API call to fetch the DataTable result
            var couApiResponse = await _bankConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.Get_GetAuditTrailBy_DataTable_Pagging,
                auditTrailsDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(auditTrailsDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: auditTrailsDataTableQuery.DataTableOptions
            );
        }





    }

}
