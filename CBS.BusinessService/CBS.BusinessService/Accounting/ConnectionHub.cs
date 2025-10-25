
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{

    public class ConnectionHub : Hub
    {
        // This method will be called by the client to check the connection Status
        public bool CheckConnection()
        {
            bool isConnected = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            // Send a message to the client that the connection is active
            Clients.Caller.ReceiveConnectionStatus(isConnected);
            return isConnected;
        }
    }

}

