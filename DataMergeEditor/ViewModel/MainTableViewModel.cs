using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DataMergeEditor.Model;
using DataMergeEditor.View.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using DataMergeEditor.Model.Exports;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight.Messaging;
using DataMergeEditor.DBConnect.Data.ListData;
using DataMergeEditor.View.Windows.Exports;
using System.Collections.Generic;

//-- Database to datagrid: https://stackoverflow.com/questions/33588055/drawing-a-chart-from-a-datatable

namespace DataMergeEditor.ViewModel
{
    public class MainTableViewModel : ViewModelBase
    {
        private ExportGrid exportGrid = new ExportGrid();
        private MainTableModel _MainTableModel = new MainTableModel();
        private CommandsModel CommandsModel = new CommandsModel();
        private CancellationTokenSource TableCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource ProgressBarTokenSource = new CancellationTokenSource();
        public ICommand ClearTableCommand => new RelayCommand(cleartablerows);
        public ICommand AddCountCommand => new RelayCommand(AddRowCounter);
        public ICommand ResetdatatableCommand => new RelayCommand(resetdatatable);
        public ICommand RefreshMainGridWithFiltersCommand => new RelayCommand(() => ReturnDataTableForGridView(true));
        public ICommand AddNewColumnCommand => new RelayCommand(AddNewColumn);
        public ICommand RenameColumeHeaderCommand => new RelayCommand(RenameColumnHeader);
        public ICommand DeleteColumeHeaderCommand => new RelayCommand(DeleteColumnHeader);
        public ICommand clearSelectedColumnRowDataCommand => new RelayCommand(clearSelectedColumnRowData);
        public ICommand ExportToPdfCommand => new RelayCommand(() => exportGrid.ExportToPDF(DatatableMerger));
        public ICommand ExportToCSVCommand => new RelayCommand(() => exportGrid.exportToCSV(DatatableMerger));
        public ICommand ExportToTXTCommand => new RelayCommand(() => exportGrid.exportToTXT(DatatableMerger));
        public ICommand ExportToXMLCommand => new RelayCommand(() => exportGrid.ExportToXML(DatatableMerger));
        public ICommand ReplaceWordsForColumnRowCommand => new RelayCommand(ReplaceWordsForColumnRowsCell);
        public ICommand ReplaceWordsForDatableCommand => new RelayCommand(ReplaceWordsForTable);
        public ICommand CancelFetchedRowsCommand => new RelayCommand(cancelFetchedRows);
        public ICommand RunQueryExectuerCommand => new RelayCommand(async () => DatatableMerger = await RunSqlQuery(QueryTXT));
        public ICommand GetLastKeyUpCommand => new RelayCommand(getQueryKeyUp);
        public ICommand GetLastKeyDownCommand => new RelayCommand(getQueryKeyDown);
        public ICommand ChangeTextBoxCommand => new RelayCommand(setTextBoxFromSelectedList);
        public ICommand CreateTableCommand => new RelayCommand(CreateTable);
        public ICommand CreateColumnInsertIntoCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateColumnInsertIntoString);
        public ICommand CreateColumnInsertCommand => new RelayCommand(() => QueryTXT = CommandsModel.TableInsertColumnString);
        public ICommand ReplaceWordsForCommandFieldCommand => new RelayCommand(ReplaceWordsForCommandField);
        public ICommand ExportCurrentTableToExternalDatabaseCommand => new RelayCommand(ExportTableToExternalDatabase);
        public ICommand ShowHistoricLogCommand => new RelayCommand(ShowDatabaseQueryHistoric);
        public ICommand ClearCommandFieldCommand => new RelayCommand(ClearCommandField);
        public ICommand DisplaytableInformationCommand => new RelayCommand(displayDatatableInformation);

        /// <summary>
        /// Viser tabel informationer til brugeren
        /// </summary>
        public void displayDatatableInformation()
        {          
            MessageBox.Show(
                 $"Column count: {DatatableMerger.Rows.Count.ToString()}"
                + Environment.NewLine
                + $"Row count: {DatatableMerger.Columns.Count.ToString()}"
                + Environment.NewLine
                + $"Remote format: {DatatableMerger.RemotingFormat.ToString()}" 
                + Environment.NewLine
                + $"Has errors: {DatatableMerger.HasErrors.ToString()}",
                "DataMergeEditor - Show grid information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }


        /// <summary>
        ///  Dataservice for at dele data mellem viewmodels
        /// </summary>
        private readonly IDataService dataservice;

        /// <summary>
        /// Constructoren for MainTableViewModel
        /// </summary>
        public MainTableViewModel(IDataService dataService)
        {
            this.dataservice = dataService;
            SqlQueryTableHasChanged = false;
            HistoryCommands.Add("");
            CommandNumbIndex = 1;
            FetchRowCountGlobal = 100;
            CurrentDBName = "None";
            //-- Modtager Datatabeller fra alle de andre ViewModels --- PULL
            MessengerInstance.Register<DataTable>(this, MergeWithRecivedData);
            //-- Modtager besked fra sidepanelet, at der er lavet en ændring i listen.
           //-- Derfor skal tabellen opdateres ud fra indholdet.  --- PULL
            MessengerInstance.Register<bool>(this, ReturnDataTableForGridView);
            //-- Modtager database navnet fra DatabaseConnectionTreeViewModel,
            //-- ved valg af - Set connection globally -- PULL
            MessengerInstance.Register<string>(this, (connection) => { CurrentDBName = connection; });
            //-- Modtager Listen af strings til filtrering fra sidepanelviewmodel --- PULL
            MessengerInstance.Register<ObservableCollection<TextBox>>(this, (SideList) => { SidePanelFilterListBoxes = SideList; });
        }


        /// <summary>
        /// Hoved tabellen brugeren kombinere data i.
        /// </summary>
        public DataTable DatatableMerger
        {
            get => _MainTableModel.DataTableMergeren;
            set => Set(ref _MainTableModel.DataTableMergeren, value);
        }

        /// <summary>
        /// Til at sette 
        /// </summary>
        ObservableCollection<TextBox> SidePanelFilterListBoxes
        {
            get => _MainTableModel._sidePanelFilterListBoxes;
            set => Set(ref _MainTableModel._sidePanelFilterListBoxes, value);
        }

        /// <summary>
        /// Merger modtagede DataTabeller fra andre viewmodels, og viser det til brugeren
        /// </summary>
        /// <param name="recivedTable"></param>
        public void MergeWithRecivedData(DataTable recivedTable)
        {
            DatatableMerger = TableAddons.MergeTables(DatatableMerger, recivedTable);
        }
        
        /// <summary>
        /// Den aktuelle Database man er forbundet til på sin fane
        /// </summary>
        public string CurrentDBName
        {
            get => _MainTableModel._currentDBName;
            set => Set(ref _MainTableModel._currentDBName, value);
        }

        /// <summary>
        /// Afventer implementering. - Fix
        /// </summary>
        public bool SqlQueryTableHasChanged
        {
            get => _MainTableModel._sqlQueryTableHasChanged;
            set => Set(ref _MainTableModel._sqlQueryTableHasChanged, value);
        }

        /// <summary>
        /// Vælger den markeret  tabel index i tabellen til renaming
        /// </summary>
        public DataGridCellInfo TableCoumnIndexisSelected
        {
            get => _MainTableModel._tableColumnIndexisSelected;
            set => Set(ref _MainTableModel._tableColumnIndexisSelected, value);
        }

        /// <summary>
        /// delimiteren for filerne
        /// </summary>
        public string FileDelimiter
        {
            get => _MainTableModel._columnDelimiter;
            set
            {
                if(value.Equals(";") || value.Equals(",") || value.Equals(":") || value.Equals("."))
                {
                    Set(ref _MainTableModel._columnDelimiter, value);
                    ReturnDataTableForGridView(true);
                }
                else if(string.IsNullOrWhiteSpace(value)) //-- Kan ikke gøre andet end dette.
                {
                    Set(ref _MainTableModel._columnDelimiter, "");
                }
                else
                {
                    Set(ref _MainTableModel._columnDelimiter, "");
                    MessageBox.Show("Delimiter value must be ; , . or ;",
                        "DataMergeEditor - delimiter message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }


        /// <summary>
        /// Viser historic loggen som ligger lokalt på maskinen, ellers oprettet hvis ikke den findes.
        /// </summary>
        public void ShowDatabaseQueryHistoric()
        {
            dataservice.ShowDatabaseQueryHistoric();
        }


        /// <summary>
        /// Rename column header funktionen
        /// </summary>
        //-- https://stackoverflow.com/questions/4853202/wpf-datagrid-how-do-i-determine-the-column-index-of-an-item 
        public void RenameColumnHeader()
        {
            RenameColumHeaderWindow renameColumeHeader = new RenameColumHeaderWindow();
            if (renameColumeHeader.ShowDialog() == true)
            {
                DatatableMerger = TableAddons.RenameColumnHeader(DatatableMerger, RenamedColumnValue, TableCoumnIndexisSelected);
                TableAddons.writeLogFile($"Renamed column header {TableCoumnIndexisSelected.Column.Header} to {RenamedColumnValue}", dataservice.LogLocation);
                RenamedColumnValue = "";
            }
        }

        /// <summary>
        /// slet kolonne column header funktionen
        /// </summary>
        public void DeleteColumnHeader()
        {
            DatatableMerger = TableAddons.DeleteColumnHeader(DatatableMerger, TableCoumnIndexisSelected);
            TableAddons.writeLogFile($"Deleted column header {TableCoumnIndexisSelected.Column.Header}", dataservice.LogLocation);
        }


        /// <summary>
        /// Ny kolonne navn (rename column)
        /// </summary>
        public string RenamedColumnValue
        {
            get => _MainTableModel._renamedColumnValue;
            set => Set(ref _MainTableModel._renamedColumnValue, value);
        }


        /// <summary>
        /// erstat alle (hele ord) for tabellen
        /// nyt ord: NewCellValueTextString
        /// ord at erstatte: CellValueTextString
        /// </summary>
        public void ReplaceWordsForTable()
        {
            //-- Laver vindue instansen så den bruger DENNE aktuelle åbne fane's viewmodel, og bruger dataen hertil.
            var window = new ReplaceWordsInCommandMainWindow();
            window.DataContext = this;
            
            if (window.ShowDialog() == true && !DatatableMerger.Rows.Count.Equals(0))
            {
                if(CellValueTextString != null && NewCellValueTextString != null)
                {
                    foreach (DataColumn colmn in DatatableMerger.Columns)
                    {
                        for (int i = 0; i < DatatableMerger.Rows.Count; i++)
                        {
                            DatatableMerger.Rows[i][colmn] = DatatableMerger.Rows[i][colmn].ToString().Replace(CellValueTextString, NewCellValueTextString);
                        }
                    }
                    TableAddons.writeLogFile($"Replaced row cell values for {CellValueTextString} to {NewCellValueTextString}", dataservice.LogLocation);
                }
                else
                {
                    MessageBox.Show("Values cannot be null",
                        "DataMergeEditor - Replace word message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Resetter command feltet
        /// </summary>
        public void ClearCommandField()
        {
            QueryTXT = "";
        }

        /// <summary>
        /// erstat alle (hele ord) for command feltet
        /// nyt ord: NewCellValueTextString
        /// ord at erstatte: CellValueTextString
        /// </summary>
        public void ReplaceWordsForCommandField()
        {
            //-- Laver vindue instansen så den bruger DENNE aktuelle åbne fane's viewmodel, og bruger dataen hertil.
            var window = new ReplaceWordsInCommandMainWindow();
            window.DataContext = this;

            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(QueryTXT))
            {
                //-- Tjekker om værdien overhovedet findes og sætter den som en bool
                bool contains = QueryTXT.Contains(CellValueTextString);
                //-- Verificere ovenstående
                if (contains != false)
                {
                    QueryTXT = QueryTXT.Replace(CellValueTextString, NewCellValueTextString);
                    TableAddons.writeLogFile($"Replaced command values for {CellValueTextString} to {NewCellValueTextString}", dataservice.LogLocation);
                }
                else
                {
                    MessageBox.Show($"{CellValueTextString} does not exist in your command field", "DataMergeEditor - Replace word message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Datatable cannot be empty", 
                    "DataMergeEditor - Replace word message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// erstat alle bogstaver for den valgte kolonne, og dens rækker
        /// </summary>        
        public void ReplaceWordsForColumnRowsCell()
        {
            ReplaceWordsInColumnMainWindow ReplaceWordsInColumnMainWindow = new ReplaceWordsInColumnMainWindow();
            if (ReplaceWordsInColumnMainWindow.ShowDialog() == true && !DatatableMerger.Rows.Count.Equals(0))
            {
                try
                {
                    foreach (DataRow row in DatatableMerger.Rows)
                    {
                        var rowcellvalue = row[DatatableMerger.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName].ToString();
                        if (rowcellvalue.Contains(CellValueTextString))
                        {
                            row[DatatableMerger.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName] = rowcellvalue.ToString().Replace(CellValueTextString, NewCellValueTextString);
                        }
                    }
                    //-- logger scriptet kørt
                    TableAddons.writeLogFile($"Replaced row value {CellValueTextString} with {NewCellValueTextString} for column {DatatableMerger.Columns[TableCoumnIndexisSelected.Column.DisplayIndex].ColumnName}", dataservice.LogLocation);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString().Substring(0, 250),
                        "DataMergeEditor - Replace word message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Datatable cannot be empty",
                    "DataMergeEditor - Replace word message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Connection liste af de gyldige forbindelser i sidepanelets tre liste.
        /// Bruges til exportering af tabeller, skifte forbindelse til combobox nederst højre side.
        /// </summary>
        public ObservableCollection<string> ConnectionList => dataservice.ConnectionListNames;

        /// <summary>
        /// Til den valgte database som går til ExportTableToExternalDatabase
        /// </summary>
        public string SelectedDatabaseText
        {
            get => _MainTableModel._selectedDatabaseText;
            set => Set(ref _MainTableModel._selectedDatabaseText, value);
        }

        /// <summary>
        /// Binder det nye navn til oprettelsen af tabellen
        /// Bruges til EXportTableToExeternDatabase
        /// </summary>
        public string CreateTableEXP
        {
            get => _MainTableModel._createTableEXP;
            set => Set(ref _MainTableModel._createTableEXP, value);
        }

        /// <summary>
        ///   Export af tabel, til pre-view via.  ExportToExternalDatabaseWindowMain
        /// </summary>
        public DataTable ExportLabelFields
        {
            get => _MainTableModel._exportLabelFields;
            set => Set(ref _MainTableModel._exportLabelFields, value);
        }

        /// <summary>
        /// Viser et nyt pop up vindue som vil vise en liste med connectionlist samt OK til exportering
        /// </summary>
        public void ExportTableToExternalDatabase()
        {
            //-- resetter tabellen
            ExportLabelFields = new DataTable();

            string command = CommandsModel.setCreateItemCommand(DatatableMerger);
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                ProgressFill = i;
                txtchange = $"{ProgressFill} out of {FetchRowCountGlobal}";
            });

            //-- Det nye popup vindue
            var exportToExternalDatabaseWindow = new ExportToExternalDatabaseWindowMain();

            //-- Når brugeren har sagt OK, export griddet til en ny database
            //-- SelectedMarkedText = databasenavn
            exportToExternalDatabaseWindow.DataContext = this;
            if (exportToExternalDatabaseWindow.ShowDialog() == true)
            {
                try
                {
                    dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.ExportToExternalDatabase(DatatableMerger, CreateTableEXP, false, "");
                    //-- log
                    TableAddons.writeLogFile($"Exportet main grid to {dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.Name} as table {CreateTableEXP}", dataservice.LogLocation);
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                }
                catch (Exception e)
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- log
                    TableAddons.writeLogFile($"Failed exporting maingrid table as {CreateTableEXP} to {dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.Name}", dataservice.LogLocation);
                    //-- Besked
                    MessageBox.Show($"Failed exporting maingrid table as {CreateTableEXP} to {SelectedDatabaseText}"
                            + Environment.NewLine
                            + Environment.NewLine
                            + "The error sounded like: "
                            + e.ToString().Substring(0, 250),
                            "Data Merge Editor - Error exporting to external message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
           }
        }

        /// <summary>
        /// Til at opdatere ShowCellContentWindow indholdet.
        /// </summary>
        public string NewCellValueTextString
        {
            get => _MainTableModel._newCellValueTextString;
            set => Set(ref _MainTableModel._newCellValueTextString, value);
        }

        /// <summary>
        /// Til at opdatere ShowCellContentWindow indholdet.
        /// </summary>
        public string CellValueTextString
        {
            get => _MainTableModel._cellValueTextString;
            set => Set(ref _MainTableModel._cellValueTextString, value);
        }

        /// <summary>
        /// Datatabel som printer tabellen ud i datagriddet
        /// Sat til opdater knap
        /// </summary>
        public async void ReturnDataTableForGridView(bool ChangesMade)
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                ProgressFill = i;
                txtchange = $"{ProgressFill} out of {FetchRowCountGlobal}";
            });

            if (ChangesMade.Equals(true))
            {
                //-- Laver en ny tabel
                var MainTable = new DataTable();
                //-- Hvis listen er tom, tilføj en tom tabel til griddet.
                //-- Callback mod sidepanelviewmodel MainListContainer ----------------------------------- PULL
                //-- assigner filetablelist (frit valgt navn), som er Mainlistcontaineren man har modtaget. 
                //-- Derefter assigner den videre.
                ObservableCollection<ContentList> SidePanelList = null;
                MessengerInstance.Send<NotificationMessageAction<ObservableCollection<ContentList>>>(
                    new NotificationMessageAction<ObservableCollection<ContentList>>("", fileTableList => SidePanelList = fileTableList)
                    );

                //-- HVis listen er tom, og tabellen er tom
                if (SidePanelList.Count.Equals(0) 
                    && DatatableMerger.Rows.Count.Equals(0)
                    && DatatableMerger.Columns.Count.Equals(0))
                {
                    DatatableMerger = MainTable;
                }
                else
                {
                    //-- For hvert objekt i min sidepanels liste, lav en tabel
                    foreach (var item in SidePanelList)
                    {
                        //-- Laver en ny tabel
                        var fetchedDataBaseDatatable = new DataTable();
                        //-- Logger objektet der bruges til migrering af tabeller
                        TableAddons.writeLogFile($"{item.Type} has been used in merging for the maingrid", dataservice.LogLocation);
                        //-- Hvis det er en fil og findes
                        if (File.Exists(item.Type) && !item.Type.Contains(".xml") && FileDelimiter != "")
                        {
                            try
                            {
                                //-- Kombinere alle filer
                                if (MainTable.Columns.Count.Equals(0))
                                {
                                    MainTable = TableAddons.setTable(item.Type, FileDelimiter);
                                }
                                else
                                {
                                    MainTable = TableAddons.MergeTables(MainTable, TableAddons.setTable(item.Type, FileDelimiter));
                                }
                            }
                            catch (Exception)
                            {
                                //-- Progressbar
                                PBarColorBrush = new SolidColorBrush(Colors.Red);
                                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                ProgressBarTokenSource = new CancellationTokenSource();
                                MessageBox.Show("Some files are broken, moved from fetched location or delimiter field is empty."
                                    + Environment.NewLine 
                                    + Environment.NewLine
                                    + "Please verify - and the database connection if used.",
                                    "Data Merge Editor - Merging message",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }

                        ////-- Hvis den ikke har /, og ikke findes som fil, så ved vi det er en database tabel.
                        //-- Man formoder, at når man tilføjer en HEL tabel, så er det ALt man skal have, 
                        if (!item.Type.Contains(@"\") && !File.Exists(item.Type))
                        {
                            try
                            {
                                var QueryDel = $"select * from {item.Type}";
                                fetchedDataBaseDatatable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == item.DatabaseName).Value.Execute(QueryDel, progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, int.MaxValue);
                                TableCancellationTokenSource = new CancellationTokenSource();
                                //--Merges every files(tables) into one.
                                //-- Spørg om man er sikker hvis den nye tabel har flere end 5.000 rækker.
                                //Messagebox - verify merge table table
                                if (fetchedDataBaseDatatable.Rows.Count >= 5000)
                                {

                                    if (MessageBox.Show($"{item.Type} contains more than 5.000 rows. Are you sure to add it?",
                                        "Data Merge Editor - Merging warning",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                    {
                                        MainTable = TableAddons.MergeTables(MainTable, fetchedDataBaseDatatable);
                                    }
                                    else
                                    {
                                        //-- Progressbar
                                        PBarColorBrush = new SolidColorBrush(Colors.Yellow);
                                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                        ProgressBarTokenSource = new CancellationTokenSource();

                                        MessageBox.Show($"Merging with {item.Type} has been successfully cancelled",
                                            "Data Merge Editor - Merging message",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                                    }
                                }
                                else
                                {
                                    MainTable = TableAddons.MergeTables(MainTable, fetchedDataBaseDatatable);
                                }
                            }
                            catch (Exception e)
                            {
                                //-- Progressbar
                                PBarColorBrush = new SolidColorBrush(Colors.Red);
                                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                ProgressBarTokenSource = new CancellationTokenSource();

                                MessageBox.Show("There was an error fetching a table."
                                    + Environment.NewLine
                                    + Environment.NewLine
                                    + "The error sounded like:"
                                    + Environment.NewLine
                                    + e.ToString().Substring(0, 250),
                                    "Data Merge Editor - Fetching database tables from list",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }

                        //-- Hvis det er xml, converter det til en tabel.
                        if (item.Type.Contains(".xml"))
                        {
                            try
                            {
                                MainTable = TableAddons.MergeTables(MainTable, TableAddons.setXmlTable(item.Type));
                            }
                            catch (Exception e)
                            {
                                //-- Progressbar
                                PBarColorBrush = new SolidColorBrush(Colors.Red);
                                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                ProgressBarTokenSource = new CancellationTokenSource();

                                MessageBox.Show("There was an error fetching a file."
                                                     + Environment.NewLine
                                                     + Environment.NewLine
                                                     + "The error sounded like:"
                                                     + Environment.NewLine
                                                     + e.ToString().Substring(0, 250),
                                                     "Data Merge Editor - Fetching database tables from list",
                                                     MessageBoxButton.OK,
                                                     MessageBoxImage.Information);
                            }
                        }
                    }

                    //-- Filtrering ud fra filter felterne
                    try
                    {
                        //-- Filteret
                        var ColumnFilter = new List<string>();
                        foreach (var item in SidePanelFilterListBoxes)
                        {
                            if (!String.IsNullOrWhiteSpace(item.Text))
                            {
                                ColumnFilter.Add(item.Text);
                            }
                        }
                        //-- den nye tabel med filter
                        DatatableMerger = new DataView(MainTable).ToTable(false, ColumnFilter.ToArray());
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                    }
                    catch (Exception)
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();

                        MessageBox.Show("Filtering field does not match any of the columns in the select(ed) files & tables" 
                            + Environment.NewLine
                            + Environment.NewLine
                            + "Please verify that the written filtering words matches with the added content",
                            "DataMergeEditor - Filtering message",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                }
            }
        }

        //-- Nyt navn til renamed column værdier
        public string SearchColumnValue
        {
            get => _MainTableModel._searchColumnValue;
            set => Set(ref _MainTableModel._searchColumnValue, value);
        }

        //-- Add new column værdi
        public string AddColumnValueName
        {
            get => _MainTableModel._addColumnValueName;
            set => Set(ref _MainTableModel._addColumnValueName, value);
        }


        /// <summary>
        /// Add new column funktion
        /// </summary>
        public void AddNewColumn()
        {
            NewColumnWindow newColumnPopUpWindow = new NewColumnWindow();
            if (newColumnPopUpWindow.ShowDialog() == true)
            {
                DatatableMerger = TableAddons.AddNewColumn(DatatableMerger, AddColumnValueName);
                TableAddons.writeLogFile($"{AddColumnValueName} colummn has been added", dataservice.LogLocation);

            }

            //-- Hvis tilfældet er, at der er ingen rækker, så laver den en tom række. 
            //-- Den række som vises, er for at oprette en række, men det er ikke en aktuel række som kan give forvirring.
            if (DatatableMerger.Rows.Count.Equals(0))
            {
                DataRow row = DatatableMerger.NewRow();
                DatatableMerger.Rows.Add(row);
            }
        }


        /// <summary>
        /// Add new RowCounter
        /// </summary>        
        public void AddRowCounter()
        {
            DatatableMerger = TableAddons.AddRowCounter(DatatableMerger);
        }

        /// <summary>
        /// Sletter den valgte kolonne rækkes data. (blanke)
        /// </summary>
        public void clearSelectedColumnRowData()
        {
            DatatableMerger = TableAddons.clearSelectedColumnRowData(DatatableMerger, TableCoumnIndexisSelected);
            TableAddons.writeLogFile($"Cleard row data for column {TableCoumnIndexisSelected.Column.Header}", dataservice.LogLocation);
        }


        /// <summary>
        /// Clear table row content
        /// </summary>
        public void cleartablerows()
        {
            if (DatatableMerger == null)
            {
                MessageBox.Show("Cannot clear rows when table data is not fetched", 
                    "DataMergeEditor - Clear rows message", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                DatatableMerger.Clear();
                TableAddons.writeLogFile($"All rows in the main grid has been removed", dataservice.LogLocation);
            }
        }

        /// <summary>
        /// Reset datatable (griddet)
        /// </summary>
        public void resetdatatable()
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                ProgressFill = i;
                txtchange = $"{ProgressFill} out of {FetchRowCountGlobal}";
            });

            //Messagebox - verify clear table content
            if (MessageBox.Show("You are about to clear your content. Are you sure? ",
                "Data Merge Editor - Dropping table warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DatatableMerger = new DataTable();
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
                //-- Logging
                TableAddons.writeLogFile($"The Main grid has been reset", dataservice.LogLocation);
            }
            else
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
            }
        }

        //------------------------- Tab index -------------------------------
        public int MainTabIndexMenu
        {
            get => _MainTableModel._mainTabIndexName;
            set => Set(ref _MainTableModel._mainTabIndexName, value);
        }

        //------------------------------------------- SQL query -------------------------------------------
        public ObservableCollection<string> HistoryCommands
        {
            get => _MainTableModel.HistoryCommandsList;
            set => Set(ref _MainTableModel.HistoryCommandsList, value);
        }

        /// <summary>
        ///  laver en linje af kommando
        /// </summary>
        public void CreateTable()
        {
            if (DatatableMerger != null)
            {
                QueryTXT = CommandsModel.setCreateItemCommand(DatatableMerger);
                TableAddons.writeLogFile(QueryTXT, dataservice.LogLocation);
            }
            else
            {
                MessageBox.Show("Grid data cannot be emtpy",
                    "DataMergeEditor - Creating table message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// Til at sætte Index nummering, når man vælger en udført kommando ved piltasterne
        /// </summary>
        public int CommandNumbIndex { get; set; }

        /// <summary>
        /// Tryk key-up på tasteturet, og du for sidste udført kommando.
        /// </summary>
        public void getQueryKeyUp()
        {
            CommandNumbIndex++;
            if (HistoryCommands.Count != 0 && CommandNumbIndex <= HistoryCommands.Count && CommandNumbIndex >= 1)
            {
                QueryTXT = HistoryCommands[HistoryCommands.Count - CommandNumbIndex];   
            }
        }

        /// <summary>
        /// Tryk key-down på tasteturet, og du for nye udført kommando.
        /// </summary>
        public void getQueryKeyDown()
        {
            CommandNumbIndex--;
            if (HistoryCommands.Count != 0 && CommandNumbIndex <= HistoryCommands.Count && CommandNumbIndex >= 1)
            {
                QueryTXT = HistoryCommands[HistoryCommands.Count - CommandNumbIndex];
            }
        }


        /// <summary>
        ///  action progressBar text
        /// </summary>
        public string txtchange
        {
            get => _MainTableModel._txtchange;
            set => Set(ref _MainTableModel._txtchange, value);
        }

        /// <summary>
        /// Vælger det valgte fra comboboxen
        /// </summary>
        public string SelectedIndexOfComboBoxItem
        {
            get => _MainTableModel._selectedIndexOfComboBoxItem;
            set
            {
                Set(ref _MainTableModel._selectedIndexOfComboBoxItem, value);
                setTextBoxFromSelectedList();
            }
        }

        /// <summary>
        /// action progressBarFarve
        /// </summary>
        public SolidColorBrush PBarColorBrush
        {
            get => _MainTableModel._pBarColorBrush;
            set => Set(ref _MainTableModel._pBarColorBrush, value);
        }

        //-- Sætter værdien for ActionProgressBar
        //-- Ved handling udførelse, sættes værdien til 100, og den er fyldt (grøn)
        //-- Ved forkert handling, sættes den til 0 først, og derefter 100 rød;
        public int ProgressFill
        {
            get => _MainTableModel._progressFill;
            set => Set(ref _MainTableModel._progressFill, value);
        }

        /// <summary>
        /// Sætter værdien for query feltet
        /// </summary>
        public string QueryTXT
        {
            get => _MainTableModel._queryTXT;
            set => Set(ref _MainTableModel._queryTXT, value);
        }

        /// <summary>
        /// Sætter textboxen til det valgte item i listen
        /// </summary>
        public void setTextBoxFromSelectedList()
        {
            if (SelectedIndexOfComboBoxItem != null && SelectedIndexOfComboBoxItem != "")
            {
                QueryTXT = SelectedIndexOfComboBoxItem;
            }
        }

        /// <summary>
        /// Til at stoppe antallet af rækker, men virker ikke helt endnu. FIX PLZ
        /// </summary>
        public void cancelFetchedRows()
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                ProgressFill = i;
                txtchange = $"{ProgressFill} out of {FetchRowCountGlobal}";
            });
            //-- async token
            TableCancellationTokenSource.Cancel();
            TableCancellationTokenSource.Dispose();
            TableCancellationTokenSource = new CancellationTokenSource();
            //-- Progressbar
            PBarColorBrush = new SolidColorBrush(Colors.Yellow);
            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
            ProgressBarTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Til popup record count beskeden
        /// </summary>
        public bool RecordCancelProgressFillGlobal
        {
            get => _MainTableModel._recordCancelProgressFillGlobal;
            set => Set(ref _MainTableModel._recordCancelProgressFillGlobal, value);
        }

        /// <summary>
        /// Til popup record count beskeden
        /// </summary>
        public int RecordCountingINTGlobal
        {
            get => _MainTableModel._recordCountingINTGlobal;
            set => Set(ref _MainTableModel._recordCountingINTGlobal, value);
        }

        /// <summary>
        /// Til popup record count beskeden
        /// </summary>
        public int FetchRowCountGlobal
        {
            get => _MainTableModel._fetchRowCountGlobal;
            set => Set(ref _MainTableModel._fetchRowCountGlobal, value);
        }

        /// <summary>
        /// Sætter en datatabel via. en task<datatabel>
        /// Returnere tabellen hvis forbindelsen er gyldig.
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> RunSqlQuery(string Query)
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                ProgressFill = i;
                txtchange = $"{ProgressFill} out of {FetchRowCountGlobal}";
            });

            //-- logging
            HistoryCommands.Add(Query);
            TableAddons.writeLogFile(Query, dataservice.LogLocation);

            if (!string.IsNullOrWhiteSpace(Query))
            {
                if (!string.IsNullOrEmpty(CurrentDBName) 
                    && !Query.ToLower().Contains("change database to") 
                    && !CurrentDBName.ToLower().Equals("none") 
                    && !Query.ToLower().Equals("help") 
                    && !Query.ToLower().Equals("reset"))
                {
                    try
                    {
                        if (Query.ToLower().StartsWith("insert into"))
                        {
                            //-- kopi af nuværende datatable
                            var dataTable = DatatableMerger.Copy();
                            //-- finder index nr, på select query delen
                            var queryDelIndex = QueryTXT.IndexOf("select");
                            //-- opdeler query stringen og lader den tage alt fra ordet select
                            var QueryDel = QueryTXT.Substring(queryDelIndex);
                            //-- laver en ny kommando med opdelingen så der står select test fra.. etc.
                            //-- Kalder kommandoen og laver en ny tabel med dataen
                            var pr = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute(QueryDel, progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, int.MaxValue);
                            //-- Verficere om brugeren har stoppet anmodningen
                            if (!TableCancellationTokenSource.IsCancellationRequested)
                            {
                                TableCancellationTokenSource.Dispose();
                            }
                            //-- resetter token
                            TableCancellationTokenSource = new CancellationTokenSource();
                            //Empty array
                            string[] arrvalues = new string[pr.Rows.Count];
                            //-- Opdeling af string
                            string DatabaseTabel = TableAddons.getBetween(QueryTXT, "select", "from");
                            string WorkColumn = TableAddons.getBetween(QueryTXT, "into", "as");

                            //-- loopcounter
                            for (int loopcounter = 0; loopcounter < pr.Rows.Count; loopcounter++)
                            {
                                arrvalues[loopcounter] = pr.Rows[loopcounter][DatabaseTabel].ToString();
                            }

                            //-- 1) Tjek om der er rækker nok. Hvis der er rækker nok i forhold til indholdet der skal tilføjes, så tilføj indholdet.
                            //-- 2) Hvis der ikke er rækker nok, så tilføj rækkerne så der er plads.
                            //-- 3) Hvis der er plads i forvejen til indholdet, så skal indholdet bare tilføjes.
                            //-- 4) Hvis den row position er tomt, skal den fylde positionen ud. 
                            //-- 5) Hvis den row position ikke er tomt, så skal den ignore den og gå videre.
                            if (arrvalues.Length > dataTable.Rows.Count)
                            {
                                //--Fylder valgte index column ud, hvis den er tom
                                for (int i = 0; i < arrvalues.Length; i++)
                                {
                                    if (dataTable.Rows[i][dataTable.Columns.IndexOf(WorkColumn)].ToString() == "")
                                    {
                                        DataRow row = dataTable.NewRow();
                                        dataTable.Rows.Add(row);
                                        dataTable.Rows[i][dataTable.Columns.IndexOf(WorkColumn)] = arrvalues[i];
                                    }
                                    else
                                    {
                                        //-- ellers tilføjer den en ny række under
                                        DataRow row = dataTable.NewRow();
                                        row[dataTable.Columns.IndexOf(WorkColumn)] = arrvalues[i];
                                        dataTable.Rows.Add(row);
                                    }
                                }
                            }
                            else
                            {
                                //--Fylder valgte index column ud, fordi der er plads nok
                                for (int i = 0; i < arrvalues.Length; i++)
                                {
                                    //-- hvis den position ikke har indhold eller er tomt, skal den fylde dataen ud
                                    if (dataTable.Rows[i][dataTable.Columns.IndexOf(WorkColumn)].ToString() == "" || dataTable.Rows[i][dataTable.Columns.IndexOf(WorkColumn)].Equals(null))
                                    {
                                        dataTable.Rows[i][dataTable.Columns.IndexOf(WorkColumn)] = arrvalues[i];
                                    }
                                    else
                                    {
                                        //-- ellers tilføjer den en ny række under
                                        DataRow row = dataTable.NewRow();
                                        row[dataTable.Columns.IndexOf(WorkColumn)] = arrvalues[i];
                                        dataTable.Rows.Add(row);
                                    }
                                }
                            }
                            //--resetter kommando feltet
                            QueryTXT = "";
                            //-- returnere tabellen
                            return dataTable;
                        }
                        else if (Query.ToLower().StartsWith("insert select"))
                        {
                            //-- kopi af nuværende datatable
                            var dataTable = DatatableMerger.Copy();
                            //-- finder index nr, på select query delen
                            var queryDelIndex = QueryTXT.IndexOf("select");
                            //-- opdeler query stringen og lader den tage alt fra ordet select
                            var QueryDel = QueryTXT.Substring(queryDelIndex);
                            //-- Kalder kommandoen og laver en ny tabel med dataen
                            var pr = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute(QueryDel, progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, int.MaxValue);
                            //-- token verficering
                            if (!TableCancellationTokenSource.IsCancellationRequested)
                            {
                                TableCancellationTokenSource.Dispose();
                            }
                            //-- resetter token
                            TableCancellationTokenSource = new CancellationTokenSource();
                            //-- resetter kommando felt
                            QueryTXT = "";
                            //-- returnere tabbellen
                            return TableAddons.MergeTables(dataTable, pr);
                        }
                        else if (Query.ToLower().StartsWith("create table"))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.AddToDatabase(Query, progress);
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //-- besked
                            MessageBox.Show($"Table {TableAddons.getBetween(Query.ToLower(), "table", "(")} has been created",
                                "DataMergeEditor - Change database message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            QueryTXT = "";
                            return DatatableMerger;
                        }
                        else if (Query.ToLower().StartsWith("disconnect") && !string.IsNullOrEmpty(CurrentDBName))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Disconnect(CurrentDBName);
                            CurrentDBName = "None";
                            QueryTXT = "";
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            return DatatableMerger;
                        }
                        else if (Query.ToLower().Equals("reconnect") && !string.IsNullOrEmpty(CurrentDBName))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Reconnect(CurrentDBName);
                            QueryTXT = "";
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            ProgressBarTokenSource = new CancellationTokenSource();
                            return DatatableMerger;
                        }
                        else
                        {
                            MessageBox.Show("The following staments are only allowed in the main window: "
                                + Environment.NewLine
                                + Environment.NewLine
                                + "The following staments are only allowed in the Main window: "
                                + Environment.NewLine
                                + Environment.NewLine
                                + "insert select xxx from xxx"
                                + Environment.NewLine
                                + "insert into {Column header} as select xxx from xxx"
                                + Environment.NewLine
                                + "Change database to xxx"
                                + Environment.NewLine
                                + "Create table"
                                + Environment.NewLine
                                + "Disconnect"
                                + Environment.NewLine
                                + "Reconnect"
                                + Environment.NewLine
                                + "Reset"
                                + Environment.NewLine
                                + "help"
                                , "DataMergeEditor - Change database message",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return DatatableMerger;
                        }
                    }
                    catch (Exception e)
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- besked
                        MessageBox.Show("The error sounded like: "
                            + Environment.NewLine
                            + Environment.NewLine
                            + e.ToString().Substring(0, 250),
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        //-- returnere nuværende tabel
                        return DatatableMerger;
                    }
                }
                else if (Query.ToLower().StartsWith("reset"))
                {
                    QueryTXT = "";
                    return new DataTable();
                }
                else if (Query.ToLower().Equals("help"))
                {
                    MessageBox.Show($"You are currently connected to {CurrentDBName}"
                      + Environment.NewLine
                      + Environment.NewLine
                      + "The following staments are only allowed in the Main window: "
                      + Environment.NewLine
                      + Environment.NewLine
                      + "insert select xxx from xxx"
                      + Environment.NewLine
                       + "insert into {Column header} as select xxx from xxx"
                      + Environment.NewLine
                       + "Change database to xxx"
                      + Environment.NewLine
                      + "Create table"
                      + Environment.NewLine
                      + "Disconnect"
                      + Environment.NewLine
                      + "Reconnect"
                      + Environment.NewLine
                      + "Reset"
                      + Environment.NewLine
                      + "help"
                      , "DataMergeEditor - help message",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
                    //-- resetter kommando feltet
                    QueryTXT = "";
                    //-- returnere tabellen
                    return DatatableMerger;
                }
                else if (Query.ToLower().StartsWith("change database to"))
                {
                    if (dataservice.ConnectionList.ContainsKey(Query.Substring(Query.IndexOf("to")).Split(' ')[1]))
                    {
                        CurrentDBName = Query.Substring(Query.IndexOf("to")).Split(' ')[1];
                        MessageBox.Show($"Changed database connection to {CurrentDBName}", 
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        QueryTXT = "";
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();

                        return DatatableMerger;
                    }
                    else
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        ProgressBarTokenSource = new CancellationTokenSource();
                        //-- besked
                        MessageBox.Show("Invalid database choosen",
                            "DataMergeEditor - Change database message", 
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return DatatableMerger;
                    }
                }
                else
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- besked
                    MessageBox.Show("Please choose an valid database connection"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Command: Change database to xxx", 
                        "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    //-- returnere tabellen
                    return DatatableMerger;
                }
            }
            else
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
                //-- besked
                MessageBox.Show("Query cannot be empty", "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                //-- returnere tabellen
                return DatatableMerger;
            }
        }
    }
}
