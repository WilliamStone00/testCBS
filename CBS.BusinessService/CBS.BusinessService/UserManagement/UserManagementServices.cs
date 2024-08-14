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
using CBS.FrontDesk.Data.Entity.Accounting;

namespace CBS.BusinessService.UserManagement
{
    public class UserManagementServices : BaseService, IUserManagementServices
    {
        public async Task<ExecutionMessages> CreateUser(User user)
        {
            try
            {
                // Initialize lists
                user.userRoles = new List<UserRole>();
                if (user.allowedIP != null)
                {
                    user.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = user.allowedIP } };
                }
                else
                {
                    user.userAllowedIPs = new List<UserAllowedIP>();
                }

                // Add user role
                user.userRoles.Add(new UserRole { roleId = user.roleID });
                user.BankID = GetBankID();
                // Call API to create user
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<User>>(APICallHelper.createUserUrl, user);

                // Handle API response
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.firstName + " " + user.lastName, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
                else
                {
                    GetExecutionMessages(user, false, user.firstName, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        //public async Task<ExecutionMessages> CreateUser(User user)
        //{
        //    try
        //    {
        //        //user.userRoles=new List<UserRole>{new UserRole{roleId = user.roleID} };
        //        if (user.allowedIP != null)
        //        {
        //            user.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = user.allowedIP } };
        //        }
        //        user.userRoles.Add(new UserRole { roleId = user.roleID });
        //        user.userAllowedIPs = new List<UserAllowedIP>();
        //        var ApiCallerHelper =new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        //        var reUser = await ApiCallerHelper.PostAsync<ResponseObject<User>>(APICallHelper.createUserUrl, user);
        //        if (reUser.IsSuccess)
        //        {
        //            GetExecutionMessages(reUser, true, user.firstName+" "+ user.lastName, MessagesResults.Success,
        //                ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
        //                null);
        //            return ExecutionMessage;
        //        }
        //        GetExecutionMessages(user, false, user.firstName, MessagesResults.Failed,
        //            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
        //            reUser.Message);

        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}
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
                if (IsHeadOffice())
                {

                    return roles.ApiResponseData.Data;
                }
                else
                {
                    var rolesx = roles.ApiResponseData.Data.Where(x => x.branchId == GetBranchID());
                    return rolesx;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            try
            {
                var identityServerBaseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var apiCallerHelper = new ApiCallerHelper(identityServerBaseUrl);

                var UsersResponse = await apiCallerHelper.GetAsync<ResponseObject<List<User>>>(APICallHelper.GetUsers);
                var newList = new List<User>();

                if (UsersResponse != null && UsersResponse.IsSuccess)
                {
                    var Users = UsersResponse.ApiResponseData.Data;
                    var isHeadOffice = IsHeadOffice();
                    var branchId = GetBranchID();
                    var branches = await GetBranches();

                    foreach (var user in Users.Where(u => isHeadOffice || u.BranchID == branchId))
                    {
                        user.name = $"{user.firstName} {user.lastName}";
                        user.strlastLoginDate = user.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        user.status = user.isActive ? "Active" : "In-active";

                        if (user.BranchID != null)
                        {
                            var branch = branches.FirstOrDefault(b => b.Id == user.BranchID);
                            if (branch != null)
                            {
                                user.Brancch = branch;
                                user.Bank = branch.Bank ?? new Bank();
                            }
                        }
                        else
                        {
                            user.Brancch = new Branch();
                            user.Bank = new Bank();
                        }

                        newList.Add(user);
                    }

                    return newList;
                }
                else
                {
                    return Enumerable.Empty<User>();
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

                var UsersResponse = await apiCallerHelper.GetAsync<ResponseObject<List<User>>>(APICallHelper.GetUsers);
                var newList = new List<User>();
                var stringValues = new List<StringValues>();
                if (UsersResponse != null && UsersResponse.IsSuccess)
                {
                    var Users = UsersResponse.ApiResponseData.Data;
                    var isHeadOffice = IsHeadOffice();
                    var branchId = GetBranchID();
                    var branches = await GetBranches();

                    foreach (var user in Users.Where(u => isHeadOffice || u.BranchID == branchId))
                    {
                        user.name = $"{user.firstName} {user.lastName}";
                        user.strlastLoginDate = user.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        user.status = user.isActive ? "Active" : "In-active";

                        if (user.BranchID != null)
                        {
                            var branch = branches.FirstOrDefault(b => b.Id == user.BranchID);
                            if (branch != null)
                            {
                                user.Brancch = branch;
                                user.Bank = branch.Bank ?? new Bank();
                            }
                        }
                        else
                        {
                            user.Brancch = new Branch();
                            user.Bank = new Bank();
                        }

                        newList.Add(user);

                    }

                    stringValues = (from a in newList
                                    select new StringValues
                                    {
                                        Text = $"{a.firstName} {a.lastName}, Branch: {a.Brancch.Name}",
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

        public async Task<IEnumerable<StringValues>> GetUsesForTellerAssignation()
        {
            try
            {
                var identityServerBaseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var apiCallerHelper = new ApiCallerHelper(identityServerBaseUrl);

                var UsersResponse = await apiCallerHelper.GetAsync<ResponseObject<List<User>>>(APICallHelper.GetUsers);
                var newList = new List<User>();
                var stringValues = new List<StringValues>();
                if (UsersResponse != null && UsersResponse.IsSuccess)
                {
                    var Users = UsersResponse.ApiResponseData.Data;
                    var isHeadOffice = IsHeadOffice();
                    var branchId = GetBranchID();
                    var branches = await GetBranches();

                    foreach (var user in Users.Where(u => isHeadOffice || u.BranchID == branchId))
                    {
                        user.name = $"{user.firstName} {user.lastName}";
                        user.strlastLoginDate = user.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        user.status = user.isActive ? "Active" : "In-active";

                        if (user.BranchID != null)
                        {
                            var branch = branches.FirstOrDefault(b => b.Id == user.BranchID);
                            if (branch != null)
                            {
                                user.Brancch = branch;
                                user.Bank = branch.Bank ?? new Bank();
                            }
                        }
                        else
                        {
                            user.Brancch = new Branch();
                            user.Bank = new Bank();
                        }

                        newList.Add(user);

                    }

                    stringValues = (from a in newList
                                    select new StringValues
                                    {
                                        Text = $"[{a.firstName} {a.lastName}] [{a.Brancch.Name}]",
                                        Value = $"{a.id.ToString()}-{a.firstName} {a.lastName}",
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
                if (IsHeadOffice())
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
        public async Task<User> GetUser(string userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<User>>(string.Format(APICallHelper.GetUserByID, ConvertStringToGuid(userid)));
                if (user.ApiResponseData != null)
                {
                    return user.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<User> GetUser(Guid userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var userResponse = await ApiCallerHelper.GetAsync<ResponseObject<User>>(string.Format(APICallHelper.GetUserByID, userid));
                if (userResponse.IsSuccess)
                {
                    var user = userResponse.ApiResponseData.Data;
                    user.name = $"{user.firstName} {user.lastName}";
                    user.strlastLoginDate = user.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.status = user.isActive ? "Active" : "In-active";
                    user.ChangePassword.userName = user.userName;
                    user.roleID = user.userRoles.Select(role => role.roleId).First();
                    user.MFAActivation = new MFAActivation { Code = null, Email = user.email, Status = user.IsGoogleAuthenticatorEnabled };
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<UserDto> GetUserDto(Guid userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var userResponse = await ApiCallerHelper.GetAsync<ResponseObject<UserDto>>(string.Format(APICallHelper.GetUserByID, userid));
                if (userResponse.IsSuccess)
                {
                    var user = userResponse.ApiResponseData.Data;
                    return user;
                }
                return null;
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
                    GetExecutionMessages(staus, true, "User", MessagesResults.Success,
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
        public async Task<ExecutionMessages> UpdateUserProfile(User user)
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
                var reUser = await ApiCallerHelper.PutAsync<ResponseObject<User>>(string.Format(APICallHelper.UpdateUser, user.id), user);
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
        public async Task<ExecutionMessages> ChangePassword(User user)
        {
            try
            {
                user.ChangePasswordOnFirstLogin = true;
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<User>>(APICallHelper.ChangePassword, user.ChangePassword);
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
        public async Task<ExecutionMessages> FLoginChangePassword(FLoginChangePassword fLogin)
        {
            try
            {

                var password = new ResetPassword { userName = fLogin.UserName, password = fLogin.Password };
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<bool>>(APICallHelper.FLoginChangePasswordCommand, password);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(null, true, fLogin.UserName, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(fLogin, false, fLogin.UserName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ResetPassword(User user)
        {
            try
            {

                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<User>>(APICallHelper.ResetPassword, user.ChangePassword);
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

        public async Task<ExecutionMessages> EnableMFA(MFAActivation mFAActivation)
        {
            try
            {
                if (mFAActivation.Code!=null)
                {
                    mFAActivation.Status = true;
                }
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<bool>>(APICallHelper.MFAActivation, mFAActivation);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(null, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> MFACodeVerification(MFAActivation mFAActivation)
        {
            try
            {

                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<bool>>(APICallHelper.MFAVerification, mFAActivation);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(null, false, null, MessagesResults.Failed,
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
        public async Task<CustomDataTable> GetUsersDataTable(DataTableOptions dataTableOptions)

        {
            Func<Task<List<User>>> getUsersFunc = async () => (await GetUsers()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<User>(dataTableOptions, getUsersFunc);
            return dataTable;
        }
        public async Task<ExecutionMessages> UploadPicture(HttpPostedFileBase user)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
         {
             { "UserID", GetUserID() },
         };
                List<HttpPostedFileBase> image = new List<HttpPostedFileBase>();
                image.Add(user);
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostFilesAndParamsAsync<ResponseObject<User>>(APICallHelper.UploadProfilePhoto, additionalParams, image);
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

    }



}
