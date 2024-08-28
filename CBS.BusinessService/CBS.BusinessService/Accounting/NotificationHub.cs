using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class NotificationHub : Hub
    {
        public async Task NotifyClients()
        {
            await Clients.All.ReceiveNotification();
        }
    }
}
