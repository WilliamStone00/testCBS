using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeNumber
{
    public class VariablePosition : BaseEntity
    {
        public string Id { get; set; }              
        public string ConfigId { get; set; }  // FK vers CheckConfig
        public int Order { get; set; }     // Position dans l'identifiant
        public string VariableName { get; set; } = null; // ex: "BankCode", "Year"
    }
}
