using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using swengine.desktop.Models;
using swengine.desktop.Scrapers;

namespace swengine.desktop.Services;

public class MoewallsService : IBgsProvider
{
    public async Task<List<WallpaperResponse>> LatestAsync(int Page = 1)
    {
        try
        {
            return await MoewallsScraper.LatestOrSearchAsync(Page, Function: "latest")!;
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
            return await MoewallsScraper.InfoAsync(Query, Title);
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
            return await MoewallsScraper.LatestOrSearchAsync(Page, Function: "search", Query: Query)!;
        }
        catch
        {
            throw new Exception("Error al obtener los wallpapers");
        }
    }
}
