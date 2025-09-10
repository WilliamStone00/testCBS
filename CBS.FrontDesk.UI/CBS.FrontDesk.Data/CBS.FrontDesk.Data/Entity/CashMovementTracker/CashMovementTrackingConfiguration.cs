using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CashMovementTracker
{

    public class CashMovementConfiguration
    {
        //

        //
        public CashMovementTrackingConfiguration CashMovementTrackingConfiguration { get; set; } = new CashMovementTrackingConfiguration();
        public List<CashMovementTrackingConfiguration> CashMovementTrackingConfigurationData { get; set; } = new List<CashMovementTrackingConfiguration>();
        public List<CashMovementDataStatus> CashMovementDataStatus { get; set; } = new List<CashMovementDataStatus>();
        public  CashMovementTracker CashMovementTracker { get; set; } = new CashMovementTracker();
        public List<CashReplenimentRequestDto> CashReplenimentRequestDtos { get; set; } = new List<CashReplenimentRequestDto>();
        public List<DepositNotificationDto> DepositNotificationDto = new List<DepositNotificationDto>();
        // public List<CashReplenimentRequestDto> requestDtos = new List<CashReplenimentRequestDto>();
        public List<CashMovement> CashMovementDtos = new List<CashMovement>();
        public string ServiceOption { get; set; }

        public string Action { get; set; }
    }
    public class CashMovementDataStatus
    {
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string MovementType { get; set; }
        public string Destination { get; set; }
        public decimal Amount { get; set; }
        public string DoneBy { get; set; }
        public string DoneAt { get; set; }
        public string ExpiresAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class CashMovement
    {
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string OperationType { get; set; }
        public string RequestedBy { get; set; }
        public string Amount { get; set; }
        public string ApprovedBy { get; set; }
      
    }

    public class CashMovementTrackingConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MovementType { get; set; }
        public string Duration { get; set; }
        public string Distance { get; set; }
        public string BankingZoneId { get; set; }
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
        public string CreatedBy { get; set; }
    }
}
