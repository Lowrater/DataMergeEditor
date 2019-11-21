using DataMergeEditor.DBConnect.Data.ListData;
using System.Collections.ObjectModel;

namespace DataMergeEditor.Model
{
    public class GetTablesFromDBModel
    {
        private ObservableCollection<ContentList> __mainTableContainer;
        public ref ObservableCollection<ContentList> _mainTableContainer => ref __mainTableContainer;
        private ContentList __tableisSelected;
        public ref ContentList _tableisSelected => ref __tableisSelected;
        private ContentList __tableItemSelected;
        public ref ContentList _tableItemSelected => ref __tableItemSelected;
    }
}
