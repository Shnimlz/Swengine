using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Services;
public class MyLiveWallpapersService : IBgsProvider
{
    public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
    {
        try
        {
            return await Scrapers.MyLiveWallpapersScraper.InfoAsync(Query);
        }
        catch
        {
        throw new Exception("Error al obtener los wallpapers");
        }
    }

    public async Task<List<WallpaperResponse>> LatestAsync(int Page = 1)
    {
        try
        {
            return await Scrapers.MyLiveWallpapersScraper.LatestOrSearchAsync(
                    page: Page, function: "latest");
        }
        catch
        {
          throw new Exception("Error al obtener los wallpapers");
        }
    }

    public async Task<List<WallpaperResponse>> SearchAsync(string Query,
                                                           int Page = 1)
    {
        try
        {
            return await Scrapers.MyLiveWallpapersScraper.LatestOrSearchAsync(
                    page: Page, function: "search", query: Query);
        }
        catch
        {
          throw new Exception("Error al obtener los wallpapers");
        }
    }
}
