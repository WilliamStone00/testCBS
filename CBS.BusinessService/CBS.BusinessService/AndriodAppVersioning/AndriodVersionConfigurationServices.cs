using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Entity.ChangeNumber;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AndriodAppVersioning
{
    public class AndriodVersionConfigurationServices : BaseService
    {
        private readonly ApiCallerHelper _customerConfigApiHelper;

        public AndriodVersionConfigurationServices()
        {
            _customerConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _customerConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteAndriodVersion, id));
                if (inResponse.ApiResponseData != null)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
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
        public async Task<IEnumerable<AndriodVersionConfiguration>> GetAllAndroidVersions()
        {
            try
            {
                var couApiResponse = await _customerConfigApiHelper.GetAsync<ResponseObject<List<AndriodVersionConfiguration>>>(APICallHelper.GetAllAndriodVersions);
                if (couApiResponse.ApiResponseData != null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<AndriodVersionConfiguration>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
      
       

        public async Task<AndriodVersionConfiguration> GetAndroidVersionById(string id)
        {
            try
            {
                var cusResponseObject = await _customerConfigApiHelper.GetAsync<ResponseObject<AndriodVersionConfiguration>>(string.Format(APICallHelper.GetAndriodVersion, id));
                if (cusResponseObject.ApiResponseData != null)
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
        public async Task<ExecutionMessages> Create(AndriodVersionConfiguration model)
        {
            try
            {
                var response = await _customerConfigApiHelper.PostAsync<ServiceResponse<AndriodVersionConfiguration>>(APICallHelper.AddOrUpdateAndriodVersion, model);
                if (response.ApiResponseData != null)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false,null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(AndriodVersionConfiguration model)
        {
            try
            {
                var response = await _customerConfigApiHelper.PostAsync<ServiceResponse<AndriodVersionConfiguration>>(APICallHelper.ApprovePhoneNumberRequest, model);
                if (response.ApiResponseData != null)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(null, false, "", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
