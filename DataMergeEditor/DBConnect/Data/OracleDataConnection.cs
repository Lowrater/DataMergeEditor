using System;
using DataMergeEditor.Interfaces;
using System.Data;
using System.Data.Odbc;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace DataMergeEditor.DBConnect.Data
{
    public class OracleDataConnection : DataConnection
    {
        private readonly OdbcConnection connection;

        public OracleDataConnection(OdbcConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// navnet på databasen
        /// </summary>
        public override string Name => connection.Database;
        public override object Type => connection.GetType();


        /// <summary>
        /// Til at få en korrekt CreateTableKommando ud fra databasens korrekte DataTyper
        /// </summary>
        /// <param name="command"></param>
        /// <param name="TableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override string CreateTableCommand(string command, string TableName)
        {
            List<string> ColumnsList = new List<string>();
            var cmd = new OdbcCommand(command, connection);
            var OdbcDataReader =  cmd.ExecuteReader();

            //-- sætter kolonnerne
            for (int i = 0; i < OdbcDataReader.FieldCount; i++)
            {
                switch (OdbcDataReader.GetDataTypeName(i).ToString().ToLower())
                {
                    case "char":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "nchar":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (2000)");
                        break;                       
                    case "varchar":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (65535)");
                        break;
                    case "varchar2":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (4000)");
                        break;
                    case "nvarchar2":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (4000)");
                        break;
                    case "number":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (22)");
                        break;
                    case "float":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (22)");
                        break;
                    case "binary":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (1)");
                        break;
                    case "varbinary":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (1)");
                        break;
                    case "tinyblob":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "tinytext":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "text":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (65535)");                     
                        break;
                    case "blob":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (65535)");
                        break;
                    case "mediumtext":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (16777215)");
                        break;
                    case "mediumblob":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (16777215)");
                        break;
                    case "longtext":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (4294967295)");
                        break;
                    case "longblob":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (4294967295)");
                        break;
                    case "bit":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)} (64)");
                        break;
                    case "tinyint":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                    case "bool":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                    case "smallint":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                    case "int":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                    case "integer":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                    case "bigint":
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;                       
                    default:
                        ColumnsList.Add($"{OdbcDataReader.GetName(i)} {OdbcDataReader.GetDataTypeName(i)}");
                        break;
                }
            }

            string AllColumnItems = string.Join(", ", ColumnsList.ToArray());
            string columnreturn = $"Create table {TableName} (" + AllColumnItems + ")";
            return columnreturn;
        }

        /// <summary>
        /// Exportere via. bulkcopy til extern database
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="TableName"></param>
        public override void ExportToExternalDatabase(DataTable dataTable, string TableName, bool sameDB, string CurrentTable)
        {
            if(sameDB.Equals(true))
            {
                var cmd = new OdbcCommand($"create table {TableName} as select * from {CurrentTable}", connection);
                cmd.ExecuteNonQuery();
                MessageBox.Show($"Success fully created {TableName} to {connection.Database}", 
                    "DataMergeEditor - Export table message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var columnList = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    columnList.Add(column.ColumnName);
                }
                string AllColumnsString = string.Join(",", columnList.ToArray());

                //-- oracle format
                var dateformat = "dd/mm/yyy hh:mi:ss a.m.";
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    DateTime datetime;
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        if(DateTime.TryParse(dataTable.Rows[row][col].ToString(), out datetime))
                        {
                            dataTable.Rows[row][col] = $"TO_DATE('{dataTable.Rows[row][col].ToString()}', '{dateformat}')";
                        }
                    }

                    var cmd = new OdbcCommand($"insert into {TableName} ({AllColumnsString}) VALUES " +
                        $"('{string.Join("', '", dataTable.Rows[row].ItemArray)}')", connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///  Await = venter på at den er færdig samt skifter context.
        /// Kan køre i baggrunden uden at stoppe resterende ting i programmets flow.
        ///Hvis ting som tager sin tid, kan brugeren forsætte med andre ting uden at skulle begrænses. 
        /// Er den færdig, kan vi derefter benytte den.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <param name="rowlimiter"></param>
        /// <returns></returns>
        public async override Task<DataTable> Execute(string command, IProgress<int> progress, 
            CancellationToken cancellationToken, CancellationTokenSource source, int rowlimiter)
        {
            var cmd = new OdbcCommand(command, connection);
            var RecapUpdatedTable = new DataTable();
            var OdbcDataReader = await cmd.ExecuteReaderAsync(cancellationToken);
            //-- Pop up beskeden med fremskridt på importering af alle rækker.
            var RecordCount = 0;          

            await Task.Run(async () =>
            {
                //-- sætter kolonnerne
                for (int i = 0; i < OdbcDataReader.FieldCount; i++)
                {
                    RecapUpdatedTable.Columns.Add(OdbcDataReader.GetName(i));
                }

                //-- Sætter rækkerne
                while ((await OdbcDataReader.ReadAsync()) && !cancellationToken.IsCancellationRequested && RecordCount != rowlimiter)
                {
                    object[] o = new object[OdbcDataReader.FieldCount];
                    for (int j = 0; j < OdbcDataReader.FieldCount; j++)
                    {
                        o[j] = OdbcDataReader[j].ToString();
                    }

                    RecapUpdatedTable.Rows.Add(o);
                    RecordCount++;
                    progress.Report(RecordCount);
                    LimitRecordFetching(source, RecordCount);
                }
            }, cancellationToken);
            OdbcDataReader.Close();

            return RecapUpdatedTable;
        }

     

        //.. Await = venter på at den er færdig samt skifter context.
        //-- Kan køre i baggrunden uden at stoppe resterende ting i programmets flow.
        //-- Hvis ting som tager sin tid, kan brugeren forsætte med andre ting uden at skulle begrænses. 
        //-- Er den færdig, kan vi derefter benytte den.
        public async override Task<DataTable> FetchTableList(CancellationToken cancellationToken, CancellationTokenSource source)
        {
            var cmd = new OdbcCommand("SELECT * FROM user_tables", connection);
            var RecapUpdatedTable = new DataTable();

            ///--------------------------------------------------
            var OdbcDataReader = await cmd.ExecuteReaderAsync(cancellationToken);
            //-- Pop up beskeden med fremskridt på importering af alle rækker.

            await Task.Run(async () =>
            {
                //-- sætter kolonnerne
                for (int i = 0; i < OdbcDataReader.FieldCount; i++)
                {
                       RecapUpdatedTable.Columns.Add(OdbcDataReader.GetName(i));
                }

                //-- Sætter rækkerne
                while ((await OdbcDataReader.ReadAsync()) && !cancellationToken.IsCancellationRequested)
                {
                    object[] o = new object[OdbcDataReader.FieldCount];
                    for (int j = 0; j < OdbcDataReader.FieldCount; j++)
                    {
                        o[j] = OdbcDataReader[j].ToString();
                    }

                    RecapUpdatedTable.Rows.Add(o);             
                }
            }, cancellationToken);
            OdbcDataReader.Close();

            return RecapUpdatedTable;
        }



        /// <summary>
        /// begrænsning for 5.000 records besked
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="RecordCount"></param>
        public  override void LimitRecordFetching(CancellationTokenSource ct, int RecordCount)
        {
            //-- Dobbelt bekræfte at de ønsker X antal rækker end 5.000 for ikke at knække udtrækket.
            if (RecordCount ==  5000)
            {
                if (MessageBox.Show($"You have fetched more than 5.000 rows. Are you sure to continue?", 
                    "Data Merge Editor - Fetching records warning",
                     MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    //-- Do nothing
                }
                else
                {
                    ct.Cancel();
                }
            }
        }

        /// <summary>
        /// Sammenligner databasens indhold med den redigerede tabel,
        /// og opdatere databasen automatisk ud fra den redigerede tabel.
        /// Stack overflow: https://stackoverflow.com/questions/57321075/c-sharp-wpf-mvvm-using-existing-datatable-to-dataset-while-updating-database 
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/76201ea5-2c94-4d86-b138-645178b59113/update-database-table-with-datatable-in-c?forum=csharpgeneral 
        /// TABELLER SKAL HAVE EN PRIMARY KEY, ellers virker det ikke.
        /// </summary>
        public override void ApplyChanges(DataTable dataTable, string command, bool AskOnapply, IProgress<int> progress)
        {   //--Finder tabel navn fra select xx from { tabelnavn}
            string tabelnavn = command.ToLower().Substring(command.IndexOf("FROM".ToLower())).Split(' ')[1];
            //////- Navngiver tabellen efter querien.
            dataTable.TableName = tabelnavn;
            ////////-- Gemmer ændringerne på tabellen
            dataTable.AcceptChanges();
            //////-- Genbruger kommandoen udført til at bruge samme tabel
            var myCmd = new OdbcCommand(command, connection);
            var odbcj = new OdbcDataAdapter(myCmd);
            var odbcbuilder = new OdbcCommandBuilder(odbcj);
            var OriDataset = new DataSet();
            odbcj.Fill(OriDataset, tabelnavn);
            int RowAffected = 0;

            ////-- old NOT WORKING
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                if (string.Join(",", OriDataset.Tables[0].Rows[row].ItemArray) != string.Join(",", dataTable.Rows[row].ItemArray))
                {
                    OriDataset.Tables[0].Rows[row].ItemArray = dataTable.Rows[row].ItemArray;
                    RowAffected++;
                }
            }

            if (AskOnapply != false)
            {
                if (MessageBox.Show("Are you sure to apply changes to the database?", "Data Merge Editor - Apply changes confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    odbcj.Update(OriDataset, tabelnavn);
                    progress.Report(100);
                    MessageBox.Show($"{RowAffected} rows was changed", "DataMergeEditor - Updating datatable message",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    progress.Report(100);
                    MessageBox.Show($"Changes cancelled", "DataMergeEditor - Updating datatable message", 
                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                odbcj.Update(OriDataset, tabelnavn);
                progress.Report(100);
                MessageBox.Show($"{RowAffected} rows was changed", "DataMergeEditor - Updating datatable message", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            }         
        }


        /// <summary>
        /// Laver en select count(*) på tabellen, og returnere som int
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public  override int GetTableRowCount(string command)
        {
            string table = command.Substring(command.ToLower().IndexOf("from")).Split(' ')[1];
            //-- Kommando til at få antal af rækker fra databasen.
            var cmdRowCount = new OdbcCommand($"select count(*) from {table}", connection);
            //-- Til progress baren. Begrænser x antal rækker til visning
            return int.Parse(cmdRowCount.ExecuteScalar().ToString());
        }

        /// <summary>
        /// Executer create statement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public override void AddToDatabase(string command, IProgress<int> progress)
        {
            var cmd = new OdbcCommand(command, connection);
            cmd.ExecuteNonQuery();
            progress.Report(100);
        }

        /// <summary>
        ///   Executer delete statement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public override void DeleteFromDatabase(string command, IProgress<int> progress, bool askBeforeDel)
        {
            if(askBeforeDel != false)
            {
                if (MessageBox.Show("You are about to delete some data. Are you sure?", 
                    "Data Merge Editor - deleting content warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var cmd = new OdbcCommand(command, connection);
                    cmd.ExecuteNonQuery();
                    progress.Report(100);
                    MessageBox.Show("Table content has been successfully deleted", "Data Merge Editor - Drop table table message", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Delete action has been successfully cancelled", "Data Merge Editor - deleting content table message", 
                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                var cmd = new OdbcCommand(command, connection);
                cmd.ExecuteNonQuery();
                progress.Report(100);
                MessageBox.Show("Table content has been successfully deleted", "Data Merge Editor - Drop table table message", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        /// <summary>
        /// Disconnecter for pågældne forbindelse
        /// </summary>
        /// <param name="progress"></param>
        public override void Disconnect(string DBName)
        {
             //-- Tjekker om forbindelsen er aktiv og lukker den
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                MessageBox.Show($"Disconnected from {DBName}", $"Disconnecting from Oracle connection",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Gendanner forbindelse 
        /// </summary>
        /// <param name="progress"></param>
        public override void Reconnect(string DBName)
        {
            //-- Tjekker om forbindelsen er aktiv og lukker den
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                MessageBox.Show($"Re-connected to {DBName}", $"Re-connecting to Oracle connection",
                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// Laver en drop statement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public override void RemoveFromDatabase(string command, IProgress<int> progress, bool askBeforeDel)
        {
            int Endpositionlenght = command.Length;
            int startpoistion = command.IndexOf("table");

            if(askBeforeDel != false)
            {
                    // Messagebox - verify drop table
                if (MessageBox.Show("You are about to drop " + command.Substring(startpoistion, Endpositionlenght - startpoistion) 
                    + ". Are you sure?", "Data Merge Editor - Dropping table warning",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        OdbcCommand cmd = new OdbcCommand(command, connection);
                        cmd.ExecuteNonQuery();
                        progress.Report(100);
                        MessageBox.Show("Table " + command.Substring(startpoistion, Endpositionlenght - startpoistion) 
                            + " has been successfully dropped", "Data Merge Editor - Drop table table message", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Dropping table action has been successfully cancelled", 
                            "Data Merge Editor - Drop table table message", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
            }
            else
            {
                OdbcCommand cmd = new OdbcCommand(command, connection);
                cmd.ExecuteNonQuery();
                progress.Report(100);
                MessageBox.Show("Dropping table action has been successfully cancelled", 
                    "Data Merge Editor - Drop table table message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// laver en insert statement
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public override void SaveToDataBase(string command, IProgress<int> progress)
        {
            OdbcCommand cmd = new OdbcCommand(command, connection);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                progress.Report(100);
        }

        /// <summary>
        /// Udføre alle tilfældige kommandoer. Eks, triggers, views, scripts mm
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public override void UnknownReadCreateToDatabaes(string command)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand(command, connection);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Successfully Executed", "Data Merge Editor - Unknown Read Create To Database message", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to execute"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "The error sounded like:"
                    + e.ToString().Substring(0,250),
                    "Data Merge Editor - Unknown Read Create To Database",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
