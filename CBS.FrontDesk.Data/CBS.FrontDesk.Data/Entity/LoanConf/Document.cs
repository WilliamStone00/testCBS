using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Document
    {
        public string Id { get; set; }
        [Required]
        public string DocumentType { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string LinkDoc { get; set; }
        public ICollection<LoanApplication> LoanApplication { get; set; }
    }

}
