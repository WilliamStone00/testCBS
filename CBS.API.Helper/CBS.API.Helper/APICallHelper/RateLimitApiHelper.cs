using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CBS.API.Helper.APICallHelper
{

    public class RateLimitApiHelper
    {
        private readonly string _baseUrl;

        public RateLimitApiHelper(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));

            _baseUrl = baseUrl;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var stringResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(stringResult);
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var stringResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(stringResult);
            }
        }

        /// <summary>
        /// Synchronous GET call for environments where async/await is not supported (e.g., IHttpModule).
        /// </summary>
        //public T GetSync<T>(string endpoint)
        //{
        //    try
        //    {
        //        return GetAsync<T>(endpoint).GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"❌ GetSync failed for endpoint '{endpoint}': {ex.Message}");
        //        return default;
        //    }
        //}
        public ApiResponse<T> GetSync<T>(string apiUrl)
        {
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    _httpClient.BaseAddress = new Uri(_baseUrl);
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = _httpClient.GetAsync(apiUrl).GetAwaiter().GetResult();
                    return HandleResponse<T>(response).GetAwaiter().GetResult();
                }

                
            }
            catch (HttpRequestException ex)
            {
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
            }

            return null;
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
                                Message = $"Request failed with status code {(int)response.StatusCode}, Message: The server is temporally unavailable/unreachable."
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

    }

}
