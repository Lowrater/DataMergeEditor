using DataMergeEditor.DBConnect.Adapter;
using System;

namespace DataMergeEditor.Messages
{
    public class ConnectionMessages
    {
        public ConnectionMessages(string connectionString, DBAdapterType type,
            Action<bool> callback, string datatbasename)
        {
            this.ConnectionString = connectionString;
            this.Type = type;
            this.Callback = callback;
            this.Databasename = datatbasename;
        }

        public string Databasename { get;  }
        public string ConnectionString { get; }
        public DBAdapterType Type { get; }
        private Action<bool> Callback { get; }

        public void Execute(bool success)
        {
            Callback?.Invoke(success);
        }
    }
}
