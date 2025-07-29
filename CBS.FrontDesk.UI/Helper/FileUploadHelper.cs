using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class FileUploadHelper
    {
        public static async Task<string> PostFileToApiAsync(
            string apiUrl,
            HttpPostedFileBase file,
            string fileFieldName,
            Dictionary<string, string> additionalFormData = null)
        {
            if (file == null || file.ContentLength == 0)
                throw new ArgumentException("File is missing or empty.");

            using (var client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                // Add file part
                var fileContent = new StreamContent(file.InputStream);
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = $"\"{fileFieldName}\"",
                    FileName = $"\"{Path.GetFileName(file.FileName)}\""
                };
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, fileFieldName, file.FileName);

                // Add additional form fields
                if (additionalFormData != null)
                {
                    foreach (var kvp in additionalFormData)
                    {
                        content.Add(new StringContent(kvp.Value ?? ""), kvp.Key);
                    }
                }

                // POST to API
                var response = await client.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error uploading file: {response.StatusCode} - {responseContent}");

                return responseContent;
            }
        }
    }

}