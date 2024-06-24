using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class TellerServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;
        private readonly BranchServices _branchServices;

        public TellerServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objTeller = await GetTeller(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_Teller, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objTeller.name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objTeller, false, $"{objTeller.name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Teller>> GetTellers()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<Teller>>>(APICallHelper.GetAllTeller);
                var branches = await _branchServices.GetBranches();

                if (IsHeadOffice())
                {

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var data = from teller in couApiResponse.ApiResponseData.Data
                                   join branch in branches on teller.branchId equals branch.Id
                                   select new Teller
                                   {
                                       id = teller.id,
                                       isPrimary = teller.isPrimary,
                                       name = teller.name,
                                       code = teller.code,
                                       bankId = teller.bankId,
                                       branchId = teller.branchId,
                                       MinimumAmountToManage = teller.MinimumAmountToManage,
                                       MaximumAmountToManage = teller.MaximumAmountToManage,
                                       MinimumDepositAmount = teller.MinimumDepositAmount,
                                       MaximumDepositAmount = teller.MaximumDepositAmount,
                                       MinimumWithdrawalAmount = teller.MinimumWithdrawalAmount,
                                       MaximumWithdrawalAmount = teller.MaximumWithdrawalAmount,
                                       MinimumTransferAmount = teller.MinimumTransferAmount,
                                       MaximumTransferAmount = teller.MaximumTransferAmount,
                                       Branch = branch,
                                       inUseStatus = teller.inUseStatus,
                                       activeStatus = teller.activeStatus,
                                       Transactions = teller.Transactions
                                   };

                        return data;
                    }
                }
                else
                {
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var tellers = couApiResponse.ApiResponseData.Data.Where(x => x.branchId == GetBranchID())
                                                                        .Select(teller =>
                                                                        {
                                                                            teller.Branch = branches.FirstOrDefault(b => b.Id == teller.branchId);
                                                                            return teller;
                                                                        });
                        return tellers;
                    }
                }

                return new List<Teller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetTellersStringValuesAsync()
        {
            try
            {
                var data = await GetTellers();
                var stringValues = data.Select(a => new StringValues
                {
                    Text = $"[{a.name}] [{a.Branch.Name}] [{(a.isPrimary ? "Primary" : "Sub")}-Teller]",
                    Value = $"{a.id}",
                }).ToList();

                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<Teller>> GetTellersPrimary()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<Teller>>>(APICallHelper.GetAllTeller);
                if (couApiResponse!=null)
                {
                    var results= couApiResponse.ApiResponseData.Data.Where(x => x.isPrimary == true);
                    return results;
                }
                return new List<Teller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Teller> GetTeller(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Teller, id));
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
        public async Task<ExecutionMessages> Create(Teller model)
        {
            try
            {
                model.bankId = GetBankID();
                //model.branchId = GetBranchID();
                // Make an API call to create an individual profile
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<Teller>>(APICallHelper.CreateTeller, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.name}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(Teller model)
        {
            try
            {

                var Teller = await GetTeller(model.id);
                if (Teller != null)
                {
                    Teller.code = model.code;
                    Teller.name = model.name;
                    Teller.MinimumAmountToManage = model.MinimumAmountToManage;
                    Teller.MaximumAmountToManage = model.MaximumAmountToManage;
                    Teller.MinimumWithdrawalAmount = model.MinimumWithdrawalAmount;
                    Teller.MaximumWithdrawalAmount = model.MaximumWithdrawalAmount;
                    Teller.MaximumTransferAmount = model.MaximumTransferAmount;
                    Teller.MinimumTransferAmount = model.MinimumTransferAmount;
                    Teller.MaximumDepositAmount = model.MaximumDepositAmount;
                    Teller.MinimumDepositAmount = model.MinimumDepositAmount;
                    Teller.isPrimary = model.isPrimary;
                    Teller.activeStatus = model.activeStatus;
                    Teller.inUseStatus = model.inUseStatus;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Teller, model.id), Teller);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{Teller.name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{Teller.name}", MessagesResults.Failed,
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
