using BusinessServices;
using CBS.API.Helper;
using CBS.API.Helper.APICallHelper;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

public class RateLimitConfigService : BaseService
{
    // ✅ Lazy init for RateLimitApiHelper using appsettings value
    private static readonly Lazy<RateLimitApiHelper> _lazyApiCaller = new Lazy<RateLimitApiHelper>(() =>
    {
        var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("❌ IdentityServerBaseUrl is missing from AppSettings.");

        return new RateLimitApiHelper(baseUrl);
    });

    private static RateLimitApiHelper ApiCaller => _lazyApiCaller.Value;
    private readonly ApiCallerHelper _identityServerBaseUrl;
    public RateLimitConfigService()
    {
        _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());

    }
    /// <summary>
    /// Adds or updates the RateLimitConfig (only one should exist).
    /// </summary>
    public async Task<ExecutionMessages> AddOrUpdateAsync(RateLimitConfig command)
    {
        try
        {
            var response = await _identityServerBaseUrl.PostAsync<ResponseObject<RateLimitConfig>>(
                APICallHelper.AddOrUpdateRateLimitConfig, command);

            if (response.IsSuccess)
            {
                GetExecutionMessages(response, true, command.Id, MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
            }
            else
            {
                GetExecutionMessages(null, false, command.Id, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
        }
        catch (Exception ex)
        {
            GetExecutionMessages(null, false, command.Id, MessagesResults.Failed,
                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
        }

        return ExecutionMessage;
    }

    /// <summary>
    /// Deletes the RateLimitConfig by ID.
    /// </summary>
    public async Task<ExecutionMessages> DeleteAsync(string id)
    {
        try
        {
            var response = await _identityServerBaseUrl.DeleteAsync<ServiceResponse<bool>>(
                string.Format(APICallHelper.Get_Delete_RateLimitConfig, id)); // Assuming GET is used for delete endpoint call

            if (response.IsSuccess)
            {
                GetExecutionMessages(response, true, id, MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
            }
            else
            {
                GetExecutionMessages(null, false, id, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
        }
        catch (Exception ex)
        {
            GetExecutionMessages(null, false, id, MessagesResults.Failed,
                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
        }

        return ExecutionMessage;
    }

    /// <summary>
    /// Retrieves a RateLimitConfig by ID.
    /// </summary>
    public async Task<RateLimitConfig> GetRateLimitConfig(string id)
    {
        try
        {
            var response = await _identityServerBaseUrl.GetAsync<ResponseObject<RateLimitConfig>>(
                string.Format(APICallHelper.Get_Delete_RateLimitConfig, id));

            return response.ApiResponseData.Data;
        }
        catch (Exception ex)
        {
            throw new Exception("❌ Failed to retrieve RateLimitConfig by ID", ex);
        }
    }

    /// <summary>
    /// Retrieves all RateLimitConfigs (only one expected).
    /// </summary>
    public async Task<List<RateLimitConfig>> GetAllAsync()
    {
        try
        {
            var response = await ApiCaller.GetAsync<ResponseObject<List<RateLimitConfig>>>(
                APICallHelper.GetAllRateLimitConfig);

            return response?.Data ?? new List<RateLimitConfig>();
        }
        catch (Exception ex)
        {
            throw new Exception("❌ Failed to retrieve all RateLimitConfig records", ex);
        }
    }

    public static class RateLimitConfigHolder
    {
        public static RateLimitConfig Config { get; private set; }

        public static async Task LoadAsync(RateLimitConfigService configService)
        {
            var configs = await configService.GetAllAsync();
            Config = configs.FirstOrDefault() ?? new RateLimitConfig
            {
                RequestLimit = 50,
                TimeWindowSeconds = 20,
                BlockDurationMinutes = 30
            };
        }
    }

}


