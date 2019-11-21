using System.Collections.Generic;
using DataMergeEditor.ViewModel;
using MySql.Data.MySqlClient;

namespace DataMergeEditor.DBConnect.Data.TreeData
{
    public interface IMysqlTree
    {
        IEnumerable<TreeNodeViewModel> BuildSchemaTrees();
        TreeNodeViewModel BuildTree(MySqlConnection connection);
    }
}