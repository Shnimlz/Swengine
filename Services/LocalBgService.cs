using System.Collections.Generic;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Services;
public class LocalBgService : IBgsProvider
{
   public async Task<Wallpaper> InfoAsync(string Query, string Title = "")
        {
            // Simulación de una operación asincrónica
            await Task.Delay(100);
            return new Wallpaper(){
                NeedsReferrer = false,
                Preview = Query,
                SourceFile = Query,
                Title = Query,
            };
        }

    public Task<List<WallpaperResponse>> LatestAsync(int Page = 1)
    {
        throw new System.NotImplementedException();
    }

    public Task<List<WallpaperResponse>> SearchAsync(string Query, int Page = 1)
    {
        throw new System.NotImplementedException();
    }
}