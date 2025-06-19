using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using swengine.desktop.Helpers;
using swengine.desktop.Models;

namespace swengine.desktop.Services;

public class WallpaperEngineService : IBgsProvider
{
    public async Task<List<WallpaperResponse>> LatestOrSearchAsync(int Page = 1, string Function = "latest", string Query = "")
    {
        var wallpapersWithThumbnails = await WallpaperEngineHelper.GetWallpapersWithThumbnailsAsync();
        return wallpapersWithThumbnails.Select(wt => new WallpaperResponse
        {
            Title = Path.GetFileNameWithoutExtension(wt.VideoPath),
            Src = wt.VideoPath,
            Thumbnail = wt.ThumbnailPath
        }).ToList();
    }

    public async Task<List<WallpaperResponse>> LatestAsync(int Page)
    {
        return await LatestOrSearchAsync(Page);
    }

    public async Task<List<WallpaperResponse>> SearchAsync(string Query, int Page)
    {
        return await LatestOrSearchAsync(Page, "search", Query);
    }

   public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
    {
    // For Wallpaper Engine, the Query is the file path
        return await Task.FromResult(new Wallpaper
            {
                Title = Title,
                Preview = Query, // Assuming the preview is the same as the source for simplicity
                SourceFile = Query,
                WallpaperType = WallpaperType.Live
            });
    }
}
