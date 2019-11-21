using System;
using System.ComponentModel;
using System.Windows;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        protected override void OnClosing(CancelEventArgs e)
        {

            base.OnClosing(e);
 
            e.Cancel = true;
            this.Hide();           
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
