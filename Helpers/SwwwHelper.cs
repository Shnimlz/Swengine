using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace swengine.desktop.Helpers;

/**
Used for all backends including KDE and GNOME and not just swww
*/

public static class SwwwHelper
{
 
 public async static Task<bool> ApplyAsync(string file, string backend)
{
    try
    {
        // Si el backend es MPVPAPER, mata swww y mpvpaper antes de lanzar mpvpaper
        if (backend == "MPVPAPER")
        {
            // 1. Mata swww
            var killSwww = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "swww",
                    Arguments = "kill",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            killSwww.Start();
            killSwww.WaitForExit();
            // 2. Mata mpvpaper
            var killMpvpaper = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pkill",
                    Arguments = $"-u {Environment.UserName} mpvpaper",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            killMpvpaper.Start();
            killMpvpaper.WaitForExit();
            await Task.Delay(500); // Espera breve para asegurar cierre
        }
        // Si el backend es SWWW, mata mpvpaper antes de lanzar swww
        else if (backend == "SWWW")
        {
            var killMpvpaper = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pkill",
                    Arguments = $"-u {Environment.UserName} mpvpaper",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            killMpvpaper.Start();
            killMpvpaper.WaitForExit();
            await Task.Delay(500); // Espera breve para asegurar cierre

            // Verifica si swww-daemon está corriendo, si no, lo lanza
            var checkDaemon = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pgrep",
                    Arguments = "swww-daemon",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            checkDaemon.Start();
            string output = await checkDaemon.StandardOutput.ReadToEndAsync();
            checkDaemon.WaitForExit();
            if (string.IsNullOrWhiteSpace(output))
            {
                // Lanzar swww-daemon &
                var startDaemon = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "swww-daemon",
                        Arguments = "",
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                startDaemon.Start();
                await Task.Delay(800); // Espera un poco para asegurar que el daemon arranque
            }
        }
        var applyProcess = new Process()
        {
            StartInfo = ApplyProcessStartInfo(backend, file)
        };
        applyProcess.OutputDataReceived += (sender, args) => { Debug.WriteLine($"Received Output: {args.Data}"); };
        applyProcess.ErrorDataReceived += (sender, errorArgs) =>
        {
            if (errorArgs.Data != null)
            {
                Debug.WriteLine($"Received Error: {errorArgs.Data}");
            }
        };

        if (backend == "MPVPAPER")
        {
            applyProcess.Start();
            // Notificación inmediata, no esperar a que termine
            Process.Start(new ProcessStartInfo()
            {
                FileName = "notify-send",
                Arguments = "\"Wallpaper set succesfully\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            return true;
        }
        else
        {
            applyProcess.Start();
            applyProcess.BeginErrorReadLine();
            applyProcess.BeginOutputReadLine();
            await applyProcess.WaitForExitAsync(); // Utilizar await para esperar a que el proceso termine

            //send notification
            Process.Start(new ProcessStartInfo()
            {
                FileName = "notify-send",
                Arguments = "\"Wallpaper set succesfully\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        //run custom scripts asynchronously, basically a fire and forget
        if (File.Exists(CustomScriptsHelper.scripts_location))
        {
            _ = Task.Run(() =>
            {
                string script_location = CustomScriptsHelper.scripts_location;
                //export wallpaper variable then run the user's script
                string command = $"\"{script_location}\" \"\"{file}\"\"";

                //first make script executable
                //...
            });
        }
    }
    catch (Exception ex)
    {
       throw new Exception($"Error al aplicar el fondo: {ex.Message}");
    }
    return true;
}


    private static ProcessStartInfo ApplyProcessStartInfo(string backend,string file){
        string filename = String.Empty;
        string arguments = String.Empty;
        switch(backend){
            case "SWWW":
                filename = "swww";
                arguments = $"img --transition-type none --transition-fps 15 \"{file}\"";
                break;
            case "PLASMA":
                filename = "plasma-apply-wallpaperimage";
                arguments = $"\"{file}\"";
                break;
            case "GNOME":
                filename = "gsettings";
                arguments = $"set org.gnome.desktop.background picture-uri \"{file}\"";
                break;
            case "MPVPAPER":
                filename = "mpvpaper";
                arguments = $"-o \"--loop --no-audio --hwdec=auto-safe --vd-lavc-threads=2 --framedrop=vo --profile=low-latency --no-config --no-input-default-bindings --no-osc --no-osd-bar --no-border --keepaspect=yes --untimed\" all \"{file}\"";
                break;
        }
        Console.WriteLine(filename + " "+arguments);
      return new(){
            FileName = filename,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
     }
}