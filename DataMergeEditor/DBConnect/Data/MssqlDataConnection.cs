using DataMergeEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DataMergeEditor.DBConnect.Data
{
    public class MssqlDataConnection : DataConnection
    {
        private readonly SqlConnection connection;
        public MssqlDataConnection(SqlConnection connection)
        {
            this.connection = connection;
        }
        public override string Name => connection.DataSource;
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
            var cmd = new SqlCommand(command, connection);
            var SqlDataReader = cmd.ExecuteReader();

            //-- sætter kolonnerne
            for (int i = 0; i < SqlDataReader.FieldCount; i++)
            {
                switch (SqlDataReader.GetDataTypeName(i).ToString().ToLower())
                {
                    case "char":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "nchar":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (2000)");
                        break;
                    case "varchar":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (65535)");
                        break;
                    case "varchar2":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (4000)");
                        break;
                    case "nvarchar2":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (4000)");
                        break;
                    case "number":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (22)");
                        break;
                    case "float":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (22)");
                        break;
                    case "binary":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (1)");
                        break;
                    case "varbinary":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (1)");
                        break;
                    case "tinyblob":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "tinytext":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (255)");
                        break;
                    case "text":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (65535)");
                        break;
                    case "blob":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (65535)");
                        break;
                    case "mediumtext":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (16777215)");
                        break;
                    case "mediumblob":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (16777215)");
                        break;
                    case "longtext":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (4294967295)");
                        break;
                    case "longblob":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (4294967295)");
                        break;
                    case "bit":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)} (64)");
                        break;
                    case "tinyint":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    case "bool":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    case "smallint":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    case "int":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    case "integer":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    case "bigint":
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                    default:
                        ColumnsList.Add($"{SqlDataReader.GetName(i)} {SqlDataReader.GetDataTypeName(i)}");
                        break;
                }
            }

            string AllColumnItems = string.Join(", ", ColumnsList.ToArray());

            string columnreturn = $"Create table {TableName} (" + AllColumnItems + ")";

            return columnreturn;
        }



        /// <summary>
        /// Task<DataTable> - 
        /// Await = venter på at den er færdig samt skifter context.
        /// Kan køre i baggrunden uden at stoppe resterende ting i programmets flow.
        /// Hvis ting som tager sin tid, kan brugeren forsætte med andre ting uden at skulle begrænses. 
        /// Er den færdig, kan vi derefter benytte den.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async override Task<DataTable> Execute(string command, IProgress<int> progress, 
            CancellationToken cancellationToken, CancellationTokenSource source, int rowlimiter)
        {
            var cmd = new SqlCommand(command, connection);
            var RecapUpdatedTable = new DataTable();
            ///--------------------------------------------------
            var SqlDataReader = await cmd.ExecuteReaderAsync(cancellationToken);

            //-- Pop up beskeden med fremskridt på importering af alle rækker.
            int RecordCount = 0;
            await Task.Run(async () =>
            {
             //-- sætter kolonnerne
                  for (int i = 0; i < SqlDataReader.FieldCount; i++)
                    {
                        RecapUpdatedTable.Columns.Add(SqlDataReader.GetName(i));
                    }
                //-- Sætter rækkerne
                while ((await SqlDataReader.ReadAsync()) && !cancellationToken.IsCancellationRequested && RecordCount != rowlimiter)
                {
                    object[] o = new object[SqlDataReader.FieldCount];
                    for (int j = 0; j < SqlDataReader.FieldCount; j++)
                    {
                        o[j] = SqlDataReader[j].ToString();
                    }

                    RecapUpdatedTable.Rows.Add(o);
                    RecordCount++;
                    progress.Report(RecordCount);
                    LimitRecordFetching(source, RecordCount);
                }
             }, cancellationToken);
            SqlDataReader.Close();

            return RecapUpdatedTable;
        }

        /// <summary>
        /// Exportere via. bulkcopy til denne database
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="TableName"></param>
        public override void ExportToExternalDatabase(DataTable dataTable, string TableName, bool sameDB, string CurrentTable)
        {
            if (sameDB.Equals(true))
            {
                var cmd = new SqlCommand($"select * into {TableName} from {CurrentTable}", connection);
                cmd.ExecuteNonQuery();
                MessageBox.Show($"Success fully exportet {CurrentTable}'s rows to {connection.Database} in {TableName}", 
                    "DataMergeEditor - Export table message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var bulkCopy = new SqlBulkCopy(connection);

                foreach (DataColumn c in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(c.ColumnName, c.ColumnName);
                }

                bulkCopy.DestinationTableName = TableName;
                bulkCopy.BulkCopyTimeout = 600;
                bulkCopy.WriteToServer(dataTable);
            }  
        }

        

        /// <summary>
        /// Laver en select count(*) på tabellen, og returnere som int
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override int GetTableRowCount(string command)
        {
            string table = command.Substring(command.ToLower().IndexOf("from")).Split(' ')[1];
            //-- Kommando til at få antal af rækker fra databasen.
            SqlCommand cmdRowCount = new SqlCommand($"select count(*) from {table}", connection);
            //-- Til progress baren. Begrænser x antal rækker til visning
            return int.Parse(cmdRowCount.ExecuteScalar().ToString());
        }


        /// <summary>
        /// begrænsning for 5.000 records besked
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="RecordCount"></param>
        public override void LimitRecordFetching(CancellationTokenSource ct, int RecordCount)
        {
            //-- Dobbelt bekræfte at de ønsker X antal rækker end 5.000 for ikke at knække udtrækket.
            if (RecordCount == 5000)
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

        public override void AddToDatabase(string command, IProgress<int> progress)
        {
            SqlCommand cmd = new SqlCommand(command, connection);
            cmd.ExecuteNonQuery();
            progress.Report(100);
        }



        public override void DeleteFromDatabase(string command, IProgress<int> progress, bool askOnDel)
        {
            SqlCommand cmd = new SqlCommand(command, connection);

            if (askOnDel != false)
            {
                if (MessageBox.Show("You are about to delete some data. Are you sure?", 
                    "Data Merge Editor - deleting content warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    cmd.ExecuteNonQuery();
                    progress.Report(100);
                    MessageBox.Show("Table content has been successfully deleted", 
                     "Data Merge Editor - Drop table table message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Delete action has been successfully cancelled", 
                    "Data Merge Editor - deleting content table message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                cmd.ExecuteNonQuery();
                progress.Report(100);
                MessageBox.Show("Table content has been successfully deleted", 
               "Data Merge Editor - Drop table table message", MessageBoxButton.OK, MessageBoxImage.Information);
            }           
        }


        public override void Disconnect(string dbname)
        {
            //-- Tjekker om forbindelsen er aktiv og lukker den
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                MessageBox.Show($"Disconnected from {dbname}", $"Closing MsSql connection",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        public override void Reconnect(string dbname)
        {
            //-- Tjekker om forbindelsen er aktiv og lukker den
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                MessageBox.Show($"Re-connected to {dbname}", $"Re-connecting to Oracle connection",
                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public override void RemoveFromDatabase(string command, IProgress<int> progress, bool askOnDrop)
        {
            int Endpositionlenght = command.Length;
            int startpoistion = command.IndexOf("table");
            SqlCommand cmd = new SqlCommand(command, connection);
            if(askOnDrop != false)
            {
                //Messagebox - verify drop table
                if (MessageBox.Show("You are about to drop " + command.Substring(startpoistion, Endpositionlenght - startpoistion)
                    + ". Are you sure?", "Data Merge Editor - Dropping table warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    cmd.ExecuteNonQuery();
                    progress.Report(100);
                    MessageBox.Show("Table " + command.Substring(startpoistion, Endpositionlenght - startpoistion) 
                    + " has been successfully dropped", "Data Merge Editor - Drop table table message", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Dropping table action has been successfully cancelled", 
                    "Data Merge Editor - Drop table table message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                cmd.ExecuteNonQuery();
                progress.Report(100);
                MessageBox.Show("Table " + command.Substring(startpoistion, Endpositionlenght - startpoistion)
                 + " has been successfully dropped", "Data Merge Editor - Drop table table message", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        public override void SaveToDataBase(string command, IProgress<int> progress)
        {
            SqlCommand cmd = new SqlCommand(command, connection);
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
                SqlCommand cmd = new SqlCommand(command, connection);
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
                    + e.ToString().Substring(0, 250),
                    "Data Merge Editor - Unknown Read Create To Database",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        public override void ApplyChanges(DataTable dataTable, string command, bool askOnApply, IProgress<int> progress)
        {
            //--Finder tabel navn fra select xx from { tabelnavn}
            string tabelnavn = command.ToLower().Substring(command.IndexOf("FROM".ToLower())).Split(' ')[1];
            //////- Navngiver tabellen efter querien.
            dataTable.TableName = tabelnavn;
            ////////-- Gemmer ændringerne på tabellen
            dataTable.AcceptChanges();
            //////-- Genbruger kommandoen udført til at bruge samme tabel
            var myCmd = new SqlCommand(command, connection);
            var odbcj = new SqlDataAdapter(myCmd);
            var odbcbuilder = new SqlCommandBuilder(odbcj);
            var Datatable = new DataTable();
            odbcj.Fill(Datatable);
            int RowAffected = 0;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (string.Join(",", Datatable.Rows[i].ItemArray) != string.Join(",", dataTable.Rows[i].ItemArray))
                {
                    Datatable.Rows[i].ItemArray = dataTable.Rows[i].ItemArray;
                    RowAffected++;
                }
            }
            Datatable.AcceptChanges();
            if(askOnApply != false)
            {
                if (MessageBox.Show("Are you sure to apply changes to the database?", "Data Merge Editor - Apply changes confirm",
                   MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    odbcj.Update(dataTable);
                    progress.Report(100);
                    MessageBox.Show($"{RowAffected} rows was changed", "DataMergeEditor - Updating datatable message",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    progress.Report(100);
                    MessageBox.Show($"Chances cancelled", "DataMergeEditor - Updating datatable message",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                odbcj.Update(dataTable);
                progress.Report(100);
                MessageBox.Show($"{RowAffected} rows was changed", "DataMergeEditor - Updating datatable message",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }           
        }

        /// <summary>
        ///  Til at få listen af tabellerne som kontoen kan se
        /// </summary>
        public async override Task<DataTable> FetchTableList(CancellationToken cancellationToken, CancellationTokenSource source)
        {
            var cmd = new SqlCommand($"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_catalog='{connection.Database}'", connection);
            var RecapUpdatedTable = new DataTable();

            ///--------------------------------------------------
            var SqlDataReader = await cmd.ExecuteReaderAsync(cancellationToken);
            //-- Pop up beskeden med fremskridt på importering af alle rækker.

            await Task.Run(async () =>
            {
                //-- sætter kolonnerne
                for (int i = 0; i < SqlDataReader.FieldCount; i++)
                {
                    RecapUpdatedTable.Columns.Add(SqlDataReader.GetName(i));
                }
                //-- Sætter rækkerne
                while ((await SqlDataReader.ReadAsync()) && !cancellationToken.IsCancellationRequested)
                {
                    object[] o = new object[SqlDataReader.FieldCount];
                    for (int j = 0; j < SqlDataReader.FieldCount; j++)
                    {
                        o[j] = SqlDataReader[j].ToString();
                    }

                    RecapUpdatedTable.Rows.Add(o);
                }
            }, cancellationToken);
            SqlDataReader.Close();
            //-- returnere tabellens indhold
            return RecapUpdatedTable;
        }
    }
}
