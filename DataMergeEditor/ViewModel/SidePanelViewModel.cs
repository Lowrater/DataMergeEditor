using Microsoft.Win32;
using DataMergeEditor.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using DataMergeEditor.View.Windows;
using System.Data;
using System.Windows;
using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight.Messaging;
using System.Threading;
using DataMergeEditor.DBConnect.Data.ListData;
using System.Windows.Controls;
using DataMergeEditor.Model.Addons;

namespace DataMergeEditor.ViewModel
{
    public class SidePanelViewModel : ViewModelBase
    {
        private SidePanelListModel SidePanelListModel = new SidePanelListModel();
        private GetTablesFromDBModel getTablesFromDBModel = new GetTablesFromDBModel();
        private TreeAddons treeAddons = new TreeAddons();
        private SettingsWindow settingsWindow = new SettingsWindow();
        private CancellationTokenSource TableCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Dataservice for at dele data mellem viewmodels
        /// </summary>
        public IDataService dataservice;

        //-- Alle commands for click events
        public ICommand AddFilesToObservableCollectionCommand => new RelayCommand(AddFilesToListBox);
        public ICommand StartSettingsWindowCommand => new RelayCommand(ShowSettingsWindow);
        public ICommand StartHelpWindowCommand => new RelayCommand(ShowHelpWindow);
        public ICommand RemoveFromListBoxCommand => new RelayCommand(RemoveFromList);
        public ICommand AddTablesFromDBCommand => new RelayCommand(() => ShowGetTablesFromDBWindow());
        public ICommand TransferTabelToListCommand => new RelayCommand(TransferTabelFromList);
        public ICommand AddTextBoxCommand => new RelayCommand(AddTextBox);
        public ICommand ClearTextBoxCommand => new RelayCommand(ClearTextBoxes);


        /// <summary>
        /// Hoved listen med alle Tabeller og filer, som bruges op mod hoved tabellen til merging
        /// </summary>
        public ObservableCollection<ContentList> MainListContainer
        {
            get => SidePanelListModel._mainListContainer;
            set
            {
                Set(ref SidePanelListModel._mainListContainer, value);
            }
        }

        /// <summary>
        /// Hovedliste til GetTablefromDBWindow som viser alle tabelnavne
        /// </summary>
        public ObservableCollection<ContentList> maintableContainer
        {
            get => getTablesFromDBModel._mainTableContainer;
            set
            {
                Set(ref getTablesFromDBModel._mainTableContainer, value);
                ItemsFilter = this.maintableContainer;
            }
        }

        /// <summary>
        /// Sætter værdien for den database man vælger i Data
        /// Derefter for alle tabllerne med værdien sat.
        /// </summary>
        public string CurrentDBName
        {
            get => SidePanelListModel._currentDB;
            set 
                {
                  Set(ref SidePanelListModel._currentDB, value);
                  GetTables(value);
                }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataService"></param>
        public SidePanelViewModel(IDataService dataService)
        {
            this.dataservice = dataService;
            //-- sætter remove knap til false
            RemoveIsEnabledBool = false;
            ActiveConnection = false;
            settingsWindow.Hide();
            //-- til at dele sidepanelets liste via. ReturnListCount messageing -- PULL
            //-- Her tager vi imod at få en Notification som anmoder om en liste
            //-- Denne liste er ReturnListCount, Som peger på MainListContainer
            MessengerInstance.Register<NotificationMessageAction<ObservableCollection<ContentList>>>(this, ReturnListCount);

            //-- Efterspørger beskeder fra DataBaseConnectioNTreeViewModel om man har tilføjet en forbindelse.
            MessengerInstance.Register<bool>(this, SetActiveConnection);
        }



        /// <summary>
        /// til at tillade andre udefrakommende at få fil/tabel isten   -- PULL
        /// Sender MainListContaineren retur på forspørgelsen
        /// </summary>
        /// <param name="obj"></param>
        private void ReturnListCount(NotificationMessageAction<ObservableCollection<ContentList>> obj)
        {
            obj.Execute(MainListContainer);
        }


        /// <summary>
        /// Viser hjælp vinduet
        /// </summary>
        public void ShowHelpWindow()
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        /// <summary>
        /// Har alle database navne i denne liste, som bindes til drop down menuen på GetTableFromDBWindow
        /// </summary>
        public ObservableCollection<string> ConnectionList
        {
            get => SidePanelListModel._connectionList;
            set
            {
                Set(ref SidePanelListModel._connectionList, value);
                //-- havde en messenger sendt af listen til Maintable før? Ukendt rettelse..
            }         
        }

        /// <summary>
        /// Resetter textboxene
        /// </summary>
        public void ClearTextBoxes()
        {
            TextBoxCollection.Clear();
            TextBoxCount = 0;
        }

        /// <summary>
        /// Til at tilføje flere textboxe
        /// </summary>
        public void AddTextBox()
        {
            TextBoxCount++;
            TextBoxCollection.Add(new TextBox());
            //-- Sender listen til MainTableViewModel til filtrering
            Messenger.Default.Send<ObservableCollection<TextBox>, MainTableViewModel>(TextBoxCollection);
        }

        /// <summary>
        /// Tælleren for textboxene når de tilføjes til filtrering
        /// </summary>
       public int TextBoxCount { get; set; }

        /// <summary>
        /// Indeholder alle textboxe som vises til filtrerings vinduet.
        /// </summary>
        public ObservableCollection<TextBox> TextBoxCollection
        {
            get => SidePanelListModel._textBoxCollection;
            set
            {
                Set(ref SidePanelListModel._textBoxCollection, value);
            }
        }

        /// <summary>
        /// Begrænser hvorvit du må trykke på 'tilføj tabel knappen'.
        /// Kræver at du har en aktiv forbindelse
        /// Se SetActiveConnection og Constructoren med messagesinstance.
        /// </summary>
        public bool ActiveConnection 
        {
            get => SidePanelListModel._activeConnection;
            set => Set(ref SidePanelListModel._activeConnection, value);
        }

        /// <summary>
        /// Tager imod bool hvorvidt man har tilføjet en forbindelse og sætter ActiveCOnnection bool op mod sidepanel viewmodel.
        /// </summary>
        /// <param name="AllowAddTables"></param>
        public void SetActiveConnection(bool AllowAddTables)
        {
            ActiveConnection = AllowAddTables;
        }


        /// <summary>
        /// Få Alle tabeller fra databasen via. conn forbindelsen 
        /// </summary>
        /// <param name="dbnavn"></param>
        public async void GetTables(string dbnavn)
        {
            var newContainer = new ObservableCollection<ContentList>();       
            var Fetchedtable =  await dataservice.ConnectionList.FirstOrDefault(x => x.Key == dbnavn).Value.FetchTableList(TableCancellationTokenSource.Token, TableCancellationTokenSource);
            //-- Async - token - efterspørger om den er annulleret af brugeren
            if(!TableCancellationTokenSource.IsCancellationRequested)
            {
                TableCancellationTokenSource.Dispose();
            }
            //--resetter token
            TableCancellationTokenSource = new CancellationTokenSource();
            ////-- Writes all the tables out in the list
            ////-- Takes the first row which contains the name of the table the database contains (or user has access too)
            foreach (var TableRowName in Fetchedtable.AsEnumerable().Select(s => s[0].ToString()))
            {
                var content = new ContentList { DatabaseName = dbnavn, Type = TableRowName };
                if (!MainListContainer.Contains(content))
                {
                    newContainer.Add(content);
                }
            }
            maintableContainer = new ObservableCollection<ContentList>(newContainer.OrderBy(p => p.Type));
        }

        /// <summary>
        /// Vælger den markeret index tabel i listboxen til flytning
        /// </summary>
        public ContentList TableisSelected
        {
            get => getTablesFromDBModel._tableisSelected;
            set => Set(ref getTablesFromDBModel._tableisSelected, value);
        }

        /// <summary>
        /// Vælger den markeret item tabel i listboxen til flytning (string)
        /// </summary>
        public ContentList TableItemSelected
        {
            get => getTablesFromDBModel._tableItemSelected;
            set => Set(ref getTablesFromDBModel._tableItemSelected, value);
        }

        /// <summary>
        /// Flyt database tabel til sidepanelets original liste
        /// </summary>
        public void TransferTabelFromList()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TableItemSelected.Type) 
                    && !MainListContainer.Contains(TableItemSelected))
                {
                    //-- Flytter tabellen over til sidepanelets liste
                    MainListContainer.Add(TableItemSelected);
                    //-- Fjerner tabellen fra listen i GetTablefromDBWindow
                    maintableContainer.Remove(TableisSelected);
                    //-- åbner muligheden at fjerne en tabellen igen.
                    RemoveIsEnabledBool = true;
                    //-- Sender besked til MainTableViewModel, at sidepanelets liste er opdateret.
                    //-- Derfor skal den opdatere MainGrid
                    Messenger.Default.Send<bool, MainTableViewModel>(true);
                }
                else if(MainListContainer.Contains(TableItemSelected))
                {
                    MessageBox.Show($"Main list already contains the selected Table",
                        "DataMergeEditor - Table transfering",
                     MessageBoxButton.OK,
                     MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Please select a valid table to transfer", 
                        "DataMergeEditor - Table transfering",
                   MessageBoxButton.OK,
                   MessageBoxImage.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Cannot transfer table to main list." 
                    + Environment.NewLine 
                    + Environment.NewLine 
                    + " Please verify your connectivity",
                    "DataMergeEditor - Table transfering",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Start settings vindue
        /// </summary>
        public void ShowSettingsWindow()
        {
            settingsWindow.Show();
        }

        /// <summary>
        /// starter GettableFromDBWindow 
        /// Henter alle database navne til dropdownen
        /// </summary>
        public void ShowGetTablesFromDBWindow()
        {
            foreach (var item in dataservice.ConnectionList.Keys)
            {
                if(!ConnectionList.Contains(item))
                {
                    ConnectionList.Add(item);
                }
            }
            GetTablesFromDBWindow GetTablesFromDBWindow = new GetTablesFromDBWindow();
            GetTablesFromDBWindow.Show();
        }

        /// <summary>
        /// Tilføj filer til Listboxen
        /// </summary>
        public void AddFilesToListBox()
        {
            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files(All Files(*.*)|*.*"
            };

            if (file.ShowDialog() == true)
            {
                foreach (var File in file.FileNames)
                {
                    TableAddons.writeLogFile($"{file.FileName} has been added to the sidepanel", dataservice.LogLocation);
                    var content = new ContentList { DatabaseName = "FILE", Type = File };
                    if (!MainListContainer.Contains(content))
                    {
                        MainListContainer.Add(content);
                        //-- Sender godkendelse til at der er lavet ændringer i listen, 
                        //-- så MainTableViewModel kan opdatere hovedtabellen dynamisk
                        Messenger.Default.Send<bool, MainTableViewModel>(true);
                    }
                    else
                    {
                        MessageBox.Show("One or more files contains duplicated names in list.",
                            "DataMergeEditor - Table transfering",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                }
                //-- åbner muligheden at fjerne en fil igen.
                RemoveIsEnabledBool = true;
            }
        }

        /// <summary>
        /// Bundet på 'remove' knappen om der er kommet indhold i sidepanelets liste. Via. fil og tabeller.
        /// </summary>
        public bool RemoveIsEnabledBool
        {
            get => SidePanelListModel._checkIfSideListHasContentBool;
            set => Set(ref SidePanelListModel._checkIfSideListHasContentBool, value);
        }

        /// <summary>
        /// Vælger den markeret fil i sidepanelets listboxen til fjernelse
        /// </summary>
        public int IsSelected
        {
            get => SidePanelListModel._isSelected;
            set => Set(ref SidePanelListModel._isSelected, value);
        }

        /// <summary>
        /// Fjerner noget fra listen
        /// </summary>
        public void RemoveFromList()
        {
            if (MainListContainer != null 
                && !MainListContainer.Count().Equals(0)
                && IsSelected >= 0)
            {
                MainListContainer.RemoveAt(IsSelected);
                //-- resetter index til 0 så man ikke kommer ud af index placering
                IsSelected = 0;
                //- spørger igen om listen indeholder noget efter fjernelse for at sikre der ikke fjernes noget som ikke findes.
                if (MainListContainer.Count().Equals(0))
                {
                    RemoveIsEnabledBool = false;
                }
                //-- Sender godkendelse til at der er lavet ændringer i listen, så MainTableViewModel kan opdatere hovedtabellen dynamisk
                Messenger.Default.Send<bool, MainTableViewModel>(true);
            }
        }


        /// <summary>
        /// Listen af alle forbindelser som default, grundet filtrering
        /// </summary>
        public ObservableCollection<ContentList> ItemsFilter
        {
            get => SidePanelListModel._itemsFilter;
            set => Set(ref SidePanelListModel._itemsFilter, value);
        }

        /// <summary>
        /// Ord som bruges til søgemaskinen for at fremsøge specifik tabel
        /// </summary>
        public string SearchedForTableWord
        {
            get => SidePanelListModel._searchedfortableword;
            set
            {
                Set(ref SidePanelListModel._searchedfortableword, value);
                //-- Giver Items listen med alle noderne ud, til at sætte på ny.
                FindTableByMatchedWord(value);
            }
        }

        /// <summary>
        /// Skal bruges til en yderligere søgemaskine for noderne
        /// Tager imod SearchedForTableWord som er bundet til søgemaskinen.
        /// </summary>
        /// <param name="searchtext"></param>
        public void FindTableByMatchedWord(string searchtext)
        {

            if (!string.IsNullOrEmpty(searchtext) && !string.IsNullOrWhiteSpace(searchtext))
            {
                ItemsFilter = treeAddons.ReturnFilteredTreeNode(searchtext, maintableContainer);
            }
            else if (string.IsNullOrEmpty(searchtext) && string.IsNullOrWhiteSpace(searchtext))
            {
                ItemsFilter = maintableContainer;
            }
        }

    }
}
