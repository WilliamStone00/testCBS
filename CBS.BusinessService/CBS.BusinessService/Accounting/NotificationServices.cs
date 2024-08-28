using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Accounting;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class NotificationServices : INotificationServices
    {
        private readonly INotificationServices _notificationServices;
        private readonly IAccountingEntryServices _accountingEntryServices;
        private readonly IBranchServices _branchServices;
        private readonly IUserManagementServices _userServices;

        public NotificationServices(IAccountingEntryServices accountingEntryServices, IBranchServices branchServices, IUserManagementServices userServices)
        {
            _accountingEntryServices = accountingEntryServices;
            _branchServices = branchServices;
            _userServices = userServices;
        }
        private readonly IHubContext _hubContext;

        public NotificationServices()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        }

        public void SendNotification(string message)
        {
            _hubContext.Clients.All.ReceiveNotification(message);
        }


        public async Task<List<UsersNotification>> GetUserNotificationRequestFromDB()
        {
            try
            {
                var ModelRequest = await _accountingEntryServices.GetUserNotificationRequest();
                var UsersModel = await _branchServices.GetBranches();
                var Users = await _userServices.GetUsers();

                var joinedQuery = from notification in ModelRequest
                                  join branch in UsersModel on notification.BranchId equals branch.Id into branchJoin
                                  from branch in branchJoin.DefaultIfEmpty()
                                  join user in Users on notification.UserId equals user.id.ToString() into userJoin
                                  from user in userJoin.DefaultIfEmpty()
                                  select new UsersNotification
                                  {
                                      Id = notification.Id,
                                      Action = notification.Action,
                                      ActionId = notification.ActionId,
                                      ActionUrl = string.Format(notification.ActionUrl, notification.ActionId),
                                      Timestamp = TimeSince(notification.CreatedDate),
                                      IsActive = notification.IsActive,
                                      IsSeen = notification.IsSeen,
                                      BranchId = notification.BranchId,
                                      UserId = notification.UserId,
                                      BranchName = branch != null ? branch.Name : "Unknown Branch",
                                      UserName = user != null ? $"{user.firstName} {user.lastName} ({user.phoneNumber})" : "Unknown User"
                                  };

                var result = joinedQuery.ToList(); // or await joinedQuery.ToListAsync() if using EF Core

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching notifications from DB: " + ex.Message);
                throw;
            }
        }
        public string TimeSince(DateTime date)
        {
            TimeSpan timeDiff = DateTime.Now - date;
            int seconds = (int)timeDiff.TotalSeconds;

            int interval = seconds / 31536000;
            if (interval > 1)
            {
                return $"{interval} years ago";
            }
            interval = seconds / 2592000;
            if (interval > 1)
            {
                return $"{interval} months ago";
            }
            interval = seconds / 86400;
            if (interval > 1)
            {
                return $"{interval} days ago";
            }
            interval = seconds / 3600;
            if (interval > 1)
            {
                return $"{interval} hrs ago";
            }
            interval = seconds / 60;
            if (interval > 1)
            {
                return $"{interval} min ago";
            }
            return $"{Math.Floor(Convert.ToDouble(seconds))} seconds ago";
        }

    }
}
