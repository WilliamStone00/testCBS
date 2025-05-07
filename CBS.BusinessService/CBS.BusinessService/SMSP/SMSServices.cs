using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SMSP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.SMSP
{
    public class SMSServices : BaseService
    {
        private readonly ApiCallerHelper _smsConfigApiHelper;
        public SMSServices()
        {
            _smsConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CommunicationBaseUrl"].ToString());
        }


        public async Task<CustomDataTable> GetDataTableAsync(GetAllSmsLogsDataTableQuery loansDataTableQuery)
        {
            loansDataTableQuery.Options.sortColumnName = "CreatedDate";
            if (!IsHeadOffice())
            {

                loansDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _smsConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetSMSLogs,
                loansDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(loansDataTableQuery.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: loansDataTableQuery.Options
            );
        }

        public async Task<Sms> GetSmsLOgDetailAsync(string id)
        {
            try
            {
                var cusResponseObject = await _smsConfigApiHelper.GetAsync<ResponseObject<Sms>>(string.Format(APICallHelper.GetSMSLogDetail, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
    }

}
