using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;

namespace CBS.API.Helper
{
    public static class BarCodeHelper
    {
        public static string GenerateBarcode(string data, int width=300, int height=100)
        {
            // Create a BarcodeWriter instance
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128, // You can choose any barcode format supported by ZXing
                Options = new EncodingOptions
                {
                    Height = height, // Set the height of the barcode
                    Width = width,   // Set the width of the barcode
                    Margin = 0      // Set margin to zero for no border
                }
            };

            // Generate the barcode image
            Bitmap barcodeBitmap = barcodeWriter.Write(data);

            // Convert the barcode image to a Base64 string
            string base64Image = ImageToBase64(barcodeBitmap);

            return base64Image;
        }

        // Function to convert an Image to a Base64 string
        private static string ImageToBase64(Image image)
        {
            // Convert the Image to a byte array
            byte[] imageBytes;
            using (var stream = new System.IO.MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = stream.ToArray();
            }

            // Convert the byte array to a Base64 string
            string base64String = Convert.ToBase64String(imageBytes);

            return base64String;
        }
        public static string GenerateBarcodeImage(string barcodeText)
        {
            // Specify the folder path where you want to save the barcode images
            string folderPath = @"C:\BarCodeImages\BarcodeImages";

            // Check if the folder exists, if not, create it
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create a BarcodeWriter instance
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE, // Choose the barcode format (CODE_128, QR_CODE, etc.)
                Options = new ZXing.Common.EncodingOptions { Width = 100, Height = 100 } // Set the barcode image size
            };

            // Generate the barcode image
            Bitmap barcodeBitmap = writer.Write(barcodeText);

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid().ToString()}.png";

            // Combine folder path and file name to get the full file path
            string filePath = Path.Combine(folderPath, fileName);

            // Save the barcode image to the file path
            barcodeBitmap.Save(filePath, ImageFormat.Png);

            // Return the URL of the saved barcode image
            string baseUrl = "https://yourwebsite.com/BarcodeImages"; // Base URL where the images are hosted
            string imageUrl = $"{baseUrl}/{fileName}";

            return filePath;
        }
    }
}
