using System;
using System.Diagnostics;

namespace Keeys
{
    public class ClipboardService
    {
        // Kopierer tekst til udklipsholderen
        public void CopyToClipboard(string text)
        {
            try
            {
                // På Windows
                if (OperatingSystem.IsWindows())
                {
                    WindowsClipboard(text);
                }
                // På macOS
                else if (OperatingSystem.IsMacOS())
                {
                    MacOSClipboard(text);
                }
                // På Linux
                else if (OperatingSystem.IsLinux())
                {
                    LinuxClipboard(text);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFejl ved kopiering til udklipsholder: {ex.Message}");
                Console.ResetColor();
            }
        }

        // Windows implementation af clipboard
        private void WindowsClipboard(string text)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c echo {text} | clip";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }

        // macOS implementation af clipboard
        private void MacOSClipboard(string text)
        {
            var process = new Process();
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = $"-c \"echo '{text}' | pbcopy\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }

        // Linux implementation af clipboard
        private void LinuxClipboard(string text)
        {
            var process = new Process();
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = $"-c \"echo '{text}' | xclip -selection clipboard\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }
}
