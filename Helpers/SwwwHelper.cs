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
    // Guarda el último fondo aplicado para evitar repeticiones
    private static string _lastAppliedFile = null;
    public async static Task<bool> ApplyAsync(string file, string backend)
    {
        try
        {
            // Evitar aplicar el mismo fondo repetidamente
            if (_lastAppliedFile == file && backend == "SWWW")
            {
                Debug.WriteLine($"SwwwHelper: Fondo ya aplicado, se omite la operación para {file}");
                return true;
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
            applyProcess.Start();
            applyProcess.BeginErrorReadLine();
            applyProcess.BeginOutputReadLine();
            applyProcess.WaitForExit();
            applyProcess.Dispose(); // Liberar recursos

            // Actualizar el último fondo aplicado
            if (backend == "SWWW")
                _lastAppliedFile = file;

            //send notification
            using (var notifyProc = Process.Start(new ProcessStartInfo()
            {
                FileName = "notify-send",
                Arguments = "\"Wallpaper set succesfully\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }))
            {
                // Liberar recursos
                notifyProc?.Dispose();
            }

            //run custom scripts asynchronously, basically a fire and forget
            if (File.Exists(CustomScriptsHelper.scripts_location))
            {
                Task.Run(() =>
                {
                    string script_location = CustomScriptsHelper.scripts_location;
                    string command = $"\"{script_location}\" \"\"{file}\"\"";

                    //first make script executable
                    using (var chmodProc = Process.Start(new ProcessStartInfo()
                    {
                        FileName = "chmod",
                        Arguments = $"+x {script_location}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }))
                    {
                        chmodProc?.Dispose();
                    }

                    var scriptProcess = new Process()
                    {
                        StartInfo = new()
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"{command}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                        }
                    };

                    scriptProcess.Start();
                    scriptProcess.Dispose();
                });
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en SwwwHelper.ApplyAsync: {ex.Message}");
            return false;
        }
    }

    private static ProcessStartInfo ApplyProcessStartInfo(string backend, string file)
    {
        string filename = null;
        string arguments = null;
        bool useNice = false;
        switch (backend)
        {
            case "SWWW":
                // Usar flags de optimización y nice para menor consumo
                filename = "nice";
                arguments = $"-n 10 swww img --transition-type none --transition-fps 1 \"{file}\"";
                useNice = true;
                break;
            case "PLASMA":
                filename = "plasma-apply-wallpaperimage";
                arguments = $"\"{file}\"";
                break;
            case "GNOME":
                filename = "gsettings";
                arguments = $"set org.gnome.desktop.background picture-uri \"{file}\"";
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
