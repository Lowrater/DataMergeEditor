using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DataMergeEditor.Interfaces
{
    public interface IDataConnection
    {
        Task<DataTable> Execute(string command, IProgress<int> progress, 
            CancellationToken cancellationToken, CancellationTokenSource source, int rowlimiter);
        string Name { get; }
        object Type { get; }

        //-- Kommandoer
        void SaveToDataBase(string cmd, IProgress<int> progress);
        void RemoveFromDatabase(string cmd, IProgress<int> progress, bool AskBeforedel);
        void DeleteFromDatabase(string cmd, IProgress<int> progress, bool AskBeforedel);
        void AddToDatabase(string cmd, IProgress<int> progress);
        void UnknownReadCreateToDatabaes(string cmd);
        void Disconnect(string dbname);
        void Reconnect(string dbname);
        string CreateTableCommand(string command, string TableName);


        /// <summary>
        /// begrænsning af 5.000 rækker
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="RecordCount"></param>
        void LimitRecordFetching(CancellationTokenSource ct, int RecordCount);

        /// <summary>
        /// For antallet af rækker
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        int GetTableRowCount(string command);

        /// <summary>
        /// Gemmer ændringerne mod databasen via. knap ud fra den redigerede tabel fra brugeren
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="command"></param>
        /// <param name="AskOnappl"></param>
        /// <param name="progress"></param>
        void ApplyChanges(DataTable dataTable, string command, bool AskOnappl, IProgress<int> progress);

        /// <summary>
        /// Til at få tabel oversigt til permanente tabeller
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<DataTable> FetchTableList(CancellationToken cancellationToken, CancellationTokenSource source);

        /// <summary>
        /// Exportere til extern database
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="TableName"></param>
        void ExportToExternalDatabase(DataTable dataTable, string TableName, bool sameDB, string CurrentTable);
    }
}