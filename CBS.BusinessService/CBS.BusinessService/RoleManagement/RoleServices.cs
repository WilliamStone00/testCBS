using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService
{
    public class RoleServices : BaseService
    {
        private readonly ApiCallerHelper _identityConfigApiHelper;

        public RoleServices()
        {
            _identityConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objRole = await GetRole(id);
                var inResponse = await _identityConfigApiHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Get_Update_Delete_Role, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objRole.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objRole, false, $"{objRole.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> DeleteRolePermission(string id)
        {
            try
            {
                var deleteRole=new DeleteRolePermission { Ids = new List<string> { id } };
                var inResponse = await _identityConfigApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.DeleteRolePermisions, deleteRole);
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Permision", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, $"", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        ///api/RolePermission
        public async Task<IEnumerable<Role>> GetRoles()
        {
            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<Role>>>(APICallHelper.GetAllRoles);
                if (couApiResponse.ApiResponseData!=null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Role>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<RolePermission>> GetRolePermissions(string roleID)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<List<RolePermission>>>(string.Format(APICallHelper.GetRolePermissions, roleID));
                if (cusResponseObject.ApiResponseData!=null)
                {
                    var rolePermissions = cusResponseObject.ApiResponseData.Data;
                    return rolePermissions;
                }
                return new List<RolePermission>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<Role> GetRole(string id)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Role>>(string.Format(APICallHelper.Get_Update_Delete_Role, id));
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
        public async Task<ExecutionMessages> Create(Role model)
        {
            try
            {

                var response = await _identityConfigApiHelper.PostAsync<ResponseObject<Role>>(APICallHelper.CreateRole, model);
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
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(Role model)
        {
            try
            {

                var Role = await GetRole(model.Id.ToString());
                if (Role != null)
                {
                    Role.Name = model.Name;
                    Role.Description = model.Description;
                    Role.IsTeller = model.IsTeller;
                    var response = await _identityConfigApiHelper.PutAsync<ResponseObject<Role>>(string.Format(APICallHelper.Get_Update_Delete_Role, model.Id), Role);
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
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
