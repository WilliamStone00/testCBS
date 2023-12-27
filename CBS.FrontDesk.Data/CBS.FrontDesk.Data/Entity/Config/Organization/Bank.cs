using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Bank
    {
        public string Id { get; set; }
        public string BankCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string OrganizationId { get; set; } // Foreign key
        public Organization Organization { get; set; }
        public List<Branch> Branches { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Town> Towns { get; set; }
    }

}
