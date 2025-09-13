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
                var model = new DeleteUserPermissionCommand(ids); // GetAllowAnonymous model from comman
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
                    if (IsHeadOffice())
                    {
                        var menuMasters = from a in couApiResponse.ApiResponseData.Data
                                          select new StringValues
                                          {
                                              Value = a.Id.ToString(),
                                              Text = $"{a.Name}-{a.Description}"
                                          };

                        return menuMasters;
                    }
                    else
                    {
                        var menuMasters = from a in couApiResponse.ApiResponseData.Data
                                          where !a.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
                                          select new StringValues
                                          {
                                              Value = a.Id.ToString(),
                                              Text = $"{a.Name}-{a.Description}"
                                          };

                        return menuMasters;
                    }
                }

                return new List<StringValues>();
            }
            catch (Exception ex)
            {
                // TODO: Log exception properly
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
        public async Task<Permission> GetAllRolePermission()
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<Permission>>(APICallHelper.GetAllRolePermission);
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
                if (IsHeadOffice())
                {
                    // ✅ Head Office: Return all permissions from API
                    var response = await _identityConfigApiHelper.GetAsync<ResponseObject<List<PermissionMenuLoader>>>(APICallHelper.GetAssignPemissions);
                    if (response != null && response.ApiResponseData?.Data != null)
                    {
                        MenuLoaderHelper helper = new MenuLoaderHelper();
                        return helper.AddParentNames(response.ApiResponseData.Data);
                    }
                }
                else
                {
                    // 🔒 Non-Head Office: Filter based on role
                    var allDatabaseMenus = await _identityConfigApiHelper.GetAsync<ResponseObject<List<Permission>>>(APICallHelper.GetAllRolePermission);

                    if (allDatabaseMenus != null && allDatabaseMenus.ApiResponseData?.Data != null)
                    {
                        var currentRoleId = GetRoleId();

                        var roleBasedMenus = allDatabaseMenus.ApiResponseData.Data
                            .Where(d => d.RoleID.ToString() == currentRoleId)
                            .Select(menu => new PermissionMenuLoader
                            {
                                MenuMasterId = menu.MenuMasterId,
                                ParentName = menu.ParentName,
                                Create = menu.Create,
                                Read = menu.Read,
                                Delete = menu.Delete,
                                Update = menu.Update,
                                Download = menu.Download,
                                Upload = menu.Upload,
                                MenuText = menu.MenuText,
                                ParentId = menu.ParentId,
                                ControllerName = menu.ControllerName,
                                ActionName = menu.ActionName,
                                MenuGroup = menu.MenuGroup,
                                Description = menu.Description
                            }).ToList();

                        MenuLoaderHelper helper = new MenuLoaderHelper();
                        return helper.AddParentNames(roleBasedMenus);
                    }
                }

                return Enumerable.Empty<PermissionMenuLoader>();
            }
            catch (Exception ex)
            {
                // Optional: Log error here
                throw;
            }
        }

        public async Task<ExecutionMessages> Create(PermissionMenuLoaderDto  command)
        {
            try
            {
                var rolePermissionRequestCommand = new RolePermissionRequestCommand();
                rolePermissionRequestCommand.roleID = command.roleIDs;
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
