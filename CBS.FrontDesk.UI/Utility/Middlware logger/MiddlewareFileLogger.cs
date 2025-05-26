using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.Utility.Middlware_logger
{


    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR
    }

    public static class AdvancedMiddlewareLogger
    {
        private static readonly BlockingCollection<(string LogEntry, string Username, string BranchName)> LogQueue = new BlockingCollection<(string LogEntry, string Username, string BranchName)>();
        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");
        private static DateTime _lastLogHour = DateTime.Now;
        private static readonly Dictionary<string, string> UserLogFileCache = new Dictionary<string, string>();

        static AdvancedMiddlewareLogger()
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => CleanupOldLogs(TimeSpan.FromDays(7)), TaskCreationOptions.LongRunning);
        }

        public static void Log(string message, LogLevel level = LogLevel.INFO, string username = "anonymous", string branchName = "general")
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            username = (username ?? "anonymous").Trim().ToLowerInvariant().Replace(" ", "_").Replace(".", "_");
            branchName = (branchName ?? "general").Trim().ToLowerInvariant().Replace(" ", "_").Replace(".", "_");
            LogQueue.Add((logEntry, username, branchName));
        }

        private static void ProcessQueue()
        {
            foreach (var (log, username, branchName) in LogQueue.GetConsumingEnumerable(Cts.Token))
            {
                try
                {
                    string logFile = GetHourlyLogFile(username, branchName);
                    File.AppendAllText(logFile, log + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Swallow exceptions silently to avoid crashing background thread
                }
            }
        }

        private static string GetHourlyLogFile(string username, string branchName)
        {
            var now = DateTime.Now;
            string key = $"{branchName}_{username}_{now:HH}";

            if (!UserLogFileCache.ContainsKey(key) || now.Hour != _lastLogHour.Hour)
            {
                UserLogFileCache[key] = GetNewLogFilePath(username, branchName);
                _lastLogHour = now;
            }

            return UserLogFileCache[key];
        }

        private static string GetNewLogFilePath(string username, string branchName)
        {
            var now = DateTime.Now;
            string yearFolder = Path.Combine(LogFolder, now.Year.ToString());
            string dateFolder = Path.Combine(yearFolder, now.ToString("MM_dd"));
            string branchFolder = Path.Combine(dateFolder, branchName);
            string userFolder = Path.Combine(branchFolder, username);

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            string fileName = $"TSC_Console_Middleware_{now:HHmmss}.log";
            return Path.Combine(userFolder, fileName);
        }

        private static void CleanupOldLogs(TimeSpan maxAge)
        {
            while (!Cts.IsCancellationRequested)
            {
                try
                {
                    var yearDirs = Directory.GetDirectories(LogFolder);
                    foreach (var yearDir in yearDirs)
                    {
                        var dateDirs = Directory.GetDirectories(yearDir);
                        foreach (var dateDir in dateDirs)
                        {
                            var branchDirs = Directory.GetDirectories(dateDir);
                            foreach (var branchDir in branchDirs)
                            {
                                var userDirs = Directory.GetDirectories(branchDir);
                                foreach (var userDir in userDirs)
                                {
                                    var files = Directory.GetFiles(userDir, "*.log");
                                    foreach (var file in files)
                                    {
                                        if (File.GetCreationTimeUtc(file) < DateTime.Now.Subtract(maxAge))
                                            File.Delete(file);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Silent failure
                }

                Thread.Sleep(TimeSpan.FromHours(6));
            }
        }

        public static void Shutdown()
        {
            Cts.Cancel();
            LogQueue.CompleteAdding();
        }
    }

}