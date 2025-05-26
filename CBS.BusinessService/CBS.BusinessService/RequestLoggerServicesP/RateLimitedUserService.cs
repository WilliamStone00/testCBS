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
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Helper.Helper;
using CBS.BusinessService.UserManagement;
using MongoDB.Driver.Linq;
using DocumentFormat.OpenXml.EMMA;

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
        private readonly BranchServices _branchServices;
        private readonly UserManagementServices _userManagementServices;
        public RateLimitedUserService()
        {
            var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("❌ IdentityServerBaseUrl is missing from AppSettings.");
            _branchServices=new BranchServices();
            _userManagementServices=new UserManagementServices();
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
                var response = await _identityServerBaseUrl.DeleteAsync<ServiceResponse<bool>>(
                    string.Format(APICallHelper.Get_Delete_RateLimitedUser, id));

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
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<List<RateLimitedUser>>>(
                    APICallHelper.GetAllRateLimitedUsers);

                return response?.ApiResponseData.Data ?? new List<RateLimitedUser>();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve all RateLimitedUser records", ex);
            }
        }
        public async Task<bool> CheckIsBlocked(CheckRateLimitBlockQuery rateLimitBlockQuery)
        {
            try
            {
                var response = await _api.PostAsync<ResponseObject<bool>>(
                    APICallHelper.CheckRateLimitBlock, rateLimitBlockQuery
                );

                return response.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Block check failed for '{rateLimitBlockQuery.IpOrUser} {rateLimitBlockQuery.Username}': {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> CheckIsBlocked(CheckRateLimitBlockQuery rateLimitBlockQuery)
        //{
        //    try
        //    {

        //        var response = await _api.PostAsync<ResponseObject<bool>>(APICallHelper.AddRateLimitedUser, rateLimitBlockQuery);
        //        return response.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"⚠️ Block check failed for '{rateLimitBlockQuery.IpOrUser} {rateLimitBlockQuery.Username}': {ex.Message}");
        //        return false;
        //    }
        //}

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
        //,string branchid,string branchcode,string branchname,string tel,string fullname
        public async Task BlockUser(string ipOrUsername, string userName, string blockType, TimeSpan blockDuration, string reason, string location, string lat, string lon, string city, string region, string country, string branchid, string branchcode, string branchname, string tel, string fullname)
        {
            var command = new BlockUserCommand
            {
                IpAddress = ipOrUsername,
                UserName = userName,
                BlockType = blockType,
                BlockEndTime = DateTime.UtcNow.Add(blockDuration),
                BlockedBy="SYSTEM",
                ComputerName="",
                BranchId=branchid,
                BranchName=branchname,
                Location=location,
                Reason=reason,
                FullName=fullname,
                BranchCode=branchcode,
                City=city,
                Country=country,
                Latitude=lat,
                Longitude=lon,
                PhoneNumber=tel,
                Region=region

            };

            try
            {
                var result = await _api.PostAsync<ResponseObject<bool>>(APICallHelper.AddRateLimitedUser, command);
                System.Diagnostics.Debug.WriteLine($"✅ Blocked {ipOrUsername}: {result?.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to block user '{ipOrUsername}': {ex.Message}");
            }
        }


        public async Task<ExecutionMessages> Add(BlockRequest blockRequest)
        {
            if (!GeolocalizationHelper.IsPublicIp(blockRequest.UserIP))
            {
                    GetExecutionMessages(
                  null,
                  false,
                  null,
                  MessagesResults.Failed,
                  ExecutionProcessOption.DefaultFailedMessages,
                  SystemMessageStatus.Failed.ToString(),
                  null,
                  "The IP address provided is not a public address. Please enter a valid public IPv4 or IPv6 address that is accessible over the internet. Private or internal addresses (e.g., 192.168.x.x, 10.x.x.x, 127.0.0.1) are not allowed for blacklist registration.");
                return ExecutionMessage;
            }

            var user = await _userManagementServices.GetUser(blockRequest.UserId);
            var branch = await _branchServices.GetBranch(user.BranchID);
            var (ip, location, lat, lon, city, region, country) = GeolocalizationHelper.GetIpAndLocationSync();
            var blockUserCommand = new BlockUserCommand { BranchId=branch.Id, BranchCode=branch.BranchCode, BranchName=branch.Name, BlockedBy=GetUserFullName(), BlockEndTime=DateTime.Now.AddDays(blockRequest.NumberOfDays), BlockType="Manual", City=city, ComputerName="-", Country=country, FullName=$"{user.firstName} {user.lastName}", IpAddress=blockRequest.UserIP, Latitude=lat, Location=location, Longitude=lon, PhoneNumber=user.phoneNumber, Reason=blockRequest.Reason, Region=region, UserName=user.userName };

            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ResponseObject<bool>>(APICallHelper.AddRateLimitedUser, blockUserCommand);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }



    }

}
