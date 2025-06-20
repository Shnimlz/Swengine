
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Services;

public class MotionBgsService : IBgsProvider
{
    public async Task<List<WallpaperResponse>> LatestAsync(int Page)
    {
        try
        {
            return await Scrapers.MotionBgsScraper.LatestAsync(Page);
        }
        catch
        {
             throw new Exception("Error al obtener los wallpapers");
        }
    }

    public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
    {
        try
        {

            return await Scrapers.MotionBgsScraper.InfoAsync(Query, Title);
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
            return await Scrapers.MotionBgsScraper.SearchAsync(Query, Page);
        }
        catch
        {
            throw new Exception("Error al obtener los wallpapers");
        }
    }
}
