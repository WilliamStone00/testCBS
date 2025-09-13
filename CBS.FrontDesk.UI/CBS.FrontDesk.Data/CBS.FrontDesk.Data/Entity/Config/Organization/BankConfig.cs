using CBS.FrontDesk.Data.Entity.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class BankConfig
    {
        public Bank Bank { get; set; }=new Bank();
        public Branch Branch { get; set; }=new Branch();
        public Oranization Oranization { get; set; } = new Oranization();
        public List<Bank> Banks { get; set; }= new List<Bank>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public List<Oranization> Oranizations { get; set; } = new List<Oranization>();
        public CustomerDocumentRequest CustomerDocumentRequest { get; set; } = new CustomerDocumentRequest();
        public string Action { get; set; }
        public string ServiceOption { get; set; }
    }
}
