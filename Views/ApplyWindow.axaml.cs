using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using swengine.desktop.ViewModels;
using swengine.desktop.Helpers;
using System;

namespace swengine.desktop.Views;

public partial class ApplyWindow : Window
{
    public ApplyWindow()
    {
        InitializeComponent();
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