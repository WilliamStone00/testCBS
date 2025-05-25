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

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class RateLimitedUserService : BaseService
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

        public RateLimitedUserService()
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
        public async Task<ExecutionMessages> BlockAsync(BlockUserCommand command)
        {
            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ResponseObject<bool>>(
                    APICallHelper.AddRateLimitedUser, command);

                if (response.IsSuccess && response.ApiResponseData?.Data == true)
                {
                    GetExecutionMessages(response, true, command.IpAddress, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, command.IpAddress, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, command.IpAddress, MessagesResults.Failed,
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
                var response = await _identityServerBaseUrl.GetAsync<ServiceResponse<bool>>(
                    string.Format(APICallHelper.Get_Delete_RateLimitedUser, id));

                if (response.IsSuccess && response.ApiResponseData?.Data == true)
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
        public async Task<RateLimitedUser> GetByIdAsync(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<RateLimitedUser>>(
                    string.Format(APICallHelper.Get_Delete_RateLimitedUser, id));

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
        public async Task<List<RateLimitedUser>> GetAllAsync()
        {
            try
            {
                var response = await ApiCaller.GetAsync<ResponseObject<List<RateLimitedUser>>>(
                    APICallHelper.GetAllRateLimitedUsers);

                return response?.Data ?? new List<RateLimitedUser>();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve all RateLimitedUser records", ex);
            }
        }
        public bool CheckIsBlocked(string ipOrUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipOrUsername)) return false;

                var response = _api.GetSync<ResponseObject<bool>>(
                    string.Format(APICallHelper.CheckRateLimitBlock, ipOrUsername)
                );

                return response.ApiResponseData.Data == true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Block check failed for '{ipOrUsername}': {ex.Message}");
                return false;
            }
        }

        public bool ShouldBlock(string ipOrUsername, int requestLimit, TimeSpan timeWindow)
        {
            var now = DateTime.UtcNow;

            if (!RequestHistory.ContainsKey(ipOrUsername))
                RequestHistory[ipOrUsername] = new List<DateTime>();

            var timestamps = RequestHistory[ipOrUsername];

            lock (timestamps)
            {
                timestamps.RemoveAll(t => now - t > timeWindow);
                timestamps.Add(now);

                return timestamps.Count > requestLimit;
            }
        }

        public void BlockUser(string ipOrUsername, string userName, string blockType, TimeSpan blockDuration, string reason, string location)
        {
            var command = new BlockUserCommand
            {
                IpAddress = ipOrUsername,
                UserName = userName,
                BlockType = blockType,
                BlockEndTime = DateTime.UtcNow.Add(blockDuration),
                BlockedBy="SYSTEM",
                BranchId=GetBranchID(),
                BranchName=GetBranchName(),
                ComputerName="",
                Location=location,
                Reason=reason

            };

            try
            {
                var result = _api.PostAsync<ResponseObject<bool>>(APICallHelper.AddRateLimitedUser, command).GetAwaiter().GetResult();
                System.Diagnostics.Debug.WriteLine($"✅ Blocked {ipOrUsername}: {result?.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to block user '{ipOrUsername}': {ex.Message}");
            }
        }



    }

}
