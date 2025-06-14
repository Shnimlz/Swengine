using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using swengine.desktop.ViewModels;
using swengine.desktop.Helpers;

namespace swengine.desktop.Views;

public partial class ApplyWindow : Window
{
    public ApplyWindow()
    {
        InitializeComponent();
        video.Loaded += ((sender, args) =>
        {
            var datacontext = DataContext as ApplyWindowViewModel;
            video.MediaPlayer = datacontext.MediaPlayer;
        });
        Closed += (sender, args) =>
        {
            //stop all players
            (DataContext as ApplyWindowViewModel).MediaPlayer.Stop();
          
        };
    }

    private async void LoadWallpapers(object? sender, RoutedEventArgs e)
    {
        var wallpapers = await WallpaperEngineHelper.GetWallpapersAsync();
        if (wallpapers.Length == 0)
        {
            // Handle no wallpapers found
            WallpaperList.ItemsSource = new[] { "No wallpapers found." };
            return;
        }
        // Display wallpapers in the UI
        WallpaperList.ItemsSource = wallpapers;
    }

    private void ApplyWallpaper(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as ApplyWindowViewModel;
        
        // Call LoadWallpapers to load and display wallpapers
        LoadWallpapers(sender, e);
    }
}