using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DataMergeEditor.Model
{
    public static class TableAddons /*: ViewModelBase*/
    {
        /// <summary>
        /// Merge funktionen af tabeller til en ny på korrekt index
        /// </summary>
        /// <param name="Table1"></param>
        /// <param name="Table2"></param>
        /// <returns></returns>
        public static DataTable MergeTables(DataTable Table1, DataTable Table2)
        {
            int DuplicatedNames = 0;
            DataTable Mergetable = new DataTable();

            foreach (DataColumn d in Table1.Columns)
            {
                Mergetable.Columns.Add(d.ColumnName);
            }

            foreach (DataColumn d in Table2.Columns)
            {
                if (!Mergetable.Columns.Contains(d.ColumnName))
                {
                    Mergetable.Columns.Add(d.ColumnName);
                }
                else
                {
                    Mergetable.Columns.Add(d.ColumnName + DuplicatedNames.ToString());
                    DuplicatedNames++;
                }              
            }

            int Table1Cols = Table1.Columns.Count;
            int Table1Rows = Table1.Rows.Count;
            int Table2Cols = Table2.Columns.Count;
            int Table2Rows = Table2.Rows.Count;

            DataRow row2;
            bool end = false;
            int RowCount = 0;

            while (!end)
            {
                end = true;
                if (RowCount < Table1Rows || RowCount < Table2Rows)
                {
                    end = false;
                    row2 = Mergetable.NewRow();

                    if (RowCount < Table1Rows)
                    {
                        for (int col = 0; col < Table1Cols; col++)
                        {
                            row2[col] = Table1.Rows[RowCount][col];
                        }
                    }

                    if (RowCount < Table2Rows)
                    {
                        for (int col = 0; col < Table2Cols; col++)
                        {
                            row2[col + Table1Cols] = Table2.Rows[RowCount][col];
                        }
                    }
                    Mergetable.Rows.Add(row2);
                }
                RowCount++;
            }
            return Mergetable;
        }


        /// <summary>
        /// tilføje row counter
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataTable AddRowCounter(DataTable dataTable)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Cannot add counter, when table data is not fetched", 
                    "Data Merge Editor - Adding counter message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //-- Hvis der ikke findes en tabel med counter, skal den tilføjes og merges til.
                DataTable MainTable2 = dataTable.Copy();
                DataTable Counttable = new DataTable();
                if (!MainTable2.Columns.Contains("Counter"))
                {
                    int i = 1;
                    Counttable.Columns.Add("Counter");
                    foreach (DataRow row in dataTable.Rows)
                    {
                        Counttable.Rows.Add(i.ToString());
                        i++;
                    }
                    dataTable = MergeTables(MainTable2, Counttable);                  
                }
                else
                {
                    string ValgteKolonne = "Counter";
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataTable.Rows[i][ValgteKolonne] = i.ToString();
                    }
                    MessageBox.Show("Current scheme already contains a row counter." 
                        + Environment.NewLine 
                        + Environment.NewLine
                        + "Refreshing current row counter.", 
                        "Data Merge Editor - Row counter message", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);                   
                }
            }
            return dataTable;
        }

        /// <summary>
        /// fjerner kolonne headeren
        /// </summary>
        /// <param name="MaindataTable"></param>
        /// <param name="TableCoumnIndexisSelected"></param>
        /// <returns></returns>
        public static DataTable DeleteColumnHeader(DataTable MaindataTable, DataGridCellInfo TableCoumnIndexisSelected)
        {
            DataTable dataTable = MaindataTable.Copy();
            if (MaindataTable != null)
            {         
                try
                {
                    if (dataTable.Columns.Contains(dataTable.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName)
                        && TableCoumnIndexisSelected.Column.DisplayIndex >= 0)
                    {
                        dataTable.Columns.RemoveAt(TableCoumnIndexisSelected.Column.DisplayIndex);
                    }
                }
                catch (Exception)
                {

                    MessageBox.Show("Column index is invalid. Please point and select in the row field",
                        "DataMergeEditor - Column deletion message", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Cannot delete when grid is empty.", 
                    "DataMergeEditor - Column deletion message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            return dataTable;
        }

        /// <summary>
        /// omdøber kolonneheaderen
        /// </summary>
        /// <param name="MaindataTable"></param>
        /// <param name="NewColumnNameHeader"></param>
        /// <param name="TableCoumnIndexisSelected"></param>
        /// <returns></returns>
        public static DataTable RenameColumnHeader(DataTable MaindataTable, string NewColumnNameHeader,
            DataGridCellInfo TableCoumnIndexisSelected)
        {        
            DataTable dataTable = MaindataTable.Copy();
            if (MaindataTable != null)
            {
                try
                {
                    if (NewColumnNameHeader != null && NewColumnNameHeader != "" && !dataTable.Columns.Contains(NewColumnNameHeader)
                        && TableCoumnIndexisSelected.Column.DisplayIndex >= 0)
                    {
                        dataTable.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName = NewColumnNameHeader;
                    }
                    else
                    {
                        MessageBox.Show("Column name cannot be empty or be duplicates.",
                            "Data Merge Editor - Column renaming message", 
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Column index is invalid. Please point and select in the row field", 
                        "Data Merge Editor - Column deletion message",
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Cannot rename when grid is empty.", 
                    "Data Merge Editor - Column renaming message",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            return dataTable;
        }


        /// <summary>
        /// Tilføjer ny kolonne
        /// </summary>
        /// <param name="MainDatatable"></param>
        /// <param name="AddNewColumnHeaderName"></param>
        /// <returns></returns>
        public static DataTable AddNewColumn(DataTable MainDatatable, string AddNewColumnHeaderName)
        {       
            DataTable MainTable2 = new DataTable();
            if (MainDatatable == null && !string.IsNullOrEmpty(AddNewColumnHeaderName))
            {               
                    MainTable2.Columns.Add(AddNewColumnHeaderName);
            }
            else
            {
                MainTable2 = MainDatatable.Copy();
                if (!string.IsNullOrEmpty(AddNewColumnHeaderName))
                {
                    if (!MainTable2.Columns.Contains(AddNewColumnHeaderName))
                    {
                        MainTable2.Columns.Add(AddNewColumnHeaderName);                    
                    }
                    else
                    {
                        MessageBox.Show("Column name already exist", "DataMergeEditor - Error new column",
                                            MessageBoxButton.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Column name cannot be empty", "DataMergeEditor - Error new column",
                                    MessageBoxButton.OK);
                }              
            }
            return MainTable2;
        }

        /// <summary>
        /// Clear valgte kolonne data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="TableCoumnIndexisSelected"></param>
        /// <returns></returns>
        public static DataTable clearSelectedColumnRowData(DataTable dataTable, DataGridCellInfo TableCoumnIndexisSelected)
        {
            DataTable MainTable = dataTable.Copy();
            if (MainTable == null)
            {
                MessageBox.Show("Cannot delete row(s) when table data is not fetched", 
                    "DataMergeEditor - Clear rows message", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                try
                {
                    string valgtekolonnenavn = MainTable.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName;

                    for (int i = 0; i < MainTable.Rows.Count; i++)
                    {
                        MainTable.Rows[i][valgtekolonnenavn] = null;
                    }
                    
                }
                catch (Exception)
                {

                    MessageBox.Show("Cannot clear selected column rows." 
                        + Environment.NewLine
                        + Environment.NewLine 
                        + " Please mark and select in the row grid.",
                        "DataMergeEditor - Clear rows message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            return MainTable;
        }


        /// <summary>
        /// Find et ord mellem to ord
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start).Trim();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Funktion som returnere Celle text som string
        /// </summary>
        /// <param name="textcellText"></param>
        /// <returns></returns>
        public static string setCellValueText(DataGridCellInfo textcellText)
        {
            string CellText;
            if(textcellText.Column != null)
            {
                var CellBlock = textcellText.Column.GetCellContent(textcellText.Item);
                if ((CellBlock as TextBlock) != null && (CellBlock as TextBlock).Text != "")
                {
                    CellText = (CellBlock as TextBlock).Text;
                }
                else
                {
                    CellText = "";
                }
            }
            else
            {
                CellText = "";
            }
            
            return CellText;
        }


        /// <summary>
        /// returnere en simpel tabel. - fil navn, delimiter
        /// </summary>
        /// <param name="file"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DataTable setTable(string file, string delimiter)
        {
            DataTable FileTable = new DataTable();

            //-- Input fra listenmedpaths
            string[] Lines = File.ReadAllLines(file);
            string[] Cells;
            Cells = Lines[0].Split(new char[] { Convert.ToChar(delimiter) });
            int Cols = Cells.GetLength(0);

            //-- 1st row skal være kolonne navn i filen
            //-- Tilføjer kolonnerne
            for (int X = 0; X < Cols; X++)
            {
                FileTable.Columns.Add(Cells[X].ToLower(), typeof(string));
            }

            //-- Laver rækkerne
            DataRow Row;
            for (int T = 1; T < Lines.GetLength(0); T++)
            {
                //-- Tjekker om array'eds position er tomt.
                if(!string.IsNullOrWhiteSpace(Lines[T]))
                {
                    Cells = Lines[T].Split(new char[] { Convert.ToChar(delimiter) });
                    Row = FileTable.NewRow();
                    for (int f = 0; f < Cols; f++)
                    {
                        Row[f] = Cells[f];
                    }

                    FileTable.Rows.Add(Row);
                }                        
            }
            return FileTable;
        }


        /// <summary>
        /// returnere xml tabel
        /// </summary>
        /// <param name="xmlfile"></param>
        /// <returns></returns>
        public static DataTable setXmlTable(string xmlfile)
         {
            DataTable xmlTable = new DataTable();
            DataSet ds = new DataSet();
            ds.ReadXml(xmlfile);
            xmlTable = ds.Tables[0];
            return xmlTable;
        }


        /// <summary>
        /// Tjekker hurtigt om indhold er ens på begge tabeller
        /// </summary>
        /// <param name="tbl1"></param>
        /// <param name="tbl2"></param>
        /// <returns></returns>
        public static bool TellTableDifferences(DataTable tbl1, DataTable tbl2)
        {
            if (tbl1.Rows.Count != tbl2.Rows.Count || tbl1.Columns.Count != tbl2.Columns.Count)
                return false;

            for (int i = 0; i < tbl1.Rows.Count; i++)
            {
                for (int c = 0; c < tbl1.Columns.Count; c++)
                {
                    if (!Equals(tbl1.Rows[i][c], tbl2.Rows[i][c]))
                        return false;
                }
            }
            return true;
        }




        /// <summary>
        /// Cancel progress bar
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="tokenSource"></param>
        /// <param name="token"></param>
        public static async void setProgressBar(IProgress<int> progress, CancellationTokenSource tokenSource,
            CancellationToken token)
        {
            //-- Task i baggrunden som sætter progressbaren ved fuldførelse
            await Task.Run(() =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    progress.Report(i);
                    i++;
                    Thread.Sleep(5);
                }

                //-- venter 2,5 sec, før den sættes til blank igen.
                Thread.Sleep(2500);
            }, token);

            tokenSource.Dispose();
        }


        /// <summary>
        /// Logger alt hvad brugeren udføre i en fil. Default sti:
        /// eks. @"C:\DME\log.txt"
        /// </summary>
        public static void writeLogFile(string txt, string FileLogLocation)
        {
            if (string.IsNullOrEmpty(FileLogLocation))
            {
                FileLogLocation = @"C:\DME\log.txt";
            }

            var fileinfo = new System.IO.FileInfo(FileLogLocation);

            //-- Hvis fil stien ikke findes
            if (!Directory.Exists(fileinfo.Directory.FullName))
            {
                //-- Laver mappe stien
                Directory.CreateDirectory(fileinfo.Directory.FullName);

                //-- Skriver til filen
                using (StreamWriter sw = File.AppendText(FileLogLocation))
                {
                    sw.WriteLine(DateTime.Now.ToString("M/d/yyyy HH:mm:ss") + " : " + txt);
                    sw.Close();
                }
            }
            else
            {       
                //-- Skriver til filen
                using (StreamWriter sw = File.AppendText(FileLogLocation))
                {
                    sw.WriteLine(DateTime.Now.ToString("M/d/yyyy HH:mm:ss") + " : " + txt);
                    sw.Close();                   
                }             
            }
        }


        /// <summary>
        /// Returnere filtreret tabel.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="Maintable"></param>
        /// <returns></returns>
        public static DataTable ReturnFilteredSearchedTable(string word, DataTable Maintable)
        {
            var Table = new DataTable();
           
            foreach (DataColumn colm in Maintable.Columns)
            {
                Table.Columns.Add(new DataColumn(colm.ColumnName));
            }

            foreach (DataRow row in Maintable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    if (item.ToString().ToLower().Contains(word.ToLower()))
                    {
                        Table.Rows.Add(row.ItemArray);
                        goto done;
                    }              
                }
                done:;
            }
            return Table;
        }


        /// <summary>
        /// Fokusere på Table1's indhold.
        /// Returnere en tabel med indhold op mod Table2.
        /// (TabellenDuVilHaveDataI, TabelSomBrugesTilfiltrering)
        /// Dataen SKAL findes i begge tabeller.
        /// </summary>
        /// <param name="Table1"></param>
        /// <param name="Table2"></param>
        /// <returns></returns>
        public static DataTable ReturnFilteredTableByMatch(DataTable Table1, DataTable Table2)
        {
            //-- filtered table by match
            var MatchedTable = new DataTable();
            //-- columns
            foreach (DataColumn leftColm in Table1.Columns)
            {
                MatchedTable.Columns.Add(new DataColumn(leftColm.ColumnName));
            }

            //-- rows
            foreach (DataRow Leftrows in Table1.Rows)
            {
                foreach (DataRow RightRows in Table2.Rows)
                {
                    if (!Leftrows.ItemArray.Except(RightRows.ItemArray).Any())
                    {
                        MatchedTable.Rows.Add(Leftrows.ItemArray);
                        //goto done;
                    }
                }
                //done:;
            }
            return MatchedTable;
        }



        /// <summary>
        /// Fokusere på Table1's indhold.
        /// Returnere en tabel med indhold op mod Table2.
        /// (TabellenDuVilHaveDataI, TabelSomBrugesTilfiltrering)
        /// Dataen SKAL findes i begge tabeller.
        /// </summary>
        /// <param name="Table1"></param>
        /// <param name="Table2"></param>
        /// <returns></returns>
        public static DataTable ReturnFilteredTableByMissMatch(DataTable Table1, DataTable Table2)
        {
            //-- filtered table by missmatch
            var MatchedTable = new DataTable();
            //-- columns
            foreach (DataColumn leftColm in Table1.Columns)
            {
                MatchedTable.Columns.Add(new DataColumn(leftColm.ColumnName));
            }
            //-- Begrænser for gennemløb, hvis en række findes i en anden tabel
            bool DoNotExist = true;
            ////-- Gennem løber tabel 1 rækker
            foreach (DataRow Leftrows in Table1.Rows)
            {
                //-- Gennem løber tabel 2 rækker
                foreach (DataRow RightRows in Table2.Rows)
                {
                    //-- Spørger om rækken ikke findes i tabel 2
                    if(!Leftrows.ItemArray.SequenceEqual(RightRows.ItemArray))
                    {
                        DoNotExist = true;
                    }
                    else
                    {
                        //-- Rækken findes
                        DoNotExist = false;
                        //-- skip til næste søgning
                        goto NextRow;
                    }
                }
                //-- Hvis den er sand betyder det at den forskellig fra indholdet i tabel 2
                if(DoNotExist.Equals(true))
                {
                    MatchedTable.Rows.Add(Leftrows.ItemArray);
                }
                NextRow:;
            }
            return MatchedTable;
        }

        /// <summary>
        ///  Returnere en tabel ud fra en valgt fil.
        /// </summary>
        public static DataTable FetchTableByFile(string delimetertxt)
        {
            var table = new DataTable();

            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Text and CSV Files(*.txt, *.csv)|" +
                "*.txt;*.csv|Text Files(*.txt)|" +
                "*.txt|CSV Files(*.csv)|*.csv|" +
                "All Files(*.*)|*.*"
            };

            if (file.ShowDialog() == true)
            {
                //-- Hvis det er en fil og findes
                if (File.Exists(file.FileName) && !file.FileName.Contains(".xml"))
                {
                    try
                    {
                        //-- tildeler tabellen til venstre tabel
                        table = setTable(file.FileName, delimetertxt);
                        //table.TableName = file.FileName;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Some files are broken or moved from " +
                            "fetched location. Please verify files.",
                            "DataMergeEditor - Merging message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                //-- Hvis det er xml, converter det til en tabel.
                if (file.FileName.Contains(".xml"))
                {
                    //-- tildeler tabellen til venstre tabel
                    table = setXmlTable(file.FileName);
                }
            }

            //-- returnere tabellen
            return table;
        }
    }
}