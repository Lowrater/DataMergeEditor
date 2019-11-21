using System.Collections.ObjectModel;

namespace DataMergeEditor.Model.Connections
{
    public class oracle_connectionModel
    {     
        private ObservableCollection<string> __odbclist;
        public ref ObservableCollection<string> _odbclist => ref __odbclist;

        //-- billede
        private string __dbConnectionImage;
        public ref string _dbConnectionImage => ref __dbConnectionImage;

        //-- Til connectionstring
        private string __oracleDriver;
        public ref string _oracleDriver => ref __oracleDriver;

        private string __oracleDatabase;
        public ref string _oracleDatabase => ref __oracleDatabase;

        private string __oracleUsername;
        public ref string _oracleUsername => ref __oracleUsername;

        private string __oraclePassword;
        public ref string _oraclePassword => ref __oraclePassword;
    }
}
