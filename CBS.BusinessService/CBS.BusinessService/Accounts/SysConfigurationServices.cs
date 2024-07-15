using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config.System;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class SysConfigurationServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public SysConfigurationServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Create(SysConfiguration model)
        {
            try
            {
                
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<SysConfiguration>>(APICallHelper.CreateSysConfiguration, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.Name}", MessagesResults.Failed,
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
        public async Task<IEnumerable<SysConfiguration>> GetSysConfigurations()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<SysConfiguration>>>(APICallHelper.GetAllSysConfiguration);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<SysConfiguration>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<SysConfiguration> GetSysConfiguration(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<SysConfiguration>>(string.Format(APICallHelper.Get_Update_Delete_SysConfiguration, id));
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
       
        public async Task<ExecutionMessages> Update(SysConfiguration model)
        {
            try
            {
                var SysConfiguration = await GetSysConfiguration(model.Id);
                
                
                if (SysConfiguration != null)
                {
                    if (model.ServiceOption == "close_open_day")
                    {
                        SysConfiguration.IsDayOpen = model.IsDayOpen;
                    }
                    else if (model.ServiceOption == "close_open_year")
                    {
                        SysConfiguration.IsYearOpen = model.IsYearOpen;
                    }
                    else if (model.ServiceOption == "automatic_charging_system")
                    {
                        SysConfiguration.UseAutomaticChargingSystem = model.UseAutomaticChargingSystem;
                    }
                    else if (model.ServiceOption == "set_off_days")
                    {
                        SysConfiguration.MondayIsHoliday = model.MondayIsHoliday;
                        SysConfiguration.TuesDayIsHoliday = model.TuesDayIsHoliday;
                        SysConfiguration.MWednessdayIsHoliday = model.MWednessdayIsHoliday;
                        SysConfiguration.ThursdayIsHoliday = model.ThursdayIsHoliday;
                        SysConfiguration.FridayIsHoliday = model.FridayIsHoliday;
                        SysConfiguration.SaturdayIsHoliday = model.SaturdayIsHoliday;
                        SysConfiguration.SundayIsHoliday = model.SundayIsHoliday;
                    }
                    else if (model.ServiceOption == "profile")
                    {
                        SysConfiguration.Description = model.Description;
                        SysConfiguration.Value = model.Value;
                        SysConfiguration.Name = model.Name;
                        SysConfiguration.SetCloseDayTime = model.SetCloseDayTime;
                    }
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<SysConfiguration>>(string.Format(APICallHelper.Get_Update_Delete_SysConfiguration, model.Id), SysConfiguration);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"System-{model.ServiceOption}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "SysConfiguration", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
