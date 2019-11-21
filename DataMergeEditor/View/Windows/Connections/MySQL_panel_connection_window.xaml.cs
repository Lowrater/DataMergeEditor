using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.View.Windows.Connections
{
    /// <summary>
    /// Interaction logic for MySQL_panel_connection_window.xaml
    /// </summary>
    public partial class MySQL_panel_connection_window : Window
    {
        public MySQL_panel_connection_window()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
