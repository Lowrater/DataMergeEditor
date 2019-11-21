namespace DataMergeEditor.Model.Connections
{
    public class mssql_connectionModel
    {
        //-- connectionstring
        private string __mSDataSource;
        public ref string _mSDataSource => ref __mSDataSource;

        private string __mSInitialCatalog;
        public ref string _mSInitialCatalog => ref __mSInitialCatalog;

        private string __mSPersistSecurityInfo;
        public ref string _mSPersistSecurityInfo => ref __mSPersistSecurityInfo;

        private string __mSUserID;
        public ref string _mSUserID => ref __mSUserID;

        private string __mSUserPassword;
        public ref string _mSUserPassword => ref __mSUserPassword;
    }
}
