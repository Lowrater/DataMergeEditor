using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for ShowCellContentWindow.xaml
    /// </summary>
    public partial class ShowCellContentWindow : Window
    {
        public ShowCellContentWindow()
        {  
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
