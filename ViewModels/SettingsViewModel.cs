using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System;

namespace Swengine.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        // Extrae automáticamente los nombres de temas definidos en ThemeHelper
        public List<string> AvailableThemes { get; } = Swengine.Helpers.ThemeHelper.GetAllThemes();

        [ObservableProperty]
        private string selectedTheme = "dark";

        private readonly Window _window;

        public static event Action<string>? ThemeChanged;

        public SettingsViewModel(Window window)
        {
            _window = window;
            LoadSettings();
            // Aplica el tema al cargar
            Swengine.Helpers.ThemeHelper.ApplyTheme(_window, SelectedTheme);
        }

        [RelayCommand]
        private void Close()
        {
            _window.Close();
        }

        [RelayCommand]
        private void Apply()
        {
            SaveSettings();
            // Aplica el tema usando el helper
            Swengine.Helpers.ThemeHelper.ApplyTheme(_window, SelectedTheme);
            ThemeChanged?.Invoke(SelectedTheme);
            _window.Close();
        }

        private void LoadSettings()
        {
            var path = GetSettingsPath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data != null)
                {
                    SelectedTheme = AvailableThemes.Contains(data.SelectedTheme ?? "dark") ? data.SelectedTheme ?? "dark" : "dark";
                    //SelectedTheme = AvailableThemes.Contains(data.SelectedTheme) ? data.SelectedTheme : "dark";
                }
            }
        }

        private void SaveSettings()
        {
            var path = GetSettingsPath();
            var data = new SettingsData { SelectedTheme = SelectedTheme };
            File.WriteAllText(path, JsonSerializer.Serialize(data));
        }

        private string GetSettingsPath()
        {
            var folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".config", "swengine");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "settings.json");
        }

        private class SettingsData
        {
            public string? SelectedTheme { get; set; }
        }
    }
}
