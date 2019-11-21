using System;
using System.Collections.Generic;
using System.Data;

namespace DataMergeEditor.Model
{
    public class CommandsModel
    {
        //-- Mod SQL
        public string selectString = "select * from ????;";
        public string CreateUpdateString = "UPDATE table_name\nSET column1 = value1,\ncolumn2 = value2,\n ...\nWHERE condition;";
        public string CreateCaseString = "CASE\nWHEN condition1 THEN result1\nWHEN condition2 THEN result2\nWHEN conditionN THEN resultN\nELSE result\nEND;";
        public string CreateIndexString = "CREATE INDEX index_name\nON table_name(column1, column2, ...); ";
        public string CreateDatabaseString = "CREATE DATABASE ????;";
        public string CreateViewString = "CREATE VIEW view_name\nAS SELECT ????\nFROM ????;";
        public string CreateDataBaseBackupString = "BACKUP DATABASE ????\nTO DISK = 'filepath';";
        public string DropTableString = "Drop table ????;";
        public string AltertableString = "Alter table ????";

        //-- Mod maingrid tabellen
        public string TableInsertColumnString = "Insert ???? select ???? from ????;";
        public string CreateColumnInsertIntoString = "Insert into ???? as select ???? from ????;";

        /// <summary>
        /// returnere string af create statement med alle kolonner i angivet tabel
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public string setCreateItemCommand(DataTable table)
        {
            //-- COLUMNS with varchar
            var ColumnsList = new List<string>();
            //-- without varchar
            var ColumnsList2 = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                ColumnsList.Add(column.ToString() + " varchar(255)");
                ColumnsList2.Add(column.ToString());
            }

            string AllColumnItems = string.Join(", ", ColumnsList.ToArray());
            string AllColumnItems2 = string.Join(", ", ColumnsList2.ToArray());

            //-- ROWS
            List<string> RowList = new List<string>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                RowList.Add($"Insert into ???? ({AllColumnItems2}) VALUES ('{string.Join("', '", table.Rows[i].ItemArray)}')");
            }

            string AllRowItems = string.Join(Environment.NewLine, RowList.ToArray());
            return "Create table ???? (" + AllColumnItems + ")" + Environment.NewLine + AllRowItems;
        }

        /// <summary>
        /// Laver insert statements 
        /// </summary>
        /// <param name="datatable"></param>
        /// <param name="tablenavn"></param>
        /// <returns></returns>
        public string CreateInsertString(DataTable datatable, string tablenavn)
        {           
            //-- without varchar
            var ColumnsList = new List<string>();
            foreach (DataColumn column in datatable.Columns)
            {
                ColumnsList.Add(column.ToString());
            }

            string AllColumnItems = string.Join(", ", ColumnsList.ToArray());

           return $"Insert into ???? ({AllColumnItems}) "+ Environment.NewLine +" VALUES ('')";                
        }


        /// <summary>
        /// Laver en create table string ud fra tabellens indhold
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public string CreateTableString(DataTable dataTable, string tabelnavn)
        {
            //-- COLUMNS with varchar
            var ColumnsList = new List<string>();

            foreach (DataColumn column in dataTable.Columns)
            {
                ColumnsList.Add(column.ToString() + " varchar(255)");
            }
            string AllColumnItems = string.Join(", ", ColumnsList.ToArray());

            return $"Create table {tabelnavn} (" + AllColumnItems + ")";
        }
    }
}
