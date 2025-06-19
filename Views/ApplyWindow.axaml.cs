using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using swengine.desktop.ViewModels;
using swengine.desktop.Helpers;
using System;
using Avalonia.Media;
using Swengine.Helpers;

namespace swengine.desktop.Views;

public partial class ApplyWindow : Window
{
    public ApplyWindow()
    {
        InitializeComponent();
         // Inicializa los colores por defecto
       AvaloniaXamlLoader.Load(this);
        Swengine.ViewModels.SettingsViewModel.ThemeChanged += OnThemeChanged;
        ThemeHelper.ApplyTheme(this, GetCurrentTheme());
        Loaded += ((sender, args) =>
        {
            var datacontext = DataContext as ApplyWindowViewModel;
            
            // SOLUCIÓN: Obtener referencia al control VideoView usando el nombre correcto del XAML
            var videoView = this.FindControl<VideoView>("video");
            if (videoView != null && datacontext?.MediaPlayer != null)
            {
                videoView.MediaPlayer = datacontext.MediaPlayer;
            }
        });
        
        Closed += (sender, args) =>
        {
            // Stop all players
            (DataContext as ApplyWindowViewModel)!.MediaPlayer.Stop();
        };
    }

    

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

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
    
    private void SetMainBackgroundColor(IBrush brush)
    {
        if (this.Resources.ContainsKey("MainBackgroundColor"))
        {
            this.Resources["MainBackgroundColor"] = brush;
        }
        else
        {
            this.Resources.Add("MainBackgroundColor", brush);
        }
        var mainGrid = this.FindControl<Grid>("MainGrid");
        if (mainGrid != null)
        {
            mainGrid.Background = brush;
        }
    }
    private void OnThemeChanged(string newTheme)
    {
        ThemeHelper.ApplyTheme(this, newTheme);
    }

    private void ApplyTheme(string theme)
    {
        var (mainBg, secondaryBg, textColor, mainBgGrad, secondaryBgGrad) = ThemeHelper.GetThemeColors(theme);
        var mainGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop { Color = mainBgGrad.Item1, Offset = 0 },
                new GradientStop { Color = mainBgGrad.Item2, Offset = 1 }
            }
        };
        SetMainBackgroundColor(mainGradient);
        SetSecondaryBackgroundColor(secondaryBg);
        SetTextForegroundColor(textColor);
    }


    private void SetOrUpdateBrushResource(string key, Avalonia.Media.Color color)
    {
        if (this.Resources.ContainsKey(key))
        {
            if (this.Resources[key] is Avalonia.Media.SolidColorBrush brush)
                brush.Color = color;
            else
                this.Resources[key] = new Avalonia.Media.SolidColorBrush(color);
        }
        else
        {
            this.Resources.Add(key, new Avalonia.Media.SolidColorBrush(color));
        }
    }

    public string GetCurrentTheme()
    {
        try
        {
            var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".config", "swengine", "settings.json");
            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("SelectedTheme", out var themeProp))
                {
                    return themeProp.GetString() ?? "Default";
                }
            }
        }
        catch { }
        return "Default";
    }

    private static Avalonia.Media.Color GetColorForTheme(string theme)
    {
        return theme switch
        {
            _ => Avalonia.Media.Color.Parse("#24293E"), // Oscuro por defecto
        };
    }

    private async void LoadWallpapers(object? sender, RoutedEventArgs e)
    {
        var wallpapers = await WallpaperEngineHelper.GetWallpapersAsync();
        if (wallpapers.Length == 0)
        {
            // Handle no wallpapers found
            var wallpaperList = this.FindControl<ListBox>("WallpaperList");
            if (wallpaperList != null)
            {
                wallpaperList.ItemsSource = new[] { "No wallpapers found." };
            }
            return;
        }
        
        // Display wallpapers in the UI
        var wallpaperListControl = this.FindControl<ListBox>("WallpaperList");
        if (wallpaperListControl != null)
        {
            wallpaperListControl.ItemsSource = wallpapers;
        }
    }

    private void ApplyWallpaper(object? sender, RoutedEventArgs e)
    {
        _ = DataContext as ApplyWindowViewModel;
        // Call LoadWallpapers to load and display wallpapers
        LoadWallpapers(sender, e);
    }
}