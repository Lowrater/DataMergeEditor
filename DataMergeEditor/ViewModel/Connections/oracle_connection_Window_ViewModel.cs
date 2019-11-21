using DataMergeEditor.DBConnect.Adapter;
using DataMergeEditor.Messages;
using DataMergeEditor.Model;
using DataMergeEditor.Model.Connections;
using DataMergeEditor.Model.Connections.Odbc;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataMergeEditor.ViewModel.Connections
{
    //-- guide: https://stackoverflow.com/questions/6457973/odbc-driver-list-from-net 

    public class oracle_connection_Window_ViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        //-- klassens constructor
        public oracle_connection_Window_ViewModel(IDataService dataService)
        {
            this.dataService = dataService;
            //-- sætter odbc driver listen til odbc driver listen (32bit)
            odbclist = odbcaddons.GetOdbcDriverNames();
            ActiveConnectionimagePath = @"/Resources/inactive_database_tree_icon.png";
        }
        private odbc_addons odbcaddons = new odbc_addons();
        private oracle_connectionModel oracle_ConnectionModel = new oracle_connectionModel();
        //-- til Oraclesql
        public ICommand OracleConnectBtnCommand => new RelayCommand<PasswordBox>(Oracleconnect_button);
        //-- driver liste til odbc valg
        public ObservableCollection<string> odbclist
        {
            get => oracle_ConnectionModel._odbclist;
            set => Set(ref oracle_ConnectionModel._odbclist, value);
        }   
        public string ActiveConnectionimagePath
        {
            get => oracle_ConnectionModel._dbConnectionImage;
            set => Set(ref oracle_ConnectionModel._dbConnectionImage, value);
        }
        //-- Vælger den valgte driver fra comboboxen
        public string OracleDriver
        {
            get => oracle_ConnectionModel._oracleDriver;
            set => Set(ref oracle_ConnectionModel._oracleDriver, value);
        }
        public string OracleDatabase
        {
            get => oracle_ConnectionModel._oracleDatabase;
            set => Set(ref oracle_ConnectionModel._oracleDatabase, value);
        }
        public string OracleUsername
        {
            get => oracle_ConnectionModel._oracleUsername;
            set => Set(ref oracle_ConnectionModel._oracleUsername, value);
        }
        public string OraclePassword
        {
            get => oracle_ConnectionModel._oraclePassword;
            set => Set(ref oracle_ConnectionModel._oraclePassword, value);
        }
        //------------------- Connect button
        //-- Oracle Connection button
        private void Oracleconnect_button(PasswordBox passwordSecured)
        {
            OraclePassword = passwordSecured.Password;

            //-- SENDER Oracle forbindelsen af typen string, Adaptertype, Action<bool>, string databasenavn
            //-- SENDER TIL DatabaseConnectionViewModel
            MessengerInstance.Send(new ConnectionMessages
                (
                    $"Driver={OracleDriver};Dbq={OracleDatabase};Uid={OracleUsername};pwd={OraclePassword}",
                    DBAdapterType.Oracle,
                    new Action<bool>(s =>
                    {
                        if (s) onSuccess();
                        else onFailure();
                    }),
                    OracleDatabase
                ));
            void onSuccess()
            {
                //-- Lukker alle Windows som har Denne klasse som indhold.
                //-- Er virkelig dårlig ting at gøre, hvis man har flere vinduer åbne, men ikke udfyldte.
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.Close();
                    }
                }
                MessageBox.Show($"Connected to {OracleDatabase}", "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            void onFailure()
            {
                TableAddons.writeLogFile(@"Failed connecting to {OracleDatabase}", dataService.LogLocation);
                MessageBox.Show($"failed connecting to {OracleDatabase}", "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }  
    }
}

