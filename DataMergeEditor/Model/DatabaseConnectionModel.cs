using DataMergeEditor.ViewModel;
using System.Collections.ObjectModel;

namespace DataMergeEditor.Model
{
    public class DatabaseConnectionModel
    {
        private string __searchedfortableword;
        public ref string _searchedfortableword => ref __searchedfortableword;
        private string __connectionSelected;
        public ref string _connectionSelected => ref __connectionSelected;
        private ObservableCollection<TreeNodeViewModel> _items = new ObservableCollection<TreeNodeViewModel>();
        public ref ObservableCollection<TreeNodeViewModel> Items => ref _items;
        private ObservableCollection<TreeNodeViewModel> __itemsFilter = new ObservableCollection<TreeNodeViewModel>();
        public ref ObservableCollection<TreeNodeViewModel> _itemsFilter => ref __itemsFilter;       
        private ObservableCollection<TreeNodeViewModel> __treeNodeItemsContainer = new ObservableCollection<TreeNodeViewModel>();
        public ref ObservableCollection<TreeNodeViewModel> _treeNodeItemsContainer => ref __treeNodeItemsContainer;
        private TreeNodeViewModel __choosenmenuitem;
        public ref TreeNodeViewModel _choosenmenuitem => ref __choosenmenuitem;
    }
}
