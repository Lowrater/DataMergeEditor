using System.Collections.Generic;
using System.Data.SqlClient;
using DataMergeEditor.ViewModel;

namespace DataMergeEditor.DBConnect.Data.TreeData
{
    public interface IMssqlTree
    {
        IEnumerable<TreeNodeViewModel> BuildSchemaTrees();
        TreeNodeViewModel BuildTree(SqlConnection connection);
    }
}