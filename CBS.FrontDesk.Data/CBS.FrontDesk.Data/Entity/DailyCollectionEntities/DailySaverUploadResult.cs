using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{

    public class DailySaverUploadResult
    {

        public string BranchName { get; set; }
        public string CollectorName { get; set; }
        public int NumberOfSaversUploaded { get; set; }
        public string LinkOfSaversUploaded { get; set; }

    }
}
