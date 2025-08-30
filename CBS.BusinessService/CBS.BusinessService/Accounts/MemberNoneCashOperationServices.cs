
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.BusinessService.Accounts
{

    public class MemberNoneCashOperationServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public MemberNoneCashOperationServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_MemberNoneCashOperation, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<FileUploadDto>> GetFileUploads()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(string.Format(APICallHelper.GetAllSalaryUploadByFileCategory, "SalaryModelExtraction"));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileUploadDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ExecutionMessages> Validate(MemberNoneCashOperation model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Validate_MemberNoneCashOperation, model), model.Id);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<IEnumerable<MemberNoneCashOperation>> GetMemberNoneCashOperations(string branchid = null, string status = null)
        {
            try
            {
                if (IsHeadOffice())
                {
                    branchid="all";
                    status="all";
                }
                else
                {
                    branchid=GetBranchID();
                }
                GetAllMemberNoneCashOperationsQuery getAllMemberNone = new GetAllMemberNoneCashOperationsQuery(status, branchid);

                var queryString = ToQueryString(getAllMemberNone);
                var fullUrl = $"{APICallHelper.GetAll_MemberNoneCashOperations}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<MemberNoneCashOperation>>>(fullUrl);
                var data = new List<MemberNoneCashOperation>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<MemberNoneCashOperation> GetMemberNoneCashOperation(string id)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<MemberNoneCashOperation>>(string.Format(APICallHelper.Get_MemberNoneCashOperation, id));

                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new MemberNoneCashOperation();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ExecutionMessages> Create(List<BulkDeposit> deposits)
        {
            try

            {
                var data = deposits.FirstOrDefault();
                var model = new AddMemberNoneCashOperationCommand { AccountNUmber=data.AccountNumber, Amount=data.Amount, BookingDirection=data.BookingDirection, ChartOfAccountId=data.ChartOfAccountId, MemberReference=data.CustomerId, Note=data.Note, MemberName=data.MemberName, ChartOfAccountName=data.ChartOfAccountName, BranchId=data.BranchId, MobileMoneyPath=data.MobileMoneyPath, IsMobileMoneyOperation=data.IsMobileMoneyOperation, AccountingDate=data.AccountingDate, NoneMemberMobileReference=data.NoneMemberMobileReference };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<MemberNoneCashOperation>>(APICallHelper.Create_MemberNoneCashOperation, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> ValidateMemberNoneCashOperation(ValidateMemberNoneCashOperationCommand model)
        {
            try

            {


                var response = await _transactionApiHelper.PutAsync<ServiceResponse<MemberNoneCashOperation>>(string.Format(APICallHelper.Validate_MemberNoneCashOperation, model.OperationId), model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
