using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper
{
    public static class EncryptionHelper
    {
        public static string Encrypt(string data, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // Initialization Vector, should be randomly generated and stored securely

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes;

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }

                    encryptedBytes = msEncrypt.ToArray();
                }

                return Convert.ToBase64String(encryptedBytes);
            }
        }
        public static string Decrypt(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // Initialization Vector, should be retrieved securely

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                using (var msDecrypt = new System.IO.MemoryStream(cipherTextBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        public static string GenerateAesKeyAsString(int integerValue)
        {
            // Convert the integer to bytes
            byte[] intBytes = BitConverter.GetBytes(integerValue);

            // Use a unique salt (you should use a different salt for each key)
            byte[] salt = Encoding.UTF8.GetBytes("UniqueSalt123");

            // Use PBKDF2 to derive a secure key from the integer and salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(intBytes, salt, 10000, HashAlgorithmName.SHA256))
            {
                // Generate a 128-bit key
                byte[] aesKey = pbkdf2.GetBytes(16);

                // Convert the key to a string representation
                return BitConverter.ToString(aesKey).Replace("-", "");
            }
        }
    }
   
    public class IntegrityHelper
    {
        public static string ComputeHash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
