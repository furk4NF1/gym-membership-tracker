using System;
using System.Security.Cryptography;
using System.Text;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip.Helpers
{
    internal static class PasswordHasher
    {
        public const int Iterations = 120000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static PasswordCredential Create(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            return new PasswordCredential { Salt = salt, Iterations = Iterations, Hash = ToHex(Pbkdf2(password, salt, Iterations, HashSize)) };
        }

        public static bool Verify(string password, PasswordCredential credential)
        {
            if (credential == null || credential.Salt == null || credential.Iterations < 1) return false;
            byte[] expected;
            try { expected = FromHex(credential.Hash); } catch { return false; }
            byte[] actual = Pbkdf2(password, credential.Salt, credential.Iterations, expected.Length);
            return FixedTimeEquals(actual, expected);
        }

        public static bool VerifyLegacySha256(string password, string expectedHex)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] actual = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                string actualHex = ToHex(actual);
                return String.Equals(actualHex, expectedHex.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputLength)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);
            byte[] output = new byte[outputLength];
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                int blockCount = (outputLength + 31) / 32;
                int offset = 0;
                for (int block = 1; block <= blockCount; block++)
                {
                    byte[] input = new byte[salt.Length + 4];
                    Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
                    input[input.Length - 4] = (byte)(block >> 24);
                    input[input.Length - 3] = (byte)(block >> 16);
                    input[input.Length - 2] = (byte)(block >> 8);
                    input[input.Length - 1] = (byte)block;
                    byte[] u = hmac.ComputeHash(input);
                    byte[] result = (byte[])u.Clone();
                    for (int i = 1; i < iterations; i++)
                    {
                        u = hmac.ComputeHash(u);
                        for (int j = 0; j < result.Length; j++) result[j] ^= u[j];
                    }
                    int take = Math.Min(result.Length, outputLength - offset);
                    Buffer.BlockCopy(result, 0, output, offset, take);
                    offset += take;
                }
            }
            Array.Clear(key, 0, key.Length);
            return output;
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int difference = 0;
            for (int i = 0; i < a.Length; i++) difference |= a[i] ^ b[i];
            return difference == 0;
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder text = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) text.Append(value.ToString("x2"));
            return text.ToString();
        }

        private static byte[] FromHex(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length % 2 != 0) throw new FormatException();
            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            return bytes;
        }
    }
}
