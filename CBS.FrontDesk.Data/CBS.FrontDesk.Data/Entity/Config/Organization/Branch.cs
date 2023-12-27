using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Branch
    {
        public string Id { get; set; }
        public string BranchCode { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string BankId { get; set; } // Foreign key
        public Bank Bank { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Town> Towns { get; set; }
    }

}
