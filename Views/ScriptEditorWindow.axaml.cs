using Avalonia.Controls;
using Avalonia.Interactivity;

namespace swengine.desktop.Views
{
    public partial class ScriptEditorWindow : Window
    {
        public string ScriptContent { get; private set; } = string.Empty;

        // Parameterless constructor required for XAML
        public ScriptEditorWindow()
        {
            InitializeComponent();
        }

        // Constructor with parameters for actual usage
        public ScriptEditorWindow(string initialContent, string theme) : this()
        {
            // Apply the current theme when opening the window
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
    }
}