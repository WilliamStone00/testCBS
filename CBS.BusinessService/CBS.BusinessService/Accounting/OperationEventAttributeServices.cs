using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper.Helper;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounting
{
   

    public class OperationEventAttributeServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public OperationEventAttributeServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objOperationEvent = await GetOperationEventAttribute(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_OperationEvent, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOperationEvent.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOperationEvent, false, $"{objOperationEvent.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>> GetOperationEventAttributes()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>>>(APICallHelper.GetAllOperationEventAttribute);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData==null)
                    {
                        return new List<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                
                }
                return new List<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FrontDesk.Data.Entity.Accounting.OperationEventAttribute> GetOperationEventAttribute(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>>(string.Format(APICallHelper.Get_Update_Delete_OperationEventAttribute, id));
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
        public async Task<ExecutionMessages> Create(FrontDesk.Data.Entity.Accounting.OperationEventAttribute model)
        {
            try
            {

                // Make an API call to create an individual profile

               
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>>(APICallHelper.CreateOperationEventAttribute, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, (string)model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FrontDesk.Data.Entity.Accounting.OperationEventAttribute model)
        {
            try
            {

                var OperationEvent = await GetOperationEventAttribute(model.Id);
                if (OperationEvent != null)
                {
                    OperationEvent.Name = model.Name;
             
                    OperationEvent.OperationEventId = model.OperationEventId;
                    OperationEvent.Id = model.Id;
               
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.OperationEventAttribute>>(string.Format(APICallHelper.Get_Update_Delete_OperationEventAttribute, model.Id), OperationEvent);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.Name, MessagesResults.Failed,
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

        public IEnumerable ConvertToSelectedList(List<OperationEventAttribute> operationEventAttributes)
        {
            var data = from operationEventAttribute in operationEventAttributes
                       select new SelectListItem
                       {
                           Text = operationEventAttribute.Name,
                           Value = operationEventAttribute.Id
                       };
            return data;
        }
    }
}
