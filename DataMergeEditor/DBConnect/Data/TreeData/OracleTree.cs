using DataMergeEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Odbc;
using System.Linq;

namespace DataMergeEditor.DBConnect.Data.TreeData
{
    public class OracleTree : IOracleTree
    {
        //--------------------------------------------------------- TREE 
        //-- For fingrene i Dataservicen som kan fange enkelte metoder tværs henover viewmodels
        //-- Dataservice for at dele data mellem viewmodels
        private List<OdbcConnection> Connections = new List<OdbcConnection>();
        //-- Liste med tabeller der trækkes ud
        //-- returneres i BuildTree funktionen
        private IEnumerable<string> GetNameList(DataRowCollection drc, int index)
        {
            return drc.Cast<DataRow>().Select(r => r.ItemArray[index].ToString()).OrderBy(r => r).ToList();
        }

        //-- Bygger træet for den pågældne forbindelse
        public TreeNodeViewModel BuildTree(OdbcConnection connection)
        {
            //-- 1. Root node
            var rootNode = new TreeNodeViewModel
            {
                Name = connection.DataSource + " - Oracle",
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            //---  Tilføjer noderne
            //-- Er lavet unikt pga. arbejdsmarkedets standarder er anderledes end iflg. oracle.
            //- Dvs. Der er for mange ting, end det relevante til visning.
            try
            {
                rootNode.Children.Add(CustomNode(connection, "Triggers", "SELECT * FROM USER_TRIGGERS"));
                rootNode.Children.Add(CustomNode(connection, "Tables", "SELECT * FROM USER_tables"));
                rootNode.Children.Add(CustomNode(connection, "Indexes", "select * from user_indexes"));
                rootNode.Children.Add(CustomNode(connection, "Views", "select * from user_views"));
            }
            catch (Exception)
            {
                //-- Do nothing
            }

            //-- Tilføj rootNode i TreeNodeItemsContainer via DataBaseConnectionTreeViewModel    
            return rootNode;
        }

        //-- Tilføjer en node til træet alt efter parametre sendt
        public TreeNodeViewModel CustomNode(OdbcConnection connection, string MainNode, string command)
        {
            //-- 1. Schemaernes navn
            var MainDBNode = new TreeNodeViewModel { Name = MainNode };

            var odbcDataAdapter = new OdbcDataAdapter(command, connection);
            var table = new DataTable();
            odbcDataAdapter.Fill(table);

            IEnumerable<string> schemalist = GetNameList(table.Rows, 0);

            //-- 2. Schemaernes tabeller
            foreach (string tableName in schemalist)
            {
                MainDBNode.Children.Add(new TreeNodeViewModel { Name = tableName });
            }
            return MainDBNode;
        }


        //-- generator
        //-- lazy evaluation - 
        //-- 
        public IEnumerable<TreeNodeViewModel> BuildSchemaTrees()
        {
            yield return BuildTree(Connections.First());
        }
    }
}
