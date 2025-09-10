using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.BusinessService
{
    public class MenuMasterServices : BaseService
    {
        private readonly ApiCallerHelper _identityConfigApiHelper;

        public MenuMasterServices()
        {
            _identityConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objMenuMaster = await GetMenuMaster(id);
                var inResponse = await _identityConfigApiHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Get_Update_Delete_MenuMaster,id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objMenuMaster.MenuText}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objMenuMaster, false, $"{objMenuMaster.MenuText}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<MenuMaster>> GetMenuMasters()
        {
            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<MenuMaster>>>(APICallHelper.GetAllMenuMaster);
                if (couApiResponse.ApiResponseData!=null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<MenuMaster>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<StringValues>> GetMenuMastersDropDowns()
        {

            try
            {
                var couApiResponse = await _identityConfigApiHelper.GetAsync<ResponseObject<List<MenuMaster>>>(APICallHelper.GetAllMenuMaster);
                if (couApiResponse.ApiResponseData != null)
                {
                    var menuMasters = (from a in couApiResponse.ApiResponseData.Data
                                      select new StringValues
                                      {
                                          Value = a.Id.ToString(),
                                          Text = $"{a.Id}-{a.MenuText}-{a.ControllerName}-{a.ActionName}"
                                      }).ToList();
                    menuMasters.Add(new StringValues { Text = "None", Value = "0" });
                    return menuMasters;
                }
                return new List<StringValues>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<MenuMaster> GetMenuMaster(string id)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<MenuMaster>>(string.Format(APICallHelper.Get_Update_Delete_MenuMaster, id));
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
        public async Task<ExecutionMessages> Create(MenuMaster model)
        {
            try
            {
                if (model.ParentId==null)
                {
                    model.ParentId = "0";
                }
                var response = await _identityConfigApiHelper.PostAsync<ResponseObject<MenuMaster>>(APICallHelper.CreateMenuMaster, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.MenuText}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.MenuText, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(MenuMaster model)
        {
            try
            {
                var response = await _identityConfigApiHelper.PutAsync<ResponseObject<MenuMaster>>(string.Format(APICallHelper.Get_Update_Delete_MenuMaster, model.Id), model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.MenuText}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.MenuText, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
                //var MenuMaster = await GetMenuMaster(model.Id.ToString());
                //if (MenuMaster != null)
                //{
                //    MenuMaster.MenuText = model.MenuText;
                //    MenuMaster.MenuGroup = model.MenuGroup;
                //    MenuMaster.ActionName = model.ActionName;
                //    MenuMaster.ControllerName = model.ControllerName;
                //    MenuMaster.ParentId = model.ParentId;
                //    MenuMaster.IsVisible = model.IsVisible;
                //    MenuMaster.Description = model.Description;
                //    MenuMaster.IconClass = model.IconClass;
                //    MenuMaster.MenuOrder = model.MenuOrder;

                //}

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
