using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config.System
{
    public class SysConfiguration
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }
        public bool IsYearOpen { get; set; }
        public bool IsDayOpen { get; set; }
        public bool IsDayClosedAutomatic { get; set; }
        public bool MondayIsHoliday { get; set; }
        public bool UseAutomaticChargingSystem { get; set; }
        public bool TuesDayIsHoliday { get; set; }
        public bool MWednessdayIsHoliday { get; set; }
        public bool ThursdayIsHoliday { get; set; }
        public bool FridayIsHoliday { get; set; }
        public bool SaturdayIsHoliday { get; set; }
        public bool SundayIsHoliday { get; set; }
        public DateTime SetCloseDayTime { get; set; }
        public string LastModifiedOperation { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
    }
}
