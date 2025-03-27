using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keeys.Models;

namespace Keeys
{
    public class PasswordManager
    {
        private readonly DatabaseService _databaseService;
        private readonly EncryptionService _encryptionService;

        public PasswordManager(
            DatabaseService databaseService,
            EncryptionService encryptionService)
        {
            _databaseService = databaseService;
            _encryptionService = encryptionService;
        }

        // Tilføjer et nyt password
        public async Task AddPasswordAsync(string name, string plainPassword, string masterPassword)
        {
            var encryptedPassword = _encryptionService.EncryptPassword(plainPassword, masterPassword);
            await _databaseService.AddPasswordAsync(name, encryptedPassword);
        }

        // Henter alle password-navne
        public async Task<List<Password>> GetAllPasswordsAsync()
        {
            return await _databaseService.GetAllPasswordsAsync();
        }

        // Dekrypterer et specifikt password
        public string DecryptPassword(Password password, string masterPassword)
        {
            return _encryptionService.DecryptPassword(password.EncryptedPassword, masterPassword);
        }
    }
}