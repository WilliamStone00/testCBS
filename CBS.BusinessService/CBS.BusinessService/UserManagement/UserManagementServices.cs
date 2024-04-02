using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity.DataTable;
using System.Web;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.User;
using CBS.FrontDesk.Data.Entity.SavingProducts;

namespace CBS.BusinessService.UserManagement
{
    public class UserManagementServices : BaseService, IUserManagementServices
    {
        public async Task<ExecutionMessages> CreateUser(User user)
        {
            try
            {
                //user.userRoles=new List<UserRole>{new UserRole{roleId = user.roleID} };
                if (user.allowedIP != null)
                {
                    user.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = user.allowedIP } };
                }
                user.userRoles.Add(new UserRole { roleId = user.roleID });
                user.userAllowedIPs = new List<UserAllowedIP>();
                var ApiCallerHelper =new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<UserList>>(APICallHelper.createUserUrl, user);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.firstName+" "+ user.lastName, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, user.firstName, MessagesResults.Failed,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                    reUser.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Role>> GetRoles()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var roles = await ApiCallerHelper.GetAsync<ResponseObject<List<Role>>>(APICallHelper.GetAllRoles);
                return roles.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<UserRoleDto>> GetUSerRoles()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var roles = await ApiCallerHelper.GetAsync<ResponseObject<List<UserRoleDto>>>(APICallHelper.GetAllUserRoles);
                return roles.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<UserList>> GetUserList()
        {
            try
            {
                var identityServerBaseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var apiCallerHelper = new ApiCallerHelper(identityServerBaseUrl);

                var userListsResponse = await apiCallerHelper.GetAsync<ResponseObject<List<UserList>>>(APICallHelper.GetUsers);
                var newList = new List<UserList>();

                if (userListsResponse != null && userListsResponse.IsSuccess)
                {
                    var userLists = userListsResponse.ApiResponseData.Data;
                    var isHeadOffice = HttpContext.Current.Session["IsHeadOffice"] == "True";
                    var branchId = GetBranchID();
                    var branches = await GetBranches();

                    foreach (var user in userLists.Where(u => isHeadOffice || u.BranchID == branchId))
                    {
                        user.name = $"{user.firstName} {user.lastName}";
                        user.strlastLoginDate = user.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        user.status = user.isActive ? "Active" : "In-active";

                        if (user.BranchID != null)
                        {
                            var branch = branches.FirstOrDefault(b => b.Id == user.BranchID);
                            if (branch != null)
                            {
                                user.Branch = branch;
                                user.Bank = branch.Bank ?? new Bank();
                            }
                        }
                        else
                        {
                            user.Branch = new Branch();
                            user.Bank = new Bank();
                        }

                        newList.Add(user);
                    }

                    return newList;
                }
                else
                {
                    return Enumerable.Empty<UserList>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetUserDropDownList()
        {
            try
            {
                var identityServerBaseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var apiCallerHelper = new ApiCallerHelper(identityServerBaseUrl);

                var userListsResponse = await apiCallerHelper.GetAsync<ResponseObject<List<UserList>>>(APICallHelper.GetUsers);
                var newList = new List<UserList>();
                var stringValues = new List<StringValues>();
                if (userListsResponse != null && userListsResponse.IsSuccess)
                {
                    var userLists = userListsResponse.ApiResponseData.Data;
                    var isHeadOffice = HttpContext.Current.Session["IsHeadOffice"] == "True";
                    var branchId = GetBranchID();
                    var branches = await GetBranches();

                    foreach (var user in userLists.Where(u => isHeadOffice || u.BranchID == branchId))
                    {
                        user.name = $"{user.firstName} {user.lastName}";
                        user.strlastLoginDate = user.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        user.status = user.isActive ? "Active" : "In-active";

                        if (user.BranchID != null)
                        {
                            var branch = branches.FirstOrDefault(b => b.Id == user.BranchID);
                            if (branch != null)
                            {
                                user.Branch = branch;
                                user.Bank = branch.Bank ?? new Bank();
                            }
                        }
                        else
                        {
                            user.Branch = new Branch();
                            user.Bank = new Bank();
                        }

                        newList.Add(user);

                    }

                    stringValues = (from a in newList
                                    select new StringValues
                                    {
                                        Text = $"{a.firstName}{a.lastName}, Branch: {a.Branch.Name}",
                                        Value = a.id.ToString(),
                                    }).ToList();

                   
                    return stringValues.ToList();
                }
                else
                {
                    return Enumerable.Empty<StringValues>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Branch>> GetBranches()
        {////780400915211061
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
                if (HttpContext.Current.Session["IsHeadOffice"] == "True")
                {
                    var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                    var bracBranches = branchApiResponse.ApiResponseData.Data;
                    var branches = bracBranches;
                    return branches;
                }
                else
                {
                    var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<Branch>>((string.Format(APICallHelper.Get_Update_Delete_Branch, GetBranchID())));
                    var bracBranches = branchApiResponse.ApiResponseData.Data;
                    var branches = new List<Branch>();
                    branches.Add(bracBranches);
                    return branches;
                }
         
           ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<UserList> GetUser(Guid userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<UserList>>(string.Format(APICallHelper.GetUserByID, userid));
                if (user.IsSuccess)
                {
                    user.ApiResponseData.Data.name = $"{user.ApiResponseData.Data.firstName} {user.ApiResponseData.Data.lastName}";
                    user.ApiResponseData.Data.strlastLoginDate = user.ApiResponseData.Data.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.ApiResponseData.Data.status = user.ApiResponseData.Data.isActive ? "Active" : "In-active";
                    user.ApiResponseData.Data.ChangePassword.userName = user.ApiResponseData.Data.userName;
                    user.ApiResponseData.Data.roleID= user.ApiResponseData.Data.userRoles.Select(role => role.roleId).First();
                }
                return user.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ExecutionMessages> DeleteUser(Guid userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var staus = await ApiCallerHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.DeleteUser, userid));
                if (staus.IsSuccess)
                {
                    GetExecutionMessages(staus, true, "", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                else
                {
                    GetExecutionMessages(staus, false, "", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        staus.Message);
                    return ExecutionMessage;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ExecutionMessages> UpdateUserProfile(UserList user)
        {
            try
            {
                if (user.allowedIP != null)
                {
                    user.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = user.allowedIP } };

                }
                else
                {
                    user.userAllowedIPs = new List<UserAllowedIP>();

                }
                user.id = Guid.Parse(GetUserToDoAction());
                user.userRoles.Add(new UserRole { roleId = user.roleID, userId = user.id });

                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PutAsync<ResponseObject<UserList>>(string.Format(APICallHelper.UpdateUser, user.id), user);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, reUser.ApiResponseData.Data.firstName, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, user.firstName, MessagesResults.Failed,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                    reUser.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> ChangePassword(UserList user)
        {
            try
            {
                user.ChangePasswordOnFirstLogin = true;
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<UserList>>(APICallHelper.ChangePassword, user.ChangePassword);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.ChangePassword.userName, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, user.ChangePassword.userName, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Failed.ToString(), null,
                    reUser.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> ResetPassword(UserList user)
        {
            try
            {

                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<UserList>>(APICallHelper.ResetPassword, user.ChangePassword);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.ChangePassword.userName, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, user.ChangePassword.userName, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                    reUser.Message);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UploadPicture(HttpPostedFileBase user)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    { "UserID", GetUserID() },
                };
                List<HttpPostedFileBase> image =new List<HttpPostedFileBase>();
                image.Add(user);
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostFilesAndParamsAsync<ResponseObject<UserList>>(APICallHelper.UploadProfilePhoto, additionalParams, image);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, null, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                    null);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<CustomDataTable> GetUsersDataTable(DataTableOptions dataTableOptions)
        
        {
            Func<Task<List<UserList>>> getUsersFunc = async () => (await GetUserList()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<UserList>(dataTableOptions, getUsersFunc);
            return dataTable;
        }



    }



}
