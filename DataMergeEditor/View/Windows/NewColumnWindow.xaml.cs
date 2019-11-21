using System.Windows;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for NewColumnWindow.xaml
    /// </summary>
    public partial class NewColumnWindow : Window
    {
        public NewColumnWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
