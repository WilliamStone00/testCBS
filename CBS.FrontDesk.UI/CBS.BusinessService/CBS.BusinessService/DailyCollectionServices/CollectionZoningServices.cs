using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.DailyCollectionServices
{
    public class CollectionZoningServices
    {
        public ZoneServices _zoneServices { get; set; }

        public BranchServices _branchServices { get; set; }

        public CollectionZoningServices()
        {
            _zoneServices = new ZoneServices();
            _branchServices = new BranchServices();
        }

        //public async Task<ExecutionMessages> Create(Zone model)
        //{
        //    try
        //    {
        //        var branch = await _branchServices.GetBranch(model.BranchId);

        //        model.BranchId = branch.Id;
        //        model.RegionId = branch.RegionId;   
        //        model.DivisionId = branch.DivisionId;
        //        model.SubDivisionId = branch.SubDivisionId;
        //        model.TownId = branch.TownId;
        //        model.Id = "NO_ID"; // Set a default ID for creation


        //        if (response.IsSuccess)
        //        {
        //            // Successful creation
        //            GetExecutionMessages(response, true, null, MessagesResults.Success,
        //                ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
        //            return ExecutionMessage;
        //        }
        //        else
        //        {
        //            // Failed creation
        //            GetExecutionMessages(model, false, null, MessagesResults.Failed,
        //                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}
    }
}
