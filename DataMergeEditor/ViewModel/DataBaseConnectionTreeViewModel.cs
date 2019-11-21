using DataMergeEditor.DBConnect;
using DataMergeEditor.DBConnect.Adapter;
using DataMergeEditor.DBConnect.Data;
using DataMergeEditor.Messages;
using DataMergeEditor.Model;
using DataMergeEditor.Services;
using DataMergeEditor.View.Windows.Connections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.ViewModel
{
    public class DataBaseConnectionTreeViewModel : ViewModelBase
    {
        //-- Dataservice for at dele data mellem viewmodels
        private readonly IDataService dataService;
        public DataBaseConnectionTreeViewModel(IDataService dataService)
        {
            this.dataService = dataService;

            //-- Modatger MYSQL, Oracle forbindelser etc..
            //-- Giver reference til metoden som har argumentet ConnectionMessage
            //-- Tager imod beskeder fra Vores forbindelses klasser. EKs. mysql_connection_Window_ViewModel
            //-- SE ONConnection Metoden
            MessengerInstance.Register<ConnectionMessages>(this, OnConnection);
        }

        private DatabaseConnectionModel databaseConnectionModel = new DatabaseConnectionModel();
        public ICommand startMSSQL_window_command => new RelayCommand(start_MSSQL_window);
        public ICommand startMySQL_window_command => new RelayCommand(start_MySQL_window);
        public ICommand startOracle_window_command => new RelayCommand(start_oracle_window);
        public ICommand DisconnectFromSelectedMenuItemCommand => new RelayCommand(DisconnectByMenuItemName);
        public ICommand ReconnectFromSelectedMenuItemCommand => new RelayCommand(ReconnectByMenuItemName);
        public ICommand ChangeConnectionGlobalBySelectedMenuItemCommand => new RelayCommand(ChangeConnectionGlobalByMenuItemName);
        public ICommand RemoveConnectionByMenuItemNameCommand => new RelayCommand(RemoveConnectionByMenuItemName);

        /// <summary>
        /// Til at modtage nye forbindelser via. Enkelte valgte forbindelse i sidepanelet.
        /// Tilføjer forbindelsen i forbindelses listen hvis den er gyldig
        /// Åbner forbindelsen
        /// </summary>
        /// <param name="message"></param>
        public void OnConnection(ConnectionMessages message)
        {
            try
            {
                switch (message.Type)
                {
                    case DBAdapterType.Mysql:
                        var MysqlCon = new MySqlConnection(message.ConnectionString);
                        MysqlCon.Open();
                        dataService.ConnectionList.Add(message.Databasename, new MySqlDataConnection(MysqlCon));
                        Items.Add(dataService.mysqlTree.BuildTree(MysqlCon));
                        message.Execute(true);
                        TableAddons.writeLogFile($"{message.Databasename} type of MySql, has been added to" +
                            $" the connection list", dataService.LogLocation);
                        //-- Sender en besked 'true' til SidepanelViewModel så du kan tilføje tabeller
                        Messenger.Default.Send<bool, SidePanelViewModel>(true);
                        ItemsFilter = Items;
                        break;
                    case DBAdapterType.Oracle:
                        var OracleCon = new OdbcConnection(message.ConnectionString);
                        OracleCon.Open();
                        dataService.ConnectionList.Add(message.Databasename, new OracleDataConnection(OracleCon));
                        Items.Add(dataService.oracleTree.BuildTree(OracleCon));
                        message.Execute(true);
                        TableAddons.writeLogFile($"{message.Databasename} type of Oracle, has been added to" +
                            $" the connection list", dataService.LogLocation);
                        //-- Sender en besked 'true' til SidepanelViewModel så du kan tilføje tabeller
                        Messenger.Default.Send<bool, SidePanelViewModel>(true);
                        ItemsFilter = Items;
                        break;
                    case DBAdapterType.Mssql:
                        var MssCon = new SqlConnection(message.ConnectionString);
                        MssCon.Open();
                        dataService.ConnectionList.Add(message.Databasename, new MssqlDataConnection(MssCon));
                        Items.Add(dataService.mssqlTree.BuildTree(MssCon));
                        message.Execute(true);
                        TableAddons.writeLogFile($"{message.Databasename} type of MsSql, has been added to" +
                            $" the connection list", dataService.LogLocation);
                        //-- Sender en besked 'true' til SidepanelViewModel så du kan tilføje tabeller
                        Messenger.Default.Send<bool, SidePanelViewModel>(true);
                        ItemsFilter = Items;
                        break;
                    default:
                        message.Execute(false);
                        break;
                }
            }
            catch
            {
                message.Execute(false);
            }
        }

        /// <summary>
        /// Testet mht. at binde en valgte menuItem, så man kan disconnect, reconnect, og fjerne forbindelsen fra listen.
        /// </summary>
        public TreeNodeViewModel ChoosenMenuitem
        {
            get => databaseConnectionModel._choosenmenuitem;
            set => Set(ref databaseConnectionModel._choosenmenuitem, value);
        }

        /// <summary>
        /// Disconnecter fra valgte markeret navn i tree-viewet
        /// Se ViewModel -> TreeViewModel -> ModdetTreeView for reference
        /// </summary>
        public void DisconnectByMenuItemName()
        {
            try
            {
                var DatabaseName = TableAddons.getBetween(ChoosenMenuitem.Name, "", "-");
                if (!string.IsNullOrEmpty(DatabaseName))
                {
                    dataService.ConnectionList.FirstOrDefault(x => x.Key == DatabaseName).Value.Disconnect(DatabaseName);
                }
                else
                {
                    MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                    "DataMergeEditor - Disconnect message", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                "DataMergeEditor - Disconnect message", 
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// ReConnecter fra valgte markeret navn i tree-viewet
        /// Se ViewModel -> TreeViewModel -> ModdetTreeView for reference
        /// </summary>
        public void ReconnectByMenuItemName()
        {
            try
            {
                var DatabaseName = TableAddons.getBetween(ChoosenMenuitem.Name, "", "-");
                if (!string.IsNullOrEmpty(DatabaseName))
                {
                    dataService.ConnectionList.FirstOrDefault(x => x.Key == DatabaseName).Value.Reconnect(DatabaseName);
                }
                else
                {
                    MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                    "DataMergeEditor - Reconnect message",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {

                MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                "DataMergeEditor - Reconnect message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Skifter forbindelsen Globalt i hele programmet til den valge markeret navn i tree-viewet
        /// Se ViewModel -> TreeViewModel -> ModdetTreeView for reference
        /// </summary>
        public void ChangeConnectionGlobalByMenuItemName()
        {
            try
            {
                var DatabaseName = TableAddons.getBetween(ChoosenMenuitem.Name, "", "-");
                if (!string.IsNullOrEmpty(DatabaseName))
                {
                    //-- sender stringen ud til alle viewmodeller og sætter HovedForbindelsen der til.
                    Messenger.Default.Send<string, NewQueryTabItemViewModel>(DatabaseName);
                    Messenger.Default.Send<string, MainTableViewModel>(DatabaseName);
                    Messenger.Default.Send<string, Compare_tabItemViewModel>(DatabaseName);

                    //-- Meddelse til brugeren.
                    MessageBox.Show($"The connection to {DatabaseName} has been set globally"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Use 'Change database to xxx' again if wished to change connection individually",
                        "DataMergeEditor - Set global connection message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                    "DataMergeEditor - Set global connection message",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                "DataMergeEditor - Set global connection message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// Disconnector, og fjerner forbindelsen permanent i hele programmet,
        /// - ud fra den valge markeret navn i tree-viewet
        /// Se ViewModel -> TreeViewModel -> ModdetTreeView for reference
        /// </summary>
        public void RemoveConnectionByMenuItemName()
        {
            try
            {
                //-- The name of the database name, is written as: Database name - {Type} as string
                var DatabaseName = TableAddons.getBetween(ChoosenMenuitem.Name, "", "-");

                if (!string.IsNullOrEmpty(DatabaseName))
                {
                    //-- Laver en disconnect først
                    dataService.ConnectionList.FirstOrDefault(x => x.Key == DatabaseName).Value.Disconnect(DatabaseName);
                    //-- Fjerner forbindelsen fra tree-viewet
                    Items.Remove(ChoosenMenuitem);
                    //-- Fjerner fra HovedListen alle klasser kan se
                    dataService.ConnectionList.Remove(DatabaseName);
                    //-- Setter listen på ny (i tilfælde den ikke opdatere automatisk, på et tidspunkt)
                    ItemsFilter = Items;
                    MessageBox.Show($"The connection to {DatabaseName} as successfully removed",
                        "DataMergeEditor - Remove connection message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if(ItemsFilter.Count.Equals(0))
                    {
                        //-- Sender en besked 'false' til SidepanelViewModel 
                        //-- så du ikke skulle kunne tilføje tabeller, da vi ikke er forbundet til nogle forbindelser
                        Messenger.Default.Send<bool, SidePanelViewModel>(false);
                    }
                }
                else
                {
                    MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                    "DataMergeEditor - Remove connection message",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid connection choosen. Please verify your selected database",
                                "DataMergeEditor - Remove connection message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        //--------------------------------------- tree view ----------------------------------
        //-- guide: https://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode 

        /// <summary>
        /// Listen af alle forbindelser som default, grundet filtrering
        /// </summary>
        public ObservableCollection<TreeNodeViewModel> ItemsFilter
        {
            get => databaseConnectionModel._itemsFilter;
            set => Set(ref databaseConnectionModel._itemsFilter, value);
        }

        /// <summary>
        /// Tilføj forbindelses listerne til denne
        ///  Den orignale liste med indhold af alle forbindelserne
        /// </summary>
        public ObservableCollection<TreeNodeViewModel> Items
        {
            get => databaseConnectionModel.Items;
            set => Set(ref databaseConnectionModel.Items, value);
        }

        /// <summary>
        /// Constructoren som et tre.
        /// Ukendt hvordan det fungere. - Umiddelbart returnere den sig selv, hvor man kan få fat i Items ovenstående.
        /// </summary>
        public DataBaseConnectionTreeViewModel Tree
        {
            get { return this; }
        }
 
        /// <summary>
        /// Ord som bruges til søgemaskinen for at fremsøge specifik tabel
        /// </summary>
        public string SearchedForTableWord
        {
            get => databaseConnectionModel._searchedfortableword;
            set
            {
                Set(ref databaseConnectionModel._searchedfortableword, value);
                //-- Giver Items listen med alle noderne ud, til at sætte på ny.
                FindTableByMatchedWord(value);              
            } 
        }

        /// <summary>
        /// skal bruges til en yderligere søgemaskine for noderne
        /// Tager imod SearchedForTableWord som er bundet til søgemaskinen.
        /// </summary>
        /// <param name="searchtext"></param>
        public void FindTableByMatchedWord(string searchtext)
        {
            var FilteredTree = new ObservableCollection<TreeNodeViewModel>();
            //-- Root af alle knyttede forbindelser
            foreach (TreeNodeViewModel node in Items)
            {
                if (!string.IsNullOrEmpty(searchtext))
                {
                    //--- Database navn
                    foreach (TreeNodeViewModel child in node.Children)
                    {
                        if (child.Name.ToLower().Contains(searchtext.ToLower()))
                        {
                            FilteredTree.Add(child);
                        }
                        //-- child node. eks. tables, indexes, triggers
                        foreach (TreeNodeViewModel child2 in child.Children)
                        {
                            if (child2.Name.ToLower().Contains(searchtext.ToLower()))
                            {
                                FilteredTree.Add(child2);
                            }
                            //-- child second node, eks content: Access_code etc.
                            foreach (TreeNodeViewModel child3 in child2.Children)
                            {
                                if (child3.Name.ToLower().Contains(searchtext.ToLower()))
                                {
                                    FilteredTree.Add(child3);
                                }
                            }
                        }
                    }
                    ItemsFilter = FilteredTree;
                }
                else if(string.IsNullOrEmpty(searchtext) || !string.IsNullOrWhiteSpace(searchtext))
                {
                    ItemsFilter = Items;
                }
            }
        }

        //-- tilføjer eksempel til tre strukturen
        //-- https://stackoverflow.com/questions/30049967/how-to-load-database-to-treeview-in-wpf-with-mvvm 
        //-- guide 2: https://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode 

        /// <summary>
        /// start mssql vinduet
        /// </summary>
        public void start_MSSQL_window()
        {
            mssql_connection_window mssql_connection_window = new mssql_connection_window();
            mssql_connection_window.Show();
        }


        /// <summary>
        /// starter oracle vinduet
        /// </summary>
        public void start_oracle_window()
        {
            oracle_connection_window oracle_Connection_Window = new oracle_connection_window();
            oracle_Connection_Window.Show();
        }

        /// <summary>
        /// starter mysql vinduet
        /// </summary>
        public void start_MySQL_window()
        {
            MySQL_panel_connection_window MySQL_panel_connection_window = new MySQL_panel_connection_window();
            MySQL_panel_connection_window.Show();
        }
    }
}

