using System.Windows;
using DataMergeEditor.View.Windows;
using System.Threading;

namespace DataMergeEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// 
    /// </summary>
    public partial class App : Application
    {
        protected  override void OnStartup(StartupEventArgs e)
        {
            WakeupLoad wakeupLoad = new WakeupLoad();
            wakeupLoad.Show();
            Thread.Sleep(3000);
            MainWindowView mainWindowView = new MainWindowView();
            wakeupLoad.Close();
        }
    }
}
