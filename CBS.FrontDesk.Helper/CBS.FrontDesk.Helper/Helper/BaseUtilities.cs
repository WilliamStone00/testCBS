using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper
{


    public static class BaseUtilities
    {
        /// <summary>
        /// Generates a unique insurance number with a specific prefix and maximum size.
        /// </summary>
        /// <param Name="maxSize">Maximum size of the generated number.</param>
        /// <param Name="prefix">Prefix for the generated number.</param>
        /// <returns>Generated insurance number.</returns>
        public static string GenerateInsuranceUniqueNumber(int maxSize, string prefix)
        {
            const string chars = "1234567890";
            return GenerateUniqueNumber(maxSize, chars, prefix);
        }

        /// <summary>
        /// Generates a unique number with a specific maximum size.
        /// </summary>
        /// <param Name="maxSize">Maximum size of the generated number.</param>
        /// <returns>Generated number.</returns>
        public static string GenerateUniqueNumber(int maxSize)
        {
            const string chars = "1234567890";
            return GenerateUniqueNumber(maxSize, chars);
        }
        /// <summary>
        /// Generates a unique number up to 15 digits.
        /// </summary>
        /// <returns>Generated number.</returns>
        public static string GenerateUniqueNumber()
        {
            const string chars = "1234567890";
            return GenerateUniqueNumber(15, chars);
        }

        /// <summary>
        /// Adds '237' prefix to the MSISDN if it doesn't already start with it.
        /// </summary>
        /// <param Name="msisdn">Mobile number without prefix.</param>
        /// <returns>MSISDN with '237' prefix.</returns>
        public static string Add237Prefix(string msisdn)
        {
            const string prefix = "237";
            if (!msisdn.StartsWith(prefix))
            {
                msisdn = $"{prefix}{msisdn}";
            }
            return msisdn;
        }

        private static string GenerateUniqueNumber(int maxSize, string allowedChars, string prefix = "")
        {
            char[] chars = allowedChars.ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return $"{prefix}{result.ToString()}";
        }
        /// <summary>
        /// Converts a DateTime to UTC if it's in Unspecified kind.
        /// </summary>
        /// <param Name="dateTime">The DateTime value.</param>
        /// <returns>UTC representation of the input DateTime.</returns>
        public static DateTime ToUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dateTime.ToUniversalTime();
        }
        public static int RoundUpToNearestWholeNumber(double number)
        {
            var value = Math.Ceiling(number);
            return Convert.ToInt32(value);
        }
        /// <summary>
        /// Converts a nullable DateTime from UTC to local time if it's in Unspecified kind.
        /// </summary>
        /// <param Name="dateTime">The nullable DateTime value in UTC.</param>
        /// <returns>Local time representation of the input DateTime, or null if input is null.</returns>
        public static DateTime? UtcToLocal(this DateTime? dateTime)
        {
            if (dateTime.HasValue && dateTime.Value.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc).ToLocalTime();
            }
            return dateTime;
        }
        public static string FormatAsCurrency(double value)
        {
            // Round the value to one decimal place
            double roundedValue = Math.Round(value, 1);

            // Format the rounded value as a string with one decimal place
            string formattedValue = roundedValue.ToString("##,#0.0");

            return formattedValue;
        }
        public static DateTime? UtcToLocal()
        {
            DateTime? dateTime = DateTime.UtcNow;
            if (dateTime.HasValue && dateTime.Value.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc).ToLocalTime();
            }
            return dateTime;
        }
        public static DateTime NewDate()
        {
            var dateTime = new DateTime();
            return dateTime;
        }
    }

}
