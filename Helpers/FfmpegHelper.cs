using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using swengine.desktop.Models;

namespace swengine.desktop.Helpers;

public static class FfmpegHelper
{
    /*
    *ConvertAsync will return the location of the converted file if successful, or null if unsuccesful
    **/
    public async static Task<string?> ConvertAsync(string file, double startAt = 0, double endAt = 5, GifQuality quality = GifQuality.q1080p, int fps = 60, bool bestSettings = false)
    {
        try
        {
            // Force garbage collection before heavy processing
            GC.Collect();
            GC.WaitForPendingFinalizers();

            string home = Environment.GetEnvironmentVariable("HOME");

            //if file is not a video, dont bother converting. Just return the image.
            if (Path.GetExtension(file).ToLower() != ".mp4" && Path.GetExtension(file).ToLower() != ".mkv")
            {
                string copyTo = home + "/Pictures/wallpapers/" + file.Split("/").Last();
                File.Copy(file, copyTo, true);
                if (!File.Exists(copyTo))
                    return null;
                File.Delete(file);
                return copyTo;
            }
            string convertTo = home + "/Pictures/wallpapers/" + file.Split("/").Last().Split(".").First() + ".gif";
            string tmpPalette = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_palette.png");

            try
            {
                // 1. Crear paleta de colores optimizada directamente del video original
                var paletteProcess = new Process
                {
                    StartInfo = new()
                    {
                        FileName = "ffmpeg",
                        Arguments = bestSettings
                            ? $"-threads 2 -hwaccel none -i \"{file}\" -vf \"palettegen=reserve_transparent=0:max_colors=256:stats_mode=diff\" -y \"{tmpPalette}\""
                            : $"-threads 2 -hwaccel none -ss {startAt} -t {endAt} -i \"{file}\" -vf \"scale=-1:{QualityParser(quality)}:flags=lanczos,fps={Math.Min(fps, 24)},palettegen=reserve_transparent=0:max_colors=256:stats_mode=diff\" -y \"{tmpPalette}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                await RunProcessWithTimeoutAsync(paletteProcess, TimeSpan.FromMinutes(3));

                if (!File.Exists(tmpPalette))
                {
                    return null;
                }

                // 2. Convertir a GIF usando la paleta optimizada para calidad de wallpaper, directamente del video original
                string gifArgs = bestSettings
                    ? $"-threads 2 -hwaccel none -i \"{file}\" -i \"{tmpPalette}\" -lavfi \"[0:v][1:v]paletteuse=dither=sierra2_4a:diff_mode=rectangle:alpha_threshold=128\" -loop 0 -y \"{convertTo}\""
                    : $"-threads 2 -hwaccel none -ss {startAt} -t {endAt} -i \"{file}\" -i \"{tmpPalette}\" -lavfi \"scale=-1:{QualityParser(quality)}:flags=lanczos,fps={Math.Min(fps, 24)}[scaled];[scaled][1:v]paletteuse=dither=sierra2_4a:diff_mode=rectangle:alpha_threshold=128\" -loop 0 -y \"{convertTo}\"";

                var convertProcess = new Process
                {
                    StartInfo = new()
                    {
                        FileName = "ffmpeg",
                        Arguments = gifArgs,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                await RunProcessWithTimeoutAsync(convertProcess, TimeSpan.FromMinutes(10));

                //if gif does not exist then conversion failed. Return false
                if (!File.Exists(convertTo))
                {
                    return null;
                }

                //if everything went smoothly, delete the mp4.
                File.Delete(file);
                return convertTo;
            }
            finally
            {
                // Clean up temporary files including palette
                CleanupTempFiles(tmpPalette, Path.Combine(Path.GetTempPath(), "*_palette.png"));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ConvertAsync: {ex.Message}");
            return null;
        }
    }

    private static async Task RunProcessWithTimeoutAsync(Process process, TimeSpan timeout)
    {
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        // Wait for process with timeout
        var completedWithinTimeout = process.WaitForExit((int)timeout.TotalMilliseconds);
        
        if (!completedWithinTimeout)
        {
            try
            {
                process.Kill();
                Debug.WriteLine("Process killed due to timeout");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error killing process: {ex.Message}");
            }
        }
        
        // Small delay to ensure cleanup
        await Task.Delay(100);
    }

    private static void CleanupTempFiles(params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            try
            {
                if (pattern.Contains("*"))
                {
                    // Handle wildcard patterns
                    var directory = Path.GetDirectoryName(pattern);
                    var fileName = Path.GetFileName(pattern);
                    if (Directory.Exists(directory))
                    {
                        var files = Directory.GetFiles(directory, fileName);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }
                else if (File.Exists(pattern))
                {
                    File.Delete(pattern);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete temp file {pattern}: {ex.Message}");
            }
        }
    }

    private static string QualityParser(GifQuality quality)
    {
        return quality switch
        {
            GifQuality.q360p => "360",
            GifQuality.q480p => "480",
            GifQuality.q720p => "720",
            GifQuality.q1080p => "1080",
            GifQuality.q1440p => "1440",
            GifQuality.q2160p => "2160",
            _ => "1080",
        };
    }

    // Method to convert extracted scene resources to mp4 - OPTIMIZED
    public static async Task<string?> ConvertSceneToMp4Async(string sceneDirectory, string outputFile)
    {
        try
        {
            // Force garbage collection before processing
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Check if directory has images
            var imageFiles = Directory.GetFiles(sceneDirectory, "*.png");
            if (imageFiles.Length == 0)
            {
                Debug.WriteLine("No PNG files found in scene directory");
                return null;
            }

            // Use memory-optimized settings for image sequence conversion
            string ffmpegArgs = $"-threads 2 -hwaccel none -framerate 30 -pattern_type glob -i '{sceneDirectory}/*.png' -c:v libx264 -preset fast -crf 23 -pix_fmt yuv420p -movflags +faststart -y \"{outputFile}\"";

            var convertProcess = new Process
            {
                StartInfo = new()
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            await RunProcessWithTimeoutAsync(convertProcess, TimeSpan.FromMinutes(5));

            if (!File.Exists(outputFile))
            {
                Debug.WriteLine("Output file was not created");
                return null;
            }

            return outputFile;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ConvertSceneToMp4Async: {ex.Message}");
            return null;
        }
    }
}