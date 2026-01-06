using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanType
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsSSFexist { get; set; } = false;

        public bool IsSSF { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string Description { get; set; }
    }

}
