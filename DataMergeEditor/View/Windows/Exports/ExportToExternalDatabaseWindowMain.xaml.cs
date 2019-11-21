using System.Windows;

namespace DataMergeEditor.View.Windows.Exports
{
    /// <summary>
    /// Interaction logic for ExportToExternalDatabaseWindow.xaml
    /// </summary>
    public partial class ExportToExternalDatabaseWindowMain : Window
    {
        public ExportToExternalDatabaseWindowMain()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        private void btnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
