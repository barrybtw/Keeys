using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Keeys
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Keeys Password Manager";
            
            // Initialiser services
            var databaseService = new DatabaseService();
            await databaseService.InitializeAsync();
            
            var encryptionService = new EncryptionService();
            var authService = new AuthenticationService(databaseService, encryptionService);
            var passwordManager = new PasswordManager(databaseService, encryptionService);
            var clipboardService = new ClipboardService();
            var consoleUI = new ConsoleUI(authService, passwordManager, clipboardService);
            
            // Start applikationen
            await consoleUI.RunAsync();
        }
    }
}