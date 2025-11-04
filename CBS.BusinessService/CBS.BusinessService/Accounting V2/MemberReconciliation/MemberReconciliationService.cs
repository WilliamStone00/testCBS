using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.MemberReconciliation
{
    public class MemberReconciliationService : BaseService
    {     
            private readonly ApiCallerHelper _apiCallerHelper;
            private readonly ApiCallerHelper _apiCallerHelper2;

            public MemberReconciliationService()
            {
                //change the base url to the actual base url
                string baseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'TransactionBaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);

                string baseUrl2 = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
                    if (string.IsNullOrEmpty(baseUrl2))
                    {
                        throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
                    }
                _apiCallerHelper2 = new ApiCallerHelper(baseUrl2);
            }



        public async Task<IEnumerable<GetBalance>> GetAccountBalanceAsync(GetBalance payload)
        {
            try
            {
                 payload.includeOnlyActive = false;
                payload.Status = "";

                // Request with generic that allows us to get the raw 'data' token
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<JToken>>(APICallHelper.GetMemeberAccountBalance, payload);

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

        public async Task<Affiliateresponse> GetByIdAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("id is required", nameof(id));

                    var encodedId = Uri.EscapeDataString(id);
                    string formattedUrl = string.Format(APICallHelper.GetMemberReconciliationById, encodedId);

                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<Affiliateresponse>>(formattedUrl);

                    if (response.IsSuccess)
                    {
                        return response.ApiResponseData?.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw;
            }
        }




        //public async Task<TrialBalanceReconciliationData> ReconcileTrialBalance(GetBalance model)
        //{
        //     try
        //     {
        //        model.Language = GetLanguage();
        //        var response = await _apiCallerHelper2.PostAsync<ServiceResponse<TrialBalanceReconciliationData>>(APICallHelper.Reconciliation, model);

        //        // For now, return success (replace with actual service call)

        //       return response?.ApiResponseData?.Data;
        //     }
        //    catch (Exception ex)
        //     {
        //        throw new Exception(ex.Message);
        //    }

        //}

        public async Task<TrialBalanceReconciliationData> ReconcileTrialBalance(GetBalance model)
        {
            model.Language = GetLanguage();

            var response = await _apiCallerHelper2.PostAsync<ServiceResponse<TrialBalanceReconciliationData>>(APICallHelper.Reconciliation, model);

            // check response for success / nulls
            if (response == null || response.ApiResponseData == null || response.ApiResponseData.Data == null)
            {
                // optionally throw or return null and let caller handle
                return null;
            }

            return response.ApiResponseData.Data;
        }



        public async Task<ExecutionMessages> FinalizeReconciliation(FinalReconciliationRequest model)
        {
            try
            {
                var response = await _apiCallerHelper2.PostAsync<ServiceResponse<FinalReconciliationRequest>>(APICallHelper.FINALReconciliation, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.BranchId, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.BranchId, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.BranchId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
            }

            public async Task<ExecutionMessages> UpdateAsync(AffiliateCommand model)
            {
                try
                {
                    var catid = model.Id;
                    string formattedUrl = string.Format(APICallHelper.UpdateMemberReconciliation, catid);
                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<AffiliateCommand>>(formattedUrl, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> DeleteAsync(string Id)
            {
                try
                {
                    string formattedUrl = string.Format(APICallHelper.DeleteMemberReconciliation, Id);
                    var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(null, true, Id, MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                            response.ApiResponseData?.Message );
                    }
                    else
                    {
                        GetExecutionMessages(null, false, Id, MessagesResults.Failed,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                            response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(null, false, Id, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }

                return ExecutionMessage;
            }
      
            public async Task<List<StringValues>> MemberAccountTypesAsync(string branchId)
            {
                try
                {
                    var Build = new GetBalance()
                    {
                        BranchId = branchId,
                        Status = "Active",
                        includeOnlyActive = false
                    };

                    string formattedUrl = string.Format(APICallHelper.MemberAccountType);

                    var response = await _apiCallerHelper.PostAsync<ResponseObject<AccountingTypes>>(formattedUrl,Build);

                    return response?.ApiResponseData?.Data?.AccountTypes ?? new List<StringValues>();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }        
    }
}


