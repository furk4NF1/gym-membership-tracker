using System;
using System.Security.Cryptography;
using System.Text;

namespace SporSalonuTakip.Helpers
{
    internal static class TemporaryPasswordGenerator
    {
        public static string Generate()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%*?";
            string all = upper + lower + digits + special;
            char[] password = new char[12];
            byte[] bytes = new byte[password.Length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            password[0] = upper[bytes[0] % upper.Length];
            password[1] = lower[bytes[1] % lower.Length];
            password[2] = digits[bytes[2] % digits.Length];
            password[3] = special[bytes[3] % special.Length];
            for (int i = 4; i < password.Length; i++) password[i] = all[bytes[i] % all.Length];
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = bytes[i] % (i + 1); char temp = password[i]; password[i] = password[j]; password[j] = temp;
            }
            return new String(password);
        }
    }
}
