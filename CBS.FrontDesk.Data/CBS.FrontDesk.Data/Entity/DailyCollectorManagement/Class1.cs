
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

    public class Zone  
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }
        public string RegionId { get; set; }
        public string DivisionId { get; set; }
        public string SubDivisionId { get; set; }
        public string TownId { get; set; }

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

    public class Agent : BaseEntity
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
        public AgentResouceInformation ResouceInformation { get; set; }
    }

    public class AgentResouceInformation
    {

        public string Recto { get; set; }

        public string Verso { get; set; }
        public string Signature { get; set; } //= GenderType.Male.ToString();
        public string ProfilePicture { get; set; }

    }


    public class AgentAccount : BaseEntity
    {
        public string Id { get; set; }
        public double AccountBalance { get; set; }
        public string AgentId { get; set; }
        public string BranchId { get; set; }
    }

    public class CommissionSetting : BaseEntity
    {
        public string Id { get; set; }
        public string AgentShare { get; set; }

        public string BranchShare { get; set; }
        public string BranchId { get; set; }
        public string AgentId { get; set; }
        public bool Isglobal { get; set; }
        public bool IsBranch { get; set; }
    }
}
