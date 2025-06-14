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
    public async static Task<string> ConvertAsync(string file, double startAt = 0, double endAt = 5, GifQuality quality = GifQuality.q1080p, int fps = 60, bool bestSettings= false )
    {
        try
        {
              string home = Environment.GetEnvironmentVariable("HOME");

            //if file is not a video, dont bother converting. Just return the image.
            if(Path.GetExtension(file).ToLower() != ".mp4" && Path.GetExtension(file).ToLower() != ".mkv"){
                string copyTo = home + "/Pictures/wallpapers/" + file.Split("/").Last();
                File.Copy(file,copyTo,true);
                if(!File.Exists(copyTo))
                    return null;
                File.Delete(file);
                return copyTo;
            }
            string convertTo = home + "/Pictures/wallpapers/" + file.Split("/").Last().Split(".").First() + ".gif";

            // ---- LOOP FIX: extrae primer frame y lo añade al final ----
            string tmpFirstFrame = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_first.jpg");
            string tmpLoopedVideo = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_looped.mp4");

            // 1. Extrae el primer frame
            var extractFrame = new Process {
                StartInfo = new() {
                    FileName = "ffmpeg",
                    Arguments = $"-y -i \"{file}\" -vf \"select=eq(n\\,0)\" -q:v 3 \"{tmpFirstFrame}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            extractFrame.Start();
            extractFrame.WaitForExit();

            // 2. Añade el primer frame al final del video
            var concatVideo = new Process {
                StartInfo = new() {
                    FileName = "ffmpeg",
                    Arguments = $"-y -i \"{file}\" -i \"{tmpFirstFrame}\" -filter_complex \"[0:v][1:v]concat=n=2:v=1:a=0\" -c:v libx264 -preset veryfast \"{tmpLoopedVideo}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            concatVideo.Start();
            concatVideo.WaitForExit();

            // 3. Ahora convierte el video extendido a GIF
            string ffmpegArgs = bestSettings
                ? $"-i \"{tmpLoopedVideo}\" -y \"{convertTo}\""
                : $"-ss {startAt} -t {endAt} -i \"{tmpLoopedVideo}\" -vf \"scale=-1:{QualityParser(quality)}:flags=lanczos,fps={fps}\" -loop 0 -y \"{convertTo}\"";

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

            // Limpia temporales después

            // convertProcess.OutputDataReceived += (sender, args) =>
            // {
            //         Debug.WriteLine($"Received Output: {args.Data}");
            // };
            // convertProcess.ErrorDataReceived += (sender, errorArgs) =>
            // {
            //     if (errorArgs.Data != null)
            //     {
            //         Debug.WriteLine($"Received Error: {errorArgs.Data}");
            //     }
            // };
            convertProcess.Start();
            convertProcess.BeginOutputReadLine();
            convertProcess.BeginErrorReadLine();
            convertProcess.WaitForExit();
            //if gif does not exist then conversion failed. Return false
            if (!File.Exists(convertTo))
            {
                return null;
            }
            //if everything went smoothly, delete the mp4.
           File.Delete(file);
            return convertTo;
        }
        catch
        {
            return null;
        }
      
    }

    private static string QualityParser(GifQuality quality)
    {
        switch (quality)
        {
            case GifQuality.q360p:
                return "360";
                break;
            case GifQuality.q480p:
                return "480";
                break;
            case GifQuality.q720p:
                return "720";
                break;
            case GifQuality.q1080p:
                return "1080";
                break;
            case GifQuality.q1440p:
                return "1440";
                break;
            case GifQuality.q2160p:
                return "2160";
                break;
            default:
                return "1080";
                break;
        }
    }

    // Method to convert extracted scene resources to mp4
    public static async Task<string> ConvertSceneToMp4Async(string sceneDirectory, string outputFile)
    {
        try
        {
            // Assuming all video resources are extracted to the sceneDirectory
            // Use FFmpeg to combine these resources into a single mp4 file
            string ffmpegArgs = $"-framerate 30 -pattern_type glob -i '{sceneDirectory}/*.png' -c:v libx264 -pix_fmt yuv420p -y {outputFile}";

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

            convertProcess.Start();
            convertProcess.BeginOutputReadLine();
            convertProcess.BeginErrorReadLine();
            convertProcess.WaitForExit();

            if (!File.Exists(outputFile))
            {
                return null; // Conversion failed
            }

            return outputFile; // Return the path to the converted mp4
        }
        catch
        {
            return null; // Handle any exceptions
        }
    }
}
