using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CashMovementTracker
{

    public class CashMovementConfiguration
    {
        public CashMovementTrackingConfiguration CashMovementTrackingConfiguration { get; set; } = new CashMovementTrackingConfiguration();
        public List<CashMovementTrackingConfiguration> CashMovementTrackingConfigurationData { get; set; } = new List<CashMovementTrackingConfiguration>();
        public string ServiceOption { get; set; }

        public string Action { get; set; }
    }
    public class CashMovementTrackingConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MovementType { get; set; }
        public string Duration { get; set; }

        public string AlertTimeBefore { get; set; }
        public string MessageBeforeAlertTime { get; set; }
        public string AlertTimeAfter { get; set; }
        public string MessageAfterAlertTime { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
    public enum CashMovementTrackerStatus
    {
        Open,
        Ongoing,
        Close,
        UnderTracking
    }
    public class CashMovementTracker
    {
        public string Id { get; set; }
        public string OperationType { get; set; }
        public string ReferenceId { get; set; }
        public string Constraint { get; set; }
        public string DoneBy { get; set; }

        public string Status { get; set; }//Open,Ongoing,Close,UnderTracking
        public string StartTime { get; set; }
        public string ExpectedEndTime { get; set; }
        public string EndTime { get; set; }
        public string CashMovementTrackingConfigurationId { get; set; }

    }
}
