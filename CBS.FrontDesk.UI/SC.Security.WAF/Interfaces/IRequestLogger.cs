using CBS.FrontDesk.UI.SC.Security.WAF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Interfaces
{
    public interface IRequestLogger
    {
        void Log(WafLogContext context);
    }
}
