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

namespace CBS.API.Helper
{
    public class ApiCallerHelper : IDisposable
    {
        private readonly HttpClient _httpClient;
        string _baseURL = string.Empty;
        public ApiCallerHelper(string baseUrl)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    _httpClient = new HttpClient();
                    _httpClient.BaseAddress = new Uri(baseUrl);
                    _baseURL = baseUrl;

                }
                else
                {
                    throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
                }
            }

        }



        public async Task<ApiResponse<T>> GetAsync<T>(string apiUrl)
        {
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleResponse<T>(response);
        }
        public async Task<ApiResponse<T>> PostImageAsync<T>(string apiUrl, HttpPostedFileBase imageFile)
        {
            try
            {
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

        public async Task<ApiResponse<T>> PostImageAsync<T>(string apiUrl, HttpPostedFileBase imageFile, string loanApplicationId)
        {
            try
            {
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

        public async Task<ApiResponse<T>> PostAsync<T>(string apiUrl, object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            return await HandleResponse<T>(response);
        }
        public ApiResponse<T> Post<T>(string apiUrl, object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = _httpClient.PostAsync(apiUrl, content).GetAwaiter().GetResult();
            return HandleResponse<T>(response).GetAwaiter().GetResult();
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string apiUrl, object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            AddAuthorizationHeader(_httpClient);
            HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string apiUrl)
        {
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
                        return new ApiResponse<T>
                        {
                            IsSuccess = false,
                            Message = "Empty response received"
                        };

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
                            return new ApiResponse<T>
                            {
                                IsSuccess = true,
                                ApiResponseData = data
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
                                statusDescription = jsonResponse["statusDescription"]?.ToString();
                                return new ApiResponse<T>
                                {
                                    IsSuccess = true,
                                    ApiResponseData = data,
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
                        
                        else if (response.StatusCode == HttpStatusCode.BadRequest||response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            //jsonResponse = JObject.Parse(responseData);
                            //message = jsonResponse["message"]?.ToString();
                            //statusDescription = jsonResponse["statusDescription"]?.ToString();
                            ////List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(responseData);
                            //return new ApiResponse<T>
                            //{
                            //    IsSuccess = false,
                            //    Message = $"Request failed with status code {(int)response.StatusCode}, Message: {message}, Description: {statusDescription}"
                            //};

                            T data;
                            try
                            {
                                if (responseData.Contains("\"data\":null"))
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
                                else
                                {
                                    jsonResponse = JObject.Parse(responseData);

                                    if (jsonResponse["errors"] != null)
                                    {
                                        var errorResponse = jsonResponse["errors"].ToObject<Dictionary<string, List<string>>>();
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


                                // If the error structure doesn't match the expected format or errors object not found
                                return new ApiResponse<T>
                                {
                                    IsSuccess = false,
                                    Message = "Error in request" // Set a generic error message
                                };
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


        private static void AddAuthorizationHeader(HttpClient client)
        {
            string token = HttpContext.Current.Session["Token"] as string;

            // Check if token exists in the session
            if (!string.IsNullOrEmpty(token))
            {
                if (client.DefaultRequestHeaders.Authorization != null)
                {
                    // Parse the existing token if present
                    var jwtHandler = new JwtSecurityTokenHandler();
                    var existingTokenData = jwtHandler.ReadToken(client.DefaultRequestHeaders.Authorization.Parameter) as JwtSecurityToken;

                    if (existingTokenData != null && existingTokenData.ValidTo < DateTime.UtcNow)
                    {
                        // Replace the token if it's expired
                        client.DefaultRequestHeaders.Remove("Authorization");
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    }
                }
                else
                {
                    // Add the token to the request headers
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
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