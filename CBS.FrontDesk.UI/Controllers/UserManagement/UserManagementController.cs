using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.UI.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    //[SessionTimeoutFilterAttribute]
    public class UserManagementController: BaseController
    {
        // GET: UserManagement
        private readonly IUserManagementServices _userManagementServices;

        public UserManagementController(IUserManagementServices userManagementServices)
        {
            _userManagementServices=userManagementServices;
        }
        public async Task<ActionResult> Index()
        {
            //var userList = await _userManagementServices.GetUserList();

            return View();
        }
        public async Task<ActionResult> InitializeData(string partialView = null, string KEY = null, string path = null)
        {

            ViewBag.KEY = KEY;
            if (path == "list")
            {
                var data = await _userManagementServices.GetUserList();
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
                var roles = await _userManagementServices.GetRoles();
                ViewBag.Branches = await _userManagementServices.GetBranches(); 
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

            var data = await _userManagementServices.CreateUser(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });


        }
        [HttpPost]
        public async Task<ActionResult> Update(UserList model)
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
            else
            {
                var data = await _userManagementServices.CreateUser(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _userManagementServices.DeleteUser(_userManagementServices.ConvertStringToGuid(KEY));
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}