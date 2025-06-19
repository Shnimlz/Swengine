using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace swengine.desktop.Helpers;

public static class WallpaperEngineHelper
{
    private static readonly string WallpaperEnginePath = $"/home/{Environment.UserName}/.local/share/Steam/steamapps/workshop/content/431960/";

    public static async Task<string[]> GetWallpapersAsync()
    {
        if (!Directory.Exists(WallpaperEnginePath))
        {
            throw new DirectoryNotFoundException("No se encontró la carpeta de Wallpaper Engine. Asegúrate de tener instalado Wallpaper Engine por Steam en este usuario.");
        }

        return await Task.Run(() =>
        {
            return Directory.GetFiles(WallpaperEnginePath, "*.mp4", SearchOption.AllDirectories)
                           .Where(file => !file.EndsWith("scene.pkg"))
                           .ToArray();
        });
    }

    public static async Task<(string VideoPath, string ThumbnailPath)[]> GetWallpapersWithThumbnailsAsync()
    {
        if (!Directory.Exists(WallpaperEnginePath))
        {
            throw new DirectoryNotFoundException("No se encontró la carpeta de Wallpaper Engine. Asegúrate de tener instalado Wallpaper Engine por Steam en este usuario.");
        }

        return await Task.Run(() =>
        {
            var videoFiles = Directory.GetFiles(WallpaperEnginePath, "*.mp4", SearchOption.AllDirectories)
                                       .Where(file => !file.EndsWith("scene.pkg"));

            return videoFiles.Select(videoPath =>
            {
                var directory = Path.GetDirectoryName(videoPath);
                var thumbnailPath = Directory.GetFiles(directory!, "*.gif", SearchOption.TopDirectoryOnly)
                                             .Concat(Directory.GetFiles(directory!, "*.png", SearchOption.TopDirectoryOnly))
                                             .Concat(Directory.GetFiles(directory!, "*.jpg", SearchOption.TopDirectoryOnly))
                                             .FirstOrDefault();

                return (VideoPath: videoPath, ThumbnailPath: thumbnailPath ?? videoPath);
            }).ToArray();
        });
    }
    public static async Task<string> ConvertPkgToMp4Async(string scenePkgPath, string outputDirectory)
    {
        var sceneDirectory = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(scenePkgPath));
        Directory.CreateDirectory(sceneDirectory);

        var outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(scenePkgPath) + ".mp4");
        var result = await FfmpegHelper.ConvertSceneToMp4Async(sceneDirectory, outputFilePath);

        return result ?? string.Empty;
    }
}
