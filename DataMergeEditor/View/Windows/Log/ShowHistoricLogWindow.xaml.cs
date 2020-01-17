using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.View.Windows.Log
{
    /// <summary>
    /// Interaction logic for ShowHistoricLogWindow.xaml
    /// </summary>
    public partial class ShowHistoricLogWindow : Window
    {
        public ShowHistoricLogWindow()
        {
            InitializeComponent();

        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
           
        }
    }
}
