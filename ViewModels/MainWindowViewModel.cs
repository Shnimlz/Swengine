using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using AvaloniaEdit.Document;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsyncImageLoader.Loaders;
using swengine.desktop.Models;
using swengine.desktop.Services;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Controls;

namespace swengine.desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject {
    private const string AurPackageName = "swengine-revanced"; // Cambiado al nombre real en el AUR
    public bool UpdateAvailable { get; private set; } = false;
    public string? LatestAurVersion { get; private set; }
    public IRelayCommand UpdateCommand { get; }

    public IRelayCommand SearchCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    
    private async void RefreshWallpapers()
    {
        if (SelectedProvider == "Wallpaper Engine")
        {
            await CheckWallpaperEngineDirectory();
        }
        else
        {
            CurrentPage++;
            Search();
        }
    }

    private static readonly Random _rnd = new();

    public MainWindowViewModel() {
        UpdateCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(LaunchAurUpdate);
        CheckForAurUpdate();
        UpdateCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(LaunchAurUpdate);
        CheckForAurUpdate();

        // Inicializar campos obligatorios antes de usar
        BgsProvider = null!; // Se inicializará en SetProvider()
        wallpaperResponses = new ObservableCollection<WallpaperResponse>();
        
        CurrentPage = _rnd.Next(1, 11); 
        SearchCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(Search);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(RefreshWallpapers);
      
        SetProvider();
        Search();
        
        // Remover estas líneas problemáticas que causaban recursión infinita:
        // RequestMoveToTop += RequestMoveToTop;
        // RequestClearImageLoader += RequestClearImageLoader;
    }

    private async void CheckForAurUpdate()
    {
        try
        {
            UpdateAvailable = await swengine.desktop.Services.AurUpdateService.IsUpdateAvailableAsync(AurPackageName);
            if (UpdateAvailable)
            {
                LatestAurVersion = await swengine.desktop.Services.AurUpdateService.GetAurVersionAsync(AurPackageName);
                await ShowWarningDialog($"¡Nueva versión disponible en AUR! Versión: {LatestAurVersion}\n¿Quieres actualizar ahora?\n\nPuedes hacerlo desde el menú de la app o ejecutando yay -Syu {AurPackageName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUR UPDATE] Error: {ex.Message}");
        }
    }

    private void LaunchAurUpdate()
    {
        swengine.desktop.Services.AurUpdateService.LaunchUpdate(AurPackageName);
      (Avalonia.Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }


    // Inicializar con null-forgiving operator ya que se garantiza la inicialización en SetProvider()
    public IBgsProvider BgsProvider = null!;
    
    public string[] Providers => new[] {
        "Motionbgs",
        "Moewalls",
        "Desktophut",
        "Mylivewallpapers",
        "Wallhaven",
        "Wallhaven(NSFW)",
        "Wallhaven(random)",
        "Wallpaperscraft",
        "Wallpapers-clan",
        "Wallpaper Engine",
    };

    public string[] Backends => new[] { "SWWW", "PLASMA", "GNOME" };
    private bool _appendingToInfinteScroll = false;

    private string _selectedProvider = "Motionbgs";
    private string _selectedBackend = "SWWW";
    
    // Eventos inicializados correctamente
    public event EventHandler<EventArgs>? RequestMoveToTop;
    public event EventHandler<EventArgs>? RequestClearImageLoader;

    private CancellationTokenSource _searchDebounceToken = new();
    public AsyncImageLoader.Loaders.BaseWebImageLoader BaseLoader => new BaseWebImageLoader();

    private int _infiniteScrollPage = 1;
    public string SelectedProvider {
        get => _selectedProvider;
        set {
            SetProperty(ref _selectedProvider, value);
            SetProvider();
        }
    }
    public string SelectedBackend {
        get => _selectedBackend;
        set {
            SetProperty(ref _selectedBackend, value);
            SetBackend();
        }
    }

    [ObservableProperty]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MainWindowViewModel))]
    private string searchTerm = "";
    partial void OnSearchTermChanged(string value)
    {
        CurrentPage = 1;
        Search();
    }

    [ObservableProperty] private int currentPage = 1;
    
    // Inicializar con lista vacía
    [ObservableProperty] private ObservableCollection<WallpaperResponse> wallpaperResponses = new();
    
    [ObservableProperty] private bool dataLoading = false;

    [ObservableProperty] private string? selectedFile = null;

    [ObservableProperty]
    private TextDocument customScriptsContent = new() {
        Text = ""
    };
    [ObservableProperty] public bool infinteScrollLoading = false;

    private async void SetProvider() {
        switch (SelectedProvider) {
            case "Motionbgs":
                BgsProvider = new MotionBgsService();
                break;
            case "Moewalls":
                BgsProvider = new MoewallsService();
                break;
            case "Wallhaven":
                BgsProvider = new WallHavenService();
                break;
            case "Mylivewallpapers":
                BgsProvider = new MyLiveWallpapersService();
                break;
            case "Wallpaperscraft":
                BgsProvider = new WallpapersCraftService();
                break;
            case "Wallpapers-clan":
                BgsProvider = new WallpapersClanService();
                break;
            case "Wallhaven(random)":
                BgsProvider = new WallHavenRandomService();
                break;
            case "Wallhaven(NSFW)":
                BgsProvider = new WallHavenNSFWService();
                break;
            case "Desktophut":
                BgsProvider = new DesktopHutService();
                break;
            case "Wallpaper Engine":
                BgsProvider = new WallpaperEngineService();
                await CheckWallpaperEngineDirectory();
                break;
            default:
                // Proporcionar un valor por defecto
                BgsProvider = new MoewallsService();
                break;
        }

        _infiniteScrollPage = CurrentPage;
        _appendingToInfinteScroll = false;
        RequestMoveToTop?.Invoke(this, EventArgs.Empty);
        try {
            ClearImageLoader();
        } catch { }
        Search();
    }

    private async Task CheckWallpaperEngineDirectory()
    {
        try
        {
            await swengine.desktop.Helpers.WallpaperEngineHelper.GetWallpapersAsync();
        }
        catch (DirectoryNotFoundException)
        {
            await ShowWarningDialog("Wallpaper Engine no está instalado o no se encontró la carpeta correspondiente.\n\nPor favor, instala Wallpaper Engine desde Steam e intenta de nuevo.");
        }
    }

    private static async Task ShowWarningDialog(string message)
    {
        var dialog = new Avalonia.Controls.Window
        {
            Title = "Advertencia",
            Width = 400,
            Height = 180,
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
            Content = new Avalonia.Controls.TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(16),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
        var mainWindow = (Avalonia.Application.Current!.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow != null)
            await dialog.ShowDialog(mainWindow);
        else
            dialog.Show();
    }

    private static void SetBackend() {

    }
    
    public void Paginate(string seek) {
        if (seek == "up") {
            CurrentPage++;
        } else if (seek == "down" && CurrentPage > 1) {
            CurrentPage--;
        }
        _infiniteScrollPage = CurrentPage;
        _appendingToInfinteScroll = false;
        ClearImageLoader();
        RequestMoveToTop?.Invoke(this, EventArgs.Empty);
        Search();
    }
    
    // Mejora: solo procesa la última búsqueda activa y evita términos muy cortos
    private int _searchRequestId = 0;
    public async void Search() {
        await _searchDebounceToken.CancelAsync();
        _searchDebounceToken = new();
        int currentRequestId = ++_searchRequestId;
        DataLoading = true;

        // Evita búsquedas con términos muy cortos (menos de 3 letras)
        if (!string.IsNullOrWhiteSpace(SearchTerm) && SearchTerm.Length < 3) {
            WallpaperResponses.Clear();
            DataLoading = false;
            return;
        }

        if (_searchDebounceToken.IsCancellationRequested) return;
        List<WallpaperResponse>? results = null;
        try {
            if (SearchTerm.Length == 0) {
                if (_searchDebounceToken.IsCancellationRequested) return;
                results = await BgsProvider.LatestAsync(Page: CurrentPage);
            } else {
                if (_searchDebounceToken.IsCancellationRequested) return;
                results = await BgsProvider.SearchAsync(SearchTerm, CurrentPage);
            }
        } catch (swengine.desktop.Scrapers.WallHavenApiException ex) {
            await ShowWarningDialog($"WallHaven API: {ex.Message}");
        } catch (Exception ex) {
            await ShowWarningDialog($"Error inesperado: {ex.Message}");
        }
        // Solo actualiza si sigue siendo la última búsqueda
        if (currentRequestId == _searchRequestId) {
            ClearImageLoader();
            WallpaperResponses.Clear();
if (results != null)
{
    foreach (var wp in results)
        WallpaperResponses.Add(wp);
}

            // Log temporal para depuración: muestra los thumbnails en consola
            if (WallpaperResponses != null && WallpaperResponses.Count > 0)
            {
                foreach (var wp in WallpaperResponses)
                {
                    Console.WriteLine($"[DEBUG] Thumbnail: {wp.Thumbnail}");
                }
            }
            DataLoading = false;
        }
    }
    
    private void ClearImageLoader() {
        RequestClearImageLoader?.Invoke(this, EventArgs.Empty);
    }
    
    public async void AppendToInfinteScroll() {
        if (_appendingToInfinteScroll) return;

        _appendingToInfinteScroll = true;
        InfinteScrollLoading = true;

        try {
            var responses = SearchTerm.Length == 0
                ? await BgsProvider!.LatestAsync(_infiniteScrollPage + 1)
                : await BgsProvider!.SearchAsync(SearchTerm, _infiniteScrollPage + 1);

            if (responses != null) {
                foreach (var response in responses) {
                    if (response != null) {
                        WallpaperResponses.Add(response);
                    }
                }
                _infiniteScrollPage++;
            }
        } catch {
        } finally {
            InfinteScrollLoading = false;
            _appendingToInfinteScroll = false;
        }
    }
}