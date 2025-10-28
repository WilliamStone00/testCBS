using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ConfigurationsManualEntry
{
    public class ConfigurationManualEntryService : BaseService
    {

        private readonly ApiCallerHelper _configurationapiCallerHelper;


        public ConfigurationManualEntryService()
        {

            _configurationapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }
        
        public async Task<List<ConfigurationManualEntries>> GetAllAsync()
        {
            try
            {
                var apiResponse = await _configurationapiCallerHelper
                    .GetAsync<List<ConfigurationManualEntries>>(APICallHelper.GetAllConfigurationManualEntry);

                return apiResponse.ApiResponseData ?? new List<ConfigurationManualEntries>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"Configuration service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ConfigurationManualEntries> GetData(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new ConfigurationManualEntries();

                var apiResponse = await _configurationapiCallerHelper
                    .GetAsync<ResponseObject<ConfigurationManualEntries>>(
                        string.Format(APICallHelper.GetConfigurationManualEntryById, id)
                    );

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new ConfigurationManualEntries();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] Error fetching ConfigurationManualEntries: {ex.Message}");
                return new ConfigurationManualEntries();
            }
        }



        public async Task<ExecutionMessages> UpdateConfigurationManualEntryAsync(ConfigurationManualEntries model)
        {
            try
            {

                // Send model to API via POST (or PUT if your API expects PUT)
                var response = await _configurationapiCallerHelper.PostAsync<ServiceResponse<ConfigurationManualEntries>>(APICallHelper.UpdateConfigurationManualEntry, model);

                if (response.IsSuccess)
                {
                    // Success execution message
                    GetExecutionMessages(
                        response.ApiResponseData.Data,      // The updated data object
                        true,                               // success
                        model.Type,                          // Object name for logs
                        MessagesResults.Success,             // Enum: Success
                        ExecutionProcessOption.UpdateUpject, // Enum: Update operation
                        SystemMessageStatus.Success.ToString(),
                        null,                                // exception
                        response.ApiResponseData?.Message    // optional API message
                    );
                }
                else
                {
                    // Failure execution message
                    GetExecutionMessages(
                        model,
                        false,
                        model.Type,
                        MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Failed.ToString(),
                        null,
                        response.ApiResponseData?.Message ?? response.Message
                    );
                }
            }
            catch (Exception ex)
            {
                // Exception execution message
                GetExecutionMessages(
                    model,
                    false,
                    model.Type,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );
            }

            return ExecutionMessage; // Return accumulated execution result
        }




        //public async Task<ConfigurationManualEntries> GetData(string id)
        //{


        //    if (!string.IsNullOrWhiteSpace(id))
        //    {

        //        var apiResponse = ConfigurationManualEntriesSample.GetSampleEntries().Where(x => x.Id == id).FirstOrDefault();
        //        return apiResponse;

        //    }
        //    else
        //    {
        //        var apiResponse = await _configurationapiCallerHelper.GetAsync<ResponseObject<ConfigurationManualEntries>>(string.Format(APICallHelper.GetConfigurationManualEntryById, id));
        //        if (apiResponse == null) { return new ConfigurationManualEntries(); }
        //        return apiResponse.ApiResponseData.Data;

        //    }
        //}




        //public List<ConfigurationManualEntries> GetSampleEntries()
        //{
        //    try
        //    {
        //       var data= ConfigurationManualEntriesSample.GetSampleEntries();

        //        return data; // return the full ApiResponse
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
        //        throw new Exception($"Configuration service unavailable: {ex.Message}", ex);
        //    }
        //}




    }
    //public static class ConfigurationManualEntriesSample
    //{
    //    public static List<ConfigurationManualEntries> GetSampleEntries()
    //    {
    //        return new List<ConfigurationManualEntries>
    //    {
    //        new ConfigurationManualEntries { Id="1", Type = "Local", Command = "Auto", Description = "Local Auto Posting", UserRequiredApproval = false, Status = true },
    //        new ConfigurationManualEntries { Id="2", Type = "Local", Command = "Auto Approval", Description = "Local Auto Approval Posting", UserRequiredApproval = true, Status = true },
    //        new ConfigurationManualEntries { Id="3", Type = "Local", Command = "Auto Approval Source", Description = "Local Auto Approval Source Posting", UserRequiredApproval = true, Status = false },
    //        new ConfigurationManualEntries { Id="4", Type = "Local", Command = "Auto Approval Source Destination", Description = "Local Auto Approval Source to Destination", UserRequiredApproval = true, Status = true },
    //        new ConfigurationManualEntries { Id="5", Type = "InterBranch", Command = "Auto", Description = "Inter Branch Auto Posting", UserRequiredApproval = false, Status = true },
    //        new ConfigurationManualEntries { Id="6", Type = "InterBranch", Command = "Auto Approval", Description = "Inter Branch Auto Approval Posting", UserRequiredApproval = true, Status = true },
    //        new ConfigurationManualEntries { Id="7", Type = "InterBranch", Command = "Auto Approval Source", Description = "Inter Branch Auto Approval Source Posting", UserRequiredApproval = true, Status = false },
    //        new ConfigurationManualEntries { Id="8", Type = "InterBranch", Command = "Auto Approval Source Destination", Description = "Inter Branch Auto Approval Source to Destination", UserRequiredApproval = true, Status = true },
    //        new ConfigurationManualEntries { Id="9", Type = "Local", Command = "Auto", Description = "Local Auto Posting 2", UserRequiredApproval = false, Status = true },

    //    };
    //    }



    //}

}

