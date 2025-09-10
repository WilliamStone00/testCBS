using BusinessServices;
using CBS.API.Helper.APICallHelper;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Bibliography;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class SuspiciousPathService : BaseService
    {
        private static readonly Lazy<RateLimitApiHelper> _lazyApiCaller = new Lazy<RateLimitApiHelper>(() =>
        {
            var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("❌ IdentityServerBaseUrl is missing from AppSettings.");

            return new RateLimitApiHelper(baseUrl);
        });
        private static readonly ConcurrentDictionary<string, List<DateTime>> RequestHistory = new ConcurrentDictionary<string, List<DateTime>>();
        private readonly RateLimitApiHelper _api;


        private static RateLimitApiHelper ApiCaller => _lazyApiCaller.Value;
        private readonly ApiCallerHelper _identityServerBaseUrl;

        public SuspiciousPathService()
        {
            var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("❌ IdentityServerBaseUrl is missing from AppSettings.");

            _api = new RateLimitApiHelper(baseUrl);

            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        /// <summary>
        /// Adds a blocked user to the system.
        /// </summary>
        public async Task<ExecutionMessages> Add(AddSuspiciousPathCommand command)
        {
            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ResponseObject<bool>>(
                    APICallHelper.AddSuspiciousPath, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, command.Pattern, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, command.Pattern, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, command.Pattern, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(UpdateSuspiciousPathCommand command)
        {
            try
            {
                var response = await _identityServerBaseUrl.PutAsync<ResponseObject<bool>>(
                    APICallHelper.UpdateSuspiciousPath, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, command.Pattern, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, command.Pattern, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, command.Pattern, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        /// <summary>
        /// Deletes a blocked user by ID.
        /// </summary>
        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.DeleteAsync<ServiceResponse<bool>>(
                    string.Format(APICallHelper.Get_Delete_SuspiciousPath, id));

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
        /// Retrieves a blocked user by ID.
        /// </summary>
        public async Task<SuspiciousPath> GetSuspiciousPath(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<SuspiciousPath>>(
                    string.Format(APICallHelper.Get_Delete_SuspiciousPath, id));

                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve RateLimitedUser by ID", ex);
            }
        }

        /// <summary>
        /// Retrieves all blocked users.
        /// </summary>
        public async Task<List<SuspiciousPath>> GetAllAsync()
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<List<SuspiciousPath>>>(
                    APICallHelper.GetAllSuspiciousPaths);

                return response?.ApiResponseData.Data ?? new List<SuspiciousPath>();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve all RateLimitedUser records", ex);
            }
        }
        public async Task<bool> CheckIfSuspiciousPath(string path)
        {
            try
            {
                var suspiciousPathQuery = new IsSuspiciousPathQuery(path);
                var response = await _api.PostAsync<ResponseObject<bool>>(
                    APICallHelper.CheckSuspiciousPath, suspiciousPathQuery
                );

                return response.Data;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }

}
