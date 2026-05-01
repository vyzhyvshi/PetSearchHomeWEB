using System;
using System.Globalization; // Додано для CultureInfo
using System.Security.Cryptography;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome.Infrastructure.Security
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; 
        private const int KeySize = 32;  
        private const int Iterations = 100000; 
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = '|';

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            return $"PBKDF2-SHA256{Delimiter}{Iterations}{Delimiter}{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string hashed)
        {
            if (string.IsNullOrWhiteSpace(hashed)) return false;

            var elements = hashed.Split(Delimiter);
            if (elements.Length != 4) return false;

            var iterations = int.Parse(elements[1], CultureInfo.InvariantCulture);
            var salt = Convert.FromBase64String(elements[2]);
            var hash = Convert.FromBase64String(elements[3]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, _hashAlgorithmName, hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}