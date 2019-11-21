using System.Security;
using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.View.Windows.Connections
{
    /// <summary>
    /// Interaction logic for oracle_connection_window.xaml
    /// </summary>
    public partial class oracle_connection_window : Window
    {
        public oracle_connection_window()
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
