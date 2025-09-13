using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core
{
    public static class StaticPathHelper
    {
        private static readonly string[] Prefixes = {
        "/content/", "/scripts/", "/bundles/", "/fonts/",
        "/images/", "/favicon.ico", "/signalr", "/CrystalImageHandler.aspx"
    };

        private static readonly string[] Extensions = {
        ".css",".js",".png",".jpg",".jpeg",".gif",".svg",
        ".ico",".woff",".woff2",".ttf",".map"
    };

        public static bool IsStatic(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            path = path.ToLowerInvariant();
            if (Prefixes.Any(path.StartsWith)) return true;
            var dot = path.LastIndexOf('.');
            if (dot >= 0)
            {
                var ext = path.Substring(dot);
                if (Extensions.Contains(ext)) return true;
            }
            return false;
        }
    }

}