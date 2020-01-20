using DataMergeEditor.DBConnect.Adapter;
using System;

namespace DataMergeEditor.Messages
{
    public class ConnectionMessages
    {
        public ConnectionMessages(string connectionString, DBAdapterType type, Action<bool> callback, string datatbasename)
        {
            this.ConnectionString = connectionString;
            this.Type = type;
            this.Callback = callback;
            this.Databasename = datatbasename;
        }

        public string Databasename { get; set; }
        public string ConnectionString { get; set; }
        public DBAdapterType Type { get; set; }
        private Action<bool> Callback { get; set; }

        public void Execute(bool success)
        {
            Callback?.Invoke(success);
        }
    }
}
