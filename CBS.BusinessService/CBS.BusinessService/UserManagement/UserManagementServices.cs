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
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.BusinessService.Config;
using CBS.BusinessService.Session;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Owin.Logging;
using System.Data.Entity.Core.Metadata.Edm;

namespace CBS.BusinessService.UserManagement
{
    public class UserManagementServices : BaseService, IUserManagementServices
    {
        private readonly ApiCallerHelper _identityServerBaseUrl;
        private readonly BranchServices _branchServices;
        private readonly RoleServices _roleServices;
        public UserManagementServices()
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            _branchServices=new BranchServices();
            _roleServices=new RoleServices();
        }
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

        public async Task<IEnumerable<Role>> GetRoles()
        {
            try
            {
                var roles = await _roleServices.GetRoles();


          

                return roles;
            }
            catch (Exception ex)
            {
                throw; // Cleaned up: rethrow preserves original stack trace
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
                                        Text = $"{a.firstName} {a.lastName}, Branch Name: {a.Brancch?.Name}",
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


                var brabces = await _branchServices.GetBranches();
                return brabces;
                //var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
                //if (IsHeadOffice())
                //{
                //    var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                //    var bracBranches = branchApiResponse.ApiResponseData.Data;
                //    var branches = bracBranches;
                //    return branches;
                //}
                //else
                //{
                //    var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<Branch>>((string.Format(APICallHelper.Get_Update_Delete_Branch, GetBranchID())));
                //    var bracBranches = branchApiResponse.ApiResponseData.Data;
                //    var branches = new List<Branch>();
                //    branches.Add(bracBranches);
                //    return branches;
                //}

           ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<UserSessionDto>> GetUserSessions()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<List<UserSessionDto>>>((string.Format(APICallHelper.GetUserSessions, GetUserID())));
                var Sessions = branchApiResponse.ApiResponseData.Data;
                return Sessions;
                ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<UserSessionDto> GetUserCurrentsession()
        {
            try
            {
                var sessionCode = GetSessionCode();
                var username = GetUserName();

                var queryParams = $"?SessionCode={HttpUtility.UrlEncode(sessionCode)}&Username={HttpUtility.UrlEncode(username)}";
                var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var fullUrl = $"{baseUrl}{APICallHelper.GetUserSessionByUserNameAndCode}{queryParams}";

                var apiCaller = new ApiCallerHelper(baseUrl);
                var response = await apiCaller.GetAsync<ResponseObject<UserSessionDto>>(fullUrl);

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<UserSessionDto>> GetUserSessions(Guid userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<List<UserSessionDto>>>((string.Format(APICallHelper.GetUserSessions, userid)));
                var Sessions = branchApiResponse.ApiResponseData.Data;
                return Sessions;
                ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //GetUserSessions
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
                    user.roleID = user.userRoles?.FirstOrDefault()?.roleId ?? Guid.Empty;
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
        public List<UserDownloadDto> MapToUserDownloadDtos(IEnumerable<UserLightDto> users)
        {
            return users.Select(MapToUserDownloadDto).ToList();
        }
        public UserDownloadDto MapToUserDownloadDto(UserLightDto user)
        {
            return new UserDownloadDto
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                RoleName = user.RoleName ?? "N/A",
                PhoneNumber = user.PhoneNumber ?? "N/A",
                IsVerified = user.IsVerified,
                IsBlocked = user.IsBlocked,
                LoginAttempts = user.LoginAttempts,
                ChangePasswordOnFirstLogin = user.ChangePasswordOnFirstLogin,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive,
                SessionRecoveryCode = user.SessionRecoveryCode ?? "N/A",
                CreatedDate = user.CreatedDate,
                BranchCode = user.BranchId ?? "N/A", // Mapping BranchId to BranchCode
                BranchName = "N/A", // No direct mapping available, set to "N/A"
                NumberOfDaysSinceLastLogin = user.LastLoginDate.HasValue ? (DateTime.Now - user.LastLoginDate.Value).Days : 0,
                ReasonForBlockingAccount = user.IsBlocked ? user.ReasonForBlockingAccount : "-"
            };
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetAllUsersDataTableQuery getAllUsersDataTableQuery)
        {
            if (!IsHeadOffice())
            {
                getAllUsersDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _identityServerBaseUrl.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoadDataTablePagginationForUsers,
                getAllUsersDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(getAllUsersDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: getAllUsersDataTableQuery.DataTableOptions
            );
        }
        public List<UserSessionDataTable> MapUserSessionDtoToDataTable(List<UserSessionDto> sessionDtos)
        {
            return sessionDtos?.Select(dto => new UserSessionDataTable
            {
                Id = dto.Id,
                UserId = dto.UserId,
                UserName = dto.UserName,
                SessionCode = dto.SessionCode,
                FullName = dto.FullName,
                IpAddress = dto.IpAddress,
                CreatedDate = dto.CreatedDate,
                ExpiryDate = dto.ExpiryDate,
                BranchId = dto.BranchId,
                BranchCode = dto.BranchCode,
                BranchName = dto.BranchName,
                Role = dto.Role,
                ErrorMessage = dto.ErrorMessage,
                NumberOfSessionsOpen = dto.NumberOfSessionsOpen,
                SessionRecoveryCode = dto.SessionRecoveryCode,
                IsDefaultSessionRecoveryCode = dto.IsDefaultSessionRecoveryCode,
                IsExpired = dto.IsExpired,
                SessionStatus = dto.SessionStatus
            }).ToList() ?? new List<UserSessionDataTable>();
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetAllUserSessionsDataTableQuery getAllUsersDataTableQuery)
        {
            getAllUsersDataTableQuery.DataTableOptions.sortColumnName = "CreatedDate";
          
            // Make API call to fetch the DataTable result
            var couApiResponse = await _identityServerBaseUrl.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoadDataTablePagginationForUserSessions,
                getAllUsersDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(getAllUsersDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: getAllUsersDataTableQuery.DataTableOptions
            );
        }

        public async Task<ExecutionMessages> UpdateUserProfile(User model)
        {
            try
            {
                var userModel = await GetUser(model.id);

                if (model.Option == "Profile")
                {
                    userModel.firstName=model.firstName;
                    userModel.lastName=model.lastName;
                    userModel.phoneNumber=model.phoneNumber;
                    userModel.email=model.email;
                    userModel.address=model.address;
                    userModel.NationalIdentityCardNumber=model.NationalIdentityCardNumber;
                    userModel.IssueDate=model.IssueDate;
                    userModel.ExpiryDate=model.ExpiryDate;
                    userModel.PlaceOfIssue=model.PlaceOfIssue;
                    userModel.BankID=GetBankID();
                    
                }
                else if (model.Option == "SetLanguage")
                {
                    userModel.UserPreferedLanguage=userModel.UserPreferedLanguage;
                }
                else if (model.Option == "ActivateDeactivateAccount")
                {
                    userModel.isActive=userModel.isActive ? false : true;
                    userModel.ReasonForBlockingAccount=model.ReasonForBlockingAccount;
                }
                else if (model.Option == "ChangeBranch")
                {   
                    userModel.BranchID=model.BranchID;
                }
                else if (model.Option == "ChangeRole")
                {
                    userModel.userRoles=new List<UserRole>();
                    userModel.userRoles.Add(new UserRole { roleId = model.roleID, userId = model.id });
                }
                else if (model.Option == "BlackList")
                {
                    userModel.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = model.allowedIP } };
                }
                else
                {
                    userModel.UserPreferedLanguage=model.UserPreferedLanguage;
                }
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PutAsync<ResponseObject<User>>(string.Format(APICallHelper.UpdateUser, userModel.id), userModel);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, reUser.ApiResponseData.Data.firstName, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(userModel, false, userModel.firstName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ChangePassword(User user)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<bool>>(APICallHelper.ChangePassword, user.ChangePassword);
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
                var reUser = await ApiCallerHelper.PostAsync<ResponseObject<User>>(APICallHelper.ResetPassword, user.ResetPassword);
                if (reUser.IsSuccess)
                {
                    GetExecutionMessages(reUser, true, user.ChangePassword.userName, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        reUser.Message);
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
                mFAActivation.MfaTypeUsed="TOTP";
                mFAActivation.Email=GetUserName();
                mFAActivation.Id=ConvertStringToGuid(GetUserID());
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
                mFAActivation.Id=ConvertStringToGuid(GetUserID());
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
