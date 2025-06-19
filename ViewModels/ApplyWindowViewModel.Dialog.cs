using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using LibVLCSharp.Shared;
using swengine.desktop.Helpers;
using swengine.desktop.Models;
using swengine.desktop.Services;

namespace swengine.desktop.ViewModels;

public partial class ApplyWindowViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private double progress;

    //Content for the content dialog that requests for FPS,resolution, e.t.c

    private StackPanel ApplyDialogContent()
    {
        ProgressBar progressBar = new()
        {
            Minimum = 0,
            Maximum = 100,
            Height = 20
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding
        {
            Source = this,
            Path = "Progress",
            Mode = BindingMode.OneWay
        });
        StackPanel panel = new();
        //resolution selector
        ComboBox ResolutionSelector = new();
        TextBlock ResolutionSelectorText = new() { Text = "Select Resolution:" };
        ResolutionSelector.ItemsSource = new[] { GifQuality.q2160p, GifQuality.q1440p, GifQuality.q1080p, GifQuality.q720p, GifQuality.q480p, GifQuality.q360p};
        CheckBox BestSettingsCheckBox = new();

        StackPanel UserSelectedContent = new();
        UserSelectedContent.Bind(StackPanel.IsVisibleProperty,new Binding(){
            Path = "!BestSettings",
            Source = this,
            Mode = BindingMode.TwoWay
        });
        //BEST SETTINGS CHECKBOX
        BestSettingsCheckBox.Bind(CheckBox.IsCheckedProperty, new Binding(){
            Path = "BestSettings",
            Source = this,
            Mode = BindingMode.TwoWay
        });
        TextBlock BestSettingsText = new()
        {
            Text = "⚠️ Remember:\n"
                 + "• Higher resolution = higher RAM usage.\n"
                 + "• If you select a different resolution, avoid setting it to 60fps on low-resource PCs, as it will consume a lot of RAM.",
            Foreground = Brushes.Red
        };
        TextBlock BestSettingsText2 = new()
        {
            Text = "⚠️ Use the most ideal settings for this wallpaper.\n\n"
        };

        //RESOLUTION SELECTOR SELECT
        ResolutionSelector.Bind(ComboBox.SelectedItemProperty, new Binding()
        {
            Path = "SelectedResolution",
            Source = this,
            Mode = BindingMode.TwoWay
        });
        //FPS selector
        TextBlock FpsSelectorText = new() { Text = "Select Frames per second:" };
        TextBox FpsSelector = new(){ };
        FpsSelector.Bind(TextBox.TextProperty, new Binding()
        {
            Path = "SelectedFps",
            Source = this,
            Mode = BindingMode.TwoWay
        });
        UserSelectedContent.Children.Add(ResolutionSelectorText);
        UserSelectedContent.Children.Add(ResolutionSelector);
        
        UserSelectedContent.Children.Add(FpsSelectorText);
        UserSelectedContent.Children.Add(FpsSelector);

        panel.Children.Insert(0, progressBar);
        panel.Children.Add(BestSettingsText);
        panel.Children.Add(BestSettingsText2);
        panel.Children.Add(BestSettingsCheckBox);
        panel.Children.Add(UserSelectedContent);
        
        return panel;
    }
}