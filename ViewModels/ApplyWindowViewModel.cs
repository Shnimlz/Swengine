using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Data;

using CommunityToolkit.Mvvm.ComponentModel;

using FluentAvalonia.UI.Controls;

using LibVLCSharp.Shared;

using swengine.desktop.Helpers;
using swengine.desktop.Models;
using swengine.desktop.Services;

namespace swengine.desktop.ViewModels;

public partial class ApplyWindowViewModel : DialogViewModelBase
{
    private WallpaperResponse? _wallpaperResponse;
    public required IBgsProvider BgsProvider { get; set; }
    public required string Backend { get; set; }
    [ObservableProperty]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ApplyWindowViewModel))]
    private GifQuality selectedResolution = GifQuality.q1080p;
    [ObservableProperty] private string selectedFps = "30";

    [ObservableProperty]
    private bool bestSettings = false;
    [ObservableProperty] private bool isVideoVisible = true;
    [ObservableProperty] private ApplicationStatusWrapper applicationStatusWrapper = new();
    private readonly LibVLC _libVlc = new LibVLC("--avcodec-hw=none");

    public ApplyWindowViewModel()
    {
        MediaPlayer = new MediaPlayer(_libVlc);
    }
    public MediaPlayer MediaPlayer { get; }
    [ObservableProperty] private Wallpaper wallpaper = null!;
    
    public WallpaperResponse WallpaperResponse
    {
        get { return _wallpaperResponse ?? throw new InvalidOperationException("WallpaperResponse is null"); }
        set
        {
            SetProperty(ref _wallpaperResponse, value);
            ObjectCreated();
        }
    }
    //Called when the WallpaperResponse object is set while the window is opening
    public async void ObjectCreated()
    {
        try
        {
            Wallpaper = await BgsProvider.InfoAsync(WallpaperResponse.Src!, Title: WallpaperResponse.Title!);
            using var media = new Media(_libVlc, new Uri(Wallpaper.Preview!));
            MediaPlayer.Play(media);
            MediaPlayer.Volume = 0;
        }
        catch { }
    }

    //Apply wallpaper. Will be abstracted for Both Live and static wallpaper
    public async void ApplyWallpaper()
    {
        //dialog cannot draw over video, so hide video when dialog is about to display
        IsVideoVisible = false;
        if (Wallpaper == null)
        {
            ContentDialog warningDialog = new()
            {
                Title = "Warning",
                Content = "Wallpaper information is still loading. Please try again",
                CloseButtonText = "Dismiss"
            };
            await Task.Delay(1000);
            IsVideoVisible = false;
            await warningDialog.ShowAsync();
            IsVideoVisible = true;
            return; 
        }
        ContentDialog dialog = new()
        {
            Title = "Apply this wallpaper",
            PrimaryButtonText = "Apply",
            IsPrimaryButtonEnabled = true,
            Content = ApplyDialogContent()
        };
        dialog.Closed += (sender, args) =>
        {
            //show the video again when dialog is closing
            IsVideoVisible = true;
        };
        var dialogResponse = await dialog.ShowAsync();

        if (dialogResponse == ContentDialogResult.Primary)
        {
            dialog.Hide();
            await Task.Delay(1000);
            IsVideoVisible = false;
            var applicationStatusDialog = new ContentDialog()
            {
                Title = "Applying Wallpaper",
                CloseButtonText = "Stop"
            };
            applicationStatusDialog.Bind(ContentDialog.ContentProperty, new Binding()
            {
                Path = "ApplicationStatusWrapper.Status",
                Source = this,
                Mode = BindingMode.TwoWay,
            });
            applicationStatusDialog.Closed += (sender, args) =>
            {
                IsVideoVisible = true;
            };
            CancellationTokenSource ctx = new();
            applicationStatusDialog.Opened += (sender, args) =>
            {
                _ = Task.Run(() =>
                {
                    _ = WallpaperHelper.ApplyWallpaperAsync(
                        wallpaper: Wallpaper,
                        applicationStatusWrapper: ApplicationStatusWrapper,
                        selectedResolution: SelectedResolution,
                        selectedFps: SelectedFps,
                        bestSettings: BestSettings,
                        backend: Backend,
                        token: ctx.Token,
                        referrer: WallpaperResponse.Src);
                });
            };
            applicationStatusDialog.Closed += (sender, args) =>
            {
                ctx.Cancel();
            };
            await applicationStatusDialog.ShowAsync();


        }

    }

    private async Task ShowApplyDialog()
    {
        ContentDialog dialog = new()
        {
            Title = "Apply this wallpaper",
            PrimaryButtonText = "Apply",
            IsPrimaryButtonEnabled = true,
            Content = ApplyDialogContent()
        };
        dialog.Closed += (sender, args) =>
        {
            // Show the video again when dialog is closing
            IsVideoVisible = true;
        };
        var dialogResponse = await dialog.ShowAsync();

        if (dialogResponse == ContentDialogResult.Primary)
        {
            dialog.Hide();
            await Task.Delay(1000);
            IsVideoVisible = false;
            var applicationStatusDialog = new ContentDialog()
            {
                Title = "Applying Wallpaper",
                CloseButtonText = "Stop"
            };
            applicationStatusDialog.Bind(ContentDialog.ContentProperty, new Binding()
            {
                Path = "ApplicationStatusWrapper.Status",
                Source = this,
                Mode = BindingMode.TwoWay,
            });
            applicationStatusDialog.Closed += (sender, args) =>
            {
                IsVideoVisible = true;
            };
            CancellationTokenSource ctx = new();
            applicationStatusDialog.Opened += (sender, args) =>
            {
                _ = Task.Run(() =>
                {
                   _ = WallpaperHelper.ApplyWallpaperAsync(
                        wallpaper: Wallpaper,
                        applicationStatusWrapper: ApplicationStatusWrapper,
                        selectedResolution: SelectedResolution,
                        selectedFps: SelectedFps,
                        bestSettings: BestSettings,
                        backend: Backend,
                        token: ctx.Token,
                        referrer: WallpaperResponse.Src);
                });
            };
            applicationStatusDialog.Closed += (sender, args) =>
            {
                ctx.Cancel();
            };
            await applicationStatusDialog.ShowAsync();
        }
    }
}

public class DesignApplyWindowViewModel : ApplyWindowViewModel
{
    public DesignApplyWindowViewModel()
    {
        Wallpaper = new()
        {
            Title = "Garp With Galaxy Impact",
            Preview = "https://www.motionbgs.com/media/6384/garp-with-galaxy-impact.960x540.mp4",
            WallpaperType = WallpaperType.Live,
            Resolution = "Resolution\":\"3840x2160"
        };

    }
}
public partial class ApplicationStatusWrapper : ObservableObject
{
    [ObservableProperty]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ApplicationStatusWrapper))]
    private string? status;
}