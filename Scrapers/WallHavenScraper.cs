using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using swengine.desktop.Models;
using swengine.desktop.Scrapers;

namespace swengine.desktop.Scrapers
{
    public static class WallHavenScraper
    {
        private static readonly string WallHavenBase = "https://wallhaven.cc";

        // Devuelve un HttpClient con cookies y headers de navegador para el scraping
        private static HttpClient GetBrowserLikeHttpClient()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer, AutomaticDecompression = DecompressionMethods.All };
            var baseUri = new Uri(WallHavenBase);

            // ⚠️ Cambia estos valores por tus cookies reales si expiran
            cookieContainer.Add(baseUri, new Cookie("cf_clearance", "cHeGuH05egMVra2efoY07XCzvT7RzsaocP5bdSWu_EY-1747431197-1.2.1.1-491dlsEHMafE7ZUh4rd74qW33y.x4Nhsflxa3uXqlTsh3ev5_lJra.l99jXSrxcf.e_Kv4gLpd6stWjWUR0IpzqGvSXGsOClOiHMXOumMFjKZtCzdkMXFR6g80U5kuttGeH_uDfEr8HZEqarMIkiw9tWiDPwSIoo6o4xISWwi4yZhdufobUKLLbXjl2ky7PQSFZEIwxW4SoLjiQdKjFCPFMmRctHRs6CLUOPZjxn0mnRU2duZN3simXzIDJm2cRYcH3O2c5bOwJO8WX.UisWjwaO0p8Haid.RNFBh2z_KGYYicUiUMeQIIi6osLLV_11EawhChlsqHx0Jfei5Fa1d2PwS1hYW8yY8cIgpyHdkh4"));
            cookieContainer.Add(baseUri, new Cookie("wallhaven_session", "eyJpdiI6Ik9oTmNqbDk1UnNnbGNGRE1FUTVJSnc9PSIsInZhbHVlIjoiR0t6aFdFNFRYbDByNlZ3TUQ4b3E4WHpyRlQ4K1BmU25GSHFXWGVoXC9FMmJiUk82cjFoXC80OWZlQXlXNDRhM1d0IiwibWFjIjoiYjRiMWM3MzFlY2Y0N2YyN2YyMzc4ZTRmM2YyNWY4OTJjM2NjNWRhOGE2NzU2ZjNkZTFlNjBkNzIzY2IzZDNkNCJ9"));
            // Puedes añadir más cookies si lo necesitas

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:139.0) Gecko/20100101 Firefox/139.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "es-MX,es;q=0.8,en-US;q=0.5,en;q=0.3");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("TE", "trailers");
            // Puedes añadir más headers si hace falta
            return client;
        }

        public static async Task<List<WallpaperResponse>> LatestOrSearchAsync(int page = 1, string Function = "latest", string Query = "")
        {
            string url;
            if (Function == "latest")
            {
                url = $"{WallHavenBase}/search?categories=111&purity=100&resolutions=2560x1080%2C1280x720%2C1280x800%2C3440x1440%2C1600x900%2C1600x1000%2C3840x1600%2C1920x1080%2C1920x1200%2C2560x1440%2C2560x1600%2C3840x2160%2C3840x2400&sorting=date_added&order=desc&ai_art_filter=1";
            }
            else if (Function == "random")
            {
                url = $"{WallHavenBase}/random?page={page}";
            }
            else if (Function == "NSFW")
            {
                url = $"{WallHavenBase}/search?categories=111&purity=001&resolutions=2560x1080%2C1280x720%2C1280x800%2C3440x1440%2C1600x900%2C1600x1000%2C3840x1600%2C1920x1080%2C1920x1200%2C2560x1440%2C2560x1600%2C3840x2160%2C3840x2400&sorting=date_added&order=desc&ai_art_filter=1";
            }
            else
            {
                url = $"{WallHavenBase}/search?q={Query}&categories=110&purity=100&sorting=date_added&order=desc&ai_art_filter=1&page={page}&ai_art_filter=0";
            }

            List<WallpaperResponse> responses = new();
            var http = GetBrowserLikeHttpClient();

            using var request = await http.GetAsync(url);
            if (request.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new WallHavenApiException(429, "Has alcanzado el límite de 45 llamadas por minuto a la API de WallHaven. Espera un momento antes de intentar de nuevo.");
            }
            if (request.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new WallHavenApiException(401, "Error de autenticación con la API de WallHaven. Puede que estés intentando acceder a contenido NSFW sin una cookie válida.");
            }
            if (request.IsSuccessStatusCode)
            {
                var response = await request.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);
                var figures = htmlDoc.DocumentNode.SelectNodes("//figure[contains(@class,'thumb')]");
                if (figures != null)
                {
                    foreach (var figure in figures)
                    {
                        string src = figure.SelectSingleNode(".//a")?.GetAttributeValue("href", null);
                        string title = "Wallhaven Wallpaper";
                        string img_src = figure.SelectSingleNode(".//img")?.GetAttributeValue("data-src", null);
                        if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(img_src))
                        {
                            responses.Add(new WallpaperResponse
                            {
                                Title = title,
                                Src = src,
                                Thumbnail = img_src
                            });
                        }
                    }
                }
                return responses;
            }
            return default;
        }

        public static async Task<Wallpaper> InfoAsync(string url)
        {
            var http = GetBrowserLikeHttpClient();

            using var request = await http.GetAsync(url);
            if (request.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new WallHavenApiException(429, "Has alcanzado el límite de 45 llamadas por minuto a la API de WallHaven. Espera un momento antes de intentar de nuevo.");
            }
            if (request.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new WallHavenApiException(401, "Error de autenticación con la API de WallHaven. Puede que estés intentando acceder a contenido NSFW sin una cookie válida.");
            }
            if (request.IsSuccessStatusCode)
            {
                var response = await request.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                string source = htmlDoc.DocumentNode.SelectSingleNode("//img[@id='wallpaper']")?.GetAttributeValue("data-cfsrc", null);
                string title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerHtml?.Trim() ?? "Wallpaper";

                return new Wallpaper
                {
                    Title = title,
                    Resolution = null,
                    Preview = source,
                    SourceFile = source,
                    WallpaperType = WallpaperType.Static
                };
            }

            return default;
        }
    }
}
