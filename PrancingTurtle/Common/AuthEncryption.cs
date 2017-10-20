using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class AuthEncryption
    {
        public static string RandomSalt(int length)
        {
            RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
            byte[] salt = new byte[length];
            csp.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public static string GetRipeMd(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            RIPEMD160Managed rMD = new RIPEMD160Managed();
            byte[] hashed = rMD.ComputeHash(bytes);
            return Convert.ToBase64String(hashed);
        }

        public static bool ValidatePassword(string password, string hash, string salt)
        {
            salt = salt.Substring(0, salt.Length - 1);
            string generatedHash = GetRipeMd(salt + password);
            return generatedHash == hash;
        }

        public static void GenerateHashAndSalt(string password, out string salt, out string hash)
        {
            salt = RandomSalt(24);
            hash = GetRipeMd(salt + password);
            salt += "=";
        }

        public static string RandomFilename()
        {
            return Path.GetRandomFileName().Replace(".", "").ToUpper();
        }
    }
}
