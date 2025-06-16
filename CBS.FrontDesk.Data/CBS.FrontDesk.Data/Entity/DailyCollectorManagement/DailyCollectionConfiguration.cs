
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectorManagement
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
    public abstract class ResourceParameter
    {
        public ResourceParameter(string OrderBy)
        {
            this.OrderBy = OrderBy;

        }

        const int MaxPageSize = 100;
        public int Skip { get; set; } = 0;

        private int _PageSize = 10;
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {

                _PageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        public string SearchQuery { get; set; }
        public string OrderBy { get; set; }


    }
    public class PagginationResource : ResourceParameter
    {
        public PagginationResource() : base("CustomerId")
        {
        }
        public string BranchId { get; set; }
        public bool IsByBranch { get; set; }
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

    public class DailyCollectionConfiguration
    {

        public Zone Zone { get; set; }= new Zone();
        public List<Zone > Zones { get; set; } = new List<Zone >();
        public CommissionSetting CommissionSetting { get; set; } = new CommissionSetting();
        public List<CommissionSetting > CommissionSettings { get; set; } = new List<CommissionSetting> ();
        public AgentDailyCashLimit AgentDailyCashLimit { get; set; } = new AgentDailyCashLimit();
        public List<AgentDailyCashLimit> AgentDailyCashLimits { get; set; } = new List<AgentDailyCashLimit>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }
    public class DailyAgentManagement
    {

        public Agent Agent { get; set; }
        public List<Agent> Agents { get; set; } = new List<Agent>();
        public AgentAccount AgentAccount { get; set; }
        public List<AgentAccount> AgentAccounts { get; set; }
        public DailyCustomer DailyCustomer { get; set; }
        public List<DailyCustomer> DailyCustomers { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
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
