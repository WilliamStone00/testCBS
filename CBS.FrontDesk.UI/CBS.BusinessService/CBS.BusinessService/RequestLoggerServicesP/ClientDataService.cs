using CBS.FrontDesk.Data.Entity.RequestManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class ClientDataService
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CBSDatabase"].ConnectionString;

        public void LogClientData(ClientDataModel data)
        {
            //using (var connection = new SqlConnection(_connectionString))
            //{
            //    string query = @"
            //        INSERT INTO ClientDataLog 
            //        (DataType, IPAddress, Browser, BrowserVersion, OS, ScreenWidth, ScreenHeight, Location, UserAgent, Timestamp) 
            //        VALUES (@DataType, @IPAddress, @Browser, @BrowserVersion, @OS, @ScreenWidth, @ScreenHeight, @Location, @UserAgent, @Timestamp)";

            //    using (var command = new SqlCommand(query, connection))
            //    {
            //        command.Parameters.AddWithValue("@DataType", data.DataType ?? "Unknown");
            //        command.Parameters.AddWithValue("@IPAddress", data.IPAddress ?? "Unknown IP");
            //        command.Parameters.AddWithValue("@Browser", data.Browser ?? "Unknown Browser");
            //        command.Parameters.AddWithValue("@BrowserVersion", data.BrowserVersion ?? "0.0");
            //        command.Parameters.AddWithValue("@OS", data.OS ?? "Unknown OS");
            //        command.Parameters.AddWithValue("@ScreenWidth", data.ScreenWidth);
            //        command.Parameters.AddWithValue("@ScreenHeight", data.ScreenHeight);
            //        command.Parameters.AddWithValue("@Location", data.Location ?? "Unknown Location");
            //        command.Parameters.AddWithValue("@UserAgent", data.UserAgent ?? "Unknown UserAgent");
            //        command.Parameters.AddWithValue("@Timestamp", data.Timestamp);

            //        connection.Open();
            //        command.ExecuteNonQuery();
            //    }
            //}
        }
    }
}
