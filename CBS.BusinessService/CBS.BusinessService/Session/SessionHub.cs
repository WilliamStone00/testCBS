using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Session
{
    using CBS.BusinessService.UserManagement;
    using CBS.FrontDesk.Data.UserManagement;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.AspNet.SignalR;
    using System.Threading.Tasks;
    using System.Web;

    public class SessionHub : Hub
    {
        private readonly UserManagementServices _userManagementServices;

        public SessionHub(UserManagementServices userManagementServices)
        {
            _userManagementServices = userManagementServices;
        }

        public override async Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // Check if UserID exists in session
                if (HttpContext.Current.Session["UserID"] != null)
                {
                    Guid userId = (Guid)HttpContext.Current.Session["UserID"];

                    // Get the refresh token from session
                    string currentSessionRefreshToken = HttpContext.Current.Session["RefresherToken"] as string;

                    // Check active sessions for the user
                    var activeSessions = await _userManagementServices.GetUserSessions(userId);

                    // Check if there are multiple active sessions for the user
                    if (activeSessions.Count() > 1)
                    {
                        // Notify the client to log out due to multiple logins
                        await Clients.Caller.SendAsync("Logout", "Multiple active sessions detected. You are being logged out.");
                        return;
                    }

                    // Find the current user session (assuming matching by refresh token)
                    var currentSession = activeSessions.FirstOrDefault(s => s.RefreshToken == currentSessionRefreshToken);

                    // If no matching session is found, or if the token doesn't match, log out the user
                    if (currentSession == null || currentSession.RefreshToken != currentSessionRefreshToken)
                    {
                        // Notify the client to log out due to refresh token mismatch
                        await Clients.Caller.SendAsync("Logout", "Session token mismatch detected. You are being logged out.");
                        return;
                    }

                    // Proceed with any additional logic if the session and token are valid
                }
            }

            await base.OnConnected(); // Await the base method call
        }

        // Notify all connected clients to log out (if needed)
        public void DisconnectAllSessions()
        {
            Clients.All.SendAsync("Logout", "All sessions are being disconnected.");
        }
    }

}
