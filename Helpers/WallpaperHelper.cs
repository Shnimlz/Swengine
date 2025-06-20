using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using swengine.desktop.Models;
using swengine.desktop.ViewModels;

namespace swengine.desktop.Helpers;

public static class WallpaperHelper
{
    public async static Task ApplyWallpaperAsync
    (
    Wallpaper wallpaper, 
     ApplicationStatusWrapper applicationStatusWrapper,
    GifQuality selectedResolution,
     string selectedFps, 
    bool bestSettings, 
    string backend,
    CancellationToken token, 
    string? referrer = null
    )
    {
        if(wallpaper == null){
            return;
        }
        long CURRENT_TIMESTAMP = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        applicationStatusWrapper.Status = "Downloading Wallpaper...";
        string? downloadResult =  await DownloadHelper.DownloadAsync(wallpaper.SourceFile!, wallpaper.Title!, wallpaper.NeedsReferrer,referrer!  );
        //if download failed, return and notify user.
        if(downloadResult == null){
            Dispatcher.UIThread.Post(() =>
                {
                    applicationStatusWrapper.Status = "An error occured while dowloading. Please try again.";
            });
            return;
        }
        //if cancelled
         if (token.IsCancellationRequested)
        {
            Debug.WriteLine("Cancellation requested");
            return;
        }
        Dispatcher.UIThread.Post(() =>
         {
            applicationStatusWrapper.Status  = "Download complete. Converting Wallpaper...";
        });

        /**
        *       Download complete begin conversion
        */

        string? convertResult = null;
        if (backend == "SWWW")
        {
            // Convertir a GIF para swww
            convertResult = await FfmpegHelper.ConvertAsync(downloadResult, 0, 5, selectedResolution, fps: int.Parse(selectedFps), bestSettings: bestSettings);
        }
        else if (backend == "MPVPAPER")
        {
            // Convertir a MP4 con opciones para mpvpaper
            string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pictures", "wallpapers", Path.GetFileNameWithoutExtension(downloadResult) + "_mpvpaper.mp4");
            convertResult = await FfmpegHelper.ConvertMp4ToMp4Async(
                downloadResult,
                outputFile,
                width: -1, // Puedes ajustar para soportar resolución personalizada si lo deseas
                height: int.TryParse(selectedResolution.ToString().Replace("q", "").Replace("p", ""), out var h) ? h : 1080,
                fps: int.TryParse(selectedFps, out var f) ? f : 30,
                crf: bestSettings ? 18 : 23
            );
        }
        else
        {
            // Otros backends: usa ConvertAsync como antes
            convertResult = await FfmpegHelper.ConvertAsync(downloadResult, 0, 5, selectedResolution, fps: int.Parse(selectedFps), bestSettings: bestSettings);
        }
        //if conversion failed, return and notify user
        if(convertResult == null){
             Dispatcher.UIThread.Post(() =>
            {
                applicationStatusWrapper.Status = "An error occured while converting. Please try again.";
            });
            return;
        }
        //if canceled
         if (token.IsCancellationRequested)
        {
            Debug.WriteLine("Cancellation requested");
            return;
        }
        Dispatcher.UIThread.Post(() =>{
                applicationStatusWrapper.Status  = "Conversion complete. Applying wallpaper. This might take a while depending on the details of your wallpaper...";

        });


        /**
        *       Conversion complete begin application
        */
        
         await SwwwHelper.ApplyAsync(convertResult,backend);
         long APPLICATION_TIME = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CURRENT_TIMESTAMP;
         TimeSpan applicationTimeSpan = TimeSpan.FromSeconds(APPLICATION_TIME);
         string applicationTimeSpanText = "";
         applicationTimeSpanText = applicationTimeSpan.TotalMinutes > 1 ? $"{applicationTimeSpan.TotalMinutes.ToString("F2")} minute(s)" : $"{applicationTimeSpan.TotalSeconds.ToString("F2")} second(s)";
        
        Dispatcher.UIThread.Post(() =>
        {
            applicationStatusWrapper.Status  = $"Wallpaper Applied Successfully in {applicationTimeSpanText}";
        });
        // Ejemplo de integración desde el ViewModel:
        // var progress = new Progress<double>(val => Progress = val);
        // await WallpaperHelper.ApplyWallpaperAsync(..., progress: progress);
       
    }
}
