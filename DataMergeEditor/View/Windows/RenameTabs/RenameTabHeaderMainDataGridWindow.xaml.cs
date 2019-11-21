using System.Windows;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for RenameTabHeader.xaml
    /// </summary>
    public partial class RenameTabHeaderMainDataGridWindow : Window
    {
        public RenameTabHeaderMainDataGridWindow()
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
