using CBS.FrontDesk.Data.Message;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.API.Helper;
using System.Linq;
using CBS.BusinessService.Config;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;
using CBS.BusinessService.Session;
using System.Web.Services.Description;
using Microsoft.Owin.Logging;
using DocumentFormat.OpenXml.EMMA;
using System.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    [CheckSessionTimeOutAttribute]
    public class UserManagementController : BaseController
    {
        private readonly RolePermissionServices _rolePermissionServices;
        private readonly UserPermissionServices _userPermissionServices;
        // GET: UserManagement
        private readonly IUserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        private readonly RoleServices _roleServices;
        private readonly LocalSession _localSession;

        public UserManagementController(IUserManagementServices userManagementServices, BranchServices branchServices = null, RoleServices roleServices = null, LocalSession localSession = null, RolePermissionServices rolePermissionServices = null, UserPermissionServices userPermissionServices = null)
        {
            _userManagementServices = userManagementServices;
            _branchServices = branchServices;
            _roleServices = roleServices;
            _localSession=localSession;
            _rolePermissionServices=rolePermissionServices;
            _userPermissionServices=userPermissionServices;
        }
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        public async Task<ActionResult> ConnectedUsers()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        
        public async Task<ActionResult> InitializeData(string partialView = null, string KEY = null, string path = null)
        {

            ViewBag.KEY = KEY;
            if (path.ToLower() == "list")
            {
                var data = await _userManagementServices.GetUsers();
                return PartialView(partialView, data);

            }
            else if (path == "new")
            {
                return PartialView(partialView, new User());
            }
            else
            {
                ViewBag.KEY = KEY;
                var data = await _userManagementServices.GetUser(_userManagementServices.ConvertStringToGuid(KEY));
                string view = null;
                //var roles = await _userManagementServices.GetRoles();
                var branch= await _branchServices.GetBranch(data.BranchID);
                var role = await _roleServices.GetRole(data.roleID.ToString());
                data.Brancch=branch;
                data.roleName=role.Name;
                return PartialView(partialView, data);
            }

            //ViewBag.KEY = KEY;
            //var data = await _userManagementServices.GetUser(_userManagementServices.ConvertStringToGuid(KEY));
            //string view = null;
            //var roles = await _userManagementServices.GetRoles();
            //ViewBag.Branches = await _userManagementServices.GetBranches();
            //if (KEY==null)
            //{

            //    return PartialView("_CreateUser", new User());
            //}
            //return PartialView(view, "");
        }
        // MyProfile
        public async Task<ActionResult> MyProfile(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null, string datefrom = null, string dateto = null)
        {
            Session["userid_action"] = KEY;
            var data = await _userManagementServices.GetUser(_userManagementServices.ConvertStringToGuid(KEY));
            data.ResetPassword=new ResetPassword { userName=data.userName, password="000000", ResetPasswordReason=data.ResetPasswordReason };
            ViewBag.Branches = await _userManagementServices.GetBranches();
            return View(data);
        }
        public async Task<ActionResult> UserProfile(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null, string datefrom = null, string dateto = null)
        {
            Session["userid_action"] = KEY;
            var data = await _userManagementServices.GetUser(_userManagementServices.ConvertStringToGuid(KEY));
            data.ResetPassword=new ResetPassword { userName=data.userName, password="000000", ResetPasswordReason=data.ResetPasswordReason };
            var permissionMenuLoaders = await _rolePermissionServices.GetAssignPermissions();
            data.PermissionMenuLoaders = permissionMenuLoaders.ToList();
            await GetList();
            ViewBag.Branches = await _userManagementServices.GetBranches();
            return View(data);
        }
        public async Task<ActionResult> Create(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null, string datefrom = null, string dateto = null)
        {
            ViewBag.Roles = await _userManagementServices.GetRoles();
            ViewBag.Branches = await _userManagementServices.GetBranches();
            return View(new User());
        }
        public async Task<bool> GetList()
        {

            var stringValues = await _rolePermissionServices.GetRolePermissions();
            ViewBag.Roles = stringValues.ToList();
            return true;
        }
        [HttpPost]
        public async Task<ActionResult> LoadData()
        {
            try
            {
                var dataTable = await _userManagementServices.GetUsersDataTable(GetDataTableOptions());
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEdit(User model)
        {

            if (model.Option == "Profile"|| model.Option == "BlackList"||model.Option == "ChangeRole"||model.Option == "ChangeBranch"||model.Option == "ActivateDeactivateAccount")
            {
                var data = await _userManagementServices.UpdateUserProfile(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "ChangePassword")
            {
                var data = await _userManagementServices.ChangePassword(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "ResetPassword")
            {
                var data = await _userManagementServices.ResetPassword(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            //else if (model.Option == "ActivateAccount")
            //{
            //    var data = await _userManagementServices.ResetPassword(model);
            //    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            //}
            else if (model.Option == "PhotoUpload")
            {
                var data = await _userManagementServices.UploadPicture(model.FileUpload);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "EnableMFA")
            {
                var data = await _userManagementServices.EnableMFA(model.MFAActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "GenerateNewRecoveryCode")
            {
                var data = await _localSession.GenerateANewSessionRecoveryCode(model.id.ToString());
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "TerminateActiveSessions")
            {
                var data = await _localSession.InvalidateAllActivetUsers(new SessionAuth { SessionRecoveryCode=model.SessionRecoveryCode});
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                var data = await _userManagementServices.CreateUser(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }

        }
        [HttpPost]
        public async Task<JsonResult> TerminateSession(string userid)
        {
            try
            {
                if (string.IsNullOrEmpty(userid))
                {
                    return Json(new { success = false, message = "Session code is required to terminate session." });
                }
                var command = new AddLogoutSessionCommand { UserId = userid };
                var result = await _localSession.LogoutuserSessions(command);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while terminating the session.", error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> LoadUsers(GetAllUsersDataTableQuery query)
        {
            try
            {
                if (!query.IsActive && !query.IsBlocked && !query.IsVerified)
                {
                    query.IsActive=true;
                }
                var dataTable = await _userManagementServices.GetDataTableAsync(query);

                var userList = JsonConvert.DeserializeObject<List<UserLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = userList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error loading users: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> LoadUserSessions(GetAllUserSessionsDataTableQuery query)
        {
            try
            {
               
                var dataTable = await _userManagementServices.GetDataTableAsync(query);

                var userList = JsonConvert.DeserializeObject<List<UserSessionDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                var usersessions = _userManagementServices.MapUserSessionDtoToDataTable(userList);
                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = usersessions
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error loading users: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetLiveSessionDashboard()
        {
            var dataTable = await _userManagementServices.GetDataTableAsync(
                new GetAllUserSessionsDataTableQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        start = 0,
                        pageSize = 10000
                    }
                }
            );

            var userSessions = JsonConvert.DeserializeObject<List<UserSessionDto>>(
                JsonConvert.SerializeObject(dataTable.data)
            );

            foreach (var s in userSessions)
            {
                s.IsExpired = s.ExpiryDate <= DateTime.UtcNow;
                s.SessionStatus = s.IsExpired ? "Expired" : "Active";
            }
            var usersessions = _userManagementServices.MapUserSessionDtoToDataTable(userSessions);
            return Json(usersessions, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public async Task<ActionResult> DownloadUsers(GetAllUsersDataTableQuery query)
        {
            try
            {
                query.DataTableOptions = new DataTableOptions
                {
                    pageSize = 10000,
                    start = 0
                };

                var dataTable = await _userManagementServices.GetDataTableAsync(query);
                var userList = JsonConvert.DeserializeObject<List<UserLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                string exportedBy = Session["FullName"]?.ToString() ?? "System Export";

                //var file = ExportUtilityUser.GenerateUsersExcel(
                //    userList,
                //    exportedBy,
                //    query.StartDate?.ToString("dd/MM/yyyy"),
                //    query.EndDate?.ToString("dd/MM/yyyy")
                //);
                return null;
                //return File(file.Content, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting user data.");
            }
        }
  
        [HttpPost]
        public async Task<ActionResult> UpdateUser(User model)
        {
            if (model.Option == "Profile")
            {
                var data = await _userManagementServices.UpdateUserProfile(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "ChangePassword")
            {
                var data = await _userManagementServices.ChangePassword(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "ResetPassword")
            {
                var data = await _userManagementServices.ResetPassword(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "ActivateAccount")
            {
                var data = await _userManagementServices.ResetPassword(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "PhotoUpload")
            {
                var data = await _userManagementServices.UploadPicture(model.FileUpload);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.Option == "EnableMFA")
            {
                var data = await _userManagementServices.EnableMFA(model.MFAActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                var data = await _userManagementServices.CreateUser(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> FLoginChangePassword(string serviceoption = "None", string KEY = "KEY", string secrete = "none", string usersecreteid = "secrete", string path = null, string userName = null)
        {
            if (VerifyIfSessionExist("PWD"))
            {
                Guid userId;
                try
                {
                    userId = _userManagementServices.ConvertStringToGuid(KEY);
                }
                catch
                {
                    return Redirect("~/Authentication/Logout"); // Redirect if KEY is not a valid GUID
                }

                var Flogin = new FLoginChangePassword
                {
                    ConfirmPassword = string.Empty,
                    Password = string.Empty,
                    UserName = userName,
                    FullName = path, // Assuming FullName should be set to userName
                    UserId = userId
                };

                return View(Flogin);
            }
            return Redirect("~/Authentication/Logout");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> FLoginChangePassword(FLoginChangePassword model)
        {

            // Check if model state is valid
            if (ModelState.IsValid)
            {


                // Verify cookies
                if (VerifyIfSessionExist("PWD"))
                {
                    var data = await _userManagementServices.FLoginChangePassword(model);
                    if (data.Result)
                    {
                        RemoveSessionName("PWD");
                    }
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }

                // Session expired
                var url = "~/Authentication/Login";
                return Json(new { success = false, message = "Session expired.", urldirect = url, state = "Expired" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Model state is not valid, return validation errors as a concatenated string with numbers
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}");

                var errorMessage = string.Join("<br>", errors);
                return Json(new { success = false, message = $"Model state is not valid. Errors:<br>{errorMessage}" });
            }



        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _userManagementServices.DeleteUser(_userManagementServices.ConvertStringToGuid(KEY));
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public async Task<JsonResult> AddUserRoles(Guid userId, List<int> selectedMenus)
        {
            try
            {
                if (selectedMenus == null || !selectedMenus.Any())
                    return Json(new { success = false, message = "No roles selected." });

                
              var data=  await _userPermissionServices.AddUserRole(userId, selectedMenus);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while assigning roles." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUserPermission(List<string> userPermissionIds)
        {
            try
            {
                if (userPermissionIds == null || !userPermissionIds.Any())
                    return Json(new { success = false, message = "No permissions selected." });

               var data= await _userPermissionServices.Delete(userPermissionIds);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error deleting user permissions.");
                return Json(new { success = false, message = "Failed to remove permissions." });
            }
        }


    }
}