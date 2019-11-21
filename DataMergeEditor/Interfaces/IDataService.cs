using DataMergeEditor.DBConnect.Data.TreeData;
using DataMergeEditor.Interfaces;
using System.Collections.Generic;

namespace DataMergeEditor.Services
{
    //-- Interface = kontrakt
    //-- eks. Metode der returnere et interface.
    //-- Interface har ingen kode, den definere APi for et object.
    //-- Praktisk = Et bibliotek af servicen, som returnere interfacen i stedet.
    //-- Typen er irrelevant, så længe man kan udskifte implementeringen. (metoder, osv er mest relevant)
    //-- guide: https://stackoverflow.com/questions/19962926/wpf-mvvm-shared-data-source 

    public interface IDataService
    {
        Dictionary<string, IDataConnection> ConnectionList { get; set; }
        string LogLocation { get; set; }
         bool AskOnInsert { get; set; }
         bool AskOnDrop { get; set; }
         bool AskOnAcceptChanges { get; set; }
        int RowLimiter { get; set; }
        IOracleTree oracleTree { get; }
        IMysqlTree mysqlTree { get;  }
        IMssqlTree mssqlTree { get; }
        /// <summary>
        /// Show Database execetuded queries
        /// Skal gøres for den aktive forbindelse for det view brugeren ser. (this.viewmodel)
        /// Find forbindelses typen til string
        /// </summary>
        void ShowDatabaseQueryHistoric();
    }
}