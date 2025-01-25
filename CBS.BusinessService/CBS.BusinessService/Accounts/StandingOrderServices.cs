
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class StandingOrderServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public StandingOrderServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_StandingOrder, id));
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
        public async Task<IEnumerable<StandingOrder>> GetStandingOrderByMemberId(string memberId)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<StandingOrder>>>(string.Format(APICallHelper.Get_StandingOrderByMemberId, memberId));
                if (cusResponseObject.IsSuccess)
                {
                    var data = cusResponseObject.ApiResponseData.Data;
                    return data;
                }
                return new List<StandingOrder>();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<StandingOrder>> GetAllStandingOrder()
        {
            try
            {
                string branchid = null;
                if (!IsHeadOffice())
                {
                    branchid=GetBranchID();
                }
                GetAllStandingOrdersQuery allStandingOrdersQuery = new GetAllStandingOrdersQuery { BranchId=branchid };
                var queryString = ToQueryString(allStandingOrdersQuery);
                var fullUrl = $"{APICallHelper.GetAllStandingOrder}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<StandingOrder>>>(fullUrl);
                var data = new List<StandingOrder>();
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


        public async Task<StandingOrder> GetStandingOrder(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<StandingOrder>>(string.Format(APICallHelper.Get_StandingOrder, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public AddOrUpdateStandingOrderCommand MapStandingOrderToCommand(StandingOrder standingOrder)
        {
            if (standingOrder == null)
                throw new ArgumentNullException(nameof(standingOrder));

            return new AddOrUpdateStandingOrderCommand
            {
                Id = standingOrder.Id,
                MemberId = standingOrder.MemberId,
                MemberName = standingOrder.MemberName,
                Amount = standingOrder.Amount,
                SourceAccountType = standingOrder.SourceAccountType,
                DestinationAccountType = standingOrder.DestinationAccountType,
                Purpose = standingOrder.Purpose,
                StartDate = standingOrder.StartDate,
                EndDate = standingOrder.EndDate,
                IsActive = standingOrder.IsActive,
                IsAutomatic = standingOrder.IsAutomatic,
                Frequency = standingOrder.Frequency,
                Priority = standingOrder.Priority
            };
        }

        public async Task<ExecutionMessages> Create(AddOrUpdateStandingOrderCommand model)
        {
            try
            {
               
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<StandingOrder>>(APICallHelper.CreateStandingOrder, model);
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
        
        public async Task<ExecutionMessages> Update(AddOrUpdateStandingOrderCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<StandingOrder>>(APICallHelper.Update_StandingOrder, model);
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
