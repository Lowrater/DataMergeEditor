namespace DataMergeEditor.Model
{
    public class SettingsWindowModel
    {
        private bool __askOnInsert;
        public ref bool _askOnInsert => ref __askOnInsert;
        private bool __askOnDrop;
        public ref bool _askOnDrop => ref __askOnDrop;
        private bool __askOnAcceptChanges;
        public ref bool _askOnAcceptChanges => ref __askOnAcceptChanges;
        private string __logFileLocation;
        public ref string _logFileLocation => ref __logFileLocation;
        private string __logFileName;
        public ref string _logFileName => ref __logFileName;
        private int __rowlimit;
        public ref int _rowlimit => ref __rowlimit;
        private bool __defaultTheme;
        public ref bool _defaultTheme => ref __defaultTheme;
        private bool __darkTheme;
        public ref bool _darkTheme => ref __darkTheme;
    }
}
