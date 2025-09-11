using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class ModelExpenses
    {
 
        public int Id { get; set; }
        public string Heading { get; set; }
        public string Reference { get; set; }

        public string Net_N { get; set; }
        public string Net_N_minus_1 { get; set; }
        public string filename { get; set; }


    }
    public class ModelExpensesServiceResponse
    {
        public List<ModelExpenses> Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
 
}
