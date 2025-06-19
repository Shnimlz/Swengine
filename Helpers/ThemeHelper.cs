using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace Swengine.Helpers
{
    public static class ThemeHelper
    {
        /// <summary>
        /// Devuelve los colores del tema (MainBg, SecondaryBg, TextColor)
        /// </summary>
        public static (Color MainBg, Color SecondaryBg, string TextColor, (Color, Color) MainBgGradient, (Color, Color) SecondaryBgGradient) GetThemeColors(string theme)
        {
            switch (theme?.ToLower())
            {
                case "red dark":
                    return (
                        Color.Parse("#2b0307"), // MainBg
                        Color.Parse("#51080d"),   // SecondaryBg
                        "#F4F5FC",               // TextColor
                        (Color.Parse("#2b0307"), Color.Parse("#51080d")), // MainBgGradient
                        (Color.Parse("#51080d"), Color.Parse("#2b0307"))  // SecondaryBgGradient
                    );
                case "purple":
                    return (
                        Color.Parse("#1e1c2f"),   // MainBg
                        Color.Parse("#2c1c36"),   // SecondaryBg
                        "#F4F5FC",               // TextColor
                        (Color.Parse("#1e1c2f"), Color.Parse("#3e0866")), // MainBgGradient
                        (Color.Parse("#2c1c36"), Color.Parse("#3e0866"))  // SecondaryBgGradient
                    );
                case "dark":
                default:
                    return (
                        Color.Parse("#24293E"),   // MainBg
                        Color.Parse("#2f3855"),   // SecondaryBg
                        "#F4F5FC",               // TextColor
                        (Color.Parse("#24293E"), Color.Parse("#23243b")), // MainBgGradient
                        (Color.Parse("#2f3855"), Color.Parse("#23243b"))  // SecondaryBgGradient
                    );
            }
        }

        /// <summary>
        /// Devuelve la lista de nombres de temas soportados
        /// </summary>
        public static List<string> GetAllThemes()
        {
            // Los nombres deben coincidir con los case del switch
            return new List<string> { "dark", "red dark", "purple" };
        }

        /// <summary>
        /// Aplica los colores del tema a la ventana dada.
        /// </summary>
        public static void ApplyTheme(Window window, string theme)
        {
            var (mainBg, secondaryBg, textColor, mainBgGrad, secondaryBgGrad) = GetThemeColors(theme);
            if (window is null) return;

            // Aplica degradado dinámico al fondo principal
            var mainGradient = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = mainBgGrad.Item1, Offset = 0 },
                    new GradientStop { Color = mainBgGrad.Item2, Offset = 1 }
                }
            };
            window.Background = mainGradient;
            // Aplica el degradado dinámico al recurso para que el Grid lo use
            window.Resources["MainBackgroundColor"] = mainGradient;
            window.Foreground = new SolidColorBrush(Color.Parse(textColor));

            // Si la ventana tiene el recurso SecondaryBackgroundColor, lo actualiza con degradado
            if (window.Resources.ContainsKey("SecondaryBackgroundColor"))
            {
                var secondaryGradient = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop { Color = secondaryBgGrad.Item1, Offset = 0 },
                        new GradientStop { Color = secondaryBgGrad.Item2, Offset = 1 }
                    }
                };
                window.Resources["SecondaryBackgroundColor"] = secondaryGradient;
                window.Background = secondaryGradient;
            }
        }
    }
}
