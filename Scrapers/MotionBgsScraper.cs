using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using HtmlAgilityPack;

using swengine.desktop.Models;

namespace swengine.desktop.Scrapers;

/*I'm not supposed to create a new instance of HttpClient each time. Will Fix this*/
public static class MotionBgsScraper
{
    private static readonly string MotionBgsBase = "https://www.motionbgs.com";
    public static async Task<List<WallpaperResponse>> LatestAsync(int Page)
    {
        string url = $"{MotionBgsBase}/hx2/latest/{Page}/";
        var http = HttpClientProvider.Client;
        var request = await http.GetAsync(url);
        if (request.IsSuccessStatusCode)
        {
            List<WallpaperResponse> wallpaper_response = new();
            var string_response = await request.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(string_response);
            var a_links = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var aLink in a_links)
            {
                string img_src = MotionBgsBase + aLink.SelectSingleNode(".//img").GetAttributeValue("src", "");
                string title = aLink.SelectSingleNode(".//span[@class='ttl']").InnerHtml;
                string resolution = aLink.SelectSingleNode(".//span[@class='frm']").InnerHtml;
                string src = MotionBgsBase + aLink.GetAttributeValue("href", "");
                wallpaper_response.Add(new()
                {
                    Title = title,
                    Src = src,
                    Thumbnail = img_src,
                    Resolution = resolution
                });
            }
            return wallpaper_response;
        }
        return default;
    }

    public static async Task<Wallpaper> InfoAsync(string Query, string Title)
    {
        var http = HttpClientProvider.Client;
        var request = await http.GetAsync(Query);
        if (request.IsSuccessStatusCode)
        {
            string response = await request.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            string source_tag = MotionBgsBase + htmlDoc.DocumentNode.SelectSingleNode("//source[@type='video/mp4']")
                .GetAttributeValue("src", null);
            string text_xs =
                htmlDoc.DocumentNode.SelectSingleNode("//div[@class='text-xs']").InnerHtml.Split(" ")[0];
            string download = MotionBgsBase + htmlDoc.DocumentNode.SelectSingleNode("//div[@class='download']")
                .SelectSingleNode(".//a").GetAttributeValue("href", null);
            return
                new Wallpaper()
                {
                    Title = Title,
                    Resolution = text_xs,
                    Preview = source_tag,
                    WallpaperType = WallpaperType.Live,
                    SourceFile = download
                };
        }

        return default;
    }

    public static async Task<List<WallpaperResponse>> SearchAsync(string Query, int Page)
    {
        string url = $"{MotionBgsBase}/search?q={Query}&page={Page}";
        Debug.WriteLine($"[DEBUG] Search URL: {url}");
        List<WallpaperResponse> result = new();
        var http = HttpClientProvider.Client;
        var request = await http.GetAsync(url);
        if (request.IsSuccessStatusCode)
        {
            var response = await request.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            // Busca todos los divs con clase "tmb"
            var tmbDivs = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tmb')]");
            if (tmbDivs != null)
            {
                Debug.WriteLine($"[DEBUG] Found {tmbDivs.Count} .tmb divs");
                int wallpaperCount = 0;
                foreach (var tmbDiv in tmbDivs)
                {
                    // Busca todos los <a> dentro de cada div.tmb
                    var alinks = tmbDiv.SelectNodes(".//a");
                    Debug.WriteLine($"[DEBUG] Found {alinks.Count} <a> tags in .tmb div");
                    // Log del HTML del primer .tmb div
                    if (wallpaperCount == 0) {
                        Debug.WriteLine($"[DEBUG] HTML del primer .tmb div:\n{tmbDiv.OuterHtml}");
                    }
                    if (alinks == null)
                    {
                        Debug.WriteLine("[DEBUG] No <a> tags found in .tmb div");
                        Debug.WriteLine($"[DEBUG] HTML de .tmb div sin <a>:\n{tmbDiv.OuterHtml}");
                        continue;
                    }

                    foreach (var alink in alinks)
                    {
                        var imgNode = alink.SelectSingleNode(".//img");
                        string img_src = null;
                        if (imgNode != null)
                        {
                            // 1. Intenta 'data-cfsrc'
                            img_src = imgNode.GetAttributeValue("data-cfsrc", null);
                            // 2. Si no, intenta 'src'
                            if (string.IsNullOrEmpty(img_src))
                                img_src = imgNode.GetAttributeValue("src", null);
                        }
                        // 3. Si sigue vacío, busca <img> dentro de <noscript>
                        if (string.IsNullOrEmpty(img_src))
                        {
                            var noscriptNode = alink.SelectSingleNode(".//noscript");
                            if (noscriptNode != null)
                            {
                                var innerHtml = noscriptNode.InnerHtml;
                                var tempDoc = new HtmlAgilityPack.HtmlDocument();
                                tempDoc.LoadHtml(innerHtml);
                                var innerImg = tempDoc.DocumentNode.SelectSingleNode(".//img");
                                if (innerImg != null)
                                {
                                    img_src = innerImg.GetAttributeValue("src", null);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(img_src) && !img_src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            img_src = MotionBgsBase + (img_src.StartsWith("/") ? img_src : "/" + img_src);

                        bool isValid = !string.IsNullOrEmpty(img_src) &&
                            (img_src.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                             img_src.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                             img_src.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                        Debug.WriteLine($"[DEBUG] isValid: {isValid}");
                        Debug.WriteLine($"[DEBUG] img_src: {img_src}");
                        string thumbnail = isValid ? img_src : "Assets/placeholder.png";

                        var titleNode = alink.SelectSingleNode(".//span[@class='ttl']");
                        string title = titleNode?.InnerText?.Trim();
                        if (string.IsNullOrEmpty(title))
                        {
                            title = alink.GetAttributeValue("title", "");
                        }

                        string src = alink.GetAttributeValue("href", null);
                        if (!string.IsNullOrEmpty(src) && !src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            src = MotionBgsBase + (src.StartsWith("/") ? src : "/" + src);

                        Debug.WriteLine($"[DEBUG] Wallpaper #{wallpaperCount + 1}:");
                        Debug.WriteLine($"    Title: {title}");
                        Debug.WriteLine($"    Thumbnail: {thumbnail}");
                        Debug.WriteLine($"    Src: {src}");
                        Debug.WriteLine($"    IsValidThumbnail: {isValid}");

                        // Extrae resolución de <span class="frm">
var frmNode = alink.SelectSingleNode(".//span[@class='frm']");
string resolution = frmNode?.InnerText?.Trim();

result.Add(new()
                        {
                            Title = title,
                            Thumbnail = thumbnail,
                            Src = src,
                            Resolution = resolution
                        });
                        wallpaperCount++;
                    }
                }
                Debug.WriteLine($"[DEBUG] Total wallpapers extracted: {wallpaperCount}");
            }
            else
            {
                Debug.WriteLine("[DEBUG] No .tmb divs found in HTML!");
                Debug.WriteLine($"[DEBUG] Raw HTML (truncated 2000 chars):\n{response.Substring(0, Math.Min(2000, response.Length))}");
            }

            return result;
        }
        else
        {
            Debug.WriteLine($"[DEBUG] HTTP request failed: {request.StatusCode}");
        }

        return default;
    }
}