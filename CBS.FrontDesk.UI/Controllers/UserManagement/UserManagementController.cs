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

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    [CheckSessionTimeOutAttribute]

    public class UserManagementController : BaseController
    {
        // GET: UserManagement
        private readonly IUserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        private readonly RoleServices _roleServices;
        public UserManagementController(IUserManagementServices userManagementServices, BranchServices branchServices = null, RoleServices roleServices = null)
        {
            _userManagementServices = userManagementServices;
            _branchServices = branchServices;
            _roleServices = roleServices;
        }
        public async Task<ActionResult> Index()
        {

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

        public async Task<ActionResult> UserProfile(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null, string datefrom = null, string dateto = null)
        {
            Session["userid_action"] = KEY;
            var data = await _userManagementServices.GetUser(_userManagementServices.ConvertStringToGuid(KEY));
            ViewBag.Roles = await _userManagementServices.GetRoles();
            ViewBag.Branches = await _userManagementServices.GetBranches();
            return View(data);
        }
        public async Task<ActionResult> Create(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null, string datefrom = null, string dateto = null)
        {
            ViewBag.Roles = await _userManagementServices.GetRoles();
            ViewBag.Branches = await _userManagementServices.GetBranches();
            return View(new User());
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
    }
}