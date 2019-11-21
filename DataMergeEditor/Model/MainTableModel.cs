using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataMergeEditor.Model
{
    //-- ref for viewbase af Mvvmlight i stedet for get; set;
    public class MainTableModel 
    {
        private DataTable __datatablemergeren = new DataTable();
        public ref DataTable DataTableMergeren => ref __datatablemergeren;
        private DataTable __exportLabelFields = new DataTable();
        public ref DataTable _exportLabelFields => ref __exportLabelFields;
        private string __newCellValueTextString;
        public ref string _newCellValueTextString => ref __newCellValueTextString;
        private string __cellValueTextString;
        public ref string _cellValueTextString => ref __cellValueTextString;
        private string __currentDBName;
        public ref string _currentDBName => ref __currentDBName;     
        private string __columnDelimiter;
        public ref string _columnDelimiter => ref __columnDelimiter;
        private bool __sqlQueryTableHasChanged;
        public ref bool _sqlQueryTableHasChanged => ref __sqlQueryTableHasChanged;
        private int __mainTabIndexName;
        public ref int _mainTabIndexName => ref __mainTabIndexName;
        private string __renamedColumnValue;
        public ref string _renamedColumnValue => ref __renamedColumnValue;
        private string ___addColumnValueName;
        public ref string _addColumnValueName => ref ___addColumnValueName;
        private string __searchColumnValue;
        public ref string _searchColumnValue => ref __searchColumnValue;
        private DataGridCellInfo __tableColumnIndexisSelected;
        public ref DataGridCellInfo _tableColumnIndexisSelected => ref __tableColumnIndexisSelected;
        //----------------------------- SQL Del
        private ObservableCollection<string> __historyCommandsList = new ObservableCollection<string>();
        public ref ObservableCollection<string> HistoryCommandsList => ref __historyCommandsList;
        private ObservableCollection<string> __connectionList = new ObservableCollection<string>();
        public ref ObservableCollection<string> _connectionList => ref __connectionList;
        private ObservableCollection<TextBox> __sidePanelFilterListBoxes = new ObservableCollection<TextBox>();
        public ref ObservableCollection<TextBox> _sidePanelFilterListBoxes => ref __sidePanelFilterListBoxes;      
        private string __queryTXT;
        public ref string _queryTXT => ref __queryTXT;
        private string __selectedDatabaseText;
        public ref string _selectedDatabaseText => ref __selectedDatabaseText;
        private string __createTableEXP;
        public ref string _createTableEXP => ref __createTableEXP;
        private int __progressFill;
        public ref int _progressFill => ref __progressFill;
        private SolidColorBrush __pBarColorBrush;
        public ref SolidColorBrush _pBarColorBrush => ref __pBarColorBrush;
        private string __txtchange;
        public ref string _txtchange => ref __txtchange;
        private string __selectedIndexOfComboBoxItem;
        public ref string _selectedIndexOfComboBoxItem => ref __selectedIndexOfComboBoxItem;
        //------------------------- pop up record besked
        private bool __recordCancelProgressFillGlobal;
        public ref bool _recordCancelProgressFillGlobal => ref __recordCancelProgressFillGlobal;
        private int __recordCountingINTGlobal;
        public ref int _recordCountingINTGlobal => ref __recordCountingINTGlobal;
        private int __fetchRowCountGlobal;
        public ref int _fetchRowCountGlobal => ref __fetchRowCountGlobal;
    }
}
