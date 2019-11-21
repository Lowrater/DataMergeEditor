using System;
using System.Windows;

namespace DataMergeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView() 
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

        }

        /// <summary>
        /// Tillader at lukke programmet helt, når det lukker
        /// guide: https://stackoverflow.com/questions/9992119/wpf-app-doesnt-shut-down-when-closing-main-window/9992888 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

    }
}
