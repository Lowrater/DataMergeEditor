namespace DataMergeEditor.Model.Connections
{
    public class mysql_connectionModel
    {          
        //-- Connectionstring
        private string __mysqlDatabaseName;
        public ref string _mysqlDatabaseName => ref __mysqlDatabaseName;

        private string __mysqlUserName;
        public ref string _mysqlUserName => ref __mysqlUserName;

        private string __mysqlUserPassword;
        public ref string _mysqlUserPassword => ref __mysqlUserPassword;

        private string __mysqlHostName;
        public ref string _mysqlHostName => ref __mysqlHostName;
    }
}
