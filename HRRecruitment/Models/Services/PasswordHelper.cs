using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace HRRecruitment.Services
{
    /// <summary>
    /// Simple secure password hashing (PBKDF2). Format: {base64 salt}.{base64 hash}
    /// </summary>
    public static class PasswordHelper
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                Iterations,
                KeySize);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
                return false;

            // Legacy plain-text support (one-time migration path)
            if (!stored.Contains('.'))
                return password == stored;

            var parts = stored.Split('.', 2);
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expected = Convert.FromBase64String(parts[1]);
                byte[] actual = KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    Iterations,
                    KeySize);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsHashed(string stored) =>
            !string.IsNullOrEmpty(stored) && stored.Contains('.');

        public static string GenerateTemporaryPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = chars[bytes[i] % chars.Length];
            return new string(result);
        }
    }
}
