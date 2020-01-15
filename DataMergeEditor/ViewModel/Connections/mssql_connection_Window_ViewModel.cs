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
    public class mssql_connection_Window_ViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        public mssql_connection_Window_ViewModel(IDataService dataService)
        {
            this.dataService = dataService;
        }
        private mssql_connectionModel mssql_ConnectionModel = new mssql_connectionModel();
       //-- til mssql
        public ICommand MsConnectBtnCommand => new RelayCommand<PasswordBox>(Msconnect_button);
        //-- properties til connection button
        public string MSDataSource
        {
            get => mssql_ConnectionModel._mSDataSource;
            set => Set(ref mssql_ConnectionModel._mSDataSource, value);
        }
        public string MSInitialCatalog
        {
            get => mssql_ConnectionModel._mSInitialCatalog;
            set => Set(ref mssql_ConnectionModel._mSInitialCatalog, value);
        }
        public string MSPersistSecurityInfo
        {
            get => mssql_ConnectionModel._mSPersistSecurityInfo;
            set => Set(ref mssql_ConnectionModel._mSPersistSecurityInfo, value);
        }
        public string MSUserID
        {
            get => mssql_ConnectionModel._mSUserID;
            set => Set(ref mssql_ConnectionModel._mSUserID, value);
        }
        public string MSUserPassword
        {
            get => mssql_ConnectionModel._mSUserPassword;
            set => Set(ref mssql_ConnectionModel._mSUserPassword, value);
        }
        //------------------- Connect button
        //-- MS Connection button
        private void Msconnect_button(PasswordBox passwordSecured)
        {
            MSUserPassword = passwordSecured.Password;
            //-- NEW
            //-- SENDER MSSQL forbindelsen, samt typen retur til main vinduet
            MessengerInstance.Send(new ConnectionMessages
                (
                    $"Data Source = {MSDataSource};" +
                    $"Initial Catalog = {MSInitialCatalog};" +
                    $" Persist Security Info ={MSPersistSecurityInfo};" +
                    $" User ID ={ MSUserID}; Password = {MSUserPassword};",
                    DBAdapterType.Mssql,
                    new Action<bool>(s =>
                    {
                        if (s) onSuccess();
                        else onFailure();
                    }),
                    MSInitialCatalog
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
     
                MessageBox.Show($"Connected to {MSDataSource}",
                    "DataMergeEditor - Database connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            void onFailure()
            {
                MessageBox.Show($"Connected failed to {MSDataSource}",
                    "DataMergeEditor - Database connection",
                     MessageBoxButton.OK,
                     MessageBoxImage.Information);
            }
        }
    }
}
