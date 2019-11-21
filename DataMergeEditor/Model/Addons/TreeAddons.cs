using DataMergeEditor.DBConnect.Data.ListData;
using System.Collections.ObjectModel;

namespace DataMergeEditor.Model.Addons
{
    public class TreeAddons
    {
        public ObservableCollection<ContentList> ReturnFilteredTreeNode(string searchtext,
            ObservableCollection<ContentList> maintableContainer)
        {
            var FilteredTree = new ObservableCollection<ContentList>();
            //-- Root af alle knyttede forbindelser
            foreach (ContentList node in maintableContainer)
            {                
                if (node.Type.ToLower().Contains(searchtext.ToLower()))
                {
                    FilteredTree.Add(node);
                }             
            }
            return FilteredTree;
        }
    }
}
