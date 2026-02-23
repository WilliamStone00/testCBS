using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
     public static class DateHelper
        {
            // =========================
            // DATE ONLY
            // =========================
            public static string FormatDate(DateTime date)
            {
                return date != DateTime.MinValue
                    ? date.ToString("dd/MM/yyyy")
                    : string.Empty;
            }

            public static string FormatNullableDate(DateTime? date)
            {
                return date.HasValue && date.Value != DateTime.MinValue
                    ? date.Value.ToString("dd/MM/yyyy")
                    : string.Empty;
            }

            // =========================
            // DATE + TIME
            // =========================
            public static string FormatDateTime(DateTime date)
            {
                return date != DateTime.MinValue
                    ? date.ToString("dd/MM/yyyy HH:mm:ss")
                    : string.Empty;
            }

            public static string FormatNullableDateTime(DateTime? date)
            {
                return date.HasValue && date.Value != DateTime.MinValue
                    ? date.Value.ToString("dd/MM/yyyy HH:mm:ss")
                    : string.Empty;
            }
        }
    }

