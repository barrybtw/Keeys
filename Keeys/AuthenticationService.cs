using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Keeys
{
    public class AuthenticationService
    {
        private readonly DatabaseService _databaseService;
        private readonly EncryptionService _encryptionService;
        private string _currentMasterKey;

        public AuthenticationService(
            DatabaseService databaseService, 
            EncryptionService encryptionService)
        {
            _databaseService = databaseService;
            _encryptionService = encryptionService;
        }

        public string CurrentMasterKey => _currentMasterKey;

        // Verificerer brugeren via hovedadgangskoden
        public async Task<bool> VerifyUserAsync(string masterPassword)
        {
            var storedHash = await _databaseService.GetMasterPasswordHashAsync();
            
            // Første gang programmet bruges - opret en hovedadgangskode
            if (string.IsNullOrEmpty(storedHash))
            {
                var _salt = _encryptionService.GenerateSalt();
                var hash = _encryptionService.HashPassword(masterPassword, _salt);
                await _databaseService.SaveMasterPasswordHashAsync(hash, _salt);
                _currentMasterKey = masterPassword;
                return true;
            }
            
            // Verificer eksisterende adgangskode
            var salt = await _databaseService.GetMasterPasswordSaltAsync();
            var inputHash = _encryptionService.HashPassword(masterPassword, salt);
            
            if (inputHash == storedHash)
            {
                _currentMasterKey = masterPassword;
                return true;
            }
            
            return false;
        }
    }
}