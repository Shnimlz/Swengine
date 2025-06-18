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
        process.StartInfo.FileName = "pacman";
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
        if (results.ValueKind == JsonValueKind.Object && results.TryGetProperty("Version", out var versionProp))
            return versionProp.GetString();
        return null;
    }

    public static async Task<bool> IsUpdateAvailableAsync(string packageName) {
        var installed = await GetInstalledVersionAsync(packageName);
        var aur = await GetAurVersionAsync(packageName);
        if (installed == null || aur == null) return false;
        return installed != aur;
    }

    public static void LaunchUpdate(string packageName) {
        var process = new Process();
        process.StartInfo.FileName = "x-terminal-emulator"; // Cambia a tu terminal preferido si es necesario
        process.StartInfo.Arguments = $"-e 'yay -Syu {packageName}'";
        process.Start();
    }
}
