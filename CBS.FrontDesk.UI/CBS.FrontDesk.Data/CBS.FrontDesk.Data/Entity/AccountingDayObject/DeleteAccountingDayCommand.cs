using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{
    public class DeleteAccountingDayCommand
    {
        public string Id { get; set; }
        public bool DelateByDate { get; set; }
        public DateTime Date { get; set; }
    }
}
