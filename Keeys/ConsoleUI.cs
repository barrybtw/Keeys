using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Keeys.Models;

namespace Keeys
{
    public class ConsoleUI
    {
        private readonly AuthenticationService _authService;
        private readonly PasswordManager _passwordManager;
        private readonly ClipboardService _clipboardService;
        private List<Password> _passwords;
        private int _selectedIndex = 0;
        private bool _authenticated = false;

        public ConsoleUI(
            AuthenticationService authService,
            PasswordManager passwordManager,
            ClipboardService clipboardService)
        {
            _authService = authService;
            _passwordManager = passwordManager;
            _clipboardService = clipboardService;
            _passwords = new List<Password>();
        }

        // Kører hovedprogrammet
        public async Task RunAsync()
        {
            Console.Clear();
            Console.CursorVisible = false;
            
            if (!await LoginAsync())
            {
                Console.WriteLine("For mange mislykkede loginforsøg. Afslutter programmet.");
                return;
            }
            
            _authenticated = true;
            await RefreshPasswordListAsync();
            
            await ShowMainMenuAsync();
        }

        // Håndterer login-processen
        private async Task<bool> LoginAsync()
        {
            int attempts = 0;
            const int maxAttempts = 3;
            
            while (attempts < maxAttempts)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════╗");
                Console.WriteLine("║          KEEYS PASSWORD            ║");
                Console.WriteLine("║             MANAGER                ║");
                Console.WriteLine("╚════════════════════════════════════╝");
                Console.ResetColor();
                
                Console.WriteLine("\nIndtast din hovedadgangskode:");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("(Første gang opretter du en ny adgangskode)");
                Console.ResetColor();
                
                string masterPassword = ReadSecureInput();
                
                if (await _authService.VerifyUserAsync(masterPassword))
                {
                    Console.Clear();
                    return true;
                }
                
                attempts++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nForkert adgangskode. Forsøg igen. ({attempts}/{maxAttempts})");
                Console.ResetColor();
                await Task.Delay(1500);
            }
            
            return false;
        }

        // Viser hovedmenuen
        private async Task ShowMainMenuAsync()
        {
            bool running = true;
            
            while (running && _authenticated)
            {
                Console.Clear();
                DrawHeader();
                DrawPasswordList();
                
                var key = Console.ReadKey(true).Key;
                
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (_selectedIndex > 0)
                            _selectedIndex--;
                        break;
                        
                    case ConsoleKey.DownArrow:
                        if (_selectedIndex < _passwords.Count)
                            _selectedIndex++;
                        break;
                        
                    case ConsoleKey.Enter:
                        if (_selectedIndex == _passwords.Count)
                            await AddNewPasswordAsync();
                        else
                            await ShowPasswordDetailsAsync(_passwords[_selectedIndex]);
                        break;
                        
                    case ConsoleKey.Escape:
                        running = false;
                        break;
                }
            }
            
            Console.Clear();
            Console.WriteLine("Tak fordi du brugte Keeys Password Manager. Farvel!");
        }

        // Tegner header
        private void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════╗");
            Console.WriteLine("║      KEEYS PASSWORD MANAGER        ║");
            Console.WriteLine("╚════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine("\nDine gemte adgangskoder:");
            Console.WriteLine("(Brug piletaster ↑↓ til at navigere, Enter for at vælge, Esc for at afslutte)\n");
        }

        // Tegner listen af passwords
        private void DrawPasswordList()
        {
            for (int i = 0; i < _passwords.Count; i++)
            {
                if (i == _selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("  → ");
                    Console.WriteLine(_passwords[i].Name);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("    ");
                    Console.WriteLine(_passwords[i].Name);
                }
            }
            
            // "Tilføj ny" mulighed
            if (_selectedIndex == _passwords.Count)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("  → ");
                Console.WriteLine("[Tilføj ny adgangskode]");
                Console.ResetColor();
            }
            else
            {
                Console.Write("    ");
                Console.WriteLine("[Tilføj ny adgangskode]");
            }
        }

        // Viser detaljer for et specifikt password
        private async Task ShowPasswordDetailsAsync(Password password)
        {
            bool running = true;
            string decryptedPassword = _passwordManager.DecryptPassword(
                password, 
                _authService.CurrentMasterKey);
    
            while (running)
            {
                Console.Clear();
                DrawHeader();
        
                Console.WriteLine($"Navn: {password.Name}");
                Console.WriteLine($"Adgangskode: {decryptedPassword}");
        
                Console.WriteLine("\nTryk 'c' for at kopiere adgangskoden til udklipsholderen");
                Console.WriteLine("Tryk 'Backspace' for at gå tilbage til listen");
        
                var key = Console.ReadKey(true).Key;
        
                switch (key)
                {
                    case ConsoleKey.C:
                        _clipboardService.CopyToClipboard(decryptedPassword);
                
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n✓ Adgangskoden er kopieret til udklipsholderen!");
                        Console.ResetColor();
                        Console.WriteLine("\nTryk på en vilkårlig tast for at fortsætte...");
                        Console.ReadKey(true);
                        break;
                
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        running = false;
                        break;
                }
            }
    
            await RefreshPasswordListAsync();
        }
        
        // Tilføjer et nyt password
        private async Task AddNewPasswordAsync()
        {
            Console.Clear();
            DrawHeader();
            
            Console.Write("Indtast navn på den nye adgangskode: ");
            Console.CursorVisible = true;
            string name = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.CursorVisible = false;
                Console.WriteLine("\nNavnet må ikke være tomt. Tryk en tast for at prøve igen...");
                Console.ReadKey(true);
                return;
            }
            
            Console.Write("Indtast adgangskode: ");
            string password = ReadSecureInput();
            
            await _passwordManager.AddPasswordAsync(
                name, 
                password, 
                _authService.CurrentMasterKey);
            
            Console.CursorVisible = false;
            await RefreshPasswordListAsync();
        }

        // Opdaterer password-listen
        private async Task RefreshPasswordListAsync()
        {
            _passwords = await _passwordManager.GetAllPasswordsAsync();
            _selectedIndex = Math.Min(_selectedIndex, _passwords.Count);
        }

        // Læser input uden at vise tegn (til passwords)
        private string ReadSecureInput()
        {
            Console.CursorVisible = true;
            string input = "";
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.Backspace)
                {
                    input += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            Console.CursorVisible = false;
            return input;
        }
    }
}
