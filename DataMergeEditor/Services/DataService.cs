using DataMergeEditor.DBConnect.Data.TreeData;
using DataMergeEditor.Interfaces;
using DataMergeEditor.View.Windows.Log;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace DataMergeEditor.Services
{
    public class DataService : ViewModelBase, IDataService
    {
        public Dictionary<string,IDataConnection> ConnectionList { get; set; } = new Dictionary<string, IDataConnection>();
        public ObservableCollection<string> ConnectionListNames { get; set; } = new ObservableCollection<string>();
        public string LogLocation { get; set; }
        public bool AskOnInsert { get; set; }
        public bool AskOnDrop { get; set; }
        public bool AskOnAcceptChanges { get; set; }
        public int RowLimiter { get; set; }
        public DataService(IOracleTree oracleTree, IMysqlTree mysqlTree, IMssqlTree mssqlTree)
        {
            this.oracleTree = oracleTree;
            this.mysqlTree = mysqlTree;
            this.mssqlTree = mssqlTree;
        }

        /// <summary>
        /// Show Database execetuded queries
        /// Skal gøres for den aktive forbindelse for det view brugeren ser. (this.viewmodel)
        /// Find forbindelses typen til string
        /// </summary>
        public void ShowDatabaseQueryHistoric()
        {

            //-- setter værdien for CellValueTextString
            //-- Sat i try catch fordi det er forskelligt pr. forbindelse, og hvor deres log er.
            try
            {
                var window = new ShowHistoricLogWindow();              
                window.Show();
            }
            catch (Exception e)
            {

                MessageBox.Show("An error accured fetchig historic log:"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "The error sounded like: "
                    + e.ToString().Substring(0, 250),
                    "DataMergeEditor - Fetch historic log",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        //------------------- TIL SETTINGS VINDUET
        public IOracleTree oracleTree { get; }
        public IMysqlTree mysqlTree { get; }
        public IMssqlTree mssqlTree { get; }  
    }
}
