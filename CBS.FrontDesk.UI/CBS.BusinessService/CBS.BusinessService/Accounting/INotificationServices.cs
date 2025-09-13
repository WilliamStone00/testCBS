using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public interface INotificationServices
    {
        Task<List<UsersNotification>> GetUserNotificationRequestFromDB();
        string TimeSince(DateTime date);
    }
}