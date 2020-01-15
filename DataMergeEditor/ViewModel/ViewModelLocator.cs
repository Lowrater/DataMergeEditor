using CommonServiceLocator;
using DataMergeEditor.DBConnect.Data.TreeData;
using DataMergeEditor.Interfaces;
using DataMergeEditor.Services;
using DataMergeEditor.ViewModel.Connections;
using DataMergeEditor.ViewModel.Log;
using GalaSoft.MvvmLight.Ioc;
using System;

namespace DataMergeEditor.ViewModel
{
    /// <summary>
    /// Oprettet af GalaSoft liberi'et
    /// </summary>
    public class ViewModelLocator
    {
        //-- Det er en alternativ måde at få adgang til viewmodels på. De kan bindes, frem for referes.

        //-- Scenarie 1) Test
        //-- Man bruger ikke dataservice klassen, men test data service. - Eks. hardcoded test data.

        //-- Secnarie 2) Prod
        //-- Bruger den oprindelige dataservice klasse.

        //-- Constructor
        public ViewModelLocator()
        {
            //-- Services
            //-- Container for alle viewmodels. - Analysere for en klasse man registrere. Har klassen paramentre, skal der laves instanser af disse.
            ServiceLocator.SetLocatorProvider(()=> SimpleIoc.Default);
            SimpleIoc.Default.Register<MainTableViewModel>();
            SimpleIoc.Default.Register<DataGridViewModel>();
            SimpleIoc.Default.Register<SidePanelViewModel>();
            SimpleIoc.Default.Register<NewQueryTabItemViewModel>();
            SimpleIoc.Default.Register<Compare_tabItemViewModel>();
            SimpleIoc.Default.Register<DataBaseConnectionTreeViewModel>();
            SimpleIoc.Default.Register<NewScriptNoteViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<LogViewModel>();

            //-- Connections
            SimpleIoc.Default.Register<oracle_connection_Window_ViewModel>();
            SimpleIoc.Default.Register<mysql_connection_Window_ViewModel>();
            SimpleIoc.Default.Register<mssql_connection_Window_ViewModel>();            

            //-- Registere services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IViewService, ViewService>();

            //-- Tree
            SimpleIoc.Default.Register<IOracleTree, OracleTree>();
            SimpleIoc.Default.Register<IMysqlTree, MysqlTree>();
            SimpleIoc.Default.Register<IMssqlTree, MssqlTree>();
        }

        //--  ViewModels
        //-- (Guid.NewGuid().ToString()); = Betyder at når der laves en ny instans af objektet, holder klasserne og referencende dataen adskilt.
        //-- Ellers peger de på den samme viewmodel hele tiden.
        public MainTableViewModel Main =>
            ServiceLocator.Current.GetInstance<MainTableViewModel>();
        public DataGridViewModel Datagrid =>
            ServiceLocator.Current.GetInstance<DataGridViewModel>();
        public SidePanelViewModel Sidepanel =>
            ServiceLocator.Current.GetInstance<SidePanelViewModel>();
        public DataBaseConnectionTreeViewModel DataTree => 
            ServiceLocator.Current.GetInstance<DataBaseConnectionTreeViewModel>();
        public SettingsViewModel Settings =>
            ServiceLocator.Current.GetInstance<SettingsViewModel>();

        //-- Log
        public LogViewModel log => 
            ServiceLocator.Current.GetInstance<LogViewModel>(Guid.NewGuid().ToString());

        //-- ViewModels - Nye Tab faner som tilføjes:
        public NewQueryTabItemViewModel NewQuery => 
            ServiceLocator.Current.GetInstance<NewQueryTabItemViewModel>(Guid.NewGuid().ToString());
        public Compare_tabItemViewModel Compare => 
            ServiceLocator.Current.GetInstance<Compare_tabItemViewModel>(Guid.NewGuid().ToString());
        public NewScriptNoteViewModel NewScript =>
            ServiceLocator.Current.GetInstance<NewScriptNoteViewModel>(Guid.NewGuid().ToString());

        //-- Forbindelser
        public oracle_connection_Window_ViewModel Oracle => 
            ServiceLocator.Current.GetInstance<oracle_connection_Window_ViewModel>();
        public mysql_connection_Window_ViewModel MySql => 
            ServiceLocator.Current.GetInstance<mysql_connection_Window_ViewModel>();
        public mssql_connection_Window_ViewModel MsSql =>
            ServiceLocator.Current.GetInstance<mssql_connection_Window_ViewModel>();      
    }
}
