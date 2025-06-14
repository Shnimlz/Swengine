using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using swengine.desktop.Models;

namespace swengine.desktop.Services
{
    public class WallpaperEngineSceneTestService : IBgsProvider
    {
        private static readonly string WallpaperEnginePath = $"/home/{System.Environment.UserName}/.local/share/Steam/steamapps/workshop/content/431960/";

        public async Task<List<string>> GetAllWallpapersAndScenesAsync()
        {
            if (!Directory.Exists(WallpaperEnginePath))
            {
                return new List<string>();
            }

            return await Task.Run(() =>
            {
                var videoFiles = Directory.GetFiles(WallpaperEnginePath, "*.mp4", SearchOption.AllDirectories);
                var sceneFiles = Directory.GetFiles(WallpaperEnginePath, "scene.pkg", SearchOption.AllDirectories);

                return videoFiles.Concat(sceneFiles).ToList();
            });
        }

        public async Task<List<WallpaperResponse>> LatestOrSearchAsync(int Page = 1, string Function = "latest", string Query = "")
        {
            var allFiles = await GetAllWallpapersAndScenesAsync();
            return allFiles.Select(file =>
            {
                var directory = Path.GetDirectoryName(file);
                var thumbnailPath = Directory.GetFiles(directory, "preview.gif", SearchOption.TopDirectoryOnly)
                                             .Concat(Directory.GetFiles(directory, "preview.png", SearchOption.TopDirectoryOnly))
                                             .Concat(Directory.GetFiles(directory, "preview.jpg", SearchOption.TopDirectoryOnly))
                                             .FirstOrDefault();

                return new WallpaperResponse
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    Src = file,
                    Thumbnail = thumbnailPath ?? file // Use the file itself if no preview is found
                };
            }).ToList();
        }

        public async Task<List<WallpaperResponse>> LatestAsync(int Page)
        {
            return await LatestOrSearchAsync(Page);
        }

        public async Task<List<WallpaperResponse>> SearchAsync(string Query, int Page)
        {
            var allFiles = await GetAllWallpapersAndScenesAsync();
            return allFiles.Where(file => Path.GetFileNameWithoutExtension(file).ToLower().Contains(Query.ToLower()))
                .Select(file => new WallpaperResponse
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    Src = file,
                    Thumbnail = file // Using the same file as thumbnail for simplicity
                }).ToList();
        }

        public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
        {
            return new Wallpaper
            {
                Title = Title,
                Preview = Query,
                SourceFile = Query,
                WallpaperType = WallpaperType.Live
            };
        }

        private bool ContainsMouseInteraction(string sceneDirectory)
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

        public async Task<List<string>> ExtractAndFilterScenesAsync()
        {
            var sceneFiles = Directory.GetFiles(WallpaperEnginePath, "scene.pkg", SearchOption.AllDirectories);
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
    }
}
