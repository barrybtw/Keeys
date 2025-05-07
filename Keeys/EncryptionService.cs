using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Keeys
{
    public class EncryptionService
    {
        // Genererer tilfældig salt til password hashing
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Hashing af adgangskoden med salt
        public string HashPassword(string password, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Convert.FromBase64String(salt),
                10000,
                HashAlgorithmName.SHA256);
            
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        // Krypterer en password-string med hovedadgangskoden
        public string EncryptPassword(string plainPassword, string masterPassword)
        {
            byte[] key = DeriveKeyFromPassword(masterPassword);
            
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); // Genererer en ny IV for hver kryptering
            
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            // Gem IV i starten af den krypterede data
            ms.Write(aes.IV, 0, aes.IV.Length);
            
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainPassword);
                cs.Write(plainBytes, 0, plainBytes.Length);
            }
            
            return Convert.ToBase64String(ms.ToArray());
        }

        // Dekrypterer en password-string med hovedadgangskoden
        public string DecryptPassword(string encryptedPassword, string masterPassword)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedPassword);
            byte[] key = DeriveKeyFromPassword(masterPassword);
            
            using Aes aes = Aes.Create();
            aes.Key = key;
            
            // Få IV fra starten af den krypterede data
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream();
            
            // Skip IV i krypteret data
            using (var cs = new CryptoStream(
                new MemoryStream(encryptedData, iv.Length, 
                encryptedData.Length - iv.Length),
                decryptor,
                CryptoStreamMode.Read))
            {
                using var reader = new StreamReader(cs, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        // Udleder en krypteringsnøgle fra hovedadgangskoden
        private byte[] DeriveKeyFromPassword(string password)
        {
            // Salt bruges kun til demo - i realiteten bør salt gemmes sikkert
            string fixedSalt = "KeyesSaltForEncryption";
            byte[] saltBytes = Encoding.UTF8.GetBytes(fixedSalt);
            
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, 
                saltBytes, 
                10000, 
                HashAlgorithmName.SHA256);
            
            return pbkdf2.GetBytes(32); // 256 bit nøgle
        }
    }
}
