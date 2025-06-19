using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using swengine.desktop.Models;

namespace swengine.desktop.Scrapers;

public static class MoewallsScraper {
    private static readonly string MoewallsBase = "https://moewalls.com";
    public async static Task<List<WallpaperResponse>>? LatestOrSearchAsync(int Page = 1, string Function = "latest", string Query = "") {
        string url;
        if (Function == "latest") {
            url = MoewallsBase + $"/page/{Page}";
        } else {
            url = MoewallsBase + $"/page/{Page}/?s={Query}";
        }
        List<WallpaperResponse> wallpaper_responses = new();
        var http = HttpClientProvider.Client;
        // string url = MoewallsBase + $"/page/{Page}";
        var request = await http.GetAsync(url);
        if (request.IsSuccessStatusCode) {
            var response = await request.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            var g1_collection_item = htmlDoc.DocumentNode.SelectNodes("//li[contains(@class,'g1-collection-item')]");
            if (g1_collection_item != null)
            {
                foreach (var item in g1_collection_item)
                {
                    try
                    {
                        var imgNode = item.SelectSingleNode(".//img");
                        var aNode = item.SelectSingleNode(".//a[@class='g1-frame']");
                        string? img_src = imgNode?.GetAttributeValue("src", String.Empty);
                        string? title = aNode?.GetAttributeValue("title", String.Empty);
                        string? src = aNode?.GetAttributeValue("href", String.Empty);
                        // Extrae la resolución
                        string? resolution = null;
                        var resNode = item.SelectSingleNode(".//div[contains(@class,'entry-resolutions')]/a");
                        if (resNode != null)
                        {
                            resolution = resNode.InnerText.Trim();
                        }
                        // Asegura que el thumbnail sea una URL absoluta
                        if (!string.IsNullOrEmpty(img_src) && !img_src.StartsWith("http"))
                        {
                            img_src = MoewallsBase + img_src;
                        }
                        if (string.IsNullOrEmpty(img_src))
                        {
                            Console.WriteLine($"[MOEWALLS DEBUG] Thumbnail vacío para: {title} - {src}");
                        }
                        else if (!img_src.StartsWith("http"))
                        {
                            Console.WriteLine($"[MOEWALLS DEBUG] Thumbnail mal formado: {img_src} para {title} - {src}");
                        }
                        wallpaper_responses.Add(new WallpaperResponse {
                            Title = title,
                            Thumbnail = img_src,
                            Src = src,
                            Resolution = resolution
                        });
                    }
                    catch { }
                }
            }
            return wallpaper_responses;
        }
        throw new Exception("Error al obtener los wallpapers");
    }
    public async static Task<Wallpaper> InfoAsync(string Query, string Title) {
        var http = HttpClientProvider.Client;
        var request = await http.GetAsync(Query);
        if (request.IsSuccessStatusCode) {
            string response = await request.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            string source_tag = htmlDoc.DocumentNode.SelectSingleNode("//source[@type='video/mp4']")!.GetAttributeValue("src", String.Empty);
            string download = "https://moewalls.com/download.php?video=" + htmlDoc.DocumentNode.SelectSingleNode("//button[@id='moe-download']")!.GetAttributeValue("data-url", String.Empty);
            string text_xs = "4k";
            return new Wallpaper() {
                Title = Title,
                Resolution = text_xs,
                Preview = source_tag,
                WallpaperType = WallpaperType.Live,
                SourceFile = download,
                NeedsReferrer = true
            };
        }
        throw new Exception("Error al obtener los wallpapers");
    }
}
