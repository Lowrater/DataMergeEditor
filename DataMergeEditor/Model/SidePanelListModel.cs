using DataMergeEditor.DBConnect.Data.ListData;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DataMergeEditor.Model
{
    public class SidePanelListModel 
    {
        private ObservableCollection<string> __fileTableObvsList = new ObservableCollection<string>();
        public ref ObservableCollection<string> _fileTableObvsList => ref __fileTableObvsList;
        private ObservableCollection<ContentList> __mainListContainer = new ObservableCollection<ContentList>();
        public ref ObservableCollection<ContentList> _mainListContainer => ref __mainListContainer;
        private string __searchedfortableword;
        public ref string _searchedfortableword => ref __searchedfortableword;
        private ObservableCollection<ContentList> __itemsFilter = new ObservableCollection<ContentList>();
        public ref ObservableCollection<ContentList> _itemsFilter => ref __itemsFilter;
        private bool __checkIfSideListHasContentBool;
        public ref bool _checkIfSideListHasContentBool => ref __checkIfSideListHasContentBool;
        private int __isSelected;
        public ref int _isSelected => ref __isSelected;
        private ObservableCollection<TextBox> __textBoxCollection = new ObservableCollection<TextBox>();
        public ref ObservableCollection<TextBox> _textBoxCollection => ref __textBoxCollection;     
        private ObservableCollection<string> __connectionList = new ObservableCollection<string>();
        public ref ObservableCollection<string> _connectionList => ref __connectionList;
        private string __currentDB;
        public ref string _currentDB => ref __currentDB;
        private bool __activeConnection;
        public ref bool _activeConnection => ref __activeConnection;       
    }
}
