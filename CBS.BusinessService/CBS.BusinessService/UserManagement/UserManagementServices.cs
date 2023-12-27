using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.UserManagement.Roles;
using System.Web.Security;
using CBS.FrontDesk.Data.Entity.DataTable;
using System.Xml.Linq;
using System.Web;
using CBS.FrontDesk.Data.Entity.Config;

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

                foreach (var id in user.roleID)
                {
                    user.userRoles.Add(new UserRole{roleId = id});
                }
                user.userAllowedIPs = new List<UserAllowedIP>();
                var ApiCallerHelper =new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<UserList>(APICallHelper.createUserUrl, user);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.firstName+" "+ user.lastName, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(user, false, user.firstName, MessagesResults.Failed,
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
        public async Task<IEnumerable<Role>> GetRoles()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var roles = await ApiCallerHelper.GetAsync<IEnumerable<Role>>(APICallHelper.Role);
                return roles.ApiResponseData;
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
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var userLists = await ApiCallerHelper.GetAsync<IEnumerable<UserList>>(APICallHelper.GetUsers);
                var newList = new List<UserList>();
                
                if (userLists!=null)
                {
                    var braches = await GetBranches();


                    foreach (var a in userLists.ApiResponseData)
                    {
                        a.name = $"{a.firstName} {a.lastName}";
                        a.strlastLoginDate = a.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        a.status = a.isActive ? "Active" : "In-active";
                        if (a.BranchID!=null)
                        {
                            a.Branch = braches.Where(x => x.Id == a.BranchID).FirstOrDefault();
                            a.Bank= braches.Where(x => x.Id == a.BranchID).FirstOrDefault().Bank;
                            if (a.Bank==null)
                            {
                                a.BankID = null;
                                a.Bank = new Bank();
                            }
                    
                        }
                        else
                        {
                            a.Bank = new Bank();
                            a.Branch = new Branch();
                        }
                        newList.Add(a);
                    }
                    return newList;
                }

               
                
                return userLists.ApiResponseData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<Branch>> GetBranches()
        {////780400915211061
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
                var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<Bank>>((string.Format(APICallHelper.Get_Update_Delete_Bank,GetBankID())));
                var bracBranches = branchApiResponse.ApiResponseData.Data;
                var branches = bracBranches.Branches;
                return branches;
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
                var user = await ApiCallerHelper.GetAsync<UserList>(string.Format(APICallHelper.GetUserByID, userid));
                if (user.IsSuccess)
                {
                    user.ApiResponseData.name = $"{user.ApiResponseData.firstName} {user.ApiResponseData.lastName}";
                    user.ApiResponseData.strlastLoginDate = user.ApiResponseData.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.ApiResponseData.status = user.ApiResponseData.isActive ? "Active" : "In-active";
                    user.ApiResponseData.ChangePassword.userName = user.ApiResponseData.userName;
                    user.ApiResponseData.roleID= user.ApiResponseData.userRoles.Select(role => role.roleId).ToList();
                }
                return user.ApiResponseData;
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
                var staus = await ApiCallerHelper.DeleteAsync<bool>(string.Format(APICallHelper.DeleteUser, userid));
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
                foreach (var id in user.roleID)
                {
                    user.userRoles.Add(new UserRole{roleId = id, userId = user.id }); 
                }
       
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PutAsync<UserList>(string.Format(APICallHelper.UpdateUser, user.id), user);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, reUser.ApiResponseData.firstName, MessagesResults.Success,
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
                
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<UserList>(APICallHelper.ChangePassword, user.ChangePassword);
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
                var reUser = await ApiCallerHelper.PostAsync<UserList>(APICallHelper.ResetPassword, user.ChangePassword);
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
                var reUser = await ApiCallerHelper.PostFilesAndParamsAsync<UserList>(APICallHelper.UploadProfilePhoto, additionalParams, image);
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
