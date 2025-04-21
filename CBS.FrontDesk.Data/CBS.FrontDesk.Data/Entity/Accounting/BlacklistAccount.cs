using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BlacklistAccount
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string Description { get; set; }
    }
    public class BlacklistedAccount
    {
        public List<string> Id { get; set; }
        
    }
}