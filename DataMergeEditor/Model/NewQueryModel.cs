using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataMergeEditor.Model
{
    public class NewQueryModel
    {
        private ObservableCollection<NewTabItemModel> __tabItems = new ObservableCollection<NewTabItemModel>();
        public ref ObservableCollection<NewTabItemModel> _tabItems => ref __tabItems;
        private DataTable __filteredTable = new DataTable();
        public ref DataTable _filteredTable => ref __filteredTable;
        private DataTable __exportLabelFields = new DataTable();
        public ref DataTable _exportLabelFields => ref __exportLabelFields;
        private ObservableCollection<string> __historyCommandsList = new ObservableCollection<string>();
        public ref ObservableCollection<string> _historyCommandsList => ref __historyCommandsList;
        private ObservableCollection<string> __connectionList = new ObservableCollection<string>();
        public ref ObservableCollection<string> _connectionList => ref __connectionList;
        private DataTable __querytable = new DataTable();
        public ref DataTable Maintable => ref __querytable; 
        private DataGridCellInfo __tableCellisSelected;
        public ref DataGridCellInfo _tableCellisSelected => ref __tableCellisSelected;
        private string __searchWord;
        public ref string _searchWord => ref __searchWord;
        private ComboBoxItem __selectedFilterSearch;
        public ref ComboBoxItem _selectedFilterSearch => ref __selectedFilterSearch;
        private string __cellValueTextString;
        public ref string _cellValueTextString => ref __cellValueTextString;
        private string __newCellValueTextString;
        public ref string _newCellValueTextString => ref __newCellValueTextString;
        private string __createTableEXP;
        public ref string _createTableEXP => ref __createTableEXP;
        private string __selectedDatabaseText;
        public ref string _selectedDatabaseText => ref __selectedDatabaseText;
        private string __currentDBName;
        public ref string _currentDBName => ref __currentDBName;       
        private string __queryCommand;
        public ref string _queryCommand => ref __queryCommand;
        private int __progressFill;
        public ref int _progressFill => ref __progressFill;
        private SolidColorBrush __pBarColorBrush;
        public ref SolidColorBrush _pBarColorBrush => ref __pBarColorBrush;   
        private string __txtchange;
        public ref string _txtchange => ref __txtchange;
        private bool _askToReviewTableInsert;
        public ref bool AskToReviewTableInsert => ref _askToReviewTableInsert;  
        private bool __recordCancelProgressFill;
        public ref bool _recordCancelProgressFill => ref __recordCancelProgressFill;
        private int __recordCountingINT;
        public ref int _recordCountingINT => ref __recordCountingINT;
        private int __fetchRowCount;
        public ref int _fetchRowCount => ref __fetchRowCount;
        private string __selectedIndexOfComboBoxItem;
        public ref string _selectedIndexOfComboBoxItem => ref __selectedIndexOfComboBoxItem;
        private string __selectedMarkedText;
        public ref string _selectedMarkedText => ref __selectedMarkedText;
        private string __lastSelectQuerytxt;
        public ref string _lastSelectQuerytxt => ref __lastSelectQuerytxt;
        private int __xScriptnoteQueryIndex;
        public ref int _xScriptnoteQueryIndex => ref __xScriptnoteQueryIndex;
        private int __selectedScriptNoteIndex;
        public ref int _selectedScriptNoteIndex => ref __selectedScriptNoteIndex;
        private string __newScriptNoteName;
        public ref string _newScriptNoteName => ref __newScriptNoteName;      
        private string __currentScheme;
        public ref string _currentScheme => ref __currentScheme;
        private DataRowView __selectedRowItem;
        public ref DataRowView _selectedRowItem => ref __selectedRowItem;
    }
}
