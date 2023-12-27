using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class SubDivision
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string DivisionId { get; set; } // Foreign key
        public Division Division { get; set; }
        public List<Town> Towns { get; set; }
    }

}
