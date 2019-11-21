using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace DataMergeEditor.Model.Connections.Odbc
{
    //-- 32 bit liste: "SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"
    //-- 64 bit liste: "SOFTWARE\Wow6432Node\ODBC\ODBCINST.INI\ODBC Drivers"
    public class odbc_addons
    {
        //-- New
        public ObservableCollection<string> GetOdbcDriverNames()
        {
            ObservableCollection<string> odbcdrivers_list = new ObservableCollection<string>();
            try
            {           
                using (RegistryKey localMachineHive = Registry.LocalMachine)
                using (RegistryKey odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
                {
                    if (odbcDriversKey != null)
                    {
                        foreach (var item in odbcDriversKey.GetValueNames())
                        {
                            odbcdrivers_list.Add(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot access ODBC list locally",
                    "DataMergeEditor - Access ODBC list error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            return odbcdrivers_list;
        }
    }
}
