using System.Collections.ObjectModel;

namespace DataMergeEditor.Model
{
    public class DataGridModel
    {
        private ObservableCollection<NewTabItemModel> __tabItems = new ObservableCollection<NewTabItemModel>();
        public ref ObservableCollection<NewTabItemModel> _tabItems => ref __tabItems;
        private int __xTabIndexesQuery;
        public ref int _xTabIndexesQuery => ref __xTabIndexesQuery;
        private int __xTabIndexesCompare;
        public ref int _xTabIndexesCompare => ref __xTabIndexesCompare;
        private int __selectedTabIndex;
        public ref int _selectedTabIndex => ref __selectedTabIndex;
        private string __newTabNoteName;
        public ref string _newTabNoteName => ref __newTabNoteName;
    }
}
