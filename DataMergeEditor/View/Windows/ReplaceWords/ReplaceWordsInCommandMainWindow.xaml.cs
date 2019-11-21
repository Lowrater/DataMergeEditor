using System.Windows;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for ReplaceWordsInColumnWindow.xaml
    /// </summary>
    public partial class ReplaceWordsInCommandMainWindow : Window
    {
        public ReplaceWordsInCommandMainWindow()
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
