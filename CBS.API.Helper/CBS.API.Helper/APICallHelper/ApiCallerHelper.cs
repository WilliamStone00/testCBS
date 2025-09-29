using CBS.API.Helper.APICallHelper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace CBS.API.Helper
{
    public class ApiCallerHelper : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseURL = string.Empty;
        private readonly string _newbaseURL = string.Empty;
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

            // GetAllowAnonymous the endpoint
            string endpoint = uri.PathAndQuery;

            return endpoint;
        }
        private string RemoveDuplicateSlashes(string url)
        {
            // Replace occurrences of double forward slashes (//) with single forward slash (/)
            return url.Replace("//", "/");
        }
        private string RemoveDuplicateSlashesException(string url)
        {
            // Replace occurrences of double forward slashes (//) with single forward slash (/)
            return url.Replace("//", "");
        }
        public static string GetBaseUrl(string fullUrl)
        {
            // Parse the URL using the Uri class
            Uri uri = new Uri(fullUrl);

            // GetAllowAnonymous the base URL
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
        public ApiResponse<T> GetAllowAnonymous<T>(string apiUrl)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");

                HttpResponseMessage response = _httpClient.GetAsync(apiUrl).GetAwaiter().GetResult();

                return HandleResponse<T>(response).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {

            }
            return null;
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


        //    public async Task<ServiceResponseDailySaverUploadResult> UploadFileToApiAsync<T>(
        //HttpPostedFileBase uploadedFile,
        //string fileFormFieldName,
        //string apiEndpointUrl,
        //Dictionary<string, string> additionalFields = null)
        //    {
        //        try
        //        {
        //            // Validate inputs
        //            if (uploadedFile == null || uploadedFile.ContentLength <= 0)
        //            {
        //                return new ServiceResponseDailySaverUploadResult
        //                {
        //                    Status = "FAILED",
        //                    Message = "No file was uploaded or the file is empty."
        //                };
        //            }

        //            if (string.IsNullOrEmpty(fileFormFieldName))
        //            {
        //                return new ServiceResponseDailySaverUploadResult
        //                {
        //                    Status = "FAILED",
        //                    Message = "File form field name cannot be null or empty."
        //                };
        //            }

        //            if (string.IsNullOrEmpty(apiEndpointUrl))
        //            {
        //                return new ServiceResponseDailySaverUploadResult
        //                {
        //                    Status = "FAILED",
        //                    Message = "API endpoint URL cannot be null or empty."
        //                };
        //            }

        //            apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

        //            using (var formContent = new MultipartFormDataContent())
        //            {
        //                // Reset stream position to beginning
        //                uploadedFile.InputStream.Position = 0;

        //                // Create stream content from uploaded file
        //                var streamContent = new StreamContent(uploadedFile.InputStream);

        //                // Set content type - handle null/empty content type
        //                var contentType = string.IsNullOrEmpty(uploadedFile.ContentType)
        //                    ? "application/octet-stream"
        //                    : uploadedFile.ContentType;
        //                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        //                // CRITICAL FIX: Add file content with proper field name and filename
        //                // Use quotes around field name for proper form-data formatting
        //                formContent.Add(streamContent, $"\"{fileFormFieldName}\"", $"\"{uploadedFile.FileName}\"");

        //                // Add any additional form fields
        //                if (additionalFields != null)
        //                {
        //                    foreach (var field in additionalFields)
        //                    {
        //                        if (!string.IsNullOrEmpty(field.Key)) // Validate field key
        //                        {
        //                            formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
        //                        }
        //                    }
        //                }

        //                // Add authorization header
        //                AddAuthorizationHeader(_httpClient);

        //                string url = RemoveDuplicateSlashes(apiEndpointUrl);
        //                var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress.OriginalString + url)
        //                {
        //                    Content = formContent
        //                };

        //                // Set timeout for this specific request to 30 seconds (30,000 milliseconds)
        //                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
        //                {
        //                    // Send POST request with timeout
        //                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Form content created, sending request to: {url}");

        //                    var response = await _httpClient.SendAsync(request, cts.Token);
        //                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Response received - Status: {response.StatusCode}");

        //                    var responseText = await response.Content.ReadAsStringAsync();

        //                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Response content read - Length: {responseText?.Length ?? 0}");

        //                    if (!response.IsSuccessStatusCode)
        //                    {
        //                        return new ServiceResponseDailySaverUploadResult
        //                        {
        //                            Status = "FAILED",
        //                            Message = $"API call failed with status {response.StatusCode}: {responseText}"
        //                        };
        //                    }

        //                    // Handle empty response
        //                    if (string.IsNullOrWhiteSpace(responseText))
        //                    {
        //                        return new ServiceResponseDailySaverUploadResult
        //                        {
        //                            Status = "FAILED",
        //                            Message = "API returned empty response."
        //                        };
        //                    }

        //                    // Deserialize to ApiResponse<T>
        //                    try
        //                    {
        //                        var result = JsonConvert.DeserializeObject<ServiceResponseDailySaverUploadResult>(responseText);

        //                        if (result == null)
        //                        {
        //                            return new ServiceResponseDailySaverUploadResult
        //                            {
        //                                Status = "FAILED",
        //                                Message = "Failed to deserialize API response - result is null."
        //                            };
        //                        }

        //                        return result;
        //                    }
        //                    catch (JsonException jsonEx)
        //                    {
        //                        return new ServiceResponseDailySaverUploadResult
        //                        {
        //                            Status = "FAILED",
        //                            Message = $"Failed to deserialize API response: {jsonEx.Message}. Raw response: {responseText}"
        //                        };
        //                    }
        //                }
        //            }
        //        }
        //        catch (OperationCanceledException ocEx) when (ocEx.CancellationToken.IsCancellationRequested)
        //        {
        //            return new ServiceResponseDailySaverUploadResult
        //            {
        //                Status = "FAILED",
        //                Message = "Request timeout: The operation was cancelled after 120 seconds."
        //            };
        //        }
        //        catch (HttpRequestException httpEx)
        //        {
        //            return new ServiceResponseDailySaverUploadResult
        //            {
        //                Status = "FAILED",
        //                Message = $"HTTP request failed: {httpEx.Message}"
        //            };
        //        }
        //        catch (TaskCanceledException tcEx)
        //        {
        //            return new ServiceResponseDailySaverUploadResult
        //            {
        //                Status = "FAILED",
        //                Message = $"Request timeout: {tcEx.Message}"
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            var model = new ServiceResponseDailySaverUploadResult();

        //            model.Status = "FAILED";

        //            model.Message = $"Unexpected error: {ex.Message}";
        //            return model;
        //        }
        //    }


        // Modern async method for uploading file to an API endpoint
        public async Task<ServiceResponseDailySaverUploadResult> UploadFileToApiAsync<T>(
            HttpPostedFileBase uploadedFile,     // File from client
            string fileFormFieldName,            // Form field name expected by the API
            string apiEndpointUrl,               // Target API endpoint
            Dictionary<string, string> additionalFields = null) // Optional extra fields
        {
            // ------------------------------
            // Step 1: Input validation
            // ------------------------------
            if (uploadedFile == null || uploadedFile.ContentLength <= 0)
                return new ServiceResponseDailySaverUploadResult().Failed("No file was uploaded or the file is empty.");

            if (string.IsNullOrEmpty(fileFormFieldName))
                return new ServiceResponseDailySaverUploadResult().Failed("File form field name cannot be null or empty.");

            if (string.IsNullOrEmpty(apiEndpointUrl))
                return new ServiceResponseDailySaverUploadResult().Failed("API endpoint URL cannot be null or empty.");

            apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");
            string url = RemoveDuplicateSlashes(apiEndpointUrl);

            try
            {
                // ------------------------------
                // Step 2: Build multipart form-data request
                // ------------------------------
                using (var formContent = new MultipartFormDataContent())
                {
                    uploadedFile.InputStream.Position = 0;

                    var streamContent = new StreamContent(uploadedFile.InputStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        string.IsNullOrEmpty(uploadedFile.ContentType) ? "application/octet-stream" : uploadedFile.ContentType
                    );

                    // Add file
                    formContent.Add(streamContent, fileFormFieldName, uploadedFile.FileName);

                    // Add extra fields
                    if (additionalFields != null)
                    {
                        foreach (var field in additionalFields.Where(f => !string.IsNullOrEmpty(f.Key)))
                            formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                    }

                    // Add authorization headers
                    AddAuthorizationHeader(_httpClient);

                    // ------------------------------
                    // Step 3: Send request
                    // ------------------------------
                    using (var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + url))
                    using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
                    {
                        request.Content = formContent;

                        var response = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
                        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!response.IsSuccessStatusCode)
                            return new ServiceResponseDailySaverUploadResult().Failed($"API call failed with status {response.StatusCode}: {responseText}");

                        if (string.IsNullOrWhiteSpace(responseText))
                            return new ServiceResponseDailySaverUploadResult().Failed("API returned empty response.");

                        var result = JsonConvert.DeserializeObject<ServiceResponseDailySaverUploadResult>(responseText);
                        return result ?? new ServiceResponseDailySaverUploadResult().Failed("Failed to deserialize API response - result is null.");
                    }

       
                    // ------------------------------
                    // Step 4: Handle response
                    // ------------------------------
             
                }
            }
            // ------------------------------
            // Step 5: Error handling
            // ------------------------------
            catch (OperationCanceledException)
            {
                return new ServiceResponseDailySaverUploadResult().Failed("Request timeout: The operation was cancelled.");
            }
            catch (HttpRequestException httpEx)
            {
                return new ServiceResponseDailySaverUploadResult().Failed($"HTTP request failed: {httpEx.Message}");
            }
           
            catch (Exception ex)
            {
                return new ServiceResponseDailySaverUploadResult().Failed($"Unexpected error: {ex.Message}");
            }
        }


        public async Task<ApiResponse<T>> UploadFileToApiAsyncoo<T>(
    HttpPostedFileBase uploadedFile,
    string fileFormFieldName,
    string apiEndpointUrl,
    Dictionary<string, string> additionalFields = null)
        {
            try
            {
                // Validate inputs
                if (uploadedFile == null || uploadedFile.ContentLength <= 0)
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "No file was uploaded or the file is empty."
                    };
                }

                if (string.IsNullOrEmpty(fileFormFieldName))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "File form field name cannot be null or empty."
                    };
                }

                if (string.IsNullOrEmpty(apiEndpointUrl))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "API endpoint URL cannot be null or empty."
                    };
                }

                apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

                using (var formContent = new MultipartFormDataContent())
                {
                    // Reset stream position to beginning
                    uploadedFile.InputStream.Position = 0;

                    // Create stream content from uploaded file
                    var streamContent = new StreamContent(uploadedFile.InputStream);

                    // Set content type - handle null/empty content type
                    var contentType = string.IsNullOrEmpty(uploadedFile.ContentType)
                        ? "application/octet-stream"
                        : uploadedFile.ContentType;
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    // CRITICAL FIX: Add file content with proper field name and filename
                    // Use quotes around field name for proper form-data formatting
                    formContent.Add(streamContent, $"\"{fileFormFieldName}\"", $"\"{uploadedFile.FileName}\"");

                    // Add any additional form fields
                    if (additionalFields != null)
                    {
                        foreach (var field in additionalFields)
                        {
                            if (!string.IsNullOrEmpty(field.Key)) // Validate field key
                            {
                                formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                            }
                        }
                    }

                    // Add authorization header
                    AddAuthorizationHeader(_httpClient);

                    string url = RemoveDuplicateSlashes(apiEndpointUrl);
                    var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress.OriginalString + url)
                    {
                        Content = formContent
                    };

                    // Send POST request
                    var response = await _httpClient.SendAsync(request);
                    var responseText = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return new ApiResponse<T>
                        {
                            IsSuccess = false,
                            Message = $"API call failed with status {response.StatusCode}: {responseText}"
                        };
                    }

                    // Handle empty response
                    if (string.IsNullOrWhiteSpace(responseText))
                    {
                        return new ApiResponse<T>
                        {
                            IsSuccess = false,
                            Message = "API returned empty response."
                        };
                    }

                    // Deserialize to ApiResponse<T>
                    try
                    {
                        var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);

                        if (result == null)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = "Failed to deserialize API response - result is null."
                            };
                        }

                        return result;
                    }
                    catch (JsonException jsonEx)
                    {
                        return new ApiResponse<T>
                        {
                            IsSuccess = false,
                            Message = $"Failed to deserialize API response: {jsonEx.Message}. Raw response: {responseText}"
                        };
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP request failed: {httpEx.Message}"
                };
            }
            catch (TaskCanceledException tcEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Request timeout: {tcEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<T>> UploadFileToApiAsync00<T>(
    HttpPostedFileBase uploadedFile,
    string fileFormFieldName,
    string apiEndpointUrl,
    Dictionary<string, string> additionalFields = null)
        {
            try
            {
                // Validate inputs
                if (uploadedFile == null || uploadedFile.ContentLength <= 0)
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "No file was uploaded or the file is empty."
                    };
                }

                if (string.IsNullOrEmpty(fileFormFieldName))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "File form field name cannot be null or empty."
                    };
                }

                if (string.IsNullOrEmpty(apiEndpointUrl))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "API endpoint URL cannot be null or empty."
                    };
                }

                apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

                using (var formContent = new MultipartFormDataContent())
                {
                    // Create stream content from uploaded file
                    using (var streamContent = new StreamContent(uploadedFile.InputStream))
                    {
                        // Set content headers properly
                        streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                        {
                            Name = fileFormFieldName, // Remove extra quotes
                            FileName = Path.GetFileName(uploadedFile.FileName) // Remove extra quotes
                        };

                        // Set content type - handle null/empty content type
                        var contentType = string.IsNullOrEmpty(uploadedFile.ContentType)
                            ? "application/octet-stream"
                            : uploadedFile.ContentType;
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                        // Add file content to form
                        formContent.Add(streamContent, fileFormFieldName, uploadedFile.FileName);

                        // Add any additional form fields
                        if (additionalFields != null)
                        {
                            foreach (var field in additionalFields)
                            {
                                if (!string.IsNullOrEmpty(field.Key)) // Validate field key
                                {
                                    formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                                }
                            }
                        }

                        // Add authorization header
                        AddAuthorizationHeader(_httpClient);
                        string url = RemoveDuplicateSlashes(apiEndpointUrl);
                        var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress.OriginalString + url)
                        {
                            Content = formContent
                        };
                        // Send POST request
                        var response = await _httpClient.SendAsync(request);
                        var responseText = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"API call failed with status {response.StatusCode}: {responseText}"
                            };
                        }

                        // Handle empty response
                        if (string.IsNullOrWhiteSpace(responseText))
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = "API returned empty response."
                            };
                        }

                        // Deserialize to ApiResponse<T>
                        try
                        {
                            var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);

                            if (result == null)
                            {
                                return new ApiResponse<T>
                                {
                                    IsSuccess = false,
                                    Message = "Failed to deserialize API response - result is null."
                                };
                            }

                            return result;
                        }
                        catch (JsonException jsonEx)
                        {
                            return new ApiResponse<T>
                            {
                                IsSuccess = false,
                                Message = $"Failed to deserialize API response: {jsonEx.Message}. Raw response: {responseText}"
                            };
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP request failed: {httpEx.Message}"
                };
            }
            catch (TaskCanceledException tcEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Request timeout: {tcEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }



        public async Task UploadFileToApi(
HttpPostedFileBase uploadedFile,
string fileFormFieldName,
string apiEndpointUrl,
Dictionary<string, string> additionalFields = null)
        {
            try
            {


                apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

                using (var formContent = new MultipartFormDataContent())
                {
                    // Create stream content from uploaded file
                    using (var streamContent = new StreamContent(uploadedFile.InputStream))
                    {
                        // Set content headers properly
                        streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                        {
                            Name = fileFormFieldName, // Remove extra quotes
                            FileName = Path.GetFileName(uploadedFile.FileName) // Remove extra quotes
                        };

                        // Set content type - handle null/empty content type
                        var contentType = string.IsNullOrEmpty(uploadedFile.ContentType)
                            ? "application/octet-stream"
                            : uploadedFile.ContentType;
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                        // Add file content to form
                        formContent.Add(streamContent, fileFormFieldName, uploadedFile.FileName);

                        // Add any additional form fields
                        if (additionalFields != null)
                        {
                            foreach (var field in additionalFields)
                            {
                                if (!string.IsNullOrEmpty(field.Key)) // Validate field key
                                {
                                    formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                                }
                            }
                        }

                        // Add authorization header
                        AddAuthorizationHeader(_httpClient);
                        string url = RemoveDuplicateSlashes(apiEndpointUrl);
                        var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress.OriginalString + url)
                        {
                            Content = formContent
                        };
                        // Send POST request
                        await _httpClient.SendAsync(request);
                        //await response.Content.ReadAsStringAsync();



                    }
                }
            }
            catch (HttpRequestException httpEx)
            {

            }
            catch (TaskCanceledException tcEx)
            {

            }
            catch (Exception ex)
            {

            }
        }


        // Alternative version with better resource management
        public async Task<ApiResponse<T>> UploadFileToApiAsyncImproved<T>(
            HttpPostedFileBase uploadedFile,
            string fileFormFieldName,
            string apiEndpointUrl,
            Dictionary<string, string> additionalFields = null,
            CancellationToken cancellationToken = default)
        {
            // Input validation
            var validationResult = ValidateUploadInputs(uploadedFile, fileFormFieldName, apiEndpointUrl);
            if (!validationResult.IsValid)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = validationResult.ErrorMessage
                };
            }

            try
            {
                apiEndpointUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

                // Copy stream to memory to avoid disposal issues
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await uploadedFile.InputStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                using (var formContent = new MultipartFormDataContent())
                using (var fileContent = new ByteArrayContent(fileBytes))
                {
                    // Set headers
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = fileFormFieldName,
                        FileName = Path.GetFileName(uploadedFile.FileName)
                    };

                    var contentType = !string.IsNullOrEmpty(uploadedFile.ContentType)
                        ? uploadedFile.ContentType
                        : "application/octet-stream";
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    formContent.Add(fileContent, fileFormFieldName, uploadedFile.FileName);

                    // Add additional fields
                    AddAdditionalFields(formContent, additionalFields);

                    // Add authorization
                    AddAuthorizationHeader(_httpClient);

                    // Make request
                    using (var response = await _httpClient.PostAsync(apiEndpointUrl, formContent, cancellationToken))
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        return await ProcessApiResponse<T>(response, responseText);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException<T>(ex);
            }
        }

        // Helper methods
        private (bool IsValid, string ErrorMessage) ValidateUploadInputs(
            HttpPostedFileBase uploadedFile,
            string fileFormFieldName,
            string apiEndpointUrl)
        {
            if (uploadedFile == null || uploadedFile.ContentLength <= 0)
                return (false, "No file was uploaded or the file is empty.");

            if (string.IsNullOrEmpty(fileFormFieldName))
                return (false, "File form field name cannot be null or empty.");

            if (string.IsNullOrEmpty(apiEndpointUrl))
                return (false, "API endpoint URL cannot be null or empty.");

            return (true, null);
        }

        private void AddAdditionalFields(MultipartFormDataContent formContent, Dictionary<string, string> additionalFields)
        {
            if (additionalFields != null)
            {
                foreach (var field in additionalFields.Where(f => !string.IsNullOrEmpty(f.Key)))
                {
                    formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                }
            }
        }

        private async Task<ApiResponse<T>> ProcessApiResponse<T>(HttpResponseMessage response, string responseText)
        {
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"API call failed with status {response.StatusCode}: {responseText}"
                };
            }

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = "API returned empty response."
                };
            }

            try
            {
                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);
                return result ?? new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = "Deserialization returned null result."
                };
            }
            catch (JsonException jsonEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"JSON deserialization failed: {jsonEx.Message}"
                };
            }
        }

        private ApiResponse<T> HandleException<T>(Exception ex)
        {
            if (ex is HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"HTTP request failed: {httpEx.Message}"
                };
            }
            else if (ex is TaskCanceledException tcEx)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Request timeout: {tcEx.Message}"
                };
            }
            else
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }



        /// <summary>
        /// Helper method for logging errors. Replace with your preferred logging mechanism.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception that occurred</param>
        private void LogError(string message, Exception exception = null)
        {
            // Replace with your logging framework (e.g., ILogger, NLog, Serilog, etc.)
            Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");

            if (exception != null)
            {
                Console.WriteLine($"[ERROR] Exception Details: {exception}");
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
        /// <summary>
        /// Uploads a file (and arbitrary additional form fields) to the given API endpoint
        /// using multipart/form-data, then deserializes the response into <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        /// The expected response payload type (e.g., <c>ServiceResponse&lt;SalaryUploadModelSummaryDto&gt;</c>).
        /// </typeparam>
        /// <param name="file">The file to upload (optional). If null, only fields are sent.</param>
        /// <param name="apiUrl">Relative API path (will be combined with the configured base URL).</param>
        /// <param name="fields">
        /// Additional form fields to include in the request. Keys should match the API’s parameter names
        /// (e.g., <c>SalaryType</c>, <c>BranchId</c>, <c>StandingOrderSourceChartOfAccountId</c>, <c>PrivateView</c>).
        /// Null values are skipped.
        /// </param>
        /// <param name="fileFieldName">
        /// The multipart field name for the file part (default: <c>"File"</c>), must match server binder.
        /// </param>
        /// <param name="explicitFileName">
        /// Optional file name to use in the multipart content; if omitted, <see cref="HttpPostedFileBase.FileName"/> is used.
        /// </param>
        /// <param name="explicitContentType">
        /// Optional content type for the file part; if omitted, uses <see cref="HttpPostedFileBase.ContentType"/> or
        /// falls back to <c>application/octet-stream</c>.
        /// </param>
        /// <param name="ct">Cancellation token to cancel the HTTP request.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> wrapping the deserialized response.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via <paramref name="ct"/>.</exception>
        /// <exception cref="Exception">
        /// Rethrows any unexpected exception after logging (e.g., network failure, serialization issues).
        /// </exception>
        public async Task<ApiResponse<T>> UploadFileAsync<T>(
            HttpPostedFileBase file,
            string apiUrl,
            IDictionary<string, string> fields,
            string fileFieldName = "File",
            string explicitFileName = null,
            string explicitContentType = null,
            CancellationToken ct = default)
        {
            try
            {
                // Build the absolute URL and remove accidental double slashes.
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");

                // Prepare multipart/form-data body.
                var formData = new MultipartFormDataContent();

                // Optionally add the file part.
                if (file != null)
                {
                    // Ensure the stream is at the beginning so the whole file is read.
                    if (file.InputStream.CanSeek) file.InputStream.Position = 0;

                    // Create the stream content for the uploaded file.
                    var streamContent = new StreamContent(file.InputStream);

                    // Resolve content type: explicit > provided by HttpPostedFileBase > safe fallback.
                    var contentType = explicitContentType
                                      ?? (!string.IsNullOrWhiteSpace(file.ContentType) ? file.ContentType : "application/octet-stream");
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    // Add file content to the multipart with the server-expected field name and filename.
                    formData.Add(streamContent, fileFieldName, explicitFileName ?? file.FileName);
                }

                // Add additional form fields (skip null values).
                if (fields != null)
                {
                    foreach (var kv in fields.Where(kv => kv.Value != null))
                    {
                        formData.Add(new StringContent(kv.Value), kv.Key);
                    }
                }

                // Attach authorization (e.g., Bearer token) to the HttpClient.
                AddAuthorizationHeader(_httpClient);

                // Execute POST request with cancellation support.
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData, ct);

                // Uniformly handle and deserialize the API response.
                return await HandleResponse<T>(response);
            }
            catch (OperationCanceledException)
            {
                // Surface cancellations to the caller (do not wrap).
                throw;
            }
            catch (Exception ex)
            {
                // Best-effort log; upstream handler may log more context.
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
        public async Task<ApiResponse<T>> UploadBulkCashPaymentFileAsync<T>(HttpPostedFileBase file, string apiUrl)
        {
            try
            {
                // Ensure URL is clean and valid
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");

                var formData = new MultipartFormDataContent();

                // Add file content
                var streamContent = new StreamContent(file.InputStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "File", file.FileName);

                // Add authorization headers
                AddAuthorizationHeader(_httpClient);

                // Send the request
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);

                // Handle the response
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                // Log and rethrow the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
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
        public async Task<AccountingEntriesReportResponse> PostJOurnalEntriesAsync(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleAccountingEntriesReportResponse(response);
            }
            catch (Exception EX)
            {

                throw (EX);

            }
        }
        public async Task<ApiResponse<T>> PostAsync<T>(string apiUrl, object data)
        {
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                // 🛡️ Add Authorization Header
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ApiResponse<List<PostedEntry>>> PostServicesEntryAsync<T>(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponsePostedEntry<ApiResponse<List<PostedEntry>>>(response);
            }
            catch (Exception EX)
            {

                throw (EX);

            }
        }
        public async Task<ApiResponse<List<CashReplenimentRequestDto>>> PostCashReplenishmentEntriesAsync<T>(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponseCashReplenimentRequestDto<ApiResponse<List<CashReplenimentRequestDto>>>(response);
            }
            catch (Exception EX)
            {

                throw (EX);

            }
        }

        public async Task<ApiResponse<List<DepositNotificationDto>>> PostDepositNotificationAsync<T>(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponseDepositNotificationDto<ApiResponse<List<DepositNotificationDto>>>(response);
            }
            catch (Exception EX)
            {

                throw (EX);

            }
        }

        public async Task<ApiResponse<List<InfoAccount>>> PostServicesAsync<T>(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleResponseInfoAccount<ApiResponse<List<InfoAccount>>>(response);
            }
            catch (Exception EX)
            {

                throw (EX);

            }
        }


        public async Task<ApiResponse<AccountResponseDto>> PostAccountResponseAsync<T>(string apiUrl, object data)
        {

            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                return await HandleAccountResponseDto<ApiResponse<AccountResponseDto>>(response);
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
        public async Task<List<BalansheetRpt>> PostBalansheetAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleBalansheetResponse(response);
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
            try
            {
                apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
                string jsonData = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                AddAuthorizationHeader(_httpClient);
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                var Model = await HandleTrialBalance6ColumnResponse(response);
                return Model.data;
            }
            catch (Exception ex)
            {

                throw;
            }
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

        public async Task<List<ModelBalanceSheetAssets>> PostIncomeAndExpenseEntriesAsync(string apiUrl, object data)
        {
            apiUrl = RemoveDuplicateSlashes($"{GetEndpoint(_newbaseURL)}{apiUrl}");
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            var Model = await HandleBalanceSheetAssetsResponse(response);
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
                                //  ApiResponseData = obj,
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

                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden|| response.StatusCode == HttpStatusCode.InternalServerError)
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

        private async Task<ApiResponse<AccountResponseDto>> HandleAccountResponseDto<T>(HttpResponseMessage response)
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
                            return new ApiResponse<AccountResponseDto>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<AccountResponseDto>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<AccountResponseDto>
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

                            message = jsonResponse["message"]?.ToString();
                            var dataMessage = jsonResponse["apiResponseData"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            var obj = JsonConvert.DeserializeObject<AccountResponseDto>(dataMessage);

                            return new ApiResponse<AccountResponseDto>
                            {
                                ApiResponseData = obj,

                                IsSuccess = true,
                                Message = statusDescription + " " + message,
                            };
                        }

                        AccountResponseDto data = JsonConvert.DeserializeObject<AccountResponseDto>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<AccountResponseDto>
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
                                data = JsonConvert.DeserializeObject<AccountResponseDto>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<AccountResponseDto>
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
                                    return new ApiResponse<AccountResponseDto>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<AccountResponseDto>
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
                            return new ApiResponse<AccountResponseDto>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<AccountResponseDto>
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
                                    return new ApiResponse<AccountResponseDto>
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

                                    return new ApiResponse<AccountResponseDto>
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
                                            return new ApiResponse<AccountResponseDto>
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

                                            return new ApiResponse<AccountResponseDto>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<AccountResponseDto>
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

                                    //    return new ApiResponse<List<InfoAccount>>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<List<InfoAccount>>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<AccountResponseDto>
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
                            return new ApiResponse<AccountResponseDto>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<AccountResponseDto>
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
                                return new ApiResponse<AccountResponseDto>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<AccountResponseDto>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<AccountResponseDto>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<AccountResponseDto>
                {
                    IsSuccess = false,
                    Message = $"Error in handling response: {ex.Message}"
                };
            }
        }
        private async Task<ApiResponse<List<InfoAccount>>> HandleResponseInfoAccount<T>(HttpResponseMessage response)
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
                            return new ApiResponse<List<InfoAccount>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<List<InfoAccount>>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<List<InfoAccount>>
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

                            message = jsonResponse["message"]?.ToString();
                            var dataMessage = jsonResponse["apiResponseData"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            var obj = JsonConvert.DeserializeObject<List<InfoAccount>>(dataMessage);

                            return new ApiResponse<List<InfoAccount>>
                            {
                                ApiResponseData = obj,

                                IsSuccess = true,
                                Message = statusDescription + " " + message,
                            };
                        }

                        List<InfoAccount> data = JsonConvert.DeserializeObject<List<InfoAccount>>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<List<InfoAccount>>
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
                                data = JsonConvert.DeserializeObject<List<InfoAccount>>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<List<InfoAccount>>
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
                                    return new ApiResponse<List<InfoAccount>>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<List<InfoAccount>>
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
                            return new ApiResponse<List<InfoAccount>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<List<InfoAccount>>
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
                                    return new ApiResponse<List<InfoAccount>>
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

                                    return new ApiResponse<List<InfoAccount>>
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
                                            return new ApiResponse<List<InfoAccount>>
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

                                            return new ApiResponse<List<InfoAccount>>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<List<InfoAccount>>
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

                                    //    return new ApiResponse<List<InfoAccount>>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<List<InfoAccount>>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<List<InfoAccount>>
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
                            return new ApiResponse<List<InfoAccount>>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<List<InfoAccount>>
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
                                return new ApiResponse<List<InfoAccount>>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<List<InfoAccount>>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<List<InfoAccount>>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<InfoAccount>>
                {
                    IsSuccess = false,
                    Message = $"Error in handling response: {ex.Message}"
                };
            }
        }


        private async Task<ApiResponse<List<PostedEntry>>> HandleResponsePostedEntry<T>(HttpResponseMessage response)
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
                            return new ApiResponse<List<PostedEntry>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<List<PostedEntry>>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<List<PostedEntry>>
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

                            message = jsonResponse["message"]?.ToString();
                            var dataMessage = jsonResponse["apiResponseData"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            var obj = JsonConvert.DeserializeObject<List<PostedEntry>>(dataMessage);

                            return new ApiResponse<List<PostedEntry>>
                            {
                                ApiResponseData = obj,

                                IsSuccess = true,
                                Message = statusDescription + " " + message,
                            };
                        }

                        List<PostedEntry> data = JsonConvert.DeserializeObject<List<PostedEntry>>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<List<PostedEntry>>
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
                                data = JsonConvert.DeserializeObject<List<PostedEntry>>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<List<PostedEntry>>
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
                                    return new ApiResponse<List<PostedEntry>>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<List<PostedEntry>>
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
                            return new ApiResponse<List<PostedEntry>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<List<PostedEntry>>
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
                                    return new ApiResponse<List<PostedEntry>>
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

                                    return new ApiResponse<List<PostedEntry>>
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
                                            return new ApiResponse<List<PostedEntry>>
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

                                            return new ApiResponse<List<PostedEntry>>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<List<PostedEntry>>
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

                                    //    return new ApiResponse<List<PostedEntry>>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<List<PostedEntry>>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};List<PostedEntry>
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<List<PostedEntry>>
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
                            return new ApiResponse<List<PostedEntry>>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<List<PostedEntry>>
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
                                return new ApiResponse<List<PostedEntry>>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<List<PostedEntry>>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<List<PostedEntry>>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PostedEntry>>
                {
                    IsSuccess = false,
                    Message = $"Error in handling response: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse<List<CashReplenimentRequestDto>>> HandleResponseCashReplenimentRequestDto<T>(HttpResponseMessage response)
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
                            return new ApiResponse<List<CashReplenimentRequestDto>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<List<CashReplenimentRequestDto>>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<List<CashReplenimentRequestDto>>
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

                            message = jsonResponse["message"]?.ToString();
                            var dataMessage = jsonResponse["apiResponseData"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            var obj = JsonConvert.DeserializeObject<List<CashReplenimentRequestDto>>(dataMessage);

                            return new ApiResponse<List<CashReplenimentRequestDto>>
                            {
                                ApiResponseData = obj,

                                IsSuccess = true,
                                Message = statusDescription + " " + message,
                            };
                        }

                        List<CashReplenimentRequestDto> data = JsonConvert.DeserializeObject<List<CashReplenimentRequestDto>>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<List<CashReplenimentRequestDto>>
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
                                data = JsonConvert.DeserializeObject<List<CashReplenimentRequestDto>>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<List<CashReplenimentRequestDto>>
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
                                    return new ApiResponse<List<CashReplenimentRequestDto>>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<List<CashReplenimentRequestDto>>
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
                            return new ApiResponse<List<CashReplenimentRequestDto>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<List<CashReplenimentRequestDto>>
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
                                    return new ApiResponse<List<CashReplenimentRequestDto>>
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

                                    return new ApiResponse<List<CashReplenimentRequestDto>>
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
                                            return new ApiResponse<List<CashReplenimentRequestDto>>
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

                                            return new ApiResponse<List<CashReplenimentRequestDto>>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<List<CashReplenimentRequestDto>>
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

                                    //    return new ApiResponse<List<CashReplenimentRequestDto>>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<List<CashReplenimentRequestDto>>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};List<CashReplenimentRequestDto>
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<List<CashReplenimentRequestDto>>
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
                            return new ApiResponse<List<CashReplenimentRequestDto>>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<List<CashReplenimentRequestDto>>
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
                                return new ApiResponse<List<CashReplenimentRequestDto>>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<List<CashReplenimentRequestDto>>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<List<CashReplenimentRequestDto>>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CashReplenimentRequestDto>>
                {
                    IsSuccess = false,
                    Message = $"Error in handling response: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse<List<DepositNotificationDto>>> HandleResponseDepositNotificationDto<T>(HttpResponseMessage response)
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
                            return new ApiResponse<List<DepositNotificationDto>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is requesting authorization token."
                            };
                        }

                        else if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new ApiResponse<List<DepositNotificationDto>>
                            {
                                IsSuccess = false,
                                Message = "InternalServerError unexpected error"
                            };
                        }
                        else
                        {
                            return new ApiResponse<List<DepositNotificationDto>>
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

                            message = jsonResponse["message"]?.ToString();
                            var dataMessage = jsonResponse["apiResponseData"]?.ToString();
                            statusDescription = jsonResponse["statusDescription"]?.ToString();
                            var obj = JsonConvert.DeserializeObject<List<DepositNotificationDto>>(dataMessage);

                            return new ApiResponse<List<DepositNotificationDto>>
                            {
                                ApiResponseData = obj,

                                IsSuccess = true,
                                Message = statusDescription + " " + message,
                            };
                        }

                        List<DepositNotificationDto> data = JsonConvert.DeserializeObject<List<DepositNotificationDto>>(responseData, new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                            DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                        }); //DeserializeJson<T>(responseData);
                        if (responseData.StartsWith("[") && responseData.EndsWith("]"))
                        {
                            return new ApiResponse<List<DepositNotificationDto>>
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
                                data = JsonConvert.DeserializeObject<List<DepositNotificationDto>>(boolResponseJson, new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> { new NullableDoubleConverter() },
                                    DateParseHandling = DateParseHandling.DateTimeOffset // Depending on your date format
                                });

                                return new ApiResponse<List<DepositNotificationDto>>
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
                                    return new ApiResponse<List<DepositNotificationDto>>
                                    {
                                        IsSuccess = false,
                                        ApiResponseData = data,
                                        Message = $"Success: {message}, Description: {statusDescription}"
                                    };
                                }
                                else
                                {
                                    return new ApiResponse<List<DepositNotificationDto>>
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
                            return new ApiResponse<List<DepositNotificationDto>>
                            {
                                IsSuccess = false,
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}"
                            };
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return new ApiResponse<List<DepositNotificationDto>>
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
                                    return new ApiResponse<List<DepositNotificationDto>>
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

                                    return new ApiResponse<List<DepositNotificationDto>>
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
                                            return new ApiResponse<List<DepositNotificationDto>>
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

                                            return new ApiResponse<List<DepositNotificationDto>>
                                            {
                                                IsSuccess = false,
                                                Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                            };
                                        }
                                    }
                                    else
                                    {
                                        // Handle case when there are no error messages in the response
                                        return new ApiResponse<List<DepositNotificationDto>>
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

                                    //    return new ApiResponse<List<DepositNotificationDto>>
                                    //    {
                                    //        IsSuccess = false,
                                    //        Message = string.IsNullOrEmpty(errorMessage) ? "Validation error occurred" : errorMessage
                                    //    };
                                    //}
                                }


                                //// If the error structure doesn't match the expected format or errors object not found
                                //return new ApiResponse<List<DepositNotificationDto>>
                                //{
                                //    IsSuccess = false,
                                //    Message = "Error in request" // Set a generic error message
                                //};List<DepositNotificationDto>
                            }
                            catch (Exception ex)
                            {
                                return new ApiResponse<List<DepositNotificationDto>>
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
                            return new ApiResponse<List<DepositNotificationDto>>
                            {
                                IsSuccess = false,
                                Message = errorMessage
                            };
                        }
                        if (jsonResponse != null && jsonResponse["data"] == null)
                        {
                            return new ApiResponse<List<DepositNotificationDto>>
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
                                return new ApiResponse<List<DepositNotificationDto>>
                                {
                                    IsSuccess = false,
                                    Message = $"Request failed with status code {(int)response.StatusCode}. Error: {string.Join(", ", errorMessages)}"
                                };
                            }
                        }

                        return new ApiResponse<List<DepositNotificationDto>>
                        {
                            IsSuccess = false,
                            Message = $"Request failed with status code {(int)response.StatusCode}"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<List<DepositNotificationDto>>
                    {
                        IsSuccess = false,
                        Message = "No content in the response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DepositNotificationDto>>
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
        private async Task<AccountingEntriesReportResponse> HandleAccountingEntriesReportResponse(HttpResponseMessage response)
        {

            AccountingEntriesReportResponse entries = new AccountingEntriesReportResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<AccountingEntriesReportResponse>(responseData);
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
        //
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
        private async Task<BalansheetServiceResponse> HandleBalansheetResponse(HttpResponseMessage response)
        {

            BalansheetServiceResponse entries = new BalansheetServiceResponse();
            try
            {
                if (response.Content != null)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    entries = JsonConvert.DeserializeObject<BalansheetServiceResponse>(responseData);
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

        //private static void AddAuthorizationHeader(HttpClient client)
        //{
        //    var context = HttpContext.Current;

        //    if (context == null || context.Request == null)
        //        return;

        //    // 🔐 1. Add Authorization from Session Token
        //    string encryptedToken = context.Session["EncryptedJWToken"] as string;
        //    if (!string.IsNullOrEmpty(encryptedToken) && client.DefaultRequestHeaders.Authorization == null)
        //    {
        //        string decryptedToken = TokenEncryptionHelper.DecryptToken(encryptedToken);
        //        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", decryptedToken);
        //    }

        //    // 🔄 2. Copy client headers from the current request

        //    // User-Agent
        //    string userAgent = context.Request.Headers["User-Agent"];
        //    if (!string.IsNullOrWhiteSpace(userAgent) && !client.DefaultRequestHeaders.Contains("User-Agent"))
        //    {
        //        client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        //    }

        //    // Referer
        //    string referer = context.Request.Headers["Referer"];
        //    if (!string.IsNullOrWhiteSpace(referer) && !client.DefaultRequestHeaders.Contains("Referer"))
        //    {
        //        client.DefaultRequestHeaders.Add("Referer", referer);
        //    }

        //    // Origin
        //    string origin = context.Request.Headers["Origin"];
        //    if (!string.IsNullOrWhiteSpace(origin) && !client.DefaultRequestHeaders.Contains("Origin"))
        //    {
        //        client.DefaultRequestHeaders.Add("Origin", origin);
        //    }

        //    // X-Forwarded-For
        //    string xff = GetRealClientIp(context.Request);
        //    if (!string.IsNullOrWhiteSpace(xff) && !client.DefaultRequestHeaders.Contains("X-Forwarded-For"))
        //    {
        //        client.DefaultRequestHeaders.Add("X-Forwarded-For", xff);
        //    }
        //    else
        //    {
        //        // Optional fallback to actual client IP
        //        string ip = context.Request.UserHostAddress;
        //        if (!client.DefaultRequestHeaders.Contains("X-Forwarded-For"))
        //            client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);
        //    }

        //    // Host
        //    string host = context.Request.Headers["Host"];
        //    if (!string.IsNullOrWhiteSpace(host))
        //    {
        //        client.DefaultRequestHeaders.Host = host;
        //    }
        //}


        private static void AddAuthorizationHeader(HttpClient client)
        {
            string encryptedtoken = HttpContext.Current.Session["EncryptedJWToken"] as string;

            // Check if token exists in the session
            if (!string.IsNullOrEmpty(encryptedtoken))
            {
                if (client.DefaultRequestHeaders.Authorization != null)
                {
                    //Parse the existing token if present
                    //var jwtHandler = new JwtSecurityTokenHandler();
                    //var existingTokenData = jwtHandler.ReadToken(TokenEncryptionHelper.DecryptToken(client.DefaultRequestHeaders.Authorization.Parameter)) as JwtSecurityToken;

                    //if (existingTokenData != null && existingTokenData.ValidTo < DateTime.UtcNow)
                    //{
                    //    // Replace the token if it's expired
                    //    client.DefaultRequestHeaders.Remove("Authorization");
                    //    //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    // }
                }
                else
                {
                    string tokendecrypted = TokenEncryptionHelper.DecryptToken(encryptedtoken);
                    // Add the token to the request headers
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokendecrypted);
                }
            }




        }



        public static string GetRealClientIp(HttpRequest request)
        {
            string ip = request?.Headers["X-Forwarded-For"]?.Split(',')?.FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(ip))
                ip = request?.UserHostAddress;

            if (string.IsNullOrWhiteSpace(ip) || ip.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                ip = Guid.NewGuid().ToString("N");
            return ip;
        }


        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public void UploadFileToApiBackground(
    HttpPostedFileBase uploadedFile,
    string fileFormFieldName,
    string apiEndpointUrl,
    Dictionary<string, string> additionalFields = null,
    Action<bool, string> onComplete = null) // Callback for completion
        {
            // Fire and forget - run in background
            Task.Run(async () =>
            {
                bool success = false;
                string message = "";

                try
                {
                    await UploadFileToApiInternal(uploadedFile, fileFormFieldName, apiEndpointUrl, additionalFields);
                    success = true;
                    message = "Upload completed successfully";
                }
                catch (Exception ex)
                {
                    success = false;
                    message = $"Upload failed: {ex.Message}";
                    // Log the exception
                    LogError(ex, "Background file upload failed");
                }
                finally
                {
                    // Notify completion if callback provided
                    onComplete?.Invoke(success, message);
                }
            });
        }

        // Alternative: Return Task for better control
        public Task UploadFileToApiBackgroundAsync(
            HttpPostedFileBase uploadedFile,
            string fileFormFieldName,
            string apiEndpointUrl,
            Dictionary<string, string> additionalFields = null)
        {
            // Return task but don't await - allows fire-and-forget
            return Task.Run(async () =>
            {
                try
                {
                    await UploadFileToApiInternal(uploadedFile, fileFormFieldName, apiEndpointUrl, additionalFields);
                }
                catch (Exception ex)
                {
                    // Log but don't throw - this is background operation
                    LogError(ex, "Background file upload failed");
                }
            });
        }

        // Refactored internal method with proper response handling
        private async Task UploadFileToApiInternal(
            HttpPostedFileBase uploadedFile,
            string fileFormFieldName,
            string apiEndpointUrl,
            Dictionary<string, string> additionalFields = null)
        {
            if (uploadedFile == null || uploadedFile.ContentLength == 0)
                throw new ArgumentException("Invalid file provided");

            apiEndpointUrl = RemoveDuplicateSlashesException($"{GetEndpoint(_newbaseURL)}{apiEndpointUrl}");

            using (var formContent = new MultipartFormDataContent())
            {
                // Create stream content from uploaded file
                using (var streamContent = new StreamContent(uploadedFile.InputStream))
                {
                    // Set content headers properly
                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = $"\"{fileFormFieldName}\"", // Proper quoting
                        FileName = $"\"{Path.GetFileName(uploadedFile.FileName)}\""
                    };

                    // Set content type
                    var contentType = string.IsNullOrEmpty(uploadedFile.ContentType)
                        ? "application/octet-stream"
                        : uploadedFile.ContentType;
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    // Add file content to form
                    formContent.Add(streamContent, fileFormFieldName, uploadedFile.FileName);

                    // Add additional form fields
                    if (additionalFields != null)
                    {
                        foreach (var field in additionalFields.Where(f => !string.IsNullOrEmpty(f.Key)))
                        {
                            formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                        }
                    }

                    // Add authorization header
                    AddAuthorizationHeader(_httpClient);

                    string url = RemoveDuplicateSlashes(apiEndpointUrl);

                    // Send POST request with proper response handling
                    using (var response = await _httpClient.PostAsync(url, formContent))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException($"Upload failed with status {response.StatusCode}: {errorContent}");
                        }

                        // Optional: Read success response
                        var responseContent = await response.Content.ReadAsStringAsync();
                        LogInfo($"Upload successful. Response: {responseContent}");
                    }
                }
            }
        }

        // Usage Examples:

        // 1. Fire-and-forget with callback
        //public void UploadWithCallback()
        //{
        //    UploadFileToApiBackground(uploadedFile, "file", "/api/upload", null,
        //        (success, message) => {
        //            if (success)
        //                Console.WriteLine("Upload completed!");
        //            else
        //                Console.WriteLine($"Upload failed: {message}");
        //        });
        //}

        // 2. Fire-and-forget without waiting
        //public void UploadFireAndForget()
        //{
        //    _ = UploadFileToApiBackgroundAsync(uploadedFile, "file", "/api/upload");
        //    // Method returns immediately, upload runs in background
        //}

        // 3. Background with optional monitoring
        //public async void UploadWithMonitoring()
        //{
        //    var uploadTask = UploadFileToApiBackgroundAsync(uploadedFile, "file", "/api/upload");

        // Do other work immediately
        //DoOtherWork();

        //// Optionally wait for completion later
        //try
        //{
        //    await uploadTask;
        //    ShowSuccessMessage();
        //}
        //catch (Exception ex)
        //{
        //    ShowErrorMessage(ex.Message);
        //}
        //}

        // Helper methods (add to your class)
        private void LogError(Exception ex, string message)
        {
            // Your logging implementation
            Console.WriteLine($"ERROR: {message} - {ex}");
        }

        private void LogInfo(string message)
        {
            // Your logging implementation
            Console.WriteLine($"INFO: {message}");
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