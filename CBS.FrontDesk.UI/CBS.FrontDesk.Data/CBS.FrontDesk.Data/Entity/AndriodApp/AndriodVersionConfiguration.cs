using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AndriodApp
{
    public class AndriodVersionConfiguration
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string VersionStatus { get; set; }
        public string ApkUrl { get; set; }
        public string AppName { get; set; }
        public string AppSecretcode { get; set; }
        public string AppCode { get; set; }
        public string Environment { get; set; }

    }

   
}
