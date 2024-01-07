using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService
{

    public class RolePermissionServices : BaseService
    {
        private readonly ApiCallerHelper _identityConfigApiHelper;

        public RolePermissionServices()
        {
            _identityConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(List<string> ids)
        {
            try
            {
                var model = new DeleteUserPermissionCommand(ids); // Get model from comman
                var inResponse = await _identityConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_RolePermission, model));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{ids.Count()}", MessagesResults.Success,
                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, $"{ids.Count()}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<Permission>> GetRolePermissions()
        {
            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<Permission>>>(APICallHelper.GetAllRolePermission);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Permission>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Permission> GetRolePermission(string id)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Permission>>(string.Format(APICallHelper.Get_Update_Delete_RolePermission, id));
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
        public async Task<Permission> GetRolePermissions(string roleID)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Permission>>(string.Format(APICallHelper.GetRolePermissions, roleID));
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
        public async Task<ExecutionMessages> Create(RolePermissionRequestCommand command)
        {
            try
            {

                var response = await _identityConfigApiHelper.PostAsync<ServiceResponse<RolePermission>>(APICallHelper.CreateRolePermission, command);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Prrmission", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(command, false, $"Prrmission", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(RolePermissionRequestCommand model)
        {
            try
            {


                if (model != null)
                {
                    var response = await _identityConfigApiHelper.PutAsync<ServiceResponse<RolePermission>>(string.Format(APICallHelper.Get_Update_Delete_RolePermission, model.roleID), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Permissions", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"Permissions", MessagesResults.Failed,
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
