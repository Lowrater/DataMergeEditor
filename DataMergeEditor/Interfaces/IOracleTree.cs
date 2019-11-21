using System.Collections.Generic;
using System.Data.Odbc;
using DataMergeEditor.ViewModel;

namespace DataMergeEditor.DBConnect.Data.TreeData
{
    public interface IOracleTree
    {
        IEnumerable<TreeNodeViewModel> BuildSchemaTrees();
        TreeNodeViewModel BuildTree(OdbcConnection connection);
        TreeNodeViewModel CustomNode(OdbcConnection connection, string MainNode, string command);
    }
}

