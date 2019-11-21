using DataMergeEditor.DBConnect.Adapter;
using DataMergeEditor.Messages;
using DataMergeEditor.Model.Connections;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataMergeEditor.ViewModel.Connections
{
    public class mysql_connection_Window_ViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        public mysql_connection_Window_ViewModel(IDataService dataService)
        {
            this._dataService = dataService;
        }
        private mysql_connectionModel MYSQLsql_ConnectionModel = new mysql_connectionModel();
        //-- til MYSQLsql
        public ICommand MYSQLConnectBtnCommand => new RelayCommand<PasswordBox>(MYSQLconnect_button);
        //-- properties til connection button
        public string MysqlDatabaseName
        {
            get => MYSQLsql_ConnectionModel._mysqlDatabaseName;
            set => Set(ref MYSQLsql_ConnectionModel._mysqlDatabaseName, value);
        }
        public string MysqlUserName
        {
            get => MYSQLsql_ConnectionModel._mysqlUserName;
            set => Set(ref MYSQLsql_ConnectionModel._mysqlUserName, value);
        }
        public string MysqlUserPassword
        {
            get => MYSQLsql_ConnectionModel._mysqlUserPassword;
            set => Set(ref MYSQLsql_ConnectionModel._mysqlUserPassword, value);
        }
        public string MysqlHostName
        {
            get => MYSQLsql_ConnectionModel._mysqlHostName;
            set => Set(ref MYSQLsql_ConnectionModel._mysqlHostName, value);
        }
        //------------------- Connect button
        //-- MYSQL Connection button
        private void MYSQLconnect_button(PasswordBox passwordSecured)
        {
            MysqlUserPassword = passwordSecured.Password;
            //-- NEW
            //-- SENDER MYSQL forbindelsen, samt typen retur til main vinduet
            MessengerInstance.Send(new ConnectionMessages
                (
                    $"Server={MysqlHostName};" +
                    $"database={MysqlDatabaseName};" +
                    $"user id={MysqlUserName};" +
                    $"password={MysqlUserPassword};",
                    DBAdapterType.Mysql,
                    new Action<bool>(s =>
                    {
                        if (s) onSuccess();
                        else onFailure();
                    }),
                    MysqlDatabaseName
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

                MessageBox.Show($"Connected to {MysqlDatabaseName}",
                    "DataMergeEditor - Database connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            void onFailure()
            {
                MessageBox.Show($"Connected failed to {MysqlDatabaseName}",
                    "DataMergeEditor - Database connection",
                     MessageBoxButton.OK,
                     MessageBoxImage.Information);
            }
        }
    }
}
