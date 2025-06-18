using Avalonia.Controls;
using Avalonia.Interactivity;

namespace swengine.desktop.Views
{
    public partial class ScriptEditorWindow : Window
    {
        public string ScriptContent { get; private set; } = string.Empty;
        public ScriptEditorWindow(string initialContent)
        {
            InitializeComponent();
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
    }
}
