using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.Data.Entity
{
    public class StringValues
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public SelectList SelectListItems { get; set; }

        public StringValues()
        {
                
        }
        public StringValues(string text, string value )
        {
            Text = text;
            Value = value;


        }
    }
}
