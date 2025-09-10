using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounting
{
    public static class ProductAccountingHelper 
    {
        private static HttpClient _httpClient= new HttpClient();
 
        private static string _newbaseURL = GetBaseUrl( ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
  
        public static string GetEndpoint(string fullUrl)
        {
            // Parse the URL using the Uri class
            Uri uri = new Uri(fullUrl);

            // GetAllowAnonymous the endpoint
            string endpoint = uri.PathAndQuery;

            return endpoint;
        }
        private static string RemoveDuplicateSlashes(string url)
        {
            // Replace occurrences of double forward slashes (//) with single forward slash (/)
            return url.Replace("//", "/");
        }
    
        public static string GetBaseUrl(string fullUrl)
        {
            // Parse the URL using the Uri class
            Uri uri = new Uri(fullUrl);

            // GetAllowAnonymous the base URL
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);

            return baseUrl;
        }
        public static async Task<List<AccountingBook>> GetAsync(string apiUrl)
        {
            _httpClient.BaseAddress = new Uri(_newbaseURL);

             
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                return JsonConvert.DeserializeObject<List<AccountingBook>>(await response.Content.ReadAsStringAsync());
             }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

    

        private static void AddAuthorizationHeader(HttpClient client)
        {
            string encryptedtoken = HttpContext.Current.Session["EncryptedJWToken"] as string;

            // Check if token exists in the session
            if (!string.IsNullOrEmpty(encryptedtoken))
            {
                if (client.DefaultRequestHeaders.Authorization != null)
                {
           
                }
                else
                {
                    string tokendecrypted = TokenEncryptionHelper.DecryptToken(encryptedtoken);
                    // Add the token to the request headers
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokendecrypted);
                }
            }




        }



    

         
    }
}
