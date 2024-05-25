using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CustomerManagement
{
    public class GroupTypeServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public GroupTypeServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objGroupType = await GetGroupType(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_GroupType, id), id));
                if (inResponse.ApiResponseData!=null)
                {

                    GetExecutionMessages(inResponse, true, $"{objGroupType.GroupTypeName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objGroupType, false, $"{objGroupType.GroupTypeName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<GroupType>> GetGroupTypes()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<GroupType>>>(APICallHelper.GetAllGroupTypes);
                if (couApiResponse.ApiResponseData!=null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<GroupType>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<GroupType> GetGroupType(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<GroupType>>(string.Format(APICallHelper.Get_Update_Delete_GroupType, id));
                if (cusResponseObject.ApiResponseData!=null)
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
        public async Task<ExecutionMessages> Create(GroupType model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<GroupType>>(APICallHelper.CreateGroupType, model);
                if (response.ApiResponseData!=null)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.GroupTypeName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.GroupTypeName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(GroupType model)
        {
            try
            {

                var GroupType = await GetGroupType(model.GroupTypeId);
                if (GroupType != null)
                {
                    GroupType.GroupTypeName = model.GroupTypeName;
                    GroupType.Description = model.Description;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<GroupType>>(string.Format(APICallHelper.Get_Update_Delete_GroupType, model.GroupTypeId), GroupType);
                    if (response.ApiResponseData!=null)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.GroupTypeName}", MessagesResults.Success,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.GroupTypeName, MessagesResults.Failed,
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
