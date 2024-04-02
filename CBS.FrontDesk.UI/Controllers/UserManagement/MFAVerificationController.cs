using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    public class MFAVerificationController : BaseController
    {
        // GET: MFAVerification
        // GET: TwoStepaccountverification
        //private IUserManagementHelper _helper;
        //string url = "/";
        //private readonly IUserManagementUIManager _manager;
        //public MFAVerificationController(IUserManagementHelper helper, IUserManagementUIManager manager)
        //{
        //    _helper = helper;
        //    _manager = manager;

        //}
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<ActionResult> MFACodeVerification(string serviceoption = "None", string KEY = "KEY", string secrete = "none", string usersecreteid = "secrete", string path = null)
        //{


        //    if (!VerifyCookies("EASIMS_MFA"))
        //    {
        //        await InitializeData(serviceoption, KEY, null, path);
        //        if (_helper._object.User.Is2faEnabled)
        //        {
        //            _helper._object.User.Code = string.Empty;
        //            return View(_helper);
        //        }
        //    }

        //    return Redirect("~/Authentication/Logout");
        //}
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ActionResult> MFACodeVerification(UserManagementHelper model)
        //{



        //    if (!VerifyCookies("EASIMS_MFA"))
        //    {
        //        _helper = await _manager.CRUD(model);
        //        if (_helper.ExecutionMessage.Result)
        //        {
        //            CreateToken(_helper._object.User);

        //        }
        //        return Json(new { success = _helper.ExecutionMessage.Result, status = _helper.ExecutionMessage.MessageStatus, message = Messaging.MessageResult(_helper.ExecutionMessage), option = model._object.OperationOption.ServiceOption, optype = model._object.OperationOption.ActionType, reloadDataView = model._object.OperationOption.ReloadDataView, resetForm = model._object.OperationOption.ResetForm, controllerName = model._object.OperationOption.ControllerName, dataLoaderActionName = model._object.OperationOption.DataLoaderActionName, reinitializedActionName = model._object.OperationOption.ReinitializedActionName, divLoaderList = model._object.OperationOption.DivLoaderList, tableName = model._object.OperationOption.TableName, reloadPartialView = model._object.OperationOption.ReloadPartialView, kEY = model._object.OperationOption.KEY, url = url }, JsonRequestBehavior.AllowGet);

        //    }
        //    url = "~/Authentication/Logout";
        //    return Json(new { success = false, message = "Session expired.", url = url, state = "Expired" }, JsonRequestBehavior.AllowGet);



        //}
        //public async Task<ActionResult> InitializeData(string serviceoption = null, string KEY = null, string ReadOptions = null, string path = null, string group = null)
        //{
        //    ViewBag.KEY = KEY;
        //    _helper = await _manager.GET(serviceoption, ReadOptions, KEY, path, group);
        //    return PartialView(_helper.HelperOperation.ViewName, _helper);
        //}
    }
}