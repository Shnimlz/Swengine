using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using swengine.desktop.Services;
using swengine.desktop.Views;
using swengine.desktop.Helpers;
namespace swengine.desktop.ViewModels;
public partial class MainWindowViewModel{
   public async void OpenUploadDialog()
{
    ContentDialog uploadDialog = new()
    {
        Title = "Upload a wallpaper to your desktop",
        Content = UploadDialogContent(),
        PrimaryButtonText = "Upload",
        IsPrimaryButtonEnabled = true
    };

    var result = await uploadDialog.ShowAsync();
     
    if (result == ContentDialogResult.Primary)
    {
        if (SelectedFile != null)
        {
            var applyWindow = new ApplyWindow()
         {
             DataContext = new ApplyWindowViewModel()
             {
                 BgsProvider = new LocalBgService(),
                 WallpaperResponse = new()
                 {
                     Src = SelectedFile,
                     Thumbnail = null,
                     Title = SelectedFile
                 },
                 Backend = SelectedBackend
             }
         };

         // Obtén la ventana principal como owner
         var mainWindow = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
         if (mainWindow != null)
         {
             await applyWindow.ShowDialog(mainWindow);
         }
         // Si mainWindow es null, simplemente no mostramos la ventana para evitar error de compilación.
        }
    }
}
   private StackPanel UploadDialogContent(){
        StackPanel panel = new();
        Button uploadFile = new(){
            Content = "Upload File"
        };
        TextBlock selectedFile = new();
        selectedFile.Bind(TextBlock.TextProperty,new Binding(){
            Source = this,
            Path = "SelectedFile",
            Mode = BindingMode.TwoWay
        });
        uploadFile.Click += (s,e)=>{
            HandleFileDialog();
        };
        panel.Children.Add(uploadFile);
        panel.Children.Add(selectedFile);
        return panel;
    }

    private async void HandleFileDialog(){
        var toplevel = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;

        // Start async operation to open the dialog.
        var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false,
             FileTypeFilter = new[] { new FilePickerFileType("filesv") {Patterns = new List<string>(){
                "*.jpg", "*.jpeg", "*.png", "*.bmp","*.mp4","*.MP4","*.mkv","*.MKV","*.gif"
             }} }
        });

        if (files.Count >= 1)
            SelectedFile = files[0].TryGetLocalPath() ?? "";
    }

    private object CustomScriptsDialogContent(){
         TextBlock customScriptText = new(){
            Text = "Enter commands you want to run after a new wallpaper has been set. One command per line. Substitute \"$1\" for the full path of the newely set wallpaper. TAKE GREAT CAUTION HERE",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
          TextEditor customScriptBox = new(){
           WordWrap = true,
           ShowLineNumbers = true,  
           Height = 100,
          Document = new() {Text = "A sample text"}
        };
        customScriptBox.Bind(TextEditor.DocumentProperty, new Binding(){
             Source = this,
             Path = "CustomScriptsContent",
             Mode = BindingMode.TwoWay
        });
        StackPanel wrapper = new();
        wrapper.Children.Add(customScriptText);
        wrapper.Children.Add(customScriptBox);

        return wrapper;
    }
    public async void OpenCustomScriptsDialog(){
        var readonly_custom_script_content = CustomScriptsHelper.ScriptsFileContent;
        if(readonly_custom_script_content == null)
            CustomScriptsContent.Text = "echo $wallpaper";
        else
            CustomScriptsContent.Text = readonly_custom_script_content;
        
        var dialog  =  new ContentDialog(){
                Title = "Custom Scripts",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Add Script",
                Content = CustomScriptsDialogContent()
        };
        var dialogResponse = await dialog.ShowAsync();
        if(dialogResponse ==  ContentDialogResult.Primary){
            // Al pulsar "Add Script", abre la ventana de edición
            var mainWindow = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var scriptEditor = new swengine.desktop.Views.ScriptEditorWindow(CustomScriptsContent.Text);
            scriptEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = await scriptEditor.ShowDialog<bool?>(mainWindow);
            if (result == true && !string.IsNullOrWhiteSpace(scriptEditor.ScriptContent))
            {
                CustomScriptsContent.Text = scriptEditor.ScriptContent;
                CustomScriptsHelper.SetScriptsFileContent(CustomScriptsContent.Text);
            }
        }
    }
}