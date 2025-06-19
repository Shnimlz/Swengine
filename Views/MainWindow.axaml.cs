using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Linq;
using swengine.desktop.Models;
using swengine.desktop.ViewModels;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Styling;
using Avalonia.Animation;
using Avalonia.Media;
using System;
using Swengine.Helpers;

namespace swengine.desktop.Views;

public partial class MainWindow : Window
{
    private bool _isSearchExpanded = false;
    private bool _isAnimating = false;
    
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Inicializa los colores por defecto
       SetMainBackgroundColor(new SolidColorBrush(GetColorForTheme("Default")));

        this.Opened += (_, __) =>
        {
            this.Width = 1280;
            this.Height = 720;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        };

        Swengine.ViewModels.SettingsViewModel.ThemeChanged += OnThemeChanged; // Asegúrate que el evento sea EventHandler<string>
        ApplyTheme(GetCurrentTheme());

        SetupScrollViewer();
        SetupViewModelEvents();
    }

    /// <summary>
    /// Establece el color del texto global para el tema actual
    /// </summary>
    /// <param name="color">Color en formato hexadecimal (#RRGGBB o #AARRGGBB)</param>
    private void SetTextForegroundColor(string color)
    {
        if (this.Resources.ContainsKey("TextForegroundColor"))
        {
            var brush = this.Resources["TextForegroundColor"] as Avalonia.Media.SolidColorBrush;
            if (brush != null)
            {
                brush.Color = Avalonia.Media.Color.Parse(color);
            }
            else
            {
                this.Resources["TextForegroundColor"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color));
            }
        }
        else
        {
            this.Resources.Add("TextForegroundColor", new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color)));
        }
    }

    /// <summary>
    /// Establece el color de fondo global para el tema actual
    /// </summary>
    /// <param name="color">Color en formato hexadecimal (#RRGGBB o #AARRGGBB)</param>
    private void SetMainBackgroundColor(IBrush brush)
    {
        if (this.Resources.ContainsKey("MainBackgroundColor"))
        {
            this.Resources["MainBackgroundColor"] = brush;
        }
        else
        {
            this.Resources.Add("MainBackgroundColor", brush);
        }
        var mainGrid = this.FindControl<Grid>("MainGrid");
        if (mainGrid != null)
        {
            mainGrid.Background = brush;
        }
    }

    private void SetupScrollViewer()
    {
        var scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");
        
        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += (object? sender, ScrollChangedEventArgs args) =>
            {
                var scrollview = sender as ScrollViewer;
                var scrollHeight = (int)scrollview!.Extent.Height - scrollview.Viewport.Height;
                
                if (scrollview.Offset.Y == scrollHeight && scrollHeight > 100)
                {
                    (DataContext as MainWindowViewModel)?.AppendToInfinteScroll();
                }
            };
        }
    }

    private void SetupViewModelEvents()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");
            
            viewModel.RequestMoveToTop += (s, e) =>
            {
                scrollViewer?.ScrollToHome();
            };
            
            viewModel.RequestClearImageLoader += (s, e) =>
            {
                var advancedImages = this.GetVisualDescendants().OfType<AsyncImageLoader.AdvancedImage>();
                foreach (var advancedImage in advancedImages)
                {
                    var parent = advancedImage.FindAncestorOfType<Grid>();
                    parent?.Children.Remove(advancedImage);
                }
            };
        }
    }

    private void OnThemeChanged(string newTheme)
    {
        ApplyTheme(newTheme);
    }

    private void ApplyTheme(string theme)
    {
        var (mainBg, secondaryBg, textColor, mainBgGrad, secondaryBgGrad) = ThemeHelper.GetThemeColors(theme);
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
        SetMainBackgroundColor(mainGradient);
        SetSecondaryBackgroundColor(secondaryBg);
        SetTextForegroundColor(textColor);
    }

    private void SetSecondaryBackgroundColor(Avalonia.Media.Color color)
    {
        SetOrUpdateBrushResource("SecondaryBackgroundColor", color);
    }

    private void SetOrUpdateBrushResource(string key, Avalonia.Media.Color color)
    {
        if (this.Resources.ContainsKey(key))
        {
            if (this.Resources[key] is Avalonia.Media.SolidColorBrush brush)
                brush.Color = color;
            else
                this.Resources[key] = new Avalonia.Media.SolidColorBrush(color);
        }
        else
        {
            this.Resources.Add(key, new Avalonia.Media.SolidColorBrush(color));
        }
    }

    public string GetCurrentTheme()
    {
        try
        {
            var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".config", "swengine", "settings.json");
            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("SelectedTheme", out var themeProp))
                {
                    return themeProp.GetString() ?? "Default";
                }
            }
        }
        catch { }
        return "Default";
    }

    private static Avalonia.Media.Color GetColorForTheme(string theme)
    {
        return theme switch
        {
            _ => Avalonia.Media.Color.Parse("#24293E"), // Oscuro por defecto
        };
    }

    private async void OpenApplyWindow(object? sender, PointerPressedEventArgs e)
    {
        var dc = (DataContext as MainWindowViewModel);
        WallpaperResponse? tag = (sender as Border)?.Tag as WallpaperResponse;
        
        if (tag == null || dc == null) return;
        
        var applyWindow = new ApplyWindow()
        {
            DataContext = new ApplyWindowViewModel()
            {
                BgsProvider = dc.BgsProvider,
                Backend = dc.SelectedBackend,
                WallpaperResponse = tag,
            }
        };
        
        await applyWindow.ShowDialog<ApplyWindowViewModel?>(this);
    }

    private void OnSearchIconClick(object sender, RoutedEventArgs e)
    {
        if (!_isAnimating)
        {
            ExpandSearch();
        }
    }

    private void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (_isAnimating) return;

        switch (e.Key)
        {
            case Key.Escape:
                CollapseSearch();
                break;
            case Key.Enter:
                ExecuteSearch();
                CollapseSearch();
                break;
        }
    }

    private void ExecuteSearch()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Search();
        }
    }

    private async void ExpandSearch()
    {
        if (_isSearchExpanded || _isAnimating) return;
        
        _isAnimating = true;
        _isSearchExpanded = true;
        
        var searchContainer = this.FindControl<Border>("SearchContainer");
        var searchIcon = this.FindControl<Button>("SearchIcon");
        var searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        var searchButton = this.FindControl<Button>("SearchButton");
        
        if (searchContainer == null || searchIcon == null || searchTextBox == null || searchButton == null)
        {
            _isAnimating = false;
            return;
        }

        try
        {
            // Paso 1: Expandir contenedor inmediatamente
            searchContainer.Width = 400;
            
            // Paso 2: Fade out del icono
            searchIcon.Opacity = 0.0;
            
            // Esperar a que termine la transición del icono
            await Task.Delay(400);
            
            // Paso 3: Ocultar icono y mostrar elementos
            searchIcon.IsVisible = false;
            searchTextBox.IsVisible = true;
            searchButton.IsVisible = true;
            
            // Pequeño delay para evitar conflictos
            await Task.Delay(50);
            
            // Paso 4: Fade in de los nuevos elementos
            searchTextBox.Opacity = 1.0;
            searchButton.Opacity = 1.0;
            
            // Esperar a que terminen las animaciones
            await Task.Delay(400);
            
            // Paso 5: Focus en el TextBox
            searchTextBox.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en ExpandSearch: {ex.Message}");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void CollapseSearch()
    {
        if (!_isSearchExpanded || _isAnimating) return;
        
        _isAnimating = true;
        
        var searchContainer = this.FindControl<Border>("SearchContainer");
        var searchIcon = this.FindControl<Button>("SearchIcon");
        var searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        var searchButton = this.FindControl<Button>("SearchButton");
        
        if (searchContainer == null || searchIcon == null || searchTextBox == null || searchButton == null)
        {
            _isAnimating = false;
            return;
        }

        try
        {
            // Paso 1: Fade out de elementos de búsqueda
            searchButton.Opacity = 0.0;
            searchTextBox.Opacity = 0.0;
            
            // Esperar a que terminen las animaciones
            await Task.Delay(400);
            
            // Paso 2: Ocultar elementos
            searchButton.IsVisible = false;
            searchTextBox.IsVisible = false;
            
            // Paso 3: Mostrar icono y contraer
            searchIcon.IsVisible = true;
            searchIcon.Opacity = 1.0;
            searchContainer.Width = 60;
            
            // Esperar a que termine la animación del contenedor
            await Task.Delay(400);
            
            _isSearchExpanded = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en CollapseSearch: {ex.Message}");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (_isSearchExpanded && !_isAnimating)
        {
            var searchContainer = this.FindControl<Border>("SearchContainer");
            if (searchContainer != null)
            {
                var position = e.GetPosition(searchContainer);
                var bounds = searchContainer.Bounds;
                
                // Si el click está fuera del contenedor de búsqueda
                if (position.X < 0 || position.Y < 0 || 
                    position.X > bounds.Width || position.Y > bounds.Height)
                {
                    CollapseSearch();
                }
            }
        }
    }
}