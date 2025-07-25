
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity 
{
    public class DailyCollectorManagementData
    {
        public AgentDto AgentDto { get; set; }

        public List<AgentDto> AgentDtos { get; set; }
        public Zone Zone { get; set; }

        public List<Zone> Zones { get; set; }
        public Agent Agent { get; set; } = new Agent();
        public AgentAccount AgentAccount { get; set; }
 
        public List<AgentAccount> AgentAccounts { get; set; }
 
        public List<Agent> Agents { get; set; }
        public List<CommissionSetting> CommissionSettings { get; set; }
        public CommissionSetting CommissionSetting { get; set; }
 

        public string option { get; set; }
    
    }
  


    public class AgentDto  
    {
        public string Id { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        public string CNI { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }
        public string Address { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string ZoneName { get; set; }
        public string Recto { get; set; }

        public string Verso { get; set; }
        public string Signature { get; set; } //= GenderType.Male.ToString();
        public string ProfilePicture { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate{ get; set; }
        public string ImageVirtualPath { get; private set; }
        public string ImageVirtualSignaturePath { get; private set; }
        public string ImageNoVirtualPath { get; private set; }
        public string ImageVirtualNoSignaturePath { get; private set; }

        public AgentDto()
        {
            ImageVirtualPath = "~/AppFiles/Images/p.jpg";
            ImageVirtualSignaturePath = "~/AppFiles/Images/s.jpg";
            ImageNoVirtualPath = "~/AppFiles/Images/noimage.jpg";
            ImageVirtualNoSignaturePath = "~/AppFiles/Images/no signature.png";
        }
    }

    public class DailyCollectorInfo
    {

        public string userId { get; set; }
     
        public string name { get; set; }

    }
    public class CollectorSalaryInfo
    {
        // 👤 Collector Info
        public string CollectorId { get; set; }
        // 🏢 Branch Info
        public string BranchId { get; set; }
        // 📅 Period & Operation Context
        public string Month { get; set; }    // Format: YYYY-MM
        public string OperationType { get; set; }           // e.g. "CashIn", "LoanRepayment", "OnboardingFee"
        public string MemberReference { get; set; }

    }
    public class DailyCollectionConfiguration
    {

        public Zone Zone { get; set; }= new Zone();
        public CollectorSalaryInfo CollectorSalarySummary { get; set; } = new CollectorSalaryInfo();
        public DailyCollectionDashboardActivitiesQuery  DashboardActivities  { get; set; } = new DailyCollectionDashboardActivitiesQuery();
        public CollectorSalarySummaryDto CollectorData { get; set; } = new CollectorSalarySummaryDto();
        public List<Zone > Zones { get; set; } = new List<Zone >();
        public CommissionSetting CommissionSetting { get; set; } = new CommissionSetting();
        public List<CommissionSetting > CommissionSettings { get; set; } = new List<CommissionSetting> ();
        public AgentDailyCashLimit AgentDailyCashLimit { get; set; } = new AgentDailyCashLimit();
        public List<AgentDailyCashLimit> AgentDailyCashLimits { get; set; } = new List<AgentDailyCashLimit>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }
    public class DailyCollectionDashboardActivitiesQuery
    {
        public string CollectorId { get; set; }
        public string Month { get; set; }

        public string BranchId { get; set; }

        public DailyCollectionActivitiesQuery ConvertToDailyCollectionActivitiesQuery()
        {
            var model = new DailyCollectionActivitiesQuery();
            model.CollectorId = CollectorId;
            model.BranchId = BranchId;
            // Parse Month format: YYYY-MM
            if (!string.IsNullOrEmpty(this.Month))
            {
                var monthParts = this.Month.Split('-');
                if (monthParts.Length == 2)
                {
                    if (int.TryParse(monthParts[0], out int year) && int.TryParse(monthParts[1], out int month))
                    {
                        model.Year = year;
                        model.Month = month;
                    }
                    else
                    {
                        throw new FormatException($"Invalid month format: {this.Month}. Expected format: YYYY-MM");
                    }
                }
                else
                {
                    throw new FormatException($"Invalid month format: {this.Month}. Expected format: YYYY-MM");
                }
            }
            return model;
        }
    }
    public class DailyCollectionActivitiesQuery
    {
        public string CollectorId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string BranchId { get; set; }
    }

      //  public async Task<ActionResult> UploadDailyCollectorModel(UploadDailyCollectorData model)
    //
    public class DailyAgentManagement
    {
        public UploadDailyCollectorData UploadDailyCollectorData { get; set; }
      
        public Agent Agent { get; set; }
        public List<Agent> Agents { get; set; } = new List<Agent>();
        public AgentAccount AgentAccount { get; set; }
        public List<AgentAccount> AgentAccounts { get; set; }
        public DailyCustomer DailyCustomer { get; set; }
        public List<DailyCustomer> DailyCustomers { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
        public string BranchId { get; set; }
    }
    public class AgentResouceInformation
    {

        public string Recto { get; set; }

        public string Verso { get; set; }
        public string Signature { get; set; } //= GenderType.Male.ToString();
        public string ProfilePicture { get; set; }

    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Resource
    {
        public string personInformation { get; set; }
        public List<string> personDocument { get; set; }
        public string resourceType { get; set; }
        public string agentId { get; set; }
    }




}
