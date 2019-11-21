using DataMergeEditor.Model;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DataMergeEditor.ViewModel
{
    //-- compare rows guide : https://www.codeproject.com/Questions/686406/How-To-Compare-and-delete-datatable-row-using-Csha 
    public class Compare_tabItemViewModel : ViewModelBase
    {
        private Compare_tabItemModel compare_TabItemModel = new Compare_tabItemModel();
        private CancellationTokenSource ProgressBarTokenSource = new CancellationTokenSource();
        public IDataService dataservice;
        //-- constructor
        public Compare_tabItemViewModel(IDataService dataservice)
        {
            this.dataservice = dataservice;
            xLeftColumnsCount = 0;
            xLeftRowsCount = 0;
            xRightColumnsCount = 0;
            xRightRowsCount = 0;
            rightdelimitertxt = ";";
            leftdelimitertxt = ";";
            CurrentDBNameRight = "None";
            CurrentDBNameLeft = "None";
            //-- Modtager database navnet fra DatabaseConnectionTreeViewModel, ved valg af - Set connection globally
            MessengerInstance.Register<string>(this, (connection) => 
            {
                CurrentDBNameLeft = connection;
                CurrentDBNameRight = connection;
            });
        }
        //----------------------------------------------- Globale funktioner
        public ICommand QuickCheckTableContentCommand => new RelayCommand(QuickCheckTableContent);
        public ICommand ShowTableDifferencesByMatchCommand => new RelayCommand(ShowTableDifferencesByMatch);
        public ICommand ShowTableDifferencesByMissMatchCommand => new RelayCommand(ShowTableDifferencesByMissMatch);
        public ICommand ShowDefaultTablesCommand => new RelayCommand(ShowDefaultTables);
        public ICommand ShowHistoricLogCommand => new RelayCommand(ShowDatabaseQueryHistoric);

        /// <summary>
        /// Viser historic loggen som ligger lokalt på maskinen, ellers oprettet hvis ikke den findes.
        /// </summary>
        public void ShowDatabaseQueryHistoric()
        {
            dataservice.ShowDatabaseQueryHistoric();
        }

       
        /// <summary>
        /// fortæller hurtigt om der er forskel på tabel a og b
        /// </summary>
        public void QuickCheckTableContent()
        {
            if (righttDataTable != null && leftDataTable != null)
            {
                if (!righttDataTable.Rows.Count.Equals(0) && !leftDataTable.Rows.Count.Equals(0))
                {
                    if (TableAddons.TellTableDifferences(righttDataTable, leftDataTable) == true)
                    {
                        MessageBox.Show("There is no differences betweeen the two tables.",
                            "DataMergeEditor -  Quick compare message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("There is differences between the two tables.",
                            "DataMergeEditor -  Quick compare message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Table A or B need to contain rows in order to compare.",
                        "DataMergeEditor - Quick compare message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Table A or B cannot be null",
                    "DataMergeEditor - Quick compare message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Viser kun rækker hvor data er korrekt i begge tabeller
        /// </summary>
        public void ShowTableDifferencesByMatch()
        {
            //-- sætter de nye tabeller
            DummyTableRight = TableAddons.ReturnFilteredTableByMatch(righttDataTable, leftDataTable);
            DummyTableLeft = TableAddons.ReturnFilteredTableByMatch(leftDataTable, righttDataTable);
        }

        /// <summary>
        /// Viser kun rækker hvor data ikke stemmer overens i begge tabeller
        /// </summary>
        public void ShowTableDifferencesByMissMatch()
        {
            //-- sætter de nye tabeller
            DummyTableRight = TableAddons.ReturnFilteredTableByMissMatch(righttDataTable, leftDataTable);
            DummyTableLeft = TableAddons.ReturnFilteredTableByMissMatch(leftDataTable, righttDataTable);
        }

        /// <summary>
        /// Sætter tabellerne fra de originale tabeller
        /// </summary>
        public void ShowDefaultTables()
        {
            DummyTableLeft = leftDataTable;
            DummyTableRight = righttDataTable;
        }


        //-------------------------------------------------- LEFT GRID ----------------------------------------
        private CancellationTokenSource TableCancellationTokenSourceLeft = new CancellationTokenSource();
        public ICommand RunQueryLeftExectuerCommand => new RelayCommand(async () => leftDataTable = await RunSqlQueryLeft(QueryCommandLeft));
        public ICommand AddFileLeftGridCommand => new RelayCommand(AddLeftFile);
        public ICommand CancelFetchOfRecordsCommandLeft => new RelayCommand(CancelFetchOfRecordsLeft);
        public ICommand MoveToMainGridLeftCommand => new RelayCommand(MoveToMainLeft);
        public ICommand ClearCommandFieldCommandLeft => new RelayCommand(ClearCommandFieldLeft);
        public ICommand getQueryKeyUpLeftCommand => new RelayCommand(getQueryKeyUpLeft);
        public ICommand getQueryKeyDownLeftCommand => new RelayCommand(getQueryKeyDownLeft);

        public DataTable DummyTableLeft
        {
            get => compare_TabItemModel._dummyTableLeft;
            set => Set(ref compare_TabItemModel._dummyTableLeft, value);
        }



        /// <summary>
        /// Resetter Command feltet LEFT
        /// </summary>
        public void ClearCommandFieldLeft()
        {
            QueryCommandLeft = "";
        }

        /// <summary>
        /// Sender Tabellen vi ser på, til hovedeTabellen
        /// </summary>
        public void MoveToMainLeft()
        {
            //-- SEND, sender en datatable til MaintableViewModel, med værdien Venstre Tabel
            Messenger.Default.Send<DataTable, MainTableViewModel>(leftDataTable);
        }

        /// <summary>
        /// Venstre database forbindelse
        /// </summary>
        public string CurrentDBNameLeft
        {
            get => compare_TabItemModel._currentDBNameLeft;
            set => Set(ref compare_TabItemModel._currentDBNameLeft, value);
        }

        /// <summary>
        /// Stopper henting af tabeller, og viser resultatet til brugeren
        /// </summary>
        public void CancelFetchOfRecordsLeft()
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
                {
                    Progress = i;
                    txtchange = $"Success fully cancelled";
                });
            //-- Async tokens
            TableCancellationTokenSourceLeft.Cancel();
            TableCancellationTokenSourceLeft.Dispose();
            TableCancellationTokenSourceLeft = new CancellationTokenSource();
            //-- Progressbar
            PBarColorBrush = new SolidColorBrush(Colors.Yellow);
            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
            ProgressBarTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Venstre tabel
        /// </summary>
        public DataTable leftDataTable
        {
            get => compare_TabItemModel._leftDataTable;
            set
            {
                Set(ref compare_TabItemModel._leftDataTable, value);
                DummyTableLeft = leftDataTable;
            }
        }
        /// <summary>
        /// Historie oversigt for venstre tabel
        /// </summary>
        public ObservableCollection<string> HistoryCommandsLeft
        {
            get => compare_TabItemModel.HistoryCommandsListLeft;
            set => Set(ref compare_TabItemModel.HistoryCommandsListLeft, value);
        }
        public string ColorDif
        {
            get => compare_TabItemModel._colorDif;
            set => Set(ref compare_TabItemModel._colorDif, value);
        }
        public string CheckCorrectDataBool
        {
            get => compare_TabItemModel._checkCorrectDataBool;
            set => Set(ref compare_TabItemModel._checkCorrectDataBool, value);
        } 
         //-- delimiter for filen
         public string leftdelimitertxt
        {
            get => compare_TabItemModel._leftdelimitertxt;
            set => Set(ref compare_TabItemModel._leftdelimitertxt, value);
        }
        /// <summary>
        /// Til at sætte Index nummering, når man vælger en udført kommando ved piltasterne
        /// </summary>
        public int CommandNumbIndexLeft { get; set; }

        /// <summary>
        /// Tryk key-up på tasteturet, og du for sidste udført kommando.
        /// </summary>
        public void getQueryKeyUpLeft()
        {
            CommandNumbIndexLeft++;
            if (HistoryCommandsLeft.Count != 0 && CommandNumbIndexLeft <= HistoryCommandsLeft.Count && CommandNumbIndexLeft >= 1)
            {
                QueryCommandLeft = HistoryCommandsLeft[HistoryCommandsLeft.Count - CommandNumbIndexLeft];
            }
        }

        /// <summary>
        /// Tryk key-down på tasteturet, og du for nye udført kommando.
        /// </summary>
        public void getQueryKeyDownLeft()
        {
            CommandNumbIndexLeft--;
            if (HistoryCommandsLeft.Count != 0 && CommandNumbIndexLeft <= HistoryCommandsLeft.Count && CommandNumbIndexLeft >= 1)
            {
                QueryCommandLeft = HistoryCommandsLeft[HistoryCommandsLeft.Count - CommandNumbIndexLeft];
            }
        }


        /// <summary>
        /// add file knap left
        /// </summary>
        public void AddLeftFile()
        {
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
                TableAddons.writeLogFile($"{file.FileName} has been used in for " +
                    $"comparing data in compare windows", dataservice.LogLocation);

                //-- Hvis det er en fil og findes
                if (File.Exists(file.FileName) && !file.FileName.Contains(".xml"))
                {
                    try
                    {
                        //-- tildeler tabellen til venstre tabel
                        leftDataTable = TableAddons.setTable(file.FileName, leftdelimitertxt);
                    
                        //-- Opdatere tællre for rows
                        if (xLeftRowsCount != TableAddons.setTable(file.FileName, leftdelimitertxt).Rows.Count)
                        {
                            xLeftRowsCount = TableAddons.setTable(file.FileName, leftdelimitertxt).Rows.Count;
                        }

                        //-- opdatere tællre for columns
                        if (xLeftColumnsCount != TableAddons.setTable(file.FileName, leftdelimitertxt).Columns.Count)
                        {
                            xLeftColumnsCount = TableAddons.setTable(file.FileName, leftdelimitertxt).Columns.Count;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Some files are broken or moved from fetched location. Please verify files.",
                            "DataMergeEditor - Merging message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                //-- Hvis det er xml, converter det til en tabel.
                if (file.FileName.Contains(".xml"))
                {
                    //-- tildeler tabellen til venstre tabel
                    leftDataTable = TableAddons.setXmlTable(file.FileName);
                }
            }
        }


        //-- Sætter textboxen til det valgte item i listen
        public void setTextBoxFromSelectedListLeft()
        {
            if (SelectedIndexOfComboBoxItemLeft != null && SelectedIndexOfComboBoxItemLeft != "")
            {
                QueryCommandLeft = SelectedIndexOfComboBoxItemLeft;
            }
        }


        //-- Vælger det valgte fra comboboxen
        public string SelectedIndexOfComboBoxItemLeft
        {
            get => compare_TabItemModel._selectedIndexOfComboBoxItemLeft;
            set
            {
                Set(ref compare_TabItemModel._selectedIndexOfComboBoxItemLeft, value);

                setTextBoxFromSelectedListLeft();
            }
        }

        //-- For kolonner (columns) oversigt
        public int xLeftColumnsCount
        {
            get => compare_TabItemModel._xLeftColumnsCount;
            set => Set(ref compare_TabItemModel._xLeftColumnsCount, value);

        }

        //-- for rækker (rows) oversigt
        public int xLeftRowsCount
        {
            get => compare_TabItemModel._xLeftRowsCount;
            set => Set(ref compare_TabItemModel._xLeftRowsCount, value);

        }
        //-------------------------------------------------- MID GRID ----------------------------------------



        //------------------------------------------------- markering på begge grids ---------------------------

        //-- Vælger den markeret  tabel index i tabellen
        public int TableCoumnIndexisSelected
        {
            get => compare_TabItemModel._tableColumnIndexisSelected;
            set => Set(ref compare_TabItemModel._tableColumnIndexisSelected, value);

        }

        //-------------------------------------------------- RIGHT GRID ----------------------------------------
        private CancellationTokenSource TableCancellationTokenSourceRight = new CancellationTokenSource();
        public ICommand AddFileRightGridCommand => new RelayCommand(AddRightFile);
        public ICommand RunQueryRightExectuerCommand => new RelayCommand(async () => righttDataTable = await RunSqlQueryRight(QueryCommandRight));
        public ICommand CancelFetchOfRecordsCommandRight => new RelayCommand(CancelFetchOfRecordsRight);
        public ICommand ClearCommandFieldCommandRight => new RelayCommand(ClearCommandFieldRight);
        public ICommand MoveToMainGridRightCommand => new RelayCommand(MoveToMainRight);
        public ICommand getQueryKeyUpRightCommand => new RelayCommand(getQueryKeyUpRight);
        public ICommand getQueryKeyDownRightCommand => new RelayCommand(getQueryKeyDownRight);

        public DataTable DummyTableRight
        {
            get => compare_TabItemModel._dummyTableRight;
            set => Set(ref compare_TabItemModel._dummyTableRight, value);
        }

        /// <summary>
        /// Resetter Command feltet LEFT
        /// </summary>
        public void ClearCommandFieldRight()
        {
            QueryCommandRight = "";
        }

        /// <summary>
        /// Databasenavn for højre tabel
        /// </summary>
        public string CurrentDBNameRight
        {
            get => compare_TabItemModel._currentDBNameRight;
            set => Set(ref compare_TabItemModel._currentDBNameRight, value);
        }

        /// <summary>
        /// Sender Tabellen vi ser på, til hovedeTabellen
        /// </summary>
        public void MoveToMainRight()
        {
            //-- SEND
            Messenger.Default.Send<DataTable, MainTableViewModel>(righttDataTable);
        }

        /// <summary>
        /// Stopper henting af tabeller
        /// </summary>
        public void CancelFetchOfRecordsRight()
        {
             //-- Alternativ progressbar opdatering
             var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"Success fully cancelled";
            });
            //-- Async token
            TableCancellationTokenSourceRight.Cancel();
            TableCancellationTokenSourceRight.Dispose();
            TableCancellationTokenSourceRight = new CancellationTokenSource();
            //-- Progressbar
            PBarColorBrush = new SolidColorBrush(Colors.Yellow);
            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
            ProgressBarTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Højre tabel
        /// </summary>
        public DataTable righttDataTable
        {
            get => compare_TabItemModel._rightDataTable;
            set
            {
                Set(ref compare_TabItemModel._rightDataTable, value);
                DummyTableRight = righttDataTable;
            }
        }
        /// <summary>
        /// Historic oversigt for højre tabel
        /// </summary>        
        public ObservableCollection<string> HistoryCommandsRight
        {
            get => compare_TabItemModel._historyCommandsListRight;
            set => Set(ref compare_TabItemModel._historyCommandsListRight, value);
        }

        /// <summary>
        /// Sætter textboxen til det valgte item i listen
        /// </summary>
        public void setTextBoxFromSelectedListRight()
        {
            if (SelectedIndexOfComboBoxItemRight != null && SelectedIndexOfComboBoxItemRight != "")
            {
                QueryCommandRight = SelectedIndexOfComboBoxItemRight;
            }
        }


        /// <summary>
        /// Vælger det valgte fra comboboxen
        /// </summary>
        public string SelectedIndexOfComboBoxItemRight
        {
            get => compare_TabItemModel._selectedIndexOfComboBoxItemRight;
            set {
                Set(ref compare_TabItemModel._selectedIndexOfComboBoxItemRight, value);
                setTextBoxFromSelectedListRight();
            }
        }

        /// <summary> 
        /// delimiter for filen
        /// </summary>
        public string rightdelimitertxt
        {
            get => compare_TabItemModel._rightdelimitertxt;
            set => Set(ref compare_TabItemModel._rightdelimitertxt, value);
        }

        /// <summary>
        /// Til at sætte Index nummering, når man vælger en udført kommando ved piltasterne
        /// </summary>
        public int CommandNumbIndexRight { get; set; }

        /// <summary>
        /// Tryk key-up på tasteturet, og du for sidste udført kommando.
        /// </summary>
        public void getQueryKeyUpRight()
        {
            CommandNumbIndexRight++;
            if (HistoryCommandsRight.Count != 0 && CommandNumbIndexRight <= HistoryCommandsRight.Count && CommandNumbIndexRight >= 1)
            {
                QueryCommandRight = HistoryCommandsRight[HistoryCommandsRight.Count - CommandNumbIndexRight];
            }
        }

        /// <summary>
        /// Tryk key-down på tasteturet, og du for nye udført kommando.
        /// </summary>
        public void getQueryKeyDownRight()
        {
            CommandNumbIndexRight--;
            if (HistoryCommandsRight.Count != 0 && CommandNumbIndexRight <= HistoryCommandsRight.Count && CommandNumbIndexRight >= 1)
            {
                QueryCommandRight = HistoryCommandsRight[HistoryCommandsRight.Count - CommandNumbIndexRight];
            }
        }

        /// <summary>
        /// add file knap right
        /// </summary>
        public void AddRightFile()
        {
            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Text and CSV Files(*.txt, *.csv)|" +
                "*.txt;*.csv|Text Files(*.txt)|" +
                "*.txt|CSV Files(*.csv)|*.csv|All Files(*.*)|*.*"
            };

            if (file.ShowDialog() == true)
            {
                TableAddons.writeLogFile($"{file.FileName} has been used in for comparing data in compare windows",
                    dataservice.LogLocation);

                if (File.Exists(file.FileName) && !file.FileName.Contains(".xml"))
                {
                    //-- Hvis det er en fil og findes
                    try
                    {
                        righttDataTable = TableAddons.setTable(file.FileName, rightdelimitertxt);

                        //-- Opdatere tællre for rows
                        if (xRightRowsCount != TableAddons.setTable(file.FileName, rightdelimitertxt).Rows.Count)
                        {
                            xRightRowsCount = TableAddons.setTable(file.FileName, rightdelimitertxt).Rows.Count;
                        }

                        //-- opdatere tællre for columns
                        if (xRightColumnsCount != TableAddons.setTable(file.FileName, rightdelimitertxt).Columns.Count)
                        {
                            xRightColumnsCount = TableAddons.setTable(file.FileName, rightdelimitertxt).Columns.Count;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Some files are broken or moved from fetched location. Please verify files.",
                            "DataMergeEditor - Merging message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                  
                if (file.FileName.Contains(".xml"))
                {
                    //-- Hvis det er xml, converter det til en tabel.
                    righttDataTable = TableAddons.setXmlTable(file.FileName);
                }
            }
        }


        /// <summary>
        /// For kolonner (columns) oversigt
        /// </summary>
        public int xRightColumnsCount
        {
            get => compare_TabItemModel._xRightColumnsCount;
            set => Set(ref compare_TabItemModel._xRightColumnsCount, value);
        }

        /// <summary>
        /// for rækker (rows) oversigt
        /// </summary>
        public int xRightRowsCount
        {
            get => compare_TabItemModel._xRightRowsCount;
            set => Set(ref compare_TabItemModel._xRightRowsCount, value);
        }

        //------------------------------------------------ Progress bar -----------------------------------------
        /// <summary>
        /// Til progressbaren
        /// </summary>        
        public int RecordProgressGlobal
        {
            get => compare_TabItemModel._recordProgressFillGlobal;
            set => Set(ref compare_TabItemModel._recordProgressFillGlobal, value);
        }

        /// <summary>
        /// Bruges til FetchingNewQueryRecordsWindow
        /// </summary>
        public int FetchRowCountGlobal
        {
            get => compare_TabItemModel._fetchRowCountGlobal;
            set => Set(ref compare_TabItemModel._fetchRowCountGlobal, value);
        }

        /// <summary>
        /// action progressBar text
        /// </summary>
        public string txtchange
        {
            get => compare_TabItemModel._txtchange;
            set => Set(ref compare_TabItemModel._txtchange, value);
        }

        /// <summary>
        /// action progressBarFarve
        /// </summary>
        public SolidColorBrush PBarColorBrush
        {
            get => compare_TabItemModel._pBarColorBrush;
            set => Set(ref compare_TabItemModel._pBarColorBrush, value);
        }

        /// <summary>
        /// Sætter værdien for ActionProgressBar
        /// Ved handling udførelse, sættes værdien til 100, og den er fyldt (grøn)
        /// Ved forkert handling, sættes den til 0 først, og derefter 100 rød;
        /// </summary>
        public int Progress
        {
            get => compare_TabItemModel._progressFill;
            set => Set(ref compare_TabItemModel._progressFill, value);
        }

        //----------------------------------------------- for sql commands left ------------------------
        //-- Sætter værdien for query feltet LEFT      
        public string QueryCommandLeft
        {
            get => compare_TabItemModel._queryCommandLeft;
            set => Set(ref compare_TabItemModel._queryCommandLeft, value);
        }


        /// <summary>
        /// Sætter en datatabel via. en task<datatabel>
        /// Returnere tabellen hvis forbindelsen er gyldig.
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> RunSqlQueryLeft(string Query)
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
                {
                    Progress = i;
                    txtchange = $"{Progress} out of {FetchRowCountGlobal}";
                });
            //-- Tilføjer kommando til venstre historic log
            HistoryCommandsLeft.Add(Query);
            //-- Skriver i logfilen
            TableAddons.writeLogFile(Query, dataservice.LogLocation);    
            
            if (!string.IsNullOrWhiteSpace(Query))
            {
                if (!string.IsNullOrEmpty(CurrentDBNameLeft) 
                    && !CurrentDBNameLeft.ToLower().Equals("none") 
                    && !Query.ToLower().Contains("change database to"))
                {
                    try
                    {
                        if (Query.ToLower().StartsWith("select"))
                        {
                            var xLefttable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBNameLeft).Value.Execute(Query, progress, TableCancellationTokenSourceLeft.Token, TableCancellationTokenSourceLeft, dataservice.RowLimiter);
                            FetchRowCountGlobal = dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBNameLeft).Value.GetTableRowCount(Query);
                            //-- Setter row/Column counts
                            xLeftRowsCount = xLefttable.Rows.Count;
                            xLeftColumnsCount = xLefttable.Columns.Count;
                            //-- Resetter kommando feltet
                            QueryCommandLeft = "";
                            //-- Async token
                            if (!TableCancellationTokenSourceLeft.IsCancellationRequested)
                            {
                                TableCancellationTokenSourceLeft.Dispose();
                            }
                            //-- Setter ny cancellation token
                            TableCancellationTokenSourceLeft = new CancellationTokenSource();
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //-- Returnere tabellen
                            return xLefttable;
                        }
                        else
                        {
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.Red);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //-- Besked
                            MessageBox.Show("Query only accepts select and reset statements in compare mode",
                                "DataMergeEditor - Compare message",
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                            QueryCommandLeft = "";
                            return leftDataTable;
                        }
                    }
                    catch (Exception e)
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();

                        MessageBox.Show("Your query must match the database content, and be valid in order to execute"
                            + Environment.NewLine
                            + Environment.NewLine 
                            + $"The error for {CurrentDBNameLeft} sounded like: " 
                            + e.ToString().Substring(0, 250),
                            "DataMergeEditor - Database connection",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return leftDataTable;
                    }
                }
                else if (Query.ToLower().Equals("reset"))
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    return new DataTable();
                }
                else if (Query.ToLower().Equals("help"))
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- besked
                    MessageBox.Show("Query only accepts select, reset and change database statements in compare mode",
                        "DataMergeEditor - Compare message",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return leftDataTable;
                }
                else if (Query.ToLower().StartsWith("change database to"))
                {
                    if (dataservice.ConnectionList.ContainsKey(Query.Substring(Query.IndexOf("to")).Split(' ')[1]))
                    {
                        CurrentDBNameLeft = Query.Substring(Query.IndexOf("to")).Split(' ')[1];
                        MessageBox.Show($"Changed database connection to {CurrentDBNameLeft}", 
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- resetter kommando feltet
                        QueryCommandLeft = "";
                        return leftDataTable;
                    }
                    else
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        MessageBox.Show("Invalid database choosen",
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return leftDataTable;
                    }
                }
                else
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    MessageBox.Show("Please choose an valid database connection" +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Command: Change database to xxx",
                        "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return leftDataTable;
                }
            }
            else
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
                MessageBox.Show("Query cannot be empty", "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return leftDataTable;
            }
        }



        //----------------------------------------------- for sql commands right ------------------------
        //-- Sætter værdien for query feltet       
        public string QueryCommandRight
        {
            get => compare_TabItemModel._queryCommandRight;
            set => Set(ref compare_TabItemModel._queryCommandRight, value);
        }

        /// <summary>
        /// Sætter en datatabel via. en task<datatabel>
        /// Returnere tabellen hvis forbindelsen er gyldig.
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> RunSqlQueryRight(string Query)
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"{Progress} out of {FetchRowCountGlobal}";
            });
            //-- Tilføjer til højre historik 
            HistoryCommandsRight.Add(Query);
            //-- skriver i loggen
            TableAddons.writeLogFile(Query, dataservice.LogLocation);

            if (!string.IsNullOrWhiteSpace(Query))
            {
                if (!string.IsNullOrEmpty(CurrentDBNameRight)
                    && !CurrentDBNameRight.ToLower().Equals("none") 
                    && !Query.ToLower().Contains("change database to"))
                {
                    try
                    {
                        if (Query.ToLower().StartsWith("select"))
                        {
                            QueryCommandRight = "";
                            var xRightTable = await dataservice.ConnectionList.FirstOrDefault(x =>
                            x.Key == CurrentDBNameRight).Value.Execute(Query, progress, 
                            TableCancellationTokenSourceRight.Token, TableCancellationTokenSourceRight, dataservice.RowLimiter);
                            FetchRowCountGlobal = xRightTable.Rows.Count;
                            xRightRowsCount = xRightTable.Rows.Count;
                            xRightColumnsCount = xRightTable.Columns.Count;
                            //-- Async token
                            if (!TableCancellationTokenSourceRight.IsCancellationRequested)
                            {
                                TableCancellationTokenSourceRight.Dispose();
                            }
                            //-- Resetter cancellation token
                            TableCancellationTokenSourceRight = new CancellationTokenSource();
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //--Returnere tabellen
                            return xRightTable;
                        }
                        else
                        {
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.Red);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //-- Besked
                            MessageBox.Show("Query only accepts select, reset and change database in compare mode",
                                "DataMergeEditor - Compare message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            //--nulstiller kommando feltet
                            QueryCommandRight = "";
                            //-- returnere tabellen
                            return righttDataTable;
                        }
                    }
                    catch (Exception e)
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- Besked
                        MessageBox.Show("Your query must match the databaes content, and be valid in order to execute"
                            + Environment.NewLine 
                            + Environment.NewLine 
                            + $"The error for {CurrentDBNameRight} sounded like: "
                            + e.ToString().Substring(0, 250),
                            "DataMergeEditor - Database connection",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        //-- returnere tabellen
                        return righttDataTable;
                    }
                }
                else if (Query.ToLower().Equals("reset"))
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- returnere tabellen
                    return new DataTable();
                }
                else if (Query.ToLower().Equals("help"))
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- besked
                    MessageBox.Show("Query only accepts select, reset and change database in compare mode",
                        "DataMergeEditor - Compare message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return righttDataTable;
                }
                else if (Query.ToLower().StartsWith("change database to"))
                {
                    if (dataservice.ConnectionList.ContainsKey(Query.Substring(Query.IndexOf("to")).Split(' ')[1]))
                    {
                        CurrentDBNameRight = Query.Substring(Query.IndexOf("to")).Split(' ')[1];
                        //-- Besked
                        MessageBox.Show($"Changed database connection to {CurrentDBNameRight}",
                            "DataMergeEditor - Change database message", 
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- resetter command feltet
                        QueryCommandRight = "";
                        return righttDataTable;
                    }
                    else
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- Besked
                        MessageBox.Show("Invalid database choosen", "DataMergeEditor - Change database message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return righttDataTable;
                    }
                }
                else
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- Besked
                    MessageBox.Show("Please choose an valid database connection" +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Command: Change database to xxx",
                        "DataMergeEditor - Database connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                    //-- Returnere tabellen
                    return righttDataTable;
                }
             }
            else
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
                //-- Besked
                MessageBox.Show("Query cannot be empty",
                    "DataMergeEditor - Database connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                //-- Returnere tabellen
                return righttDataTable;
            }
        }    
    }
}
