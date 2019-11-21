using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataMergeEditor.DBConnect;

namespace DataMergeEditor.View.Windows
{
    /// <summary>
    /// Interaction logic for GetTablesFromDBWindow.xaml
    /// </summary>
    public partial class GetTablesFromDBWindow : Window
    {
        public GetTablesFromDBWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
    }
}
