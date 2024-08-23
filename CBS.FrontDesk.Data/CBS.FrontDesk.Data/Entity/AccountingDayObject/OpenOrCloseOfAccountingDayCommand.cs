using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{

    public class OpenOrCloseOfAccountingDayCommand
    {
        [Required(ErrorMessage = "The Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "At least one branch must be specified.")]
        //[MinLength(1, ErrorMessage = "The Branches list must contain at least one branch.")]
        public List<BranchListing> Branches { get; set; }

        [Required(ErrorMessage = "IsCentraliseOpening is required.")]
        public bool IsCentraliseOpening { get; set; }

        [Required(ErrorMessage = "The action (Open or Close) must be specified.")]
        [RegularExpression("Open|Close", ErrorMessage = "The action must be either 'Open' or 'Close'.")]
        public string OpenOrCloseAccountingDay { get; set; }
    }



}
