

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class RequestLoggerService
    {
        private static readonly ConcurrentDictionary<string, LogEntry> RequestLogs = new ConcurrentDictionary<string, LogEntry>();
        private static readonly ConcurrentDictionary<string, LogEntry> WarningLogs = new ConcurrentDictionary<string, LogEntry>();
        private static readonly ConcurrentDictionary<string, LogEntry> BlockedUserLogs = new ConcurrentDictionary<string, LogEntry>();

        /// <summary>
        /// Logs a general request.
        /// </summary>
        public void LogRequest(string ip, string mac, string computerName, string path, int statusCode, string location)
        {
            var key = $"{ip}-{Guid.NewGuid()}";
            var requestData = new LogEntry
            {
                IpAddress = ip,
                MacAddress = mac,
                ComputerName = computerName,
                Path = path,
                StatusCode = statusCode,
                Location = location,
                Timestamp = DateTime.UtcNow
            };

            RequestLogs[key] = requestData;
        }

        /// <summary>
        /// Logs a warning when the 80% threshold is reached.
        /// </summary>
        public void LogWarning(string ip, string mac, string computerName, string path, string warningMessage)
        {
            var key = $"{ip}-Warning";
            var warningData = new LogEntry
            {
                IpAddress = ip,
                MacAddress = mac,
                ComputerName = computerName,
                Path = path,
                WarningMessage = warningMessage,
                Timestamp = DateTime.UtcNow
            };

            WarningLogs[key] = warningData;

        }

        /// <summary>
        /// Logs a blocked user request.
        /// </summary>
        public void LogBlockedUser(string ip, string mac, string computerName, DateTime blockEndTime, string blockType, string location)
        {
            var key = $"{ip}-Blocked";
            var blockedData = new LogEntry
            {
                IpAddress = ip,
                MacAddress = mac,
                ComputerName = computerName,
                BlockType = blockType,
                BlockEndTime = blockEndTime,
                Location = location,
                Timestamp = DateTime.UtcNow
            };

            BlockedUserLogs[key] = blockedData;
        }

        /// <summary>
        /// Retrieves the last N request logs.
        /// </summary>
        public List<LogEntry> GetRecentRequests(int count = 10)
        {
            return RequestLogs.Values
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Retrieves the last N warnings.
        /// </summary>
        public List<LogEntry> GetRecentWarnings(int count = 10)
        {
            return WarningLogs.Values
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Retrieves the last N blocked user logs.
        /// </summary>
        public List<LogEntry> GetRecentBlockedUsers(int count = 10)
        {
            return BlockedUserLogs.Values
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Log Entry Model
    /// </summary>
    public class LogEntry
    {
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string ComputerName { get; set; }
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public string Location { get; set; }
        public string WarningMessage { get; set; }
        public string BlockType { get; set; }
        public DateTime BlockEndTime { get; set; }
        public DateTime Timestamp { get; set; }
    }

    //public class RequestLoggerService
    //{
    //    private const string ApiEndpoint = "https://localhost:44346/api/log";

    //    /// <summary>
    //    /// Logs a general request.
    //    /// </summary>
    //    public void LogRequest(string ip, string mac, string computerName, string path, int statusCode, string location)
    //    {
    //        var requestData = new
    //        {
    //            IpAddress = ip,
    //            MacAddress = mac,
    //            ComputerName = computerName,
    //            Location = location,
    //            Path = path,
    //            StatusCode = statusCode,
    //            Timestamp = DateTime.UtcNow
    //        };

    //        SendToApi("request-log", requestData);
    //    }

    //    /// <summary>
    //    /// Logs a blocked user request.
    //    /// </summary>
    //    public void LogBlockedUser(string ip, string mac, string computerName, DateTime blockEndTime, string blockType, string location)
    //    {
    //        var blockedData = new
    //        {
    //            IpAddress = ip,
    //            MacAddress = mac,
    //            ComputerName = computerName,
    //            Location = location,
    //            BlockType = blockType,  // User-Based or IP-Based
    //            BlockEndTime = blockEndTime,
    //            Timestamp = DateTime.UtcNow
    //        };

    //        SendToApi("blocked-users", blockedData);
    //    }

    //    /// <summary>
    //    /// Logs a warning when 80% threshold is reached.
    //    /// </summary>
    //    public void LogWarning(string ip, string mac, string computerName, string path, string warningMessage)
    //    {
    //        var warningData = new
    //        {
    //            IpAddress = ip,
    //            MacAddress = mac,
    //            ComputerName = computerName,
    //            Path = path,
    //            WarningMessage = warningMessage,
    //            Timestamp = DateTime.UtcNow
    //        };

    //        SendToApi("warning-log", warningData);
    //    }

    //    /// <summary>
    //    /// Sends data to the specified API endpoint.
    //    /// </summary>
    //    private void SendToApi(string endpoint, object data)
    //    {
    //        //using (var client = new HttpClient())
    //        //{
    //        //    try
    //        //    {
    //        //        var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
    //        //        var result = client.PostAsync($"{ApiEndpoint}/{endpoint}", jsonContent).Result;

    //        //        if (!result.IsSuccessStatusCode)
    //        //        {
    //        //            System.Diagnostics.Debug.WriteLine($"Error logging to API: {result.StatusCode} - {result.ReasonPhrase}");
    //        //        }
    //        //    }
    //        //    catch (Exception ex)
    //        //    {
    //        //        System.Diagnostics.Debug.WriteLine($"Error sending to API: {ex.Message}");
    //        //    }
    //        //}
    //    }
    //}
}
