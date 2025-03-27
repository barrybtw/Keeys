using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Keeys.Models;

namespace Keeys
{
    public class DatabaseService
    {
        private readonly string _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Keeys",
            "passwords.db");
            
        private readonly string _connectionString;

        public DatabaseService()
        {
            var directory = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            _connectionString = $"Data Source={_dbPath}";
        }

        // Initialiserer databasen med nødvendige tabeller
        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS Passwords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    EncryptedPassword TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL
                );";
            
            await command.ExecuteNonQueryAsync();
        }

        // Gemmer hash af hovedadgangskoden
        public async Task SaveMasterPasswordHashAsync(string passwordHash, string salt)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('MasterPasswordHash', $hash);
                INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('MasterPasswordSalt', $salt);";
            
            command.Parameters.AddWithValue("$hash", passwordHash);
            command.Parameters.AddWithValue("$salt", salt);
            
            await command.ExecuteNonQueryAsync();
        }

        // Henter hash af hovedadgangskoden
        public async Task<string> GetMasterPasswordHashAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Key = 'MasterPasswordHash';";
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        // Henter salt til hovedadgangskoden
        public async Task<string> GetMasterPasswordSaltAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Key = 'MasterPasswordSalt';";
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        // Tilføjer et nyt password til databasen
        public async Task AddPasswordAsync(string name, string encryptedPassword)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Passwords (Name, EncryptedPassword, CreatedAt, ModifiedAt)
                VALUES ($name, $password, $created, $modified);";
            
            var now = DateTime.UtcNow.ToString("o");
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$password", encryptedPassword);
            command.Parameters.AddWithValue("$created", now);
            command.Parameters.AddWithValue("$modified", now);
            
            await command.ExecuteNonQueryAsync();
        }

        // Henter alle passwords fra databasen
        public async Task<List<Password>> GetAllPasswordsAsync()
        {
            var passwords = new List<Password>();
            
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, EncryptedPassword, CreatedAt, ModifiedAt FROM Passwords;";
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                passwords.Add(new Password
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    EncryptedPassword = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    ModifiedAt = DateTime.Parse(reader.GetString(4))
                });
            }
            
            return passwords;
        }
    }
}
    