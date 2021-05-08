using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace ManageUsers.Helper
{
    internal static class PasswordHelper
    {
        internal static string HashThePassword(string password, byte[] salt, bool needsOnlyHash)
        {
            if (salt == null || salt.Length != 16)
            {
                salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
            }

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if (needsOnlyHash) return hashed;

            return $"{hashed}:{Convert.ToBase64String(salt)}";
        }

        internal static bool VerifyThePassword(string passwordToCheck, string hashedPasswordWithSalt)
        {

            var passwordAndHash = hashedPasswordWithSalt.Split(':');
            if (passwordAndHash == null || passwordAndHash.Length != 2)
                return false;
            var salt = Convert.FromBase64String(passwordAndHash[1]);
            if (salt == null)
                return false;

            var hashOfpasswordToCheck = HashThePassword(passwordToCheck, salt, true);

            if (String.Compare(passwordAndHash[0], hashOfpasswordToCheck) == 0)
                return true;

            return false;
        }


        internal static string RandomGenerator(int length)
        {
            string characters = @"abcdefghijklmnopqursuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@$!%*#?&/><+=)(}¤:;.,{_£§-";

            string password = "";

            Random random = new Random();

            for (int i = 0; i < length; i++)
                password += characters[random.Next(characters.Length - 1)];

            return password;
        }
    }
}
