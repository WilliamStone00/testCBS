using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class SuspiciousPath
    {
        public string Id { get; set; }

        /// <summary>
        /// The path or pattern to match against incoming requests.
        /// Required field. Cannot be empty or whitespace. Maximum length: 500 characters.
        /// Example: "/wp-login.php", "^/app_dev\\.php.*"
        /// </summary>
        [Required(ErrorMessage = "The path pattern is required.")]
        [StringLength(500, ErrorMessage = "The path pattern must not exceed 500 characters.")]
        public string Pattern { get; set; }

        /// <summary>
        /// Indicates whether the pattern should be treated as a regular expression.
        /// Required field. Defaults to false.
        /// </summary>
        [Required(ErrorMessage = "You must specify whether the pattern is a regex.")]
        public bool IsRegex { get; set; }

        public DateTime Timestamp { get; set; }

        public string CreatedBy { get; set; }

        public string Action { get; set; }
    }
    public class IsSuspiciousPathQuery
    {
        public string Path { get; set; }

        public IsSuspiciousPathQuery(string path)
        {
            Path = path;
        }
    }
    public class AddSuspiciousPathCommand
    {
        public string Pattern { get; set; }
        public bool IsRegex { get; set; }
    }
    public class UpdateSuspiciousPathCommand
    {
        public string Id { get; set; }
        public string Pattern { get; set; }
        public bool IsRegex { get; set; }
    }
}
