using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
namespace CBS.FrontDesk.UI.Hub
{
    public class ChatHub : IHub
    {
        public HubCallerContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IHubCallerConnectionContext<dynamic> Clients { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IGroupManager Groups { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task OnConnected()
        {
            throw new NotImplementedException();
        }

        public Task OnDisconnected(bool stopCalled)
        {
            throw new NotImplementedException();
        }

        public Task OnReconnected()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(message);
        }
    }
}