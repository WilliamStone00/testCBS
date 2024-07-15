using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountPolicy
    {
        [Required]
        public string AccountId { get; set; }
        [Required]
        public decimal MaximumAlert { get; set; }
        [Required]
        public decimal MinimumAlert { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string MinMessage { get; set; }
        [Required]
        public string MaxMessage { get; set; }

        public string Id { get; set; }
    }
}
