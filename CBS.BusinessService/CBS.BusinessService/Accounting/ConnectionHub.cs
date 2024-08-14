
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
public class ConnectionHub : Hub
    {
        public void SendConnectionStatus(bool isOnline)
        {
            Clients.All.ReceiveConnectionStatus(isOnline);
        }
     
        public bool ReceiveConnectionStatus(bool isOnline)
        {
            if (isOnline)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public void CheckConnectionStatus()
        {
            // Simulate checking connection status (replace with your actual logic)
            bool isConnected = IsTerminalConnected();

            // Send the result back to the client
            Clients.Caller.ReceiveConnectionStatus(isConnected);
        }

        private bool IsTerminalConnected()
        {
            // Example logic to determine if terminal is connected
            // Replace this with your actual logic to check connection status
            return true;  // Replace with your actual logic to determine connection status
        }
    }
}

