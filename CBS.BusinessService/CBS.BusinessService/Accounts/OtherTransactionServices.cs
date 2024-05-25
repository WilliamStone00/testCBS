using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
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
    public class OtherTransactionServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;
        private readonly BranchServices _branchServices;

        public OtherTransactionServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objOtherTransaction = await GetOtherTransaction(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_OtherTransaction, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOtherTransaction.EnventName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOtherTransaction, false, $"{objOtherTransaction.EnventName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<OtherTransaction>> GetOtherTransactions()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<OtherTransaction>>>(APICallHelper.GetAllOtherTransaction);
                var branches = await _branchServices.GetBranches();

                if (IsHeadOffice())
                {
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var data = from a in couApiResponse.ApiResponseData.Data
                                   join b in branches on a.BranchId equals b.Id
                                   select new OtherTransaction
                                   {
                                       Id = a.Id,
                                       TransactionReference = a.TransactionReference,
                                       EnventName = a.EnventName,
                                       EventCode = a.EventCode,
                                       Description = a.Description,
                                       TellerId = a.TellerId,
                                       Amount = a.Amount,
                                       Debit = a.Debit,
                                       Credit = a.Credit,
                                       Direction = a.Direction,
                                       TransactionType = a.TransactionType,
                                       SourceType = a.SourceType,
                                       Naration = a.Naration,
                                       CustomerId = a.CustomerId,
                                       AccountNumber = a.AccountNumber,
                                       BranchId = a.BranchId,
                                       Branch = b,
                                       BankId = a.BankId,
                                       Teller = a.Teller
                                   };

                        return data.ToList();
                    }
                }
                else
                {
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var otherTransactions = couApiResponse.ApiResponseData.Data
                            .Where(x => x.BranchId == GetBranchID())
                            .Select(a =>
                            {
                                a.Branch = branches.FirstOrDefault(b => b.Id == a.BranchId);
                                return a;
                            });

                        return otherTransactions.ToList();
                    }
                }

                return new List<OtherTransaction>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


       
        public async Task<OtherTransaction> GetOtherTransaction(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<OtherTransaction>>(string.Format(APICallHelper.Get_Update_Delete_OtherTransaction, id));
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
        public async Task<ExecutionMessages> Create(OtherTransaction model)
        {
            try
            {
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<OtherTransaction>>(APICallHelper.CreateOtherTransaction, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.EnventName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.EnventName}", MessagesResults.Failed,
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

    }

}
