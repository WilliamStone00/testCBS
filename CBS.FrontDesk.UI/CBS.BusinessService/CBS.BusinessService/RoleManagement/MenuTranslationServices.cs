using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.MenuTranslationP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace CBS.BusinessService
{
    public class MenuTranslationServices : BaseService
    {
        private readonly ApiCallerHelper _identityConfigApiHelper;

        public MenuTranslationServices()
        {
            _identityConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<List<MenuMasterFroTranslation>> GetAllUnTransalatedMenus(string lang)
        {
            try
            {


                var getUntranslatedMenu = new GetUntranslatedMenuMastersQuery { LanguageCode=lang };
                var queryString = ToQueryString(getUntranslatedMenu);
                var fullUrl = $"{APICallHelper.GetUntreatedMenuTransaltion}?{queryString}";
                var response = await _identityConfigApiHelper.GetAsync<ResponseObject<List<MenuMaster>>>(fullUrl);
                if (response.ApiResponseData!=null)
                {
                    var menuMasterFroTranslations = response.ApiResponseData.Data.Select(m => new MenuMasterFroTranslation
                    {
                        Id = m.Id,
                        MenuText = m.MenuText,
                        Tooltip = m.Tooltip,
                        Description = m.Description
                    }).ToList();
                    return menuMasterFroTranslations;
                }
                return new List<MenuMasterFroTranslation>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<MenuTranslation>> GetAllTranslationsAsync(string language = null)
        {
            try
            {
                var getUntranslatedMenu = new GetUntranslatedMenuMastersQuery { LanguageCode=language };
                var queryString = ToQueryString(getUntranslatedMenu);
                var fullUrl = $"{APICallHelper.GetAllMenuTransaltion}?{queryString}";
                var response = await _identityConfigApiHelper.GetAsync<ResponseObject<List<MenuTranslation>>>(fullUrl);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }

                return new List<MenuTranslation>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<MenuTranslation> GetMenuTranslationAsync(string id)
        {
            try
            {
                var cusResponseObject = await _identityConfigApiHelper.GetAsync<ResponseObject<MenuTranslation>>(string.Format(APICallHelper.GetMenuTransaltion, id));
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
        public async Task<ExecutionMessages> Create(List<MenuTranslation> model)
        {
            try
            {
                var menuTranslationCommand = new AddMenuTranslationCommand { Translations=model };
                var response = await _identityConfigApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.AddMenuTransaltion, menuTranslationCommand);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(List<MenuTranslation> model)
        {
            try
            {
                var menuTranslationCommand = new AddMenuTranslationCommand { Translations=model };
                var response = await _identityConfigApiHelper.PutAsync<ResponseObject<MenuTranslation>>(APICallHelper.UpdateMenuTransaltion, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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

    }

}
