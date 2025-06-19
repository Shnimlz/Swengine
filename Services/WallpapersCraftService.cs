using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Services;
public class WallpapersCraftService : IBgsProvider
{

    public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
        {
            try
            {
                return await Scrapers.WallpapersCraftScraper.InfoAsync(Query);
            }
            catch
            {
                throw; // or return a default Wallpaper object
            }
        }


    public async Task<List<WallpaperResponse>> LatestAsync(int Page = 1)
    {

        try
        {
            return await Scrapers.WallpapersCraftScraper.LatestOrSearchAsync(page: Page, function: "latest");
        }
        catch
        {
            throw; // or return a default Wallpaper object
        }
    }

    public async Task<List<WallpaperResponse>> SearchAsync(string Query, int Page = 1)
    {
        try
        {
            return await Scrapers.WallpapersCraftScraper.LatestOrSearchAsync(page: Page, function: "search", query: Query);
        }
        catch
        {
            throw; // or return a default Wallpaper object
        }
    }
}
