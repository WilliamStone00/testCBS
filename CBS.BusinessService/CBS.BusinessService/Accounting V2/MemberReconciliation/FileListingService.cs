using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountingReportsD;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation.FileUploadData;

namespace CBS.BusinessService.Accounting_V2.MemberReconciliation
{
    public class FileListingService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public FileListingService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }



        public async Task<CustomDataTable> DataTableAsync(FileListingQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.fileuploaddatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($" service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<CustomDataTable> DataTableAsync2(GLHistoryListingQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GLhistorydatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($" service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<FileUploadData> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetReconciliationtById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<FileUploadData>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<GLHistoryDto> GetGLHByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetGLHById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<GLHistoryDto>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> ConfirmReconciliation(ConfirmReconciliation payload)
        {
            try
            {
                payload.ReconciledBy = GetUserFullName();
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<object>>(APICallHelper.ReconciliationConfirmation, payload);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, payload.Id, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(payload.Id, false, payload.Id, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(payload.Id, false, payload.Id, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        //public async Task<ExecutionMessages> DownloadFile(string fileId)
        //{
        //    string filePath = null;

        //    try
        //    {
        //        string formattedUrl = string.Format(APICallHelper.GetReconciliationtById, fileId);

        //        Console.WriteLine($"Downloading file from: {formattedUrl}");

        //        // Since the server might be returning JSON for errors, we need to handle both
        //        // 1. Direct file download (bytes)
        //        // 2. JSON error responses
        //        byte[] fileBytes = await DownloadWithErrorHandling(formattedUrl, fileId);

        //        // Validate we have bytes
        //        if (fileBytes == null || fileBytes.Length == 0)
        //        {
        //            GetExecutionMessages(null, false, fileId, MessagesResults.Failed,
        //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
        //                null, "No file data received from server");
        //            return ExecutionMessage;
        //        }

        //        // Check if it's actually a JSON error message disguised as bytes
        //        string responseAsString = Encoding.UTF8.GetString(fileBytes).Trim();

        //        // Check for JSON error patterns
        //        if (responseAsString.StartsWith("{") && responseAsString.Contains("\"Message\""))
        //        {
        //            // Try to parse as JSON error
        //            try
        //            {
        //                var jsonError = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseAsString);
        //                string errorMessage = "Unknown server error";

        //                if (jsonError.TryGetProperty("Message", out var messageProp))
        //                    errorMessage = messageProp.GetString();
        //                else if (jsonError.TryGetProperty("message", out messageProp))
        //                    errorMessage = messageProp.GetString();

        //                GetExecutionMessages(null, false, fileId, MessagesResults.Failed,
        //                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
        //                    null, $"Server error: {errorMessage}");
        //                return ExecutionMessage;
        //            }
        //            catch
        //            {
        //                // Not JSON, continue with file validation
        //            }
        //        }

        //        // Check for HTML error page (ASP.NET error)
        //        if (responseAsString.Contains("Server Error in '/' Application") ||
        //            responseAsString.Contains("JsonRequestBehavior to AllowGet"))
        //        {
        //            GetExecutionMessages(null, false, fileId, MessagesResults.Failed,
        //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
        //                null, "Server configuration error: API endpoint requires JsonRequestBehavior.AllowGet");
        //            return ExecutionMessage;
        //        }

        //        // Validate it's a valid Excel file
        //        if (!IsValidExcelFile(fileBytes))
        //        {
        //            string errorMessage = "Downloaded file is not a valid Excel format";

        //            if (!string.IsNullOrEmpty(responseAsString) && responseAsString.Length < 1000)
        //            {
        //                errorMessage = $"Server response: {responseAsString}";
        //            }

        //            GetExecutionMessages(null, false, fileId, MessagesResults.Failed,
        //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
        //                null, errorMessage);
        //            return ExecutionMessage;
        //        }

        //        // Save file to downloads folder
        //        filePath = SaveFileToDownloads(fileId, fileBytes);

        //        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        //        {
        //            GetExecutionMessages(null, false, fileId, MessagesResults.Failed,
        //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
        //                null, "Failed to save file to disk");
        //            return ExecutionMessage;
        //        }

        //        // Success
        //        var fileInfo = new FileInfo(filePath);
        //        string successMessage = $"File downloaded successfully: {fileInfo.Length} bytes";

        //        GetExecutionMessages(filePath, true, fileId, MessagesResults.Success,
        //            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
        //            null, successMessage);

        //        Console.WriteLine($"File saved to: {filePath}");

        //        return ExecutionMessage;
        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(filePath, false, fileId, MessagesResults.Error,
        //            ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
        //        return ExecutionMessage;
        //    }
        //}

        //// Helper method to download with error handling
        //private async Task<byte[]> DownloadWithErrorHandling(string url, string fileId)
        //{
        //    try
        //    {
        //        // Option 1: Try using your API helper first (might handle JSON better)
        //        byte[] result = await DownloadViaApiHelper(url, fileId);
        //        if (result != null && result.Length > 0)
        //        {
        //            Console.WriteLine($"API helper returned {result.Length} bytes");
        //            return result;
        //        }

        //        // Option 2: Try direct download with specific headers
        //        using (var httpClient = new HttpClient())
        //        {
        //            httpClient.Timeout = TimeSpan.FromMinutes(5);

        //            // Add specific headers to indicate we want binary data
        //            httpClient.DefaultRequestHeaders.Accept.Clear();
        //            httpClient.DefaultRequestHeaders.Accept.Add(
        //                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        //            httpClient.DefaultRequestHeaders.Accept.Add(
        //                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));

        //            // If you have authentication, add it here
        //            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //            var response = await httpClient.GetAsync(url);

        //            // Check content type
        //            var contentType = response.Content.Headers.ContentType?.MediaType;
        //            Console.WriteLine($"Response Content-Type: {contentType}");
        //            Console.WriteLine($"Response Status: {response.StatusCode}");

        //            var responseBytes = await response.Content.ReadAsByteArrayAsync();
        //            Console.WriteLine($"Received {responseBytes.Length} bytes");

        //            // Log first few bytes for debugging
        //            if (responseBytes.Length > 0)
        //            {
        //                string firstBytes = BitConverter.ToString(responseBytes, 0, Math.Min(20, responseBytes.Length));
        //                Console.WriteLine($"First bytes: {firstBytes}");
        //            }

        //            if (response.IsSuccessStatusCode)
        //            {
        //                return responseBytes;
        //            }
        //            else
        //            {
        //                // Try to read as text to get error message
        //                string errorText = Encoding.UTF8.GetString(responseBytes);
        //                Console.WriteLine($"Error response: {errorText}");

        //                // If it's an HTML error page, extract meaningful info
        //                if (errorText.Contains("Server Error"))
        //                {
        //                    // Extract between <h1> tags or similar
        //                    int start = errorText.IndexOf("<h1>");
        //                    int end = errorText.IndexOf("</h1>", start);
        //                    if (start >= 0 && end > start)
        //                    {
        //                        string errorTitle = errorText.Substring(start + 4, end - start - 4);
        //                        Console.WriteLine($"Extracted error: {errorTitle}");
        //                    }
        //                }

        //                return null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Download error: {ex.GetType().Name}: {ex.Message}");
        //        return null;
        //    }
        //}

        //// Updated API helper method that tries to handle JSON errors
        //private async Task<byte[]> DownloadViaApiHelper(string url, string fileId)
        //{
        //    try
        //    {
        //        // First try the original approach
        //        var apiResponse = await _apiCallerHelper.GetAsync<ApiResponse<ServiceResponse<byte[]>>>(url);

        //        if (apiResponse == null)
        //        {
        //            Console.WriteLine("API helper returned null response");
        //            return null;
        //        }

        //        // Try to extract ServiceResponse<byte[]>
        //        ServiceResponse<byte[]> serviceResp = null;
        //        var possibleProps = new[] { "ApiResponseData", "Result", "Content", "Data", "Body", "Response" };

        //        foreach (var pName in possibleProps)
        //        {
        //            var p = apiResponse.GetType().GetProperty(pName,
        //                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        //            if (p != null)
        //            {
        //                var val = p.GetValue(apiResponse);
        //                if (val is ServiceResponse<byte[]> found)
        //                {
        //                    serviceResp = found;
        //                    break;
        //                }
        //            }
        //        }

        //        if (serviceResp == null)
        //        {
        //            var prop = apiResponse.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        //                .FirstOrDefault(x => x.PropertyType == typeof(ServiceResponse<byte[]>));
        //            if (prop != null)
        //            {
        //                serviceResp = (ServiceResponse<byte[]>)prop.GetValue(apiResponse);
        //            }
        //        }

        //        if (serviceResp == null)
        //        {
        //            Console.WriteLine("Could not find ServiceResponse<byte[]> in API response");
        //            return null;
        //        }

        //        // Check success
        //        bool isSuccess = (serviceResp.Errors == null || serviceResp.Errors.Count == 0) || serviceResp.StatusCode == 200;
        //        if (!isSuccess)
        //        {
        //            Console.WriteLine($"ServiceResponse indicates failure: {serviceResp.Message}");
        //            return null;
        //        }

        //        byte[] fileBytes = serviceResp.Data;

        //        // Try base64 decoding if data is empty
        //        if ((fileBytes == null || fileBytes.Length == 0))
        //        {
        //            var dataProp = serviceResp.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
        //            if (dataProp != null && dataProp.PropertyType == typeof(string))
        //            {
        //                var base64 = (string)dataProp.GetValue(serviceResp);
        //                if (!string.IsNullOrWhiteSpace(base64))
        //                {
        //                    try
        //                    {
        //                        fileBytes = Convert.FromBase64String(base64.Trim());
        //                        Console.WriteLine($"Decoded {fileBytes?.Length ?? 0} bytes from base64");
        //                    }
        //                    catch
        //                    {
        //                        Console.WriteLine("Base64 decode failed");
        //                    }
        //                }
        //            }
        //        }

        //        if (fileBytes != null && fileBytes.Length > 0)
        //        {
        //            Console.WriteLine($"API helper approach successful: {fileBytes.Length} bytes");
        //        }

        //        return fileBytes;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Check if it's the JSON GET error
        //        if (ex.Message.Contains("JsonRequestBehavior") || ex.Message.Contains("AllowGet"))
        //        {
        //            Console.WriteLine("API endpoint requires JsonRequestBehavior.AllowGet");
        //            return null;
        //        }

        //        Console.WriteLine($"API helper exception: {ex.GetType().Name}: {ex.Message}");
        //        return null;
        //    }
        //}

        //// Helper method to validate Excel file
        //private bool IsValidExcelFile(byte[] fileBytes)
        //{
        //    if (fileBytes == null || fileBytes.Length < 4)
        //        return false;

        //    // Check for ZIP header (Excel files are ZIP archives)
        //    bool hasZipHeader = fileBytes[0] == 0x50 && fileBytes[1] == 0x4B &&
        //                        fileBytes[2] == 0x03 && fileBytes[3] == 0x04;

        //    return hasZipHeader;
        //}

        //// Helper method to save file
        //private string SaveFileToDownloads(string fileId, byte[] fileBytes)
        //{
        //    try
        //    {
        //        string downloadsFolder;
        //        try
        //        {
        //            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        //            downloadsFolder = Path.Combine(userProfile, "Downloads");

        //            if (!Directory.Exists(downloadsFolder))
        //            {
        //                downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //            }
        //        }
        //        catch
        //        {
        //            downloadsFolder = Path.GetTempPath();
        //        }

        //        Directory.CreateDirectory(downloadsFolder);

        //        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //        string baseName = $"{fileId}_{timestamp}.xlsx";
        //        string filePath = Path.Combine(downloadsFolder, baseName);

        //        int counter = 1;
        //        while (File.Exists(filePath))
        //        {
        //            filePath = Path.Combine(downloadsFolder, $"{fileId}_{timestamp}({counter}).xlsx");
        //            counter++;
        //        }

        //        File.WriteAllBytes(filePath, fileBytes);

        //        if (File.Exists(filePath))
        //        {
        //            var fileInfo = new FileInfo(filePath);
        //            if (fileInfo.Length == fileBytes.Length)
        //            {
        //                return filePath;
        //            }
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error saving file: {ex.Message}");
        //        return null;
        //    }
        //}

        public async Task<FileDownloadDto> DownloadFile(string id)
        {
            var response = await _apiCallerHelper.GetAsync<ResponseObject<FileDownloadDto>>(
                string.Format(APICallHelper.DownloadFile, id)
            );

            if (response.IsSuccess)
                return response.ApiResponseData.Data;

            return new FileDownloadDto { ErrorMessage = response.Message };
        }

    }

}
