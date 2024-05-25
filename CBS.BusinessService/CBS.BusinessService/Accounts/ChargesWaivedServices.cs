
using BusinessServices;
using CBS.API.Helper;
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
    //WithdrawalNotification
    public class ChargesWaivedServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public ChargesWaivedServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objFee = await GetChargesWaived(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_ChargesWaived, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"ChargesWaived", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objFee, false, $"ChargesWaived", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<ChargesWaived>> GetChargesWaiveds()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<ChargesWaived>>>(APICallHelper.GetAllChargesWaived);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ChargesWaived>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<ChargesWaived>> GetChargesWaiveds(string customerId)
        {
            try
            {
               var chargesWaiveds=(from a in await GetChargesWaiveds() where a.CustomerId==customerId select a).ToList();
                return chargesWaiveds;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ChargesWaived> GetChargesWaived(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<ChargesWaived>>(string.Format(APICallHelper.Get_Update_Delete_ChargesWaived, id));
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
        public async Task<ExecutionMessages> Create(ChargesWaived model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ChargesWaived>>(APICallHelper.CreateChargesWaived, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"ChargesWaived", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "ChargesWaived", MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Update(ChargesWaived model)
        {
            try
            {
                var chargesWaived = await GetChargesWaived(model.Id);
                if (chargesWaived != null)
                {
                    chargesWaived.CustomCharge = model.CustomCharge;
                    chargesWaived.DateOfWaiverRequest = model.DateOfWaiverRequest;
                    chargesWaived.Comment = model.Comment;
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<ChargesWaived>>(string.Format(APICallHelper.Get_Update_Delete_ChargesWaived, model.Id), chargesWaived);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"ChargesWaived", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "ChargesWaived", MessagesResults.Failed,
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
