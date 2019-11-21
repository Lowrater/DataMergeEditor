using DataMergeEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataMergeEditor.DBConnect.Data.TreeData
{
    public class MssqlTree : IMssqlTree
    {
        //-- For fingrene i Dataservicen som kan fange enkelte metoder tværs henover viewmodels
        //-- Dataservice for at dele data mellem viewmodels

        private List<SqlConnection> Connections = new List<SqlConnection>();

        //-- Liste med tabeller der trækkes ud
        //-- returneres i BuildTree funktionen
        private IEnumerable<string> GetNameList(DataRowCollection drc, int index)
        {
            return drc.Cast<DataRow>().Select(r => r.ItemArray[index].ToString()).OrderBy(r => r).ToList();
        }

        /// <summary>
        /// Bygger træet for den pågældne forbindelse
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>        
        public TreeNodeViewModel BuildTree(SqlConnection connection)
        {
            DataTable databasesSchemaTable = connection.GetSchema();
            IEnumerable<string> databases = GetNameList(databasesSchemaTable.Rows, 0);

            var rootNode = new TreeNodeViewModel
            {
                Name = connection.Database + " - MsSQL",
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            //--1 schemaernes navn
            foreach (string schemaname in databases)
            {
                //-- 1. Root schema navn
                var dbNode = new TreeNodeViewModel { Name = schemaname };
                rootNode.Children.Add(dbNode);
                //-- schemaernes tabeller. Mystik omkring DataSourceInformation hvad det specifikt er.
                //-- Switchen er forskellig grundet række valg er unikt pr. schema tabel. Så det skal listes korrekt.
                try
                {
                    switch (schemaname)
                    {
                        //-- Viser korrekt indhold for nr.2                       
                        case "Tables":
                            DataTable table = connection.GetSchema(schemaname);
                            IEnumerable<string> schemalist = GetNameList(table.Rows, 2);

                            //-- 2 schemaernes tabeller
                            foreach (string tableName in schemalist)
                            {
                                dbNode.Children.Add(new TreeNodeViewModel { Name = tableName });
                            }
                            break;
                        //-- viser korrekt indhold for nr.1
                        case "Users":
                        case "Views":                   
                            DataTable table2 = connection.GetSchema(schemaname);
                            IEnumerable<string> schemalist2 = GetNameList(table2.Rows, 1);

                            //-- 2 schemaernes tabeller
                            foreach (string tableName in schemalist2)
                            {
                                dbNode.Children.Add(new TreeNodeViewModel { Name = tableName });
                            }
                            break;
                        //-- viser korrekt indhold for nr.0
                        case "Databases":
                        case "DataTypes":                 
                        case "ReservedWords":
                            DataTable table3 = connection.GetSchema(schemaname);
                            IEnumerable<string> schemalist3 = GetNameList(table3.Rows, 0);

                            //-- 2 schemaernes tabeller
                            foreach (string tableName in schemalist3)
                            {
                                dbNode.Children.Add(new TreeNodeViewModel { Name = tableName });
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    //-- Do nothing
                }
            }
            //-- Tilføj rootNode i TreeNodeItemsContainer via DataBaseConnectionTreeViewModel    
            return rootNode;
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
