using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
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
using Avalonia.Animation;
using Avalonia.Styling;

namespace swengine.desktop.Views;

public partial class MainWindow : Window
{
    // Variables para la búsqueda animada
    private bool _isSearchExpanded = false;
    
    public MainWindow()
    {
        // Try InitializeComponent first
        try 
        {
            InitializeComponent();
        }
        catch
        {
            // If InitializeComponent fails, manually load the AXAML
            AvaloniaXamlLoader.Load(this);
        }
        
        // Ajustar tamaño y posición al 50% de la pantalla cuando la ventana ya está abierta
        this.Opened += (_, __) =>
        {
            this.Width = 1280;
            this.Height = 720;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        };

        // Find the ScrollViewer instance by its name from AXAML
        var scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");
        
        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += (object sender, ScrollChangedEventArgs args) =>
            {
                var scrollview = sender as ScrollViewer;
                var scrollHeight = (int)scrollview.Extent.Height - scrollview.Viewport.Height;
                
                // > 100 to prevent calling this method when the scrollview is empty
                if (scrollview.Offset.Y == scrollHeight && scrollHeight > 100)
                {
                    //append to infinite scroll
                    (DataContext as MainWindowViewModel).AppendToInfinteScroll();
                }
            };
        }
        
        // Move the other event handlers outside the ScrollChanged event
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RequestMoveToTop += (s, e) =>
            {
                //scroll up when user paginate
                scrollViewer?.ScrollToHome();
            };
            
            viewModel.RequestClearImageLoader += (s, e) =>
            {
                var advancedImages = this.GetVisualDescendants().OfType<AsyncImageLoader.AdvancedImage>();
                foreach (var advancedImage in advancedImages)
                {
                    var parent = advancedImage.FindAncestorOfType<Grid>();
                    if (parent != null)
                    {
                        parent.Children.Remove(advancedImage);
                    }
                    //advancedImage.Source = null;
                }
            };
        }
    }

    private async void OpenApplyWindow(object? sender, TappedEventArgs e)
    {
        var dc = (DataContext as MainWindowViewModel);
        WallpaperResponse Tag = (WallpaperResponse)(sender as StackPanel).Tag;
        if (Tag == null)
        {
            return;
        }
        
        var applyWindow = new ApplyWindow()
        {
            DataContext = new ApplyWindowViewModel()
            {
                BgsProvider = (DataContext as MainWindowViewModel).BgsProvider,
                Backend = (DataContext as MainWindowViewModel).SelectedBackend,
                WallpaperResponse = Tag,
                //pass the current provider so the Apply window knows which provider to query for the wallpaper
            }
        };
        
        var result = await applyWindow.ShowDialog<ApplyWindowViewModel?>(this);
    }

    private void OnSearchIconClick(object sender, RoutedEventArgs e)
    {
        ExpandSearch();
    }

    private void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CollapseSearch();
        }
        else if (e.Key == Key.Enter)
        {
            // Ejecutar búsqueda
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Search();
            }
            CollapseSearch();
        }
    }

    private async void ExpandSearch()
    {
        if (_isSearchExpanded) return;
        
        _isSearchExpanded = true;
        
        var searchContainer = this.FindControl<Border>("SearchContainer");
        var searchIcon = this.FindControl<Button>("SearchIcon");
        var searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        var searchButton = this.FindControl<Button>("SearchButton");
        
        if (searchContainer == null || searchIcon == null || searchTextBox == null || searchButton == null)
            return;

        // Usar Dispatcher.UIThread.InvokeAsync para manejar las animaciones
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // 1. Expandir contenedor - cambiar width directamente para que se active la transición
            searchContainer.Width = 400;
            
            // 2. Ocultar ícono con fade out
            searchIcon.Opacity = 0;
            
            // Esperar que la primera animación comience
            await Task.Delay(200);
            
            // Ocultar completamente el ícono
            searchIcon.IsVisible = false;
            
            // 3. Mostrar TextBox
            searchTextBox.IsVisible = true;
            searchTextBox.Opacity = 1;
            searchTextBox.Focus();
            
            // Esperar un poco antes de mostrar el botón
            await Task.Delay(200);
            
            // 4. Mostrar botón con animación de escala
            searchButton.IsVisible = true;
            searchButton.Opacity = 1;
        });
    }

    private async void CollapseSearch()
    {
        if (!_isSearchExpanded) return;
        
        var searchContainer = this.FindControl<Border>("SearchContainer");
        var searchIcon = this.FindControl<Button>("SearchIcon");
        var searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        var searchButton = this.FindControl<Button>("SearchButton");
        
        if (searchContainer == null || searchIcon == null || searchTextBox == null || searchButton == null)
            return;

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // 1. Ocultar botón primero
            searchButton.Opacity = 0;
            
            await Task.Delay(100);
            
            searchButton.IsVisible = false;
            
            // 2. Ocultar TextBox
            searchTextBox.Opacity = 0;
            
            await Task.Delay(200);
            
            searchTextBox.IsVisible = false;
            
            // 3. Contraer contenedor y mostrar ícono
            searchContainer.Width = 60;
            searchIcon.IsVisible = true;
            searchIcon.Opacity = 1;
            
            _isSearchExpanded = false;
        });
    }

    // Manejar clicks fuera del área de búsqueda
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (_isSearchExpanded)
        {
            var searchContainer = this.FindControl<Border>("SearchContainer");
            if (searchContainer != null)
            {
                var position = e.GetPosition(searchContainer);
                
                // Si el click está fuera del contenedor de búsqueda
                if (position.X < 0 || position.Y < 0 || 
                    position.X > searchContainer.Bounds.Width || 
                    position.Y > searchContainer.Bounds.Height)
                {
                    CollapseSearch();
                }
            }
        }
    }
}