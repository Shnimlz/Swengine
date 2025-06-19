using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Services;
public class WallHavenService : IBgsProvider
{
    public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
    {

        try
        {
            return await Scrapers.WallHavenScraper.InfoAsync(Query);
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

            return await Scrapers.WallHavenScraper.LatestOrSearchAsync(page: Page, Function: "latest");
        }
        catch
        {
            throw new Exception("Error al obtener los wallpapers");
        }

    }

    public async Task<List<WallpaperResponse>> SearchAsync(string Query, int Page = 1)
    {
        try
        {

            return await Scrapers.WallHavenScraper.LatestOrSearchAsync(page: Page, Function: "search", Query: Query);
        }
        catch
        {
           throw new Exception("Error al obtener los wallpapers");
        }
    }
}
