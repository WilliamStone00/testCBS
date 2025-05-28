using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{

    public class AddOrUpdateIdleTimeCommand
    {
        public string Id { get; set; } = string.Empty;
        public TimeSpan IdleDuration { get; set; }
        public bool IsCentral { get; set; }
        public int NumberOfLoginAttemps { get; set; }
        public int NumberOfDaysRequiredToResetPassword { get; set; }
        public int NumberOfDaysToBlockUserIfInactive { get; set; }
        public Dictionary<string, string> PasswordPolicies { get; set; } = new Dictionary<string, string>();
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

    }
    public class IdleTime
    {
        public string Id { get; set; }

        // 🔒 Session & Access Control
        public TimeSpan IdleDuration { get; set; }
        public bool IsCentral { get; set; }
        public int NumberOfLoginAttemps { get; set; }
        public int NumberOfDaysRequiredToResetPassword { get; set; }
        public int NumberOfDaysToBlockUserIfInactive { get; set; }

        // 🔐 Password and Security Policies (excluding duplicates)
        public Dictionary<string, string> PasswordPolicies { get; set; } = new Dictionary<string, string>();

        // 🏢 Branch Metadata
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Createdby { get; set; }
        public AddOrUpdateIdleTimeCommand AddOrUpdateIdleTimeCommand { get; set; } = new AddOrUpdateIdleTimeCommand();
        /// <summary>
        /// JSON editor-bound property for password policy configuration.
        /// Used by ACE editor and auto-binds to Dictionary upon form submission.
        /// </summary>
        [NotMapped]
        [Display(Name = "Password Policies (JSON Format)")]
        public string PasswordPoliciesJson
        {
            get => JsonConvert.SerializeObject(
                PasswordPolicies == null || PasswordPolicies.Count == 0
                    ? GetDefaultPolicies()
                    : PasswordPolicies,
                Formatting.Indented);

            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        PasswordPolicies = JsonConvert.DeserializeObject<Dictionary<string, string>>(value)
                                           ?? GetDefaultPolicies();
                    }
                    catch
                    {
                        PasswordPolicies = GetDefaultPolicies(); // fallback to default
                    }
                }
                else
                {
                    PasswordPolicies = GetDefaultPolicies(); // fallback if empty
                }
            }
        }
        public string Action { get; set; }
        /// <summary>
        /// Provides default password policy rules to enforce security best practices.
        /// These defaults are used if no admin configuration is found.
        /// </summary>
        public static Dictionary<string, string> GetDefaultPolicies() => new Dictionary<string, string>
    {
        // 🧱 Complexity Rules
        { "MinLength", "8" },
        { "MaxLength", "128" },
        { "RequireUppercase", "true" },
        { "RequireLowercase", "true" },
        { "RequireDigit", "true" },
        { "RequireSpecialChar", "true" },
        { "AllowedSpecialChars", "!@#$%^&*()_+-=[]{}|;:,.<>?" },
        { "DisallowUsernameInPassword", "true" },

        // ♻️ History & Reuse
        { "PasswordHistoryCount", "5" },
        { "DisallowPreviousPasswords", "true" },

        // 🔐 MFA
        { "EnableMFA", "true" },
        { "MFATypesAllowed", "SMS,EMAIL,TOTP" },
        { "MFAEnforcementGraceDays", "3" },

         // 🚫 Disallowed Patterns
        { "DisallowedSubstrings", "password,admin,login,1234,qwerty,abcd,root,test,guest,access,letmein,welcome,secret,iloveyou,trustno1,dragon,2020,2021,0000" },
        { "RegexPatternValidation", "^(?!.*(.)\\1{2,})(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@#$%^&+=!?]).{12,128}$" },

        // 🛠 Admin Controls
        { "AllowAdminReset", "true" },
        { "RejectSerialPatterns", "true" },
        { "AdminResetBypassHistory", "false" }
    };
    }

}
