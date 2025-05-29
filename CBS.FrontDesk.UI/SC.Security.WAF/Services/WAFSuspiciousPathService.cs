using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.UI.Utility.Middlware_logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    public class WAFSuspiciousPathService : IWAFSuspiciousPathService
    {
        private readonly SuspiciousPathService _suspiciousPathService = new SuspiciousPathService();
        /// <summary>
        /// Checks if the path matches any suspicious pattern (plain or regex).
        /// </summary>
        public async Task<bool> CheckIfSuspiciousPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            path = path.ToLowerInvariant();
            return await _suspiciousPathService.CheckIfSuspiciousPath(path);
        }

        /// <summary>
        /// Determines if the path is a static asset and can be excluded from inspection.
        /// </summary>
        public bool IsSafeStaticAsset(string path, string globalUserFullname, string globalbranchname)
        {
            var lower = path.ToLowerInvariant();

            string[] suspiciousIndicators = new[]
            {
                ".env", ".git", ".svn", ".hg", ".bak", ".old", ".backup",
                "web.config", "application.yml", "application.yaml", "settings.py",
                "config.php", "database.yml", "composer.json", "package.json",
                "requirements.txt", ".htaccess", ".htpasswd",
                "id_rsa", "id_dsa", "private.key", "access_token", "jwt", "secret",
                "phpmyadmin", "pma", "adminer", "dbadmin", "wp-admin", "wp-login",
                "cpanel", "login.jsp", "login.php", "dashboard.jsp", "dashboard.php",
                "passwd", "shadow", "boot.ini", "hosts", "system.ini",
                "windows/win.ini", "etc/passwd", "etc/shadow",
                "shell.php", "backdoor.php", "test.php", "eval.php", "mailer.php",
                "cmd.php", "rce.php", "upload.php", "exploit.php", "drupal", "magento",
                "owa", "ecp", "autodiscover", "activesync", "exchange", "server-status",
                ".php", ".jsp", ".asp", ".aspx", ".cgi", ".exe", ".sh", ".pl", ".py", ".rb", ".lua",
                ".log", ".sql", ".zip", ".tar.gz", ".7z", ".gz", ".tgz", ".rar",
                ".idea", ".vscode", ".dockerignore", "docker-compose", ".DS_Store",
                "crossdomain.xml", "clientaccesspolicy.xml", ".well-known", "favicon.ico.php"
            };

            bool isSuspicious = suspiciousIndicators.Any(ind => lower.Contains(ind));

            string logMsg = $"🔍 Checked static path: {path} | Result: {(isSuspicious ? "Suspicious" : "Safe")}";
            System.Diagnostics.Debug.WriteLine(logMsg);
            AdvancedMiddlewareLogger.Log(logMsg, LogLevel.INFO, globalUserFullname, globalbranchname);

            return !isSuspicious; // TRUE = safe, FALSE = suspicious
        }

    }


}