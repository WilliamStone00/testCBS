using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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
            _branchServices = new BranchServices();
            _roleServices = new RoleServices();
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
        public async Task<IEnumerable<UserRoleDto>> GetDailyCollectors()
        {
            try
            {

                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var roles = await ApiCallerHelper.GetAsync<ResponseObject<List<UserRoleDto>>>(APICallHelper.GetAllUserRoles);
                if (IsHeadOffice())
                {
                    return roles.ApiResponseData.Data.Where(x => x.RoleName.Equals("Daily_Collector_Agent"));

                }
                else
                {
                    return roles.ApiResponseData.Data.Where(x => x.RoleName.Equals("Daily_Collector_Agent") && (x.branchId == GetBranchID()));

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
        public async Task<IEnumerable<StringValues>> GetUserTellerRoleDropDown()
        {
            try
            {
                List<StringValues> stringValues;

                if (IsHeadOffice())
                {
                    // GetAllowAnonymous all user roles and filter for tellers
                    var userRoles = await GetUSerRoles();
                    var branches = await GetBranches();
                    stringValues = (from a in userRoles
                                    join b in branches on a.branchId equals b.Id
                                    where a.IsTeller
                                    select new StringValues
                                    {
                                        Text = $"[{a.RoleName}]-[{a.FirstName} {a.LastName}] [{b.Name}]",
                                        Value = $"{a.UserId}@{a.FirstName} {a.LastName}",
                                    }).ToList();


                }
                else
                {
                    // GetAllowAnonymous user roles for the current branch and filter for tellers
                    var userRoles = await GetUSerRoles();
                    stringValues = (from a in userRoles
                                    where a.IsTeller && a.branchId == GetBranchID()
                                    select new StringValues
                                    {
                                        Text = $"[{a.RoleName}]-[{a.FirstName} {a.LastName}]",
                                        Value = $"{a.UserId}@{a.FirstName} {a.LastName}",
                                    }).ToList();
                }

                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }

        public async Task<IEnumerable<StringValues>> GetUserDropDownList(string branchId = null)
        {
            const string RoleA = "Daily_Collector_Agent";   // canonical
            const string RoleB = "DailyCollector_Agent";    // alias (seen elsewhere)

            try
            {
                // Pull roles once
                var userRoles = await GetUSerRoles(); // adjust type if you have a concrete DTO
                if (userRoles.Count() == 0) return Enumerable.Empty<StringValues>();

                var isHeadOffice = IsHeadOffice();

                // Effective branch scoping:
                // - Head office: use explicit branchId if provided; otherwise all branches.
                // - Non-head office: force current branch.
                var effectiveBranchId = isHeadOffice
                    ? (string.IsNullOrWhiteSpace(branchId) ? null : branchId)
                    : GetBranchID();

                // Load branches for display (safe even if not HO)
                var branches = await GetBranches() ?? new List<Branch>();

                // Base filter: must be teller, in the collector role, and match branch scope
                var filtered = userRoles
                    .Where(u =>
                        u != null
                        && u.IsTeller
                        && (string.Equals(u.RoleName, RoleA, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(u.RoleName, RoleB, StringComparison.OrdinalIgnoreCase))
                        && (string.IsNullOrWhiteSpace(effectiveBranchId) || u.branchId == effectiveBranchId)
                    )
                    .ToList();

                if (filtered.Count == 0) return Enumerable.Empty<StringValues>();

                // Join to branches (safe default if not found)
                var results = (from a in filtered
                               join b in branches on a.branchId equals b.Id into bj
                               from b in bj.DefaultIfEmpty()
                               select new StringValues
                               {
                                   Text = isHeadOffice
                                       ? $"[{a.RoleName}]-[{a.FirstName} {a.LastName}] [{b?.Name}]"
                                       : $"[{a.RoleName}]-[{a.FirstName} {a.LastName}]",
                                   Value = $"{a.UserId}",
                               })
                              .OrderBy(x => x.Text)
                              .ToList();

                return results;
            }
            catch
            {
                // Keep behavior graceful on failure
                return Enumerable.Empty<StringValues>();
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
        public async Task<IEnumerable<StringValues>> GetUserDropDownList(List<Branch> branches)
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
        {
            try
            {


                var brabces = await _branchServices.GetBranches();
                return brabces;
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
                getAllUsersDataTableQuery.BranchId = GetBranchID();
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
                    userModel.firstName = model.firstName;
                    userModel.lastName = model.lastName;
                    userModel.phoneNumber = model.phoneNumber;
                    userModel.email = model.email;
                    userModel.address = model.address;
                    userModel.NationalIdentityCardNumber = model.NationalIdentityCardNumber;
                    userModel.IssueDate = model.IssueDate;
                    userModel.ExpiryDate = model.ExpiryDate;
                    userModel.AccountExpiryDate = model.AccountExpiryDate;
                    userModel.PlaceOfIssue = model.PlaceOfIssue;
                    userModel.AccountTypePolicyProfile = model.AccountTypePolicyProfile;
                    userModel.BankID = GetBankID();

                }//OverridePolicy
                else if (model.Option == "OverridePolicy")
                {
                    userModel.PolicyOverride = model.PolicyOverride;
                }
                else if (model.Option == "SetLanguage")
                {
                    userModel.UserPreferedLanguage = userModel.UserPreferedLanguage;
                }
                else if (model.Option == "ActivateDeactivateAccount")
                {
                    userModel.isActive = userModel.isActive ? false : true;
                    userModel.ReasonForBlockingAccount = model.ReasonForBlockingAccount;
                }
                else if (model.Option == "ChangeBranch")
                {
                    userModel.BranchID = model.BranchID;
                }
                else if (model.Option == "ChangeRole")
                {
                    userModel.userRoles = new List<UserRole>();
                    userModel.userRoles.Add(new UserRole { roleId = model.roleID, userId = model.id });
                }
                else if (model.Option == "BlackList")
                {
                    userModel.userAllowedIPs = new List<UserAllowedIP> { new UserAllowedIP() { ipAddress = model.allowedIP } };
                }
                else
                {
                    userModel.UserPreferedLanguage = model.UserPreferedLanguage;
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
                if (mFAActivation.Code != null)
                {
                    mFAActivation.Status = true;
                }
                mFAActivation.MfaTypeUsed = "TOTP";
                mFAActivation.Email = GetUserName();
                mFAActivation.Id = ConvertStringToGuid(GetUserID());
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
                mFAActivation.Id = ConvertStringToGuid(GetUserID());
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

        //Get Third Party user
        public async Task<IEnumerable<ThirdPartyUser>> GetUserbyrole(CancellationToken cancellationToken = default)
        {
            try
            {
                string roleName = GetRoleName();
                List<ThirdPartyUser> branchList = new List<ThirdPartyUser>();

                if (roleName == "Administrator")
                {
                    // Administrator: get all users with role "ThirdPartyProviders"
                    var query = new GetAllUsersDataTableQuery
                    {
                        Role = "ThirdPartyProviders",
                        IsActive = true,
                        DataTableOptions = new DataTableOptions
                        {
                            draw = "2",
                            length = 10,
                            sortColumnName = "LastLoginDate",     
                            sortColumnDirection = "asc",
                             pageSize = 3000,
                            skip = 0,
                            recordsTotal = 0,
                            recordsFiltered = 0,                                                                                 
                        }
                    };

                    var dataTable = await GetDataTableAsync(query);

                    // Convert datatable.data to strongly-typed list safely
                    if (dataTable?.data is JToken token)
                    {
                        branchList = token.ToObject<List<ThirdPartyUser>>() ?? new List<ThirdPartyUser>();
                    }
                    else if (dataTable?.data != null)
                    {
                        // Fallback if data is plain object
                        branchList = JsonConvert.DeserializeObject<List<ThirdPartyUser>>(JsonConvert.SerializeObject(dataTable.data))
                                    ?? new List<ThirdPartyUser>();
                    }
                }
                else if (roleName == "ThirdPartyProviders")
                {
                    // ThirdPartyProviders: get only the current user
                    string username = GetUserName();

                    if (!string.IsNullOrEmpty(username))
                    {
                        var query = new GetAllUsersDataTableQuery
                        {
                            UserName = username,
                            Role = "ThirdPartyProviders"
                        };

                        var dataTable = await GetDataTableAsync(query);

                        // Convert datatable.data to strongly-typed list safely
                        if (dataTable?.data is JToken token)
                        {
                            branchList = token.ToObject<List<ThirdPartyUser>>() ?? new List<ThirdPartyUser>();
                        }
                        else if (dataTable?.data != null)
                        {
                            // Fallback if data is plain object
                            branchList = JsonConvert.DeserializeObject<List<ThirdPartyUser>>(JsonConvert.SerializeObject(dataTable.data))
                                        ?? new List<ThirdPartyUser>();
                        }
                    }
                }
                else
                {
                    // Other roles - return empty list or handle as needed
                    return new List<ThirdPartyUser>();
                }

                var formatted = branchList
                    .Select(a => new ThirdPartyUser
                    {
                        Id = a.UserName,  // Username as ID to be sent when option is chosen
                        Name = $"[{a.UserName}] - {a.UserName}".Trim() // Format: [username] - username
                    })
                    .OrderBy(a => a.Name)
                    .ToList();

                return formatted;
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging
                // _logger.LogError(ex, "Error getting branch accounts from data table");
                throw;
            }
        }
    }



}
