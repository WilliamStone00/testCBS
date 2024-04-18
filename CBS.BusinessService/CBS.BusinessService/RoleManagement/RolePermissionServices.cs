using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
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

        public async Task<IEnumerable<StringValues>> GetRolePermissions()
        {
            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<Role>>>(APICallHelper.GetAllRoles);
                if (couApiResponse.ApiResponseData != null)
                {
                    var menuMasters = from a in couApiResponse.ApiResponseData.Data
                                      select new StringValues
                                      {
                                          Value = a.Id.ToString(),
                                          Text = $"{a.Name}-{a.IsTeller}"
                                      };

                    return menuMasters;
                }
                return new List<StringValues>();
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
        public async Task<IEnumerable<PermissionMenuLoader>> GetAssignPermissions()
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<List<PermissionMenuLoader>>>(APICallHelper.GetAssignPemissions);
                if (cusResponseObject!=null)
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
        public async Task<ExecutionMessages> Create(PermissionMenuLoaderDto  command)
        {
            try
            {
                var rolePermissionRequestCommand = new RolePermissionRequestCommand();
                rolePermissionRequestCommand.roleID = ConvertStringToGuid(command.roleID);
                foreach (var a in command.MenuMasterId)
                {
                    
                    rolePermissionRequestCommand.rolePermissionRequests.Add(new PermissionRequest { Delete=true, MenuMasterId=a,
                      Create= true, Download= true, MenuText="", Read= true, Update= true, Upload= true, Id=Guid.NewGuid().ToString()});
                  
                }
            
                var response = await _identityConfigApiHelper.PostAsync<ServiceResponse<List<RolePermission>>>(APICallHelper.CreateRolePermission, rolePermissionRequestCommand);
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
        public async Task<ExecutionMessages> Update(PermissionMenuLoaderDto model)
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
