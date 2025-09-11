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
using System.Web;

namespace CBS.BusinessService.UserManagement
{
    public class UserPermissionServices : BaseService
    {
        private readonly ApiCallerHelper _identityConfigApiHelper;

        public UserPermissionServices()
        {
            _identityConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(List<string> ids)
        {
            try
            {
                var model = new DeleteUserPermissionCommand(ids);
                var response = await _identityConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.DeleteUserPermission, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, $"{ids.Count}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, $"{ids.Count}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"{ids.Count}", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }


        public async Task<IEnumerable<Permission>> GetUserPermissions()
        {
            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<Permission>>>(APICallHelper.GetAllUserPermission);
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
        public async Task<Permission> GetUserPermission(string id)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Permission>>(string.Format(APICallHelper.Get_Update_Delete_UserPermission, id));
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
        public async Task<Permission> GetUserPermissions(string userID)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Permission>>(string.Format(APICallHelper.GetUserPermissions, userID));
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

        public async Task<ExecutionMessages> AddUserRole(Guid userId, List<int> selectedMenus)
        {
            try
            {
                var command = new UserPermissionRequestCommand
                {
                    userID = userId,
                    userPermissionRequests = selectedMenus.Select(menuId => new PermissionRequest
                    {
                        MenuMasterId = menuId,
                        Create = false, MenuText="Menu Text", Id=Guid.NewGuid().ToString(),
                        Read = true, 
                        Delete = false,
                        Update = false,
                        Download = false,
                        Upload = false
                    }).ToList()
                };

                var response = await _identityConfigApiHelper.PostAsync<ServiceResponse<List<UserPermission>>>(APICallHelper.CreateUserPermission, command);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Prrmission", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(UserPermissionRequestCommand model)
        {
            try
            {

                
                if (model != null)
                {
                    var response = await _identityConfigApiHelper.PutAsync<ServiceResponse<UserPermission>>(string.Format(APICallHelper.Get_Update_Delete_UserPermission, model.userID), model);
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
