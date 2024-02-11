using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Organization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CountryId { get; set; }
        public Country Country { get; set; }
        public List<Bank> Banks { get; set; }
    }

}
