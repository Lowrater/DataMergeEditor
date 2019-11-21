using System.Windows;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for RenameTabHeader.xaml
    /// </summary>
    public partial class RenameTabHeaderNewQueryWindow : Window
    {
        public RenameTabHeaderNewQueryWindow()
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
