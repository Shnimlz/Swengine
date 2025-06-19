using Avalonia.Controls;
using Avalonia.Interactivity;

namespace swengine.desktop.Views
{
    public partial class ScriptEditorWindow : Window
    {
        public string ScriptContent { get; private set; } = string.Empty;
      public ScriptEditorWindow(string initialContent, string theme)
        {
            InitializeComponent();
            // Aplica el tema actual al abrir la ventana
            Swengine.Helpers.ThemeHelper.ApplyTheme(this, theme);
            ScriptTextBox.Text = initialContent;
            SaveButton.Click += OnSaveClicked;
            CancelButton.Click += OnCancelClicked;
        }

        private void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            ScriptContent = ScriptTextBox.Text ?? string.Empty;
            this.Close(true);
        }

        private void OnCancelClicked(object? sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

    /// <summary>
    /// Cambia el color de fondo del diálogo dinámicamente
    /// </summary>
    /// <param name="color">Color en formato hexadecimal (#RRGGBB o #AARRGGBB)</param>
    private void SetDialogBackgroundColor(string color)
    {
        if (this.Resources.ContainsKey("DialogBackgroundColor"))
        {
            var brush = this.Resources["DialogBackgroundColor"] as Avalonia.Media.SolidColorBrush;
            if (brush != null)
            {
                brush.Color = Avalonia.Media.Color.Parse(color);
            }
            else
            {
                this.Resources["DialogBackgroundColor"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color));
            }
        }
        else
        {
            this.Resources.Add("DialogBackgroundColor", new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color)));
        }
    }
}}