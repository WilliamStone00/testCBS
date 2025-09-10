using System;
using System.Web.Security;

namespace CBS.API.Helper
{


    public static class TokenEncryptionHelper
    {
        public static string EncryptToken(string token)
        {
            byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            byte[] encryptedBytes = MachineKey.Protect(tokenBytes, "JWTToken");
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptToken(string encryptedToken)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedToken);
            byte[] decryptedBytes = MachineKey.Unprotect(encryptedBytes, "JWTToken");
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
    }


}