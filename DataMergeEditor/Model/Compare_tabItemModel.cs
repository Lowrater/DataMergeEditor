using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Media;

namespace DataMergeEditor.Model
{
    public class Compare_tabItemModel
    {
        //------------------------------------- Global --------------------------------
        private int __recordProgressFillGlobal;
        public ref int _recordProgressFillGlobal => ref __recordProgressFillGlobal;
        private int __fetchRowCountGlobal;
        public ref int _fetchRowCountGlobal => ref __fetchRowCountGlobal;
        //----------------------------------- LEFT GRID --------------------------------
        private string __currentDBNameLeft;
        public ref string _currentDBNameLeft => ref __currentDBNameLeft; 
        private string __leftdelimitertxt;
        public ref string _leftdelimitertxt => ref __leftdelimitertxt;
        private string __queryCommandLeft;
        public ref string _queryCommandLeft => ref __queryCommandLeft;
        private DataTable __leftDataTable;
        public ref DataTable _leftDataTable => ref __leftDataTable;
        private DataTable __dummyTableLeft;
        public ref DataTable _dummyTableLeft => ref __dummyTableLeft;
        private int __xLeftColumnsCount;
        public ref int _xLeftColumnsCount => ref __xLeftColumnsCount;
        private int __xLeftRowsCount;
        public ref int _xLeftRowsCount => ref __xLeftRowsCount;
        private string __selectedIndexOfComboBoxItemLeft;
        public ref string _selectedIndexOfComboBoxItemLeft => ref __selectedIndexOfComboBoxItemLeft;
        private ObservableCollection<string> __historyCommandsListLeft = new ObservableCollection<string>();
        public ref ObservableCollection<string> HistoryCommandsListLeft => ref __historyCommandsListLeft;
        private string __colorDif;
        public ref string _colorDif => ref __colorDif;
        private string __checkCorrectDataBool;
        public ref string _checkCorrectDataBool => ref __checkCorrectDataBool;
        //---------------------------------- RIGHT GRID --------------------------------
        private string __currentDBNameRight;
        public ref string _currentDBNameRight => ref __currentDBNameRight;
        private string __rightdelimitertxt;
        public ref string _rightdelimitertxt => ref __rightdelimitertxt;
        private string __queryCommandRight;
        public ref string _queryCommandRight => ref __queryCommandRight;
        private DataTable __rightDataTable;
        public ref DataTable _rightDataTable => ref __rightDataTable;
        private DataTable __dummyTableRight;
        public ref DataTable _dummyTableRight => ref __dummyTableRight;
        private int __xRightColumnsCount;
        public ref int _xRightColumnsCount => ref __xRightColumnsCount;
        private int __xRightRowsCount;
        public ref int _xRightRowsCount => ref __xRightRowsCount;
        private string __selectedIndexOfComboBoxItemRight;
        public ref string _selectedIndexOfComboBoxItemRight => ref __selectedIndexOfComboBoxItemRight;
        private ObservableCollection<string>  __historyCommandsListRight = new ObservableCollection<string>();
        public ref ObservableCollection<string> _historyCommandsListRight => ref __historyCommandsListRight;
        //--------------------------------- MID GRID -------------------------------------
        //--------------------------------- for progress bar ----------------------------
        private int __progressFill;
        public ref int _progressFill => ref __progressFill;
        private SolidColorBrush __pBarColorBrush;
        public ref SolidColorBrush _pBarColorBrush => ref __pBarColorBrush;
        private string __txtchange;
        public ref string _txtchange => ref __txtchange;
        //-------------------------------- valg af grid row --------------------------------
        private string __selectionValue;
        public ref string _selectionValue => ref __selectionValue;
        private int __tableColumnIndexisSelected;
        public ref int _tableColumnIndexisSelected => ref __tableColumnIndexisSelected;
    }
}
