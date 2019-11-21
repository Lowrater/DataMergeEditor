using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DataMergeEditor.Interfaces
{
    public abstract class DataConnection : IDataConnection
    {
        public abstract Task<DataTable> Execute(string command, IProgress<int> progress, CancellationToken cancellationToken,
                                                CancellationTokenSource source, int rowlimiter);
        public abstract string Name { get; }
        public abstract object Type { get; }
        //-- Kommandoer alt efter hvad der skal ske.
        public abstract void SaveToDataBase(string cmd, IProgress<int> progress);
        public abstract void RemoveFromDatabase(string cmd, IProgress<int> progress, bool askBeforeDel);
        public abstract void DeleteFromDatabase(string cmd, IProgress<int> progress, bool askBeforeDel);
        public abstract void AddToDatabase(string cmd, IProgress<int> progress);
        public abstract void UnknownReadCreateToDatabaes(string cmd);
        public abstract void Disconnect(string dbname);
        public abstract void Reconnect(string dbname);
        public abstract string CreateTableCommand(string command, string TableName);
        //-- begrænsning af 5.000 rækker
        public abstract void LimitRecordFetching(CancellationTokenSource ct, int RecordCount);
        //-- for antallet af rækker
        public abstract int GetTableRowCount(string command);
        //-- Gemmer ændringerne mod databasen via. knap ud fra den redigerede tabel fra brugeren
        public abstract void ApplyChanges(DataTable dataTable, string command, bool AskOnappl, IProgress<int> progress);
        public abstract Task<DataTable> FetchTableList(CancellationToken cancellationToken, CancellationTokenSource source);
        public abstract void ExportToExternalDatabase(DataTable dataTable, string TableName, bool sameDB, string CurrentTable);
    }
}
