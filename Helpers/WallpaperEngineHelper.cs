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
            return Array.Empty<string>();
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
            return Array.Empty<(string, string)>();
        }

        return await Task.Run(() =>
        {
            var videoFiles = Directory.GetFiles(WallpaperEnginePath, "*.mp4", SearchOption.AllDirectories)
                                       .Where(file => !file.EndsWith("scene.pkg"));

            return videoFiles.Select(videoPath =>
            {
                var directory = Path.GetDirectoryName(videoPath);
                var thumbnailPath = Directory.GetFiles(directory, "*.gif", SearchOption.TopDirectoryOnly)
                                             .Concat(Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly))
                                             .Concat(Directory.GetFiles(directory, "*.jpg", SearchOption.TopDirectoryOnly))
                                             .FirstOrDefault();

                return (VideoPath: videoPath, ThumbnailPath: thumbnailPath ?? videoPath);
            }).ToArray();
        });
    }

    public static async Task<List<string>> ProcessScenesAsync(string wallpaperEnginePath)
    {
        var sceneFiles = Directory.GetFiles(wallpaperEnginePath, "scene.pkg", SearchOption.AllDirectories);
        var validScenes = new List<string>();

        foreach (var sceneFile in sceneFiles)
        {
            var sceneDirectory = Path.Combine(Path.GetDirectoryName(sceneFile), Path.GetFileNameWithoutExtension(sceneFile));
            Directory.CreateDirectory(sceneDirectory);

            // Simulate extraction
            // Assume we have a method ExtractPkg that extracts the pkg file
            // ExtractPkg(sceneFile, sceneDirectory);

            if (!ContainsMouseInteraction(sceneDirectory))
            {
                validScenes.Add(sceneDirectory);
            }
        }

        return validScenes;
    }

    private static bool ContainsMouseInteraction(string sceneDirectory)
    {
        var interactionFiles = Directory.GetFiles(sceneDirectory, "*.json", SearchOption.AllDirectories)
                                        .Concat(Directory.GetFiles(sceneDirectory, "*.cfg", SearchOption.AllDirectories));
        foreach (var file in interactionFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("mouse") || content.Contains("Mouse"))
            {
                return true;
            }
        }
        return false;
    }

    public static async Task<string> ConvertPkgToMp4Async(string scenePkgPath, string outputDirectory)
    {
        var sceneDirectory = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(scenePkgPath));
        Directory.CreateDirectory(sceneDirectory);

        // Simulate extraction
        // Assume we have a method ExtractPkg that extracts the pkg file
        // ExtractPkg(scenePkgPath, sceneDirectory);

        // Convert extracted resources to MP4 using FfmpegHelper
        var outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(scenePkgPath) + ".mp4");
        var result = await FfmpegHelper.ConvertSceneToMp4Async(sceneDirectory, outputFilePath);

        return result;
    }
}
