using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class DailyTellerServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly TellerProvissioningServices _userManagementServices;
        private readonly TellerServices _tellerServices;

        public DailyTellerServices(TellerProvissioningServices userManagementServices = null, TellerServices tellerServices = null)
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
            _userManagementServices = userManagementServices;
            _tellerServices = tellerServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objDailyTeller = await GetDailyTeller(id);
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objDailyTeller.UserName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objDailyTeller, false, $"{objDailyTeller.UserName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        //

        public async Task<IEnumerable<StringValues>> LoadDailyUsers()
        {
            try
            {
                var users = await _userManagementServices.GetUserTellerRoleDropDown();
                return users;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<DailyTeller>> GetDailyTellers(DateTime DateFrom, DateTime DateTo, string branchId)
        {
            try
            {
                var Tellers = await _tellerServices.GetTellers();

                var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo, BranchId = branchId };
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTellerByBranch, query);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var branch = await _branchServices.GetBranch(GetBranchID());
                    var dailyTellers = couApiResponse.ApiResponseData.Data;
                    var filteredTellers = dailyTellers.Where(x => x.BranchId == GetBranchID())
                                                      .Select(dailyTeller => MapDailyTellerWithBranch(dailyTeller, branch, Tellers.Where(t => t.id == dailyTeller.TellerId).FirstOrDefault()));

                    return filteredTellers;
                }


                return Enumerable.Empty<DailyTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<DailyTeller>> GetDailyTellers(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                var Tellers = await _tellerServices.GetTellers();

                if (IsHeadOffice())
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo };
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTeller, query);

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branches = await _branchServices.GetBranches();

                        var dailyTellers = couApiResponse.ApiResponseData.Data;
                        var data = from dailyTeller in dailyTellers
                                   join branch in branches on dailyTeller.BranchId equals branch.Id
                                   join teller in Tellers on dailyTeller.TellerId equals teller.id
                                   select MapDailyTellerWithBranch(dailyTeller, branch, teller);

                        return data;
                    }
                }
                else
                {
                    var query = new QueryParamWithDates { DateFrom = DateFrom, DateTo = DateTo, BranchId = GetBranchID() };
                    var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<DailyTeller>>>(APICallHelper.GetAllDailyTellerByBranch, query);

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var branch = await _branchServices.GetBranch(GetBranchID());
                        var dailyTellers = couApiResponse.ApiResponseData.Data;
                        var filteredTellers = dailyTellers.Where(x => x.BranchId == GetBranchID())
                                                          .Select(dailyTeller => MapDailyTellerWithBranch(dailyTeller, branch, Tellers.Where(t => t.id == dailyTeller.TellerId).FirstOrDefault()));

                        return filteredTellers;
                    }
                }

                return Enumerable.Empty<DailyTeller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private DailyTeller MapDailyTellerWithBranch(DailyTeller dailyTeller, Branch branch,Teller teller)
        {
            return new DailyTeller
            {
                Id = dailyTeller.Id,
                UserId = dailyTeller.UserId,
                UserName = dailyTeller.UserName,
                ProvisionedBy = dailyTeller.ProvisionedBy,
                TellerId = dailyTeller.TellerId,
                Status = dailyTeller.Status,
                IsPrimary = dailyTeller.IsPrimary,
                BranchId = dailyTeller.BranchId,
                MaximumWithdrawalAmount = dailyTeller.MaximumWithdrawalAmount,
                MaximumCeilin = dailyTeller.MaximumCeilin,
                Teller = teller,
                Branch = branch,
                PrimaryTellerProvisioningHistories = dailyTeller.PrimaryTellerProvisioningHistories,
                SubTellerProvioningHistories = dailyTeller.SubTellerProvioningHistories
            };
        }


        public async Task<DailyTeller> GetDailyTeller(string id)
        {
            try
            {
                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<DailyTeller>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, id));
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
        public async Task<ExecutionMessages> Create(DailyTeller model)
        {
            try
            {
                string[] parts = model.UserId.Split('@');
                model.UserId = parts[0];
                model.UserName = parts[1];
                model.ProvisionedBy = GetUserFullName();
                // Make an API call to create an individual profile
                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<DailyTeller>>(APICallHelper.CreateDailyTeller, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.UserName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.UserName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(DailyTeller model)
        {
            try
            {
                string[] parts = model.UserId.Split('@');
                model.UserId = parts[0];
                model.UserName = parts[1];

                var DailyTeller = await GetDailyTeller(model.Id);
                if (DailyTeller != null)
                {
                    //DailyTeller.IsPrimary = model.IsPrimary;
                    DailyTeller.MaximumCeilin = model.MaximumCeilin;
                    DailyTeller.MaximumWithdrawalAmount = model.MaximumWithdrawalAmount;
                    DailyTeller.ProvisionedBy = GetUserFullName();
                    DailyTeller.Status = model.Status;
                    DailyTeller.TellerId = model.TellerId;
                    DailyTeller.UserId = model.UserId;
                    DailyTeller.BranchId = model.BranchId;
                    DailyTeller.UserName = model.UserName;
                    var response = await _transactionBaseConfigApiHelper.PutAsync<ServiceResponse<DailyTeller>>(string.Format(APICallHelper.Get_Update_Delete_DailyTeller, model.Id), DailyTeller);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{DailyTeller.UserName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{DailyTeller.UserName}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

    }

}
