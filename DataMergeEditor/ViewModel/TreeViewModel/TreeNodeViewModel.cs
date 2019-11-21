using System.Collections.ObjectModel;

namespace DataMergeEditor.ViewModel
{
    public class TreeNodeViewModel
    {
        public string Name { get; set; }
        public ObservableCollection<TreeNodeViewModel> Children { get; set; } = new ObservableCollection<TreeNodeViewModel>();
    }
}