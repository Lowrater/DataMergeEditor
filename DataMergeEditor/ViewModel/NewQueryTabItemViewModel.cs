using DataMergeEditor.Model;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using DataMergeEditor.View.UserControls;
using DataMergeEditor.Model.Exports;
using DataMergeEditor.View.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using DataMergeEditor.Interfaces.STTableCT;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight.Messaging;
using DataMergeEditor.View.Windows.Exports;
using DataMergeEditor.Interfaces;

//-- async tasks
//: https://stackoverflow.com/questions/27089263/how-to-run-and-interact-with-an-async-task-from-a-wpf-gui 
//: https://igorpopov.io/2018/06/16/asynchronous-programming-in-csharp-with-wpf/ 
//-- Stoppe async tasks
//: https://stackoverflow.com/questions/33676376/c-sharp-wpf-cancellation-of-async-function

namespace DataMergeEditor.ViewModel
{
    public class NewQueryTabItemViewModel : ViewModelBase, ITableContent
    {
        private ExportGrid exportGrid = new ExportGrid();
        private DbBlobFile DbBlobFile = new DbBlobFile();
        private CommandsModel CommandsModel = new CommandsModel();
        private NewQueryModel _newQueryModel = new NewQueryModel();
        private CancellationTokenSource TableCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource ProgressBarTokenSource = new CancellationTokenSource();

        public ICommand ApplyChangesCommand => new RelayCommand(ApplyChanges);
        //-- Køre kommandoen i textfeltet via. F5
        public ICommand RunQueryExectuerCommand => new RelayCommand(async () => Maintable = await RunSqlQuery(QueryTXT));
        //-- køre sidste select kommando (Shift + Backspace)
        public ICommand RunLastSelectTextCommand => new RelayCommand(async () => Maintable = await RunSqlQuery(LastSelectQuerytxt));
        //////-- køre markeret kommando (Shift + Enter)
        public ICommand RunMarkedTextCommand => new RelayCommand(async () => Maintable = await RunSqlQuery(SelectedMarkedText));
        public ICommand RunSqlScriptsCommand => new RelayCommand(RunSQLScripts);
        public ICommand CreateInsertCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateInsertString(Maintable, CurrentScheme));
        public ICommand CreateTableOwnTypesAndInsertsCommand => new RelayCommand(() => QueryTXT = CommandsModel.setCreateItemCommand(Maintable));
        public ICommand CreateTableOwnTypesCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateTableString(Maintable, CurrentScheme));
        public ICommand CreateTableDBTypesCommand => new RelayCommand(() => QueryTXT = dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.CreateTableCommand(LastSelectQuerytxt, CreateTableEXP));     
        public ICommand CreatealterTableCommand => new RelayCommand(() => QueryTXT = CommandsModel.AltertableString);
        public ICommand CreateDropTableCommand => new RelayCommand(() => QueryTXT = CommandsModel.DropTableString);
        public ICommand CreateBackupDatabaseCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateDataBaseBackupString);
        public ICommand CreateDatabaseCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateDatabaseString);
        public ICommand CreateViewCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateViewString);
        public ICommand CreateIndexCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateIndexString);
        public ICommand CreateCaseCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateCaseString);
        public ICommand CreateUpdateCommand => new RelayCommand(() => QueryTXT = CommandsModel.CreateUpdateString);
        public ICommand CreateSelectCommand => new RelayCommand(() => QueryTXT = CommandsModel.selectString);
        public ICommand ExportToPdfCommand => new RelayCommand(() => exportGrid.ExportToPDF(Maintable));
        public ICommand ExportToCsvCommand => new RelayCommand(() => exportGrid.exportToCSV(Maintable));
        public ICommand ExportTotxtommand => new RelayCommand(() => exportGrid.exportToTXT(Maintable));
        public ICommand ExportToxmlCommand => new RelayCommand(() => exportGrid.ExportToXML(Maintable));
        public ICommand ShowDataTableCellInfCommand => new RelayCommand(ShowCellIntel);
        public ICommand ShowDataTableRowInfCommand => new RelayCommand(ShowRowIntel);
        public ICommand AddScriptNoteTabCommand => new RelayCommand(AddMainSqlScriptField);
        public ICommand RemoveScriptNoteTabCommand => new RelayCommand(RemoveSelectedtab);
        public ICommand ReplaceWordsColumnRowsCommand => new RelayCommand(ReplaceWordsForColumnRowsCell);
        public ICommand ReplaceWordsAllTableCommand => new RelayCommand(ReplaceWordsForTable);
        public ICommand RecordCancelCommand => new RelayCommand(CancelFetchOfRecords);
        public ICommand ShowHistoryCommand => new RelayCommand(ShowDatabaseQueryHistoric);
        public ICommand MoveToMainGridCommand => new RelayCommand(MoveToMain);
        public ICommand ExportTableToExternalDatabaseCommand => new RelayCommand(ExportTableToExternalDatabase);
        public ICommand ReplaceWordsForCommandFieldCommand => new RelayCommand(ReplaceWordsForCommandField);
        public ICommand ClearCommandFieldCommand => new RelayCommand(ClearCommandField);
        public ICommand RenameTabHeaderCommand => new RelayCommand(RenameTabHeader);
        public ICommand GetLastKeyUpCommand => new RelayCommand(getQueryKeyUp);
        public ICommand GetLastKeyDownCommand => new RelayCommand(getQueryKeyDown);

        //-- Services
        private IDataService dataservice;
        private IViewService viewService;

        public NewQueryTabItemViewModel(IDataService dataservice, IViewService viewService)
        {
            this.viewService = viewService;
            this.dataservice = dataservice;
            CurrentScheme = "None";
            TabItems.Add(new NewTabItemModel { Header = "ScriptNote", Content = new NewScriptNoteView() });
            FetchRowCount = 100;
            SelectedMarkedText = "";
            CommandNumbIndex = 1;
            //-- Start default db
            CurrentDBName = "None";
            //-- Reigstrere at den skal sende Forbindelses typen til NewScriptNoteViewModel - SEND
            MessengerInstance.Register<NotificationMessageAction<string>>(this, ReturnAskForActiveConnection);
            //-- Modtager database navnet fra DatabaseConnectionTreeViewModel, ved valg af - Set connection globally
            MessengerInstance.Register<string>(this, (connection) => { CurrentDBName = connection; });
        }

        /// <summary>
        /// Resetter command feltet
        /// </summary>
        public void ClearCommandField()
        {
            QueryTXT = "";
        }

        /// <summary>
        /// Den aktuelle Database man er forbundet til på sin fane
        /// </summary>
        public string CurrentDBName
        {
            get => _newQueryModel._currentDBName;
            set
            {
                Set(ref _newQueryModel._currentDBName, value);

            }
        }

        /// <summary>
        /// Sender den aktive string forbindelse ud til NewSCriptNoteViewModel -- SEND
        /// </summary>
        private void ReturnAskForActiveConnection(NotificationMessageAction<string> obj)
        {
            obj.Execute(CurrentDBName);
        }


        /// <summary>
        /// Connection liste fra sidepanelet, som bruges til exportering til extern database
        /// </summary>
        public ObservableCollection<string> ConnectionList => dataservice.ConnectionListNames;


        /// <summary>
        /// Binder det nye navn til oprettelsen af tabellen
        /// Bruges til EXportTableToExeternDatabase
        /// </summary>
        public string CreateTableEXP
        {
            get => _newQueryModel._createTableEXP;
            set => Set(ref _newQueryModel._createTableEXP, value);
        }

        /// <summary>
        /// Binder det nye navn til oprettelsen af tabellen
        /// Bruges til EXportTableToExeternDatabase
        /// </summary>
        public string SelectedDatabaseText
        {
            get => _newQueryModel._selectedDatabaseText;
            set => Set(ref _newQueryModel._selectedDatabaseText, value);
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
        /// Tabel for exportering af tabel - test
        /// </summary>
        public  DataTable ExportLabelFields
        {
            get => _newQueryModel._exportLabelFields;
            set => Set(ref _newQueryModel._exportLabelFields, value);
        }

        /// <summary>
        /// Viser et nyt pop up vindue som vil vise en liste med connectionlist samt OK til exportering
        /// Ekstra guide, til bedre formatering: https://stackoverflow.com/questions/1348712/creating-a-sql-server-table-from-a-c-sharp-datatable 
        /// https://stackoverflow.com/questions/13854542/how-to-insert-data-from-datatable-to-oracle-database-table
        /// </summary>
        public void ExportTableToExternalDatabase()
        {
            //-- resetter tabellen
            ExportLabelFields = new DataTable();
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"{Progress} out of {FetchRowCount}";
            });
            //-- Alle kolonnerne
            foreach (DataColumn column in Maintable.Columns)
            {
                ExportLabelFields.Columns.Add(column.ColumnName);
            }      
            //-- Det nye popup vindue
            var exp = new ExportToExternalDatabaseWindowNewQuery();

            //-- Når brugeren har sagt OK, export griddet til en ny database
            //-- SelectedMarkedText = databasenavn
            exp.DataContext = this;
            if (exp.ShowDialog() == true)
            {
                try
                {
                    //-- Hvis det er den samme type database, som man vil exportere til. Så lav en intern create table.
                    //-- Ellers export via. ExpertToExternalDatabase()
                    dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.ExportToExternalDatabase(Maintable, CreateTableEXP, SelectedDatabaseText.Equals(CurrentDBName), CurrentScheme);
                    //-- log
                    TableAddons.writeLogFile($"Exportet {CurrentScheme}'s as {CreateTableEXP} to {dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.Name}", dataservice.LogLocation);
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- Besked
                    MessageBox.Show($"Success fully exportet {CurrentScheme}'s rows to {SelectedDatabaseText} in {CreateTableEXP}", "DataMergeEditor - Export table message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception e)
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    //-- log
                    TableAddons.writeLogFile($"Failed exporting {CurrentScheme}'s as {CreateTableEXP} to {dataservice.ConnectionList.FirstOrDefault(x => x.Key == SelectedDatabaseText).Value.Name}", dataservice.LogLocation);
                    //-- besked
                    MessageBox.Show($"Failed exporting {CurrentScheme} table to {SelectedDatabaseText}"
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
        /// erstat alle (hele ord) for command feltet
        /// nyt ord: NewCellValueTextString
        /// ord at erstatte: CellValueTextString
        /// </summary>
        public void ReplaceWordsForCommandField()
        {
            //-- Laver vindue instansen så den bruger DENNE aktuelle åbne fane's viewmodel, og bruger dataen hertil.
            var window = new ReplaceWordsInCommandNewQueryWindow();
            window.DataContext = this;

            //ReplaceWordsInColumnWindow ReplaceWordsInColumnWindow = new ReplaceWordsInColumnWindow();
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(QueryTXT))
            {
                //-- Tjekker om værdien overhovedet findes og sætter den som en bool
                bool contains = QueryTXT.Contains(CellValueTextString);
                //-- Verificere ovenstående
                if (contains != false)
                {
                    QueryTXT = QueryTXT.Replace(CellValueTextString, NewCellValueTextString);
                    TableAddons.writeLogFile($"Replaced words in command field for value {CellValueTextString} with {NewCellValueTextString}", dataservice.LogLocation);
                }
                else
                {
                    MessageBox.Show($"{CellValueTextString} does not exist in your command field",
                        "DataMergeEditor - Replace word message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Command field cannot be empty",
                    "DataMergeEditor - Replace word message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// Til at gemme udførte kommandoer i dropdown menuen
        /// </summary>
        public ObservableCollection<string> HistoryCommands
        {
            get => _newQueryModel._historyCommandsList;
            set => Set(ref _newQueryModel._historyCommandsList, value);
        }

        /// <summary>
        /// Tab items for scriptnote
        /// </summary>
        public ObservableCollection<NewTabItemModel> TabItems
        {
            get => _newQueryModel._tabItems;
            set => Set(ref _newQueryModel._tabItems, value);
        }

        //------------------------- Indhold fra ITableContent - STANDARD PR. Tabel
        /// <summary>
        /// HovedTabellen
        /// </summary>
        public DataTable Maintable
        {
            get => _newQueryModel.Maintable;
            set
            {
                Set(ref _newQueryModel.Maintable, value);
                FilteredTable = Maintable;
            }
        }

        /// <summary>
        ///  Sætter værdien for query feltet  
        /// </summary>        
        public string QueryTXT
        {
            get => _newQueryModel._queryCommand;
            set => Set(ref _newQueryModel._queryCommand, value);

        }

        /// <summary>
        /// Sætter værdien for ActionProgressBar
        /// Ved handling udførelse, sættes værdien til 100, og den er fyldt (grøn)
        /// Ved forkert handling, sættes den til 0 først, og derefter 100 rød;
        /// </summary>
        public int Progress
        {
            get => _newQueryModel._progressFill;
            set => Set(ref _newQueryModel._progressFill, value);
        }


        /// <summary>
        /// Cancellation token source test. Til at stoppe de metoder som kræver en token til async brug
        /// </summary>
        public void CancelFetchOfRecords()
        {
            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"{Progress} out of {FetchRowCount}";
            });
            //-- Stopper hentede rækker
            TableCancellationTokenSource.Cancel();
            TableCancellationTokenSource.Dispose();
            TableCancellationTokenSource = new CancellationTokenSource();
            //-- Progressbar
            PBarColorBrush = new SolidColorBrush(Colors.Yellow);
            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
            ProgressBarTokenSource = new CancellationTokenSource();
        }


        /// <summary>
        /// Sender Tabellen vi ser på, til hovedeTabellen
        /// </summary>
        public void MoveToMain()
        {
            //-- SEND
            Messenger.Default.Send<DataTable, MainTableViewModel>(Maintable);
        }


        /// <summary>
        /// Ændringer retted i griddet hos brugeren, blive automatisk rettet i samme tabel de lavede en select fra.
        /// </summary>
        public void ApplyChanges()
        {
            FetchRowCount = 100;
            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);

            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"{Progress} out of {FetchRowCount}";
            });

            if (string.IsNullOrWhiteSpace(CurrentDBName) || CurrentDBName.ToLower().Equals("none"))
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                ProgressBarTokenSource = new CancellationTokenSource();
                MessageBox.Show("Missing connection to database",
                    "DataMergeEditor - Apply changes message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else if (Maintable.Rows.Count != 0)
            {
                try
                {
                    dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.ApplyChanges(Maintable, LastSelectQuerytxt, dataservice.AskOnAcceptChanges, progress);
                    TableAddons.writeLogFile($"Applied chances directly to {CurrentDBName} for table {CurrentScheme}", dataservice.LogLocation);
                }
                catch (Exception e)
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    ProgressBarTokenSource = new CancellationTokenSource();
                    MessageBox.Show("Content fields and applied content must match"
                        + Environment.NewLine + Environment.NewLine +
                        "The error sounded like: " +
                        e.ToString().Substring(0, 250),
                        "DataMergeEditor - Apply changes message",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Tabel cannot have empty rows",
                    "DataMergeEditor - Apply changes message",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// Sætter en datatabel via. en task<datatabel>
        /// Returnere tabellen hvis forbindelsen er gyldig.
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> RunSqlQuery(string Query)
        {
            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
            var returntable = new DataTable();

            //-- Alternativ progressbar opdatering
            var progress = new Progress<int>(i =>
            {
                Progress = i;
                txtchange = $"{Progress} out of {FetchRowCount}";
            });      

            //-- Logger det både for brugeren, og i selve filen lokalt
            HistoryCommands.Add(Query);
            TableAddons.writeLogFile(Query, dataservice.LogLocation);

            //-- Hvis SelectedMarkedtext har indhold efter propertychanged har opdateret den ved markering, 
            //-- Så skal den ikke fjerne indholdet i kommand feltet.
            if (!string.IsNullOrEmpty(SelectedMarkedText))
            {
                // do nothing
            }
            else
            {
                QueryTXT = "";
            }

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
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.SaveToDataBase(Query, progress);
                            //-- Spørger på om brugeren har ændret indstillingerne for at blive spurgt på insert statements
                            if(dataservice.AskOnInsert != false)
                            {
                                if (MessageBox.Show("Do yu wish to review your table?", "Data Merge Editor - review table message",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                {
                                    CurrentScheme = Query.Substring(Query.ToLower().IndexOf("into")).Split(' ')[1];
                                    returntable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute("select * from " + TableAddons.getBetween(Query, "into", "("), progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, dataservice.RowLimiter);

                                    //-- Async - Spørger om den er blevet annulleret
                                    if (!TableCancellationTokenSource.IsCancellationRequested)
                                    {
                                        TableCancellationTokenSource.Dispose();
                                    }
                                    //-- fornyer token
                                    TableCancellationTokenSource = new CancellationTokenSource();
                                    //-- returnere tabellen
                                    return returntable;
                                }
                                else
                                {
                                    //-- Progressbar
                                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                    //-- resetter token sourcen
                                    ProgressBarTokenSource = new CancellationTokenSource();
                                    return Maintable;
                                }
                            }
                            else
                            {
                                CurrentScheme = Query.Substring(Query.ToLower().IndexOf("into")).Split(' ')[1];
                                returntable = await dataservice.ConnectionList.FirstOrDefault(x 
                                    => x.Key == CurrentDBName).Value.Execute("select * from " + 
                                    TableAddons.getBetween(Query, "into", "("), progress, TableCancellationTokenSource.Token, 
                                    TableCancellationTokenSource, dataservice.RowLimiter);
                                //-- async - spørger på om den er annulleret
                                if (!TableCancellationTokenSource.IsCancellationRequested)
                                {
                                    TableCancellationTokenSource.Dispose();
                                }
                                //-- fornyer token
                                TableCancellationTokenSource = new CancellationTokenSource();
                                //-- returnere tabellen
                                return returntable;
                            }             
                        }
                        else if (Query.ToLower().StartsWith("update"))
                        {
                            //-- køre query mod den aktive database
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.SaveToDataBase(Query, progress);
                            //-- Spørger om brugeren har lavet ændringer til advarselerne
                            if(dataservice.AskOnInsert != false)
                            {
                                if (MessageBox.Show("Do yu wish to review your table?", "Data Merge Editor - review table message",
                                   MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                {
                                    CurrentScheme = Query.Substring(Query.ToLower().IndexOf("update")).Split(' ')[1];
                                    returntable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute("select * from " + Query.Substring(Query.ToLower().IndexOf("update")).Split(' ')[1], progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, dataservice.RowLimiter);
                                    //-- async - spørger på om den er annulleret
                                    if (!TableCancellationTokenSource.IsCancellationRequested)
                                    {
                                        TableCancellationTokenSource.Dispose();
                                    }
                                    //-- fornyer token
                                    TableCancellationTokenSource = new CancellationTokenSource();
                                    //-- returnere tabellen
                                    return returntable;
                                }
                                else
                                {
                                    //-- Progressbar
                                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                                    //-- resetter token sourcen
                                    ProgressBarTokenSource = new CancellationTokenSource();
                                    return Maintable;
                                }
                            }
                            else
                            {
                                CurrentScheme = Query.Substring(Query.ToLower().IndexOf("update")).Split(' ')[1];
                                returntable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute("select * from " + Query.Substring(Query.ToLower().IndexOf("update")).Split(' ')[1], progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, dataservice.RowLimiter);
                                //-- spørger på token
                                if (!TableCancellationTokenSource.IsCancellationRequested)
                                {
                                    TableCancellationTokenSource.Dispose();
                                }
                                //-- fornyer token
                                TableCancellationTokenSource = new CancellationTokenSource();
                                //-- returnere tabellen
                                return returntable;
                            }         
                        }
                        else if (Query.ToLower().StartsWith("create table"))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.AddToDatabase(Query, progress);
                            CurrentScheme = Query.Substring(Query.ToLower().IndexOf("table")).Split(' ')[1];
                            MessageBox.Show($"{CurrentScheme} table successfully created", 
                                "DataMergeEditor - table creation", 
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            //-- resetter token sourcen
                            ProgressBarTokenSource = new CancellationTokenSource();
                            //-- Returnere tabellen
                            return returntable;
                        }
                        else if (Query.ToLower().StartsWith("drop table"))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.RemoveFromDatabase(Query, progress, dataservice.AskOnDrop);
                            return Maintable;
                        }
                        else if (Query.ToLower().StartsWith("delete"))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.DeleteFromDatabase(Query, progress, dataservice.AskOnDrop);
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            //-- resetter token sourcen
                            ProgressBarTokenSource = new CancellationTokenSource();
                            return Maintable;
                        }
                        else if (Query.ToLower().StartsWith("select"))
                        {
                            LastSelectQuerytxt = Query;
                            FetchRowCount = dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.GetTableRowCount(Query);
                            CurrentScheme = Query.Substring(Query.ToLower().IndexOf("from")).Split(' ')[1];
                            returntable = await dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Execute(Query, progress, TableCancellationTokenSource.Token, TableCancellationTokenSource, dataservice.RowLimiter);
                            //-- async - spørger om token er annulleret 
                            if(!TableCancellationTokenSource.IsCancellationRequested)
                            {
                                TableCancellationTokenSource.Dispose();
                            }
                            //-- fornyer token
                            TableCancellationTokenSource = new CancellationTokenSource();
                            //-- returnere tabellen
                            return returntable;
                        }
                        else if (Query.ToLower().StartsWith("disconnect") && !string.IsNullOrEmpty(CurrentDBName))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Disconnect(CurrentDBName);
                            CurrentDBName = null;
                            CurrentScheme = "None";
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            //-- resetter token sourcen
                            ProgressBarTokenSource = new CancellationTokenSource();
                            return new DataTable();
                        }
                        else if (Query.ToLower().Equals("reconnect") && !string.IsNullOrEmpty(CurrentDBName))
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.Reconnect(CurrentDBName);
                            //-- Progressbar
                            PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                            TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                            //-- resetter token sourcen
                            ProgressBarTokenSource = new CancellationTokenSource();
                            return Maintable;
                        }
                        else
                        {
                            dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.UnknownReadCreateToDatabaes(Query);
                            return Maintable;
                        }
                    }
                    catch (Exception e)
                    {
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        //-- resetter token sourcen
                        ProgressBarTokenSource = new CancellationTokenSource();
                        MessageBox.Show("ERROR: Your query must match the databaes content, an active connection and be valid in order to execute."
                            + Environment.NewLine 
                            + Environment.NewLine
                            +"The system error sounded like: " 
                            + e.ToString().Substring(0,250),
                            "DataMergeEditor - Database connection",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return Maintable;
                    }            
                }
                else if (Query.ToLower().StartsWith("reset"))
                {
                    return new DataTable();
                }
                else if (Query.ToLower().Equals("help"))
                {
                    MessageBox.Show($"You are currently connected to {CurrentDBName}"
                      + Environment.NewLine
                      + Environment.NewLine
                      + "All statements are allowed. Including following: "
                      + Environment.NewLine
                       + "Change database to xxx"
                      + Environment.NewLine
                      + "Disconnect"
                      + Environment.NewLine
                      + "Reconnect"
                      + Environment.NewLine
                      + "Reset"
                      + Environment.NewLine
                      + "Help"
                      , "DataMergeEditor - help message",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
                    return Maintable;
                }
                else if (Query.ToLower().Trim().StartsWith("change database to"))
                {
                    if (dataservice.ConnectionList.ContainsKey(Query.Substring(Query.IndexOf("to")).Split(' ')[1]))
                    {
                        CurrentDBName = Query.Substring(Query.IndexOf("to")).Split(' ')[1];
                        MessageBox.Show($"Changed database connection to {CurrentDBName}", 
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        TableAddons.writeLogFile($"Successfully changed database to {CurrentDBName}", dataservice.LogLocation);
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        //-- resetter token sourcen
                        ProgressBarTokenSource = new CancellationTokenSource();
                        return Maintable;
                    }
                    else
                    {
                        TableAddons.writeLogFile($"Failed to change database to {CurrentDBName}", dataservice.LogLocation);
                        //-- Progressbar
                        PBarColorBrush = new SolidColorBrush(Colors.Red);
                        TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                        //-- resetter token sourcen
                        ProgressBarTokenSource = new CancellationTokenSource();
                        MessageBox.Show("Invalid database choosen",
                            "DataMergeEditor - Change database message",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return Maintable;
                    }
                }
                else
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    //-- resetter token sourcen
                    ProgressBarTokenSource = new CancellationTokenSource();
                    MessageBox.Show("Please choose an valid database connection" 
                        + Environment.NewLine 
                        + Environment.NewLine 
                        + "Command: Change database to xxx",
                        "DataMergeEditor - Database connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                    return Maintable;
                }                        
            }
            else
            {
                //-- Progressbar
                PBarColorBrush = new SolidColorBrush(Colors.Red);
                TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                //-- resetter token sourcen
                ProgressBarTokenSource = new CancellationTokenSource();
                MessageBox.Show("Query cannot be empty", "DataMergeEditor - Database connection",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                return Maintable;
            }
        }

        //-----------------------------------------------------

        /// <summary>
        /// køre alle sql scripts
        /// </summary>
        public void RunSQLScripts()
        {
            var progress = new Progress<int>(i => Progress = i);
            var Scriptcount = 0;

            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = true
            };

            if (file.ShowDialog() == true)
            {

                if (file.FileName.Contains(".sql"))
                {
                    foreach (var filenameren in file.FileNames)
                    {
                        FileInfo sqlfil = new FileInfo(filenameren);
                        string scriptet = sqlfil.OpenText().ReadToEnd();
                        //-- udføre mod databasen
                        dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.UnknownReadCreateToDatabaes(scriptet);
                        //-- logger scriptet kørt
                        TableAddons.writeLogFile("Executed script:" + scriptet, dataservice.LogLocation);
                        //-- åbner / lukker scriptet
                        sqlfil.OpenText().Close();
                        Scriptcount++;
                    }
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.LightGreen);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    //-- resetter token sourcen
                    ProgressBarTokenSource = new CancellationTokenSource();
                    MessageBox.Show("Successfully executed " 
                        + Scriptcount 
                        + " script files",
                        "Data Merge Editor - Executing scripts",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
                }
                else
                {
                    //-- Progressbar
                    PBarColorBrush = new SolidColorBrush(Colors.Red);
                    TableAddons.setProgressBar(progress, ProgressBarTokenSource, ProgressBarTokenSource.Token);
                    //-- resetter token sourcen
                    ProgressBarTokenSource = new CancellationTokenSource();
                    MessageBox.Show("Only files with .sql extenstion can be added",
                        "Data Merge Editor - Executing scripts",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information);
                }
            }
        }


        //---------------------- Til at vise Indholdet af valgte celle via. ShowCellContentWindow Start
        /// <summary>
        /// Til at opdatere ShowCellContentWindow indholdet.
        /// </summary>
        public string CellValueTextString
        {
            get => _newQueryModel._cellValueTextString;
            set => Set(ref _newQueryModel._cellValueTextString, value);
        }

        /// <summary>
        /// Vælger den markeret  tabel index i tabellen og for dens oplysninger.
        /// </summary>
        public DataGridCellInfo TableCellisSelected
        {
            get => _newQueryModel._tableCellisSelected;
            set => Set(ref _newQueryModel._tableCellisSelected, value);
        }

        /// <summary>
        /// Til at opdatere ShowRowContentWindow indholdet.
        /// Til at få rækkens indhold
        /// </summary>
        public DataRowView selectedRowItem
        {
            get => _newQueryModel._selectedRowItem;
            set => Set(ref _newQueryModel._selectedRowItem, value);
        }

        //---------------------- Til at vise Indholdet af valgte celle via. ShowCellContentWindow Slut
        /// <summary>
        /// Til at opdatere ShowCellContentWindow indholdet.
        /// </summary>
        public string NewCellValueTextString
        {
            get => _newQueryModel._newCellValueTextString;
            set => Set(ref _newQueryModel._newCellValueTextString, value);
        }


        /// <summary>
        /// erstat alle bogstaver for den valgte kolonne, og dens rækker
        /// nyt ord: NewCellValueTextString
        /// ord til at finde: CellValueTextString
        /// datacelle information: TableCellisSelected
        /// hoved tabellen: Maintable
        /// </summary>
        public void ReplaceWordsForColumnRowsCell()
        {
            //-- Laver vindue instansen så den bruger DENNE aktuelle åbne fane's viewmodel, og bruger dataen hertil.
            var window = new ReplaceWordsInCommandNewQueryWindow();
            window.DataContext = this;        

            if (window.ShowDialog() == true && !Maintable.Rows.Count.Equals(0))
            {
                try
                {
                    foreach (DataRow row in Maintable.Rows)
                    {
                        var rowcellvalue = row[Maintable.Columns[TableCellisSelected.Column.DisplayIndex].ColumnName].ToString();
                        if (rowcellvalue.Contains(CellValueTextString))
                        {
                            row[Maintable.Columns[TableCellisSelected.Column.DisplayIndex].ColumnName] = rowcellvalue.ToString().Replace(CellValueTextString, NewCellValueTextString);
                        }
                    }
                    //-- logger scriptet kørt
                    TableAddons.writeLogFile($"Replaced row value {CellValueTextString} with {NewCellValueTextString} for column {Maintable.Columns[TableCellisSelected.Column.DisplayIndex].ColumnName}", dataservice.LogLocation);
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
               MessageBox.Show($"Table content cannot be empty", 
                   "DataMergeEditor - Replace word message", 
                   MessageBoxButton.OK, 
                   MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// erstat alle (hele ord) for hele tabellen
        /// nyt ord: NewCellValueTextString
        /// ord til at finde: CellValueTextString
        /// datacelle information: TableCellisSelected
        /// hoved tabellen: Maintable
        /// </summary>
        public void ReplaceWordsForTable()
        {
            //-- Laver vindue instansen så den bruger DENNE aktuelle åbne fane's viewmodel, og bruger dataen hertil.
            var window = new ReplaceWordsInCommandNewQueryWindow();
            window.DataContext = this;

            if (window.ShowDialog() == true && !Maintable.Rows.Count.Equals(0))
            {

                foreach (DataColumn colmn in Maintable.Columns)
                {
                    for (int i = 0; i < Maintable.Rows.Count; i++)
                    {
                        Maintable.Rows[i][colmn] = Maintable.Rows[i][colmn].ToString().Replace(CellValueTextString, NewCellValueTextString);
                    }
                }
                TableAddons.writeLogFile($"Replaced command text value {CellValueTextString} with {NewCellValueTextString}", dataservice.LogLocation);
            }
        }

        /// <summary>
        /// Vis datatabel celle indhold
        /// </summary>
        public void ShowCellIntel()
        {
            //-- setter værdien for CellValueTextString
            CellValueTextString = TableAddons.setCellValueText(TableCellisSelected);
            //-- opens new window and displays the data
            viewService.CreateWindowWithDataContext(new ShowCellContentWindow(), this);
        }

        /// <summary>
        /// Vis valgte række informationer
        /// </summary>
        public void ShowRowIntel()
        {
            var position = 0;
            var CleanRowIntelList = new ObservableCollection<string>();
            foreach (var item in selectedRowItem.Row.ItemArray)
            {
                //-- Fjerner alle irrelevante spaces, whitespaces, tomme og null felter før listen vises
                if (!string.IsNullOrWhiteSpace(item.ToString()) || !string.IsNullOrEmpty(item.ToString()))
                {
                    CleanRowIntelList.Add(Maintable.Columns[position] + ": " + item.ToString());
                }
                position++;
            }
            //-- setter værdien for CellValueTextString
            CellValueTextString = string.Join(System.Environment.NewLine, CleanRowIntelList);
            //-- opens new window and displays the data
            viewService.CreateWindowWithDataContext(new ShowCellContentWindow(), this);
        }

        /// <summary>
        /// Tæller nummeringen af Tabben
        /// </summary>
        public int xScriptnoteQueryIndex
        {
            get => _newQueryModel._xScriptnoteQueryIndex;
            set => Set(ref _newQueryModel._xScriptnoteQueryIndex, value);
        }

        /// <summary>
        /// Selve valgte tab til tabcontrollen
        /// </summary>
        public int SelectedScriptNoteIndex
        {
            get => _newQueryModel._selectedScriptNoteIndex;
            set => Set(ref _newQueryModel._selectedScriptNoteIndex, value);
        }

        /// <summary>
        /// Tildeler det nye scriptnote fane navn
        /// </summary>
        public string NewScriptNoteName
        {
            get => _newQueryModel._newScriptNoteName;
            set => Set(ref _newQueryModel._newScriptNoteName, value);
        }


        /// <summary>
        /// Omdøber scriptnote fanernes navne
        /// </summary>
        public void RenameTabHeader()
        {
            var RenameTabHeaderNewQueryWindow = new RenameTabHeaderNewQueryWindow();
            RenameTabHeaderNewQueryWindow.DataContext = this;

            if (RenameTabHeaderNewQueryWindow.ShowDialog() == true)
            {
                TabItems[SelectedScriptNoteIndex].Header = NewScriptNoteName;
            }
        }

        /// <summary>
        /// Fjerner Tab for ScriptNote fanerne, baseret på selectedIndex
        /// </summary>
        public void RemoveSelectedtab()
        {
            if (!TabItems.Count.Equals(1) && SelectedScriptNoteIndex != 0)
            {
                TabItems.RemoveAt(SelectedScriptNoteIndex);
                xScriptnoteQueryIndex--;
            }
            else
            {
                MessageBox.Show("It's not possible to remove the default taps", 
                    "DataMergeEditor - Removing tabs", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// tilføjer ny kommand
        /// </summary>
        public void AddMainSqlScriptField()
        {
            xScriptnoteQueryIndex++;
            var header = $"ScriptNote {xScriptnoteQueryIndex}";
            var item = new NewTabItemModel { Header = header, Content = new NewScriptNoteView() };
            TabItems.Add(item);
        }


        /// <summary>
        /// Tæller antal rows når der laves en select
        /// Bruges til FetchingNewQueryRecordsWindow
        /// </summary>
        public int FetchRowCount
        {
            get => _newQueryModel._fetchRowCount;
            set => Set(ref _newQueryModel._fetchRowCount, value);
        }

        /// <summary>
        /// For nuværende database schema text
        /// </summary>
        public string CurrentScheme
        {
            get => _newQueryModel._currentScheme;
            set => Set(ref _newQueryModel._currentScheme, value);
        }

        /// <summary>
        /// action progressBar text
        /// </summary>
        public string txtchange
        {
            get => _newQueryModel._txtchange;
            set => Set(ref _newQueryModel._txtchange, value);
        }

        /// <summary>
        /// action progressBarFarve
        /// </summary>
        public SolidColorBrush PBarColorBrush
        {
            get => _newQueryModel._pBarColorBrush;
            set => Set(ref _newQueryModel._pBarColorBrush, value);
        }

        /// <summary>
        /// Til  record count for at stoppe den
        /// </summary>
        public bool RecordCancelProgressFill
        {
            get => _newQueryModel._recordCancelProgressFill;
            set => Set(ref _newQueryModel._recordCancelProgressFill, value);
        }

        /// <summary>
        /// Er total record som er fået.
        /// </summary>
        public int RecordCountingINT
        {
            get => _newQueryModel._recordCountingINT;
            set => Set(ref _newQueryModel._recordCountingINT, value);
        }

 
        
        //--- Autocomplete textbox - SKAL VÆRE LIGESOM MS STUDIOS.
        //-- guide: https://github.com/Nimgoble/WPFTextBoxAutoComplete/
        //-- https://www.codeproject.com/Articles/44920/A-Reusable-WPF-Autocomplete-TextBox 
        public void AutoCompleteHelper()
        {
            //-- Skal vise alle kolonner hvis en tabel er fremvist
            //-- hvis ingen tabel er fremvist, skal den første liste være en tabel oversigt.
            //-- eks: select {Tabel anbefalet ord} af det start ord 
        }


        /// <summary>
        /// Viser historic loggen
        /// </summary>
        public void ShowDatabaseQueryHistoric()
        {
            dataservice.ShowDatabaseQueryHistoric();       
        }


        /// <summary>
        /// sætter sidste kørte select kommando til RunLastSelectQuery funktion (Shift + backspace)
        /// </summary>
        public string LastSelectQuerytxt
        {
            get => _newQueryModel._lastSelectQuerytxt;
            set => Set(ref _newQueryModel._lastSelectQuerytxt, value);
        }

        /// <summary>
        ///  Tager valgte (markeret) text.
        /// https://stackoverflow.com/questions/2245928/mvvm-and-the-textboxs-selectedtext-property 
        /// </summary>
        public string SelectedMarkedText
        {
            get => _newQueryModel._selectedMarkedText;
            set => Set(ref _newQueryModel._selectedMarkedText, value);
        }

        /// <summary>
        /// Sætter textboxen til det valgte item i listen
        /// </summary>
        public void setTextBoxFromSelectedList(string selectedString)
        {
            if (SelectedIndexOfComboBoxItem != null && SelectedIndexOfComboBoxItem != "")
            {
                QueryTXT = selectedString;
                //-- set fokus på textboxen her.
            }
        }


        /// <summary>
        /// Vælger det valgte fra comboboxen
        /// </summary>
        public string SelectedIndexOfComboBoxItem
        {
            get => _newQueryModel._selectedIndexOfComboBoxItem;
            set
            {
                Set(ref _newQueryModel._selectedIndexOfComboBoxItem, value);
                setTextBoxFromSelectedList(value);
            }
        }


        /// <summary>
        /// Til at modtage valgte søgnigs mulighed. række eller række celle
        /// </summary>
        public ComboBoxItem SelectedFilterSearch
        {
            get => _newQueryModel._selectedFilterSearch;
            set
            {
                Set(ref _newQueryModel._selectedFilterSearch, value);
            }
        }

        /// <summary>
        /// Ord til søgning i søgefeltet mod Filtrering af rækker til brugeren.
        /// </summary>
        public string SearchWord
        {
            get => _newQueryModel._searchWord;
            set
            {
                Set(ref _newQueryModel._searchWord, value);
                FilterDataGridSearchEngine(value);
            }
        }

        /// <summary>
        /// Som fremviser hovedtabellen (Maintable) til griddet
        /// bruges også til filtrering via. FilterDataGridSearchEngine
        /// </summary>
        public DataTable FilteredTable
        {
            get => _newQueryModel._filteredTable;
            set => Set(ref _newQueryModel._filteredTable, value);
        }

        /// <summary>
        /// Returnere tabellen med udvalgte rækker som indeholder SearchWord.
        /// </summary>
        /// <param name="word"></param>
        public void FilterDataGridSearchEngine(string word)
        {
            //-- Begrænser brugeren for søge valg via. dropdown i søge feltet.
            if(SelectedFilterSearch.Content.Equals("Row"))
            {
                    if (!string.IsNullOrEmpty(word) && !string.IsNullOrWhiteSpace(word) && !Maintable.Rows.Count.Equals(0))
                    {
                        FilteredTable = TableAddons.ReturnFilteredSearchedTable(word, Maintable);
                    }
                    else
                    {
                        FilteredTable = Maintable;
                    }                
                }
            else
            {
                //-- Do nothing
                //-- Only coloring the table
            }          
        }
    }
}
