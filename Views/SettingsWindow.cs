using Avalonia.Controls;
using Avalonia;
using Swengine.ViewModels;
using System;
using Avalonia.Markup.Xaml;
using Swengine.Helpers;
using Avalonia.Media;

namespace swengine.desktop.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            AvaloniaXamlLoader.Load(this);
            // Inicializa colores por defecto
            var themeColors = ThemeHelper.GetThemeColors(GetCurrentTheme());
            SetMainBackgroundColor(themeColors.MainBg);
            SetTextForegroundColor(themeColors.TextColor);

            this.Opened += (_, __) =>
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            };

            Swengine.ViewModels.SettingsViewModel.ThemeChanged += OnThemeChanged;
            ApplyTheme(GetCurrentTheme());

            DataContext = new SettingsViewModel(this);

    #if DEBUG
            this.AttachDevTools();
    #endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        /// <summary>
        /// Cambia el color del texto global para el tema actual
        /// </summary>
        /// <param name="color">Color en formato hexadecimal (#RRGGBB o #AARRGGBB)</param>
        private void SetTextForegroundColor(string color)
        {
            if (this.Resources.ContainsKey("TextForegroundColor"))
            {
                var brush = this.Resources["TextForegroundColor"] as Avalonia.Media.SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = Avalonia.Media.Color.Parse(color);
                }
                else
                {
                    this.Resources["TextForegroundColor"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color));
                }
            }
            else
            {
                this.Resources.Add("TextForegroundColor", new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color)));
            }
        }

        /// <summary>
        /// Cambia el color de fondo principal de la ventana
        /// </summary>
        private void SetMainBackgroundColor(Avalonia.Media.Color color)
        {
            if (this.Resources.ContainsKey("MainBackgroundColor"))
            {
                var brush = this.Resources["MainBackgroundColor"] as Avalonia.Media.SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = color;
                }
                else
                {
                    this.Resources["MainBackgroundColor"] = new Avalonia.Media.SolidColorBrush(color);
                }
            }
            else
            {
                this.Resources.Add("MainBackgroundColor", new Avalonia.Media.SolidColorBrush(color));
            }
        }

        /// <summary>
        /// Devuelve los colores del tema (MainBg, SecondaryBg, TextColor)
        /// </summary>

        /// <summary>
        /// Aplica el tema actual a la ventana
        /// </summary>
        private void ApplyTheme(string theme)
        {
            var themeColors = ThemeHelper.GetThemeColors(theme);
            SetMainBackgroundColor(themeColors.MainBg);
            SetSecondaryBackgroundColor(themeColors.SecondaryBg);
            SetTextForegroundColor(themeColors.TextColor);
        }

        private void SetSecondaryBackgroundColor(Color secondaryBg)
        {
            if (this.Resources.ContainsKey("SecondaryBackgroundColor"))
            {
                var brush = this.Resources["SecondaryBackgroundColor"] as Avalonia.Media.SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = secondaryBg;
                }
                else
                {
                    this.Resources["SecondaryBackgroundColor"] = new Avalonia.Media.SolidColorBrush(secondaryBg);
                }
            }
            else
            {
                this.Resources.Add("SecondaryBackgroundColor", new Avalonia.Media.SolidColorBrush(secondaryBg));
            }
        }

        /// <summary>
        /// Evento de cambio de tema
        /// </summary>
        private void OnThemeChanged(string theme)
        {
            ApplyTheme(theme);
        }

        /// <summary>
        /// Obtiene el tema actual desde el ViewModel o configuración
        /// </summary>
        private string GetCurrentTheme()
        {
            // Leer el tema desde el archivo de configuración
            try
            {
                var folder = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".config", "swengine");
                var path = System.IO.Path.Combine(folder, "settings.json");
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    var data = System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json);
                    if (data != null && !string.IsNullOrEmpty(data.SelectedTheme))
                        return data.SelectedTheme;
                }
            }
            catch { }
            return "dark"; // Valor por defecto
        }

        private class SettingsData
        {
            public string? SelectedTheme { get; set; }
        }



    }
}
