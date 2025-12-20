using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNet.SignalR.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.MemberReconciliation
{
    public class LoanReconciliationService : BaseService

    {
        private readonly ApiCallerHelper _apiCallerHelper;
        public LoanReconciliationService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["LoanBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'LoanBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<LoanReconciliationDto>> GetAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<LoanReconciliationDto>>>(APICallHelper.GetAllLoans);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<LoanReconciliationDto>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<IEnumerable<GetBalance>> GetAccountBalance(GetBalance payload)
        {
            try
            {
                payload.IsByBranch = true;          
                payload.StatusFilter = "Open";

                // Request with generic that allows us to get the raw 'data' token
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<JToken>>(APICallHelper.GetLoadAccountBalance, payload);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    
                    JToken data = response.ApiResponseData.Data;

                    if (data.Type == JTokenType.Array)
                    {
                        // deserialize array to list
                        var list = data.ToObject<List<GetBalance>>();
                        return list ?? new List<GetBalance>();
                    }
                    else if (data.Type == JTokenType.Object)
                    {
                        // single object -> wrap into a list
                        var single = data.ToObject<GetBalance>();
                        return single != null ? new List<GetBalance> { single } : new List<GetBalance>();
                    }
                    else if (data.Type == JTokenType.Null)
                    {
                        return new List<GetBalance>();
                    }

                    // fallback: try to convert generically
                    return data.ToObject<List<GetBalance>>() ?? new List<GetBalance>();
                }

                return new List<GetBalance>();
            }
            catch (Exception ex)
            {
                // log ex
                throw;
            }
        } 
                     

        public async Task<List<StringValues>> LoanAccountTypeAsync(string branchId)
        {
            try
            {
               string formattedUrl = string.Format(APICallHelper.LoanAccountDROP,branchId);
                var response = await _apiCallerHelper.GetAsync<ResponseObject<AccountingTypes>>(formattedUrl);
                
                return response?.ApiResponseData?.Data?.LoanTypes ?? new List<StringValues>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
       
    }
}
