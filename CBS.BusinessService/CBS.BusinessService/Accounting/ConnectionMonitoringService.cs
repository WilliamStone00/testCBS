using CBS.BusinessService.Accounting;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CBS.BusinessService.Accounting
{
    public class ConnectionMonitoringService
    {
        private readonly IHubContext _context;

        private bool _isOffline = false;
        private Timer _timer;

        public ConnectionMonitoringService()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext("ConnectionHub");

            // Check connection status every 5 seconds
            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }
     
       
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            bool isConnected = CheckInternetConnection();
            if (!isConnected && !_isOffline)
            {
                // Terminal is disconnected
                _context.Clients.All.ReceiveConnectionStatus(false);
                _isOffline = true;
            }
            else if (isConnected && _isOffline)
            {
                // Connection has been restored
                _context.Clients.All.ReceiveConnectionStatus(true);
                _isOffline = false;
            }
        }
       

      
        private bool CheckInternetConnection()
        {
            try
            {
                // Check if a network interface is available
                bool isConnected = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                return isConnected;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the check
                // Log the exception or take appropriate action
                return false;
            }
        }
    }
}
 



