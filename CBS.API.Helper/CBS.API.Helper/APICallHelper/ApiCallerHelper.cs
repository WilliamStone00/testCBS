using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using CBS.API.Helper.APICallHelper;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using CBS.FrontDesk.Data.Entity.LoanConf;
using System.Net.Http.Headers;
using System.Web.UI.WebControls;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Net.Sockets;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data;

using System.Threading;

namespace CBS.API.Helper
{
    public class ApiCallerHelper : IDisposable
    {
        private readonly HttpClient _httpClient;
        string _baseURL = string.Empty;
        string _newbaseURL = string.Empty;
        public ApiCallerHelper(string baseUrl)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                string newbaseUrl = GetBaseUrl(baseUrl);
                if (!string.IsNullOrEmpty(newbaseUrl))
                {
                    _httpClient = new HttpClient();
                    _httpClient.BaseAddress = new Uri(newbaseUrl);
                    _baseURL = newbaseUrl;
                    _newbaseURL = baseUrl;

                }
                else
                {
                    throw new ArgumentException("Base URL cannot be null or empty.", nameof(newbaseUrl));
                }
            }

        }

        public async Task<(List<IndividualProfile> customers, PaginationMetadata pagination)> GetCustomersWithPagination(string apiUrl)
        {
            try
            {
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                // Ensure the response was successful before proceeding
                response.EnsureSuccessStatusCode();

                // Read the pagination header
                PaginationMetadata pagination = null;
                if (response.Headers.TryGetValues("X-Pagination", out IEnumerable<string> headerValues))
                {
                    string paginationHeader = headerValues.FirstOrDefault();
                    pagination = JsonConvert.DeserializeObject<PaginationMetadata>(paginationHeader);
                }

                // Handle the API response
                ApiResponse<List<IndividualProfile>> apiResponse = await HandleResponse<List<IndividualProfile>>(response);

                // Return the customer list and pagination metadata as a tuple
                return (apiResponse.ApiResponseData, pagination);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public static string GetEndpoint(string fullUrl)
        {
            // Parse the URL using the Uri class
            Uri uri = new Uri(fullUrl);

            // Get the endpoint
            string endpoint = uri.PathAndQuery;

            return endpoint;
        }
        private string RemoveDuplicateSlashes(string url)
        {
            // Replace occurrences of double forward slashes (//) with single forward slash (/)
            return url.Replace("//", "/");
        }
        public static string GetBaseUrl(string fullUrl)
        {
            // Parse the URL using the Uri class
            Uri uri = new Uri(fullUrl);

            // Get the base URL
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);

            return baseUrl;
        }
        public async Task<ApiResponse<T>> GetAsync<T>(string apiUrl)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public async Task<ApiResponse<T>> PostImageAsync<T>(string apiUrl, HttpPostedFileBase imageFile)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                if (imageFile != null && imageFile.ContentLength > 0)
                {

                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(imageFile.InputStream), "file", imageFile.FileName);

                        // Add authorization header if required
                        AddAuthorizationHeader(_httpClient);

                        HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                        return await HandleResponse<T>(response);
                    }
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "No file selected or file is empty."
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP Request Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Handle other exceptions or errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ApiResponse<T>> PostImagesAsync<T>(string apiUrl, List<HttpPostedFileBase> imageFiles, string loanApplicationId)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                var formData = new MultipartFormDataContent();

                // Add loan application ID as a string content
                formData.Add(new StringContent(loanApplicationId), "LoanApplicationID");

                // Add each image file to the form data
                foreach (var imageFile in imageFiles)
                {
                    formData.Add(new StreamContent(imageFile.InputStream), "AttachedFiles", imageFile.FileName);
                }

                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP Request Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Handle other exceptions or errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ApiResponse<T>> PostFilesAndParamsAsync<T>(string apiUrl, Dictionary<string, string> additionalParams, List<HttpPostedFileBase> imageFiles)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                var formData = new MultipartFormDataContent();
                // Add additional parameters
                if (additionalParams != null)
                {
                    foreach (var param in additionalParams)
                    {
                        formData.Add(new StringContent(param.Value), param.Key);
                    }
                }

                // Add each file to the form data
                foreach (var file in imageFiles)
                {
                    formData.Add(new StreamContent(file.InputStream), "AttachedFiles", file.FileName);
                }

                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP Request Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Handle other exceptions or errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }



        public async Task<APICallBackRespose> PostFilesAndParamsAsync(string apiUrl, Dictionary<string, string> additionalParams, List<HttpPostedFileBase> imageFiles)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                var formData = new MultipartFormDataContent();
                // Add additional parameters
                if (additionalParams != null)
                {
                    foreach (var param in additionalParams)
                    {
                        formData.Add(new StringContent(param.Value), param.Key);
                    }
                }

                // Add each file to the form data
                foreach (var file in imageFiles)
                {
                    formData.Add(new StreamContent(file.InputStream), "AttachedFiles", file.FileName);
                }

                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                return await HandleResponseCallBackRespose(response);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                throw ex;
            }
            catch (Exception ex)
            {
                // Handle other exceptions or errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw ex;

            }
        }


        public async Task<ApiResponse<T>> PostImageAsync<T>(string apiUrl, HttpPostedFileBase imageFile, string loanApplicationId)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(loanApplicationId), "LoanApplicationID");
                formData.Add(new StreamContent(imageFile.InputStream), "AttachedFiles", imageFile.FileName);
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                return await HandleResponse<T>(response);

            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP Request Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Handle other exceptions or errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }



        private string[] ConvertByteArrayToStringArray(byte[] byteArray)
        {
            // Convert the byte array to an array of strings
            // For example, you might convert each byte to its string representation
            // You can use different conversion methods based on your server's requirements
            return byteArray.Select(b => b.ToString()).ToArray();
        }
        public async Task<ServiceResponseXX<T>> PostAsync<T>(string apiUrl, object data, CancellationToken cancellationToken, int timeoutSeconds = 120)
        {
            // Create a linked cancellation token source that combines the passed token and a timeout token
            using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
            {
                try
                {
                    apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                    string jsonData = JsonConvert.SerializeObject(data);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    AddAuthorizationHeader(_httpClient);

                    // Use the linked cancellation token for the PostAsync call
                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content, linkedCts.Token);

                    // Handle the response
                    return await HandleResponsed<T>(response);
                }
                catch (OperationCanceledException ex)
                {
                    if (timeoutCts.IsCancellationRequested)
                    {
                        // If the timeout token triggered the cancellation
                        return new ServiceResponseXX<T>
                        {
                            Status = false,
                            Message = $"The request timed out after {timeoutSeconds} seconds.",
                            StatusCode = System.Net.HttpStatusCode.RequestTimeout
                        };
                    }
                    else
                    {
                        // If the cancellation was triggered by the passed token
                        return new ServiceResponseXX<T>
                        {
                            Status = false,
                            Message = "The request was cancelled.",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    return new ServiceResponseXX<T>
                    {
                        Status = false,
                        Message = $"An error occurred: {ex.Message}",
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    };
                }
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string apiUrl, object data)
        {
             
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception EX)
            {

                throw (EX);
       
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string apiUrl, object data, int timeoutSeconds = 120)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception EX)
            {

                throw (EX);
            }
        }
        public async Task<List<AccountingEntry>> PostAccountingAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleAccountingResponse(response);
            return Model.Data;
        }

        public async Task<List<LiaisonLedgerEntry>> PostLiaisonAccountAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleLiaisonResponse(response);
            return Model.Data;
        }
        public async Task<List<BranchLiaisonLedgerEntry>> PostBranchLiaisonAccountAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleBranchLiaisonResponse(response);
            return Model.Data;
        }
        public async Task<AccountingGeneralLedgerDetails> PostGLAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleAccountingGeneralLedgerResponse(response);
            return Model.data;
        }
        public async Task<List<TrialBalance6ColumnDto>> PostTrialBalance6ColumnAsyncAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleTrialBalance6ColumnResponse(response);
            return Model.data;
        }
        public async Task<List<TrialBalance4ColumnDto>> PostTrialBalance4ColumnAsyncAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleTrialBalance4ColumnResponse(response);
            return Model.Data;
        }
        public async Task<UploadAccountResultServiceResponse> PostUploadAccountResultResponseAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleUploadAccountResultResponse(response);
            return Model;
        }
        public async Task<List<ModelBalanceSheetAssets>> PostModelBalanceSheetAssetsAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleBalanceSheetAssetsResponse(response);
            return Model.Data;
        }

        public async Task<List<ModelExpenses>> PostIncomeAndExpenseEntriesAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleModelExpensesServiceResponse(response);
            return Model.Data;
        }
        public async Task<ServiceResponseXX<T>> PostxxAsync<T>(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            return await HandleResponsed<T>(response);
        }

        public ApiResponse<T> Post<T>(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = _httpClient.PostAsync(apiUrl, content).GetAwaiter().GetResult();
            return HandleResponse<T>(response).GetAwaiter().GetResult();
        }
        public async Task<ApiResponse<T>> PutAsync<T>(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);

            HttpResponseMessage response = null;

            try
            {
                // Log the request
                Console.WriteLine($"Sending PUT request to {apiUrl} with data: {jsonData}");

                // Execute the PUT request
                response = await _httpClient.PutAsync(apiUrl, content);

                // Log the response status
                Console.WriteLine($"Response status code: {response.StatusCode}");

                // Handle the response
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                // Log the detailed error message
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                // Re-throw or handle accordingly
                throw;
            }
            catch (IOException ex)
            {
                // Log the detailed error message
                Console.WriteLine($"IOException: {ex.Message}");
                // Re-throw or handle accordingly
                throw;
            }
            catch (SocketException ex)
            {
                // Log the detailed error message
                Console.WriteLine($"SocketException: {ex.Message}");
                // Re-throw or handle accordingly
                throw;
            }
            catch (Exception ex)
            {
                // Log the detailed error message for any other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                // Re-throw or handle accordingly
                throw;
            }
            finally
            {
                // Dispose the response to free up resources
                response?.Dispose();
            }
        }



        //private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        //{
        //    var apiResponse = new ApiResponse<T>();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseData = await response.Content.ReadAsStringAsync();
        //        apiResponse.Data = JsonConvert.DeserializeObject<T>(responseData);
        //        apiResponse.IsSuccess = true;
        //    }
        //    else
        //    {
        //        apiResponse.IsSuccess = false;
        //        apiResponse.ErrorMessage = response.ReasonPhrase;
        //    }

        //    return apiResponse;
        //}

        //public async Task<ApiResponse<T>> PutAsync<T>(string apiUrl, object data)
        //{
        //    string jsonData = JsonConvert.SerializeObject(data);
        //    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        //    AddAuthorizationHeader(_httpClient);
        //    HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);
        //    return await HandleResponse<T>(response);
        //}

        public async Task<ApiResponse<T>> DeleteAsync<T>(string apiUrl)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);
            return await HandleResponse<T>(response);
        }




        public class NullableDoubleConverter : JsonConverter<double?>
        {
            public override double? ReadJson(JsonReader reader, Type objectType, double? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                    return Convert.ToDouble(reader.Value);

                // Handle other cases if needed

                return null; // Default return null
            }

            public override void WriteJson(JsonWriter writer, double? value, JsonSerializer serializer)
            {
                throw new NotImplementedException(); // Optional if you don't need to serialize back
            }
        }
        // Define a method to deserialize JSON data
        public static T DeserializeJson<T>(string jsonData)
        {
            try
            {
                // Check if the JSON data represents a single object
                if (!jsonData.StartsWith("["))
                {
                    // Deserialize as a single object
                    return JsonConvert.DeserializeObject<T>(jsonData, new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                        DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                    });
                }
                else
                {
                    // Deserialize as a collection
                    return JsonConvert.DeserializeObject<List<T>>(jsonData, new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                        DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                    }).FirstOrDefault(); // Return the first object from the collection
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            string message = null;
            string statusDescription = null;
            JObject jsonResponse = null;

            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseData))
                    {

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            message = "Unauthorized";
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = "Resource not Found"
                            };
                        }

                    }

                    if (response.IsSuccessStatusCode)
                    {

                        if (responseData.Contains("\"status\":SUCCESS") || responseData.Contains("\"data\":true") || responseData.Contains("\"isSuccess\":true"))
                        {
                            jsonResponse = JObject.Parse(responseData);
                            var obj = JsonConvert.DeserializeObject<dynamic>(responseData);
                            message = jsonResponse["message"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();

                            return new ApiResponse<T>
                            {
                                IsSuccess = true,
                                Message = statusDescription + " " + message
                            };
                        }

                        T data = JsonConvert.DeserializeObject<T>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = true,
                                ApiResponseData = data,
                                Message = statusDescription + " " + message
                            };
                        }
                        else
                        {
                            if (bool.TryParse(responseData, out bool isBooleanResponse) && isBooleanResponse)
                            {
                                // If responseData is a boolean, convert it to a JSON string representation
                                string boolResponseJson = JsonConvert.SerializeObject(responseData);

                                // Deserialize the JSON string
                                data = JsonConvert.DeserializeObject<T>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<T>
                                {
                                    IsSuccess = true,
                                    ApiResponseData = data,
                                    Message = $"Operation completed successfully"
                                };

                            }
                            else
                            {
                                jsonResponse = JObject.Parse(responseData);
                                message = jsonResponse["message"]?.ToString();
                                var dataStr = jsonResponse["data"]?.ToString();
                                statusDescription = jsonResponse["statusDescription"]?.ToString();
                                if (responseData.Contains("\"isSuccess\":false"))
                                {
                                    return new ApiResponse<T>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<T>
                                    {
                                        IsSuccess = true,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }

                            }


                        }
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            jsonResponse = JObject.Parse(responseData);
                            message = jsonResponse["message"]?.ToString();
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server {_baseURL} is temporally unavailable/unreachable."
                            };


                        }

                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
                        {

                            T data;
                            try
                            {
                                if (responseData.Contains("\"data\":null") || responseData.Contains("null"))
                                {
                                    data = default(T); // Assign default value for T (typically null for reference types)

                                    jsonResponse = JObject.Parse(responseData);
                                    message = jsonResponse["message"]?.ToString();
                                    statusDescription = jsonResponse["statusDescription"]?.ToString();
                                    return new ApiResponse<T>
                                    {
                                        IsSuccess = false,
                                        Message = $"Request failed with status code {(int)response.StatusCode}. Description: {statusDescription}, Message: {message}"
                                    };
                                }
                                else if (responseData.Contains("\"data\":false") || responseData.Contains("\"isSuccess\":false"))
                                {
                                    jsonResponse = JObject.Parse(responseData);
                                    var obj = JsonConvert.DeserializeObject<dynamic>(responseData);
                                    message = jsonResponse["message"]?.ToString();
                                    statusDescription = jsonResponse["statusDescription"]?.ToString();

                                    return new ApiResponse<T>
                                    {
                                        IsSuccess = false,
                                        Message = statusDescription + " " + message
                                    };

                                }
                                else
                                {
                                    jsonResponse = JObject.Parse(responseData);


                                    if (jsonResponse["errors"] != null)
                                    {
                                        var errorResponse = jsonResponse["errors"].ToObject<Dictionary<string, List<string>>>();

                                        // Check if the error message indicates a duplicate record
                                        if (errorResponse.Any(error => error.Value.Any(x => x.Contains("already exists"))))
                                        {
                                            return new ApiResponse<T>
                                            {
                                                IsSuccess = false,
                                                Message = "A customer with the provided phone number already exists."
                                            };
                                        }
                                        else
                                        {
                                            // Extract and join all error messages
                                            var errorMessages = errorResponse
                                                .SelectMany(error => error.Value) // Flatten the list of error messages
                                                .ToList();
                                            var errorMessage = string.Join(". ", errorMessages);

                                            return new ApiResponse<T>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<T>
                                        {
                                            IsSuccess = false,
                                            Message = "Unexpected error occurred: No error messages in the response."
                                        };
                                    }





                                    //if (jsonResponse["errors"] != null)
                                    //{
                                    //    var errorResponse = jsonResponse["errors"].ToObject<Dictionary<string, List<string>>>();
                                    //    var errorMessages = errorResponse
                                    //        .SelectMany(error => error.Value) // Flatten the list of error messages
                                    //        .ToList();
                                    //    var errorMessage = string.Join(". ", errorMessages);

                                    //    return new ApiResponse<T>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<T>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<T>
                                {
                                    IsSuccess = false,
                                    Message = $"Error in handling response: {ex.Message}"
                                };
                            }


                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            jsonResponse = JObject.Parse(responseData);
                            string errorMessage = jsonResponse["message"]?.ToString() ?? "An unexpected fault happened.";
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}, Description: {statusDescription}"
                            };
                        }
                        else
                        {
                            List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(responseData);
                            if (errorMessages != null && errorMessages.Count > 0)
                            {
                                return new ApiResponse<T>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<T>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Error in handling response: {ex.Message}"
                };
            }
        }



        private async Task<APICallBackRespose> HandleResponseCallBackRespose(HttpResponseMessage response)
        {

            APICallBackRespose model = new APICallBackRespose();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    model = JsonConvert.DeserializeObject<APICallBackRespose>(responseData);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async Task<BranchLiaisonLedgerEntryServiceResponse> HandleBranchLiaisonResponse(HttpResponseMessage response)
        {

            BranchLiaisonLedgerEntryServiceResponse entries = new BranchLiaisonLedgerEntryServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<BranchLiaisonLedgerEntryServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async Task<TrialBalance6ColumnDtoServiceResponse> HandleTrialBalance6ColumnResponse(HttpResponseMessage response)
        {

            TrialBalance6ColumnDtoServiceResponse entries = new TrialBalance6ColumnDtoServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<TrialBalance6ColumnDtoServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async Task<AccountingGeneralLedgerServiceResponse> HandleAccountingGeneralLedgerResponse(HttpResponseMessage response)
        {

            AccountingGeneralLedgerServiceResponse entries = new AccountingGeneralLedgerServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<AccountingGeneralLedgerServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async Task<TrialBalance4ColumnDtoServiceResponse> HandleTrialBalance4ColumnResponse(HttpResponseMessage response)
        {

            TrialBalance4ColumnDtoServiceResponse entries = new TrialBalance4ColumnDtoServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<TrialBalance4ColumnDtoServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async Task<UploadAccountResultServiceResponse> HandleUploadAccountResultResponse(HttpResponseMessage response)
        {
            string responseData = "";
            UploadAccountResultServiceResponse entries = new UploadAccountResultServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<UploadAccountResultServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async Task<ModelBalanceSheetAssetsServiceResponse> HandleBalanceSheetAssetsResponse(HttpResponseMessage response)
        {

            ModelBalanceSheetAssetsServiceResponse entries = new ModelBalanceSheetAssetsServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<ModelBalanceSheetAssetsServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async Task<ModelExpensesServiceResponse> HandleModelExpensesServiceResponse(HttpResponseMessage response)
        {

            ModelExpensesServiceResponse entries = new ModelExpensesServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<ModelExpensesServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async Task<LiaisonLedgerEntryServiceResponse> HandleLiaisonResponse(HttpResponseMessage response)
        {

            LiaisonLedgerEntryServiceResponse entries = new LiaisonLedgerEntryServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<LiaisonLedgerEntryServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async Task<AccountingEntryServiceResponse> HandleAccountingResponse(HttpResponseMessage response)
        {

            AccountingEntryServiceResponse entries = new AccountingEntryServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<AccountingEntryServiceResponse>(responseData);
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async Task<ServiceResponseXX<T>> HandleResponsed<T>(HttpResponseMessage response)
        {
            string message = null;
            string statusDescription = null;
            JObject jsonResponse = null;

            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseData))
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            message = "Unauthorized";
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.Unauthorized,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.InternalServerError,
                                Message = "InternalServerError upexpected error"
                            };
                        }
                        else
                        {
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.BadRequest,
                                Message = "Empty response data received"
                            };
                        }

                    }

                    if (response.IsSuccessStatusCode)
                    {
                        T data = JsonConvert.DeserializeObject<T>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        });
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.OK,
                                Data = data
                            };
                        }
                        else
                        {
                            if (bool.TryParse(responseData, out bool isBooleanResponse) && isBooleanResponse)
                            {
                                // If responseData is a boolean, convert it to a JSON string representation
                                string boolResponseJson = JsonConvert.SerializeObject(responseData);

                                // Deserialize the JSON string
                                data = JsonConvert.DeserializeObject<T>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ServiceResponseXX<T>
                                {
                                    StatusCode = HttpStatusCode.OK,
                                    Data = data,
                                    Message = $"Operation completed successfully"
                                };

                            }
                            else
                            {
                                jsonResponse = JObject.Parse(responseData);
                                message = jsonResponse["message"]?.ToString();
                                statusDescription = jsonResponse["statusDescription"]?.ToString();
                                return new ServiceResponseXX<T>
                                {
                                    StatusCode = HttpStatusCode.OK,
                                    Data = data,
                                    Message = $"Success: {message}, Description: {statusDescription}"
                                };
                            }


                        }
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            message = "Unauthorized";
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.Unauthorized,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.ServiceUnavailable,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server {_baseURL} is temporally unavailable/unreachable."
                            };


                        }
                        else if (response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound)
                        {
                            jsonResponse = JObject.Parse(responseData);
                            message = jsonResponse["message"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            //List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(responseData);
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = response.StatusCode == HttpStatusCode.Conflict ? HttpStatusCode.Conflict : response.StatusCode == HttpStatusCode.NotFound ? HttpStatusCode.NotFound : HttpStatusCode.Ambiguous,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}, Description: {statusDescription}"
                            };


                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            try
                            {
                                jsonResponse = JObject.Parse(responseData);

                                if (jsonResponse["errors"] != null)
                                {
                                    var errorResponse = jsonResponse["errors"].ToObject<Dictionary<string, List<string>>>();

                                    var errorMessages = errorResponse
                                        .SelectMany(error => error.Value) // Flatten the list of error messages
                                        .ToList();

                                    var errorMessage = string.Join(". ", errorMessages);

                                    return new ServiceResponseXX<T>
                                    {
                                        StatusCode = HttpStatusCode.NotAcceptable,
                                        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    };
                                }

                                // If the error structure doesn't match the expected format or errors object not found
                                return new ServiceResponseXX<T>
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = "Error in request" // Set a generic error message
                                };
                            }
                            catch (Exception ex)
                            {
                                return new ServiceResponseXX<T>
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = $"Error in handling response: {ex.Message}"
                                };
                            }


                        }
                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            jsonResponse = JObject.Parse(responseData);
                            string errorMessage = jsonResponse["message"]?.ToString() ?? "An unexpected fault happened.";
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.InternalServerError,

                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ServiceResponseXX<T>
                            {
                                StatusCode = HttpStatusCode.InternalServerError,

                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}, Description: {statusDescription}"
                            };
                        }
                        else
                        {
                            List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(responseData);
                            if (errorMessages != null && errorMessages.Count > 0)
                            {
                                return new ServiceResponseXX<T>
                                {
                                    StatusCode = HttpStatusCode.InternalServerError,

                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ServiceResponseXX<T>
                        {
                            StatusCode = HttpStatusCode.InternalServerError,

                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ServiceResponseXX<T>
                    {
                        StatusCode = HttpStatusCode.NoContent,

                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponseXX<T>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Error in handling response: {ex.Message}"
                };
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
                    // Parse the existing token if present
                    //var jwtHandler = new JwtSecurityTokenHandler();
                    //var existingTokenData = jwtHandler.ReadToken(TokenEncryptionHelper.DecryptToken(client.DefaultRequestHeaders.Authorization.Parameter)) as JwtSecurityToken;

                    //if (existingTokenData != null && existingTokenData.ValidTo < DateTime.UtcNow)
                    //{
                    //    // Replace the token if it's expired
                    //    client.DefaultRequestHeaders.Remove("Authorization");
                    //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    //}
                }
                else
                {
                    string tokendecrypted = TokenEncryptionHelper.DecryptToken(encryptedtoken);
                    // Add the token to the request headers
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokendecrypted);
                }
            }




        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
    public class NullableDoubleConverter : JsonConverter<double?>
    {
        public override double? ReadJson(JsonReader reader, Type objectType, double? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                return Convert.ToDouble(reader.Value);

            return null; // Default return null
        }

        public override void WriteJson(JsonWriter writer, double? value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // Optional if you don't need to serialize back
        }
    }

}