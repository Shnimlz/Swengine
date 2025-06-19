using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace swengine.desktop.Services;

public static class AurUpdateService {
    public static async Task<string?> GetInstalledVersionAsync(string packageName) {
        var process = new Process();
        process.StartInfo.FileName = "yay";
        process.StartInfo.Arguments = $"-Qi {packageName}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();
        var match = Regex.Match(output, @"Version\s*:\s*(\S+)");
        return match.Success ? match.Groups[1].Value : null;
    }

        public static async Task<string?> GetAurVersionAsync(string packageName) {
            using var client = new HttpClient();
            var url = $"https://aur.archlinux.org/rpc/?v=5&type=info&arg={packageName}";
            var response = await client.GetStringAsync(url);
            using var json = JsonDocument.Parse(response);
            var results = json.RootElement.GetProperty("results");
            if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
            {
                var firstResult = results[0];
                if (firstResult.TryGetProperty("Version", out var versionProp))
                    return versionProp.GetString();
            }
            return null;
        }
        
    public class AurVersionInfo
    {
        public string? Installed { get; set; }
        public string? Aur { get; set; }
        public bool UpdateAvailable => Installed != null && Aur != null && Installed != Aur;
    }

    public static async Task<AurVersionInfo> GetAurVersionStatusAsync(string packageName)
    {
        var installed = await GetInstalledVersionAsync(packageName);
        var aur = await GetAurVersionAsync(packageName);
        return new AurVersionInfo
        {
            Installed = installed,
            Aur = aur
        };
    }

    public static async Task<bool> IsUpdateAvailableAsync(string packageName) {
        var installed = await GetInstalledVersionAsync(packageName);
        var aur = await GetAurVersionAsync(packageName);
        if (installed == null || aur == null) return false;
        return installed != aur;
    }

    public static void LaunchUpdate(string packageName) {
        try {
            var process = new Process();
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = $"-c \"yay -Syu {packageName}\"";
            process.Start();
        } catch (Exception ex) {
            Console.WriteLine($"Error al lanzar la actualización: {ex.Message}");
        }
    }
}
