using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Currency
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CountryID { get; set; }
        public Country Country { get; set; }
    }

}
