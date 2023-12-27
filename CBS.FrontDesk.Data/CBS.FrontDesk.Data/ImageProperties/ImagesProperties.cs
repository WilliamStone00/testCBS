using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ImageProperties
{
    public class ImagesProperties
    {
        public string FileName { get; set; }
        public Guid FileID { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public long FileSize { get; set; }
        public bool ExecuteNow { get; set; }
        public string FullPath { get; set; }
        public string MappedPath { get; set; }
    }
}
