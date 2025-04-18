using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MoviesMinimalAPI.Classes;
using System.Security.Cryptography;

namespace MoviesMinimalAPI.Services
{
    public class HashService
    {
        /* A hash is a key that cannot be reversed. This is the right thing to do for secure passwords. The hash
		is what will be stored in the user table. The functions that generate hashes will also
		serve to compare them. A salt is a random value that is appended to the plain text to
		which we want to apply the function that generates the hash. It adds more security because, by joining a random
		salt to the password, the values ​​will always be different. If we generate the password without a salt,
		based only on the password (which is just plain text) the hashes generated based on that
		password will always be the same. The salt must be stored with the password to
		compare the login */

        // Method for generating the salt
        public HashResult Hash(string plainText)
        {
            // We generate the random salt
            var salt = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt); // Generates a random array of bytes
            }

            // We call the ResultHash method and return the hash with the salt
            return Hash(plainText, salt);
        }


        public HashResult Hash(string plainText, byte[] salt)
        {
            //Pbkdf2 is an encryption algorithm
            var derivedKey = KeyDerivation.Pbkdf2(password: plainText,
                salt: salt, prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32);

            var hash = Convert.ToBase64String(derivedKey);

            return new HashResult()
            {
                Hash = hash,
                Salt = salt
            };
        }
    }
}
