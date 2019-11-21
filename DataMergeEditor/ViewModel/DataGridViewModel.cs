using DataMergeEditor.Model;
using DataMergeEditor.Services;
using DataMergeEditor.View.UserControls;
using DataMergeEditor.View.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

//-- Til at søge efter ord fra Tree-view listen: https://www.codeproject.com/Tips/1229482/WPF-TextBlock-Highlighter

namespace DataMergeEditor.ViewModel
{
    public class DataGridViewModel : ViewModelBase
    {
        private DataGridModel _dataGridModel = new DataGridModel();
        //-- Tab control
        public ObservableCollection<NewTabItemModel> TabItems
        {
            get => _dataGridModel._tabItems;
            set => Set(ref _dataGridModel._tabItems, value);           
        }

        public ICommand AddNewTabWindowCommand => new RelayCommand(CreateNewTabWindow);
        public ICommand RemoveSelectedTabCommand => new RelayCommand(RemoveSelectedtab);
        public ICommand CreateCompareTabCommand => new RelayCommand(CreatenewCompareWindow);
        public ICommand CreateRelationTabCommand => new RelayCommand(CreateNewRelationWindow);
        public ICommand RenameTabHeaderCommand => new RelayCommand(RenameTabHeader);


        /// <summary>
        /// Bruger dataservice, - Skal gøres for alle ViewModels
        /// Dataservice for at dele data mellem viewmodels
        /// </summary>
        private readonly IDataService _dataService;
        public DataGridViewModel(IDataService dataService)
        {
            _dataService = dataService;
            //-- Kan fange metoder etc. via. DataService klassen
            TabItems.Add(new NewTabItemModel { Header = "Main grid", Content = new MainTableViewTabItem () });
            TabItems.Add(new NewTabItemModel { Header = "Query pre-view", Content = new NewQueryTabItem() });
        }

        /// <summary>
        /// Tildeler det nye scriptnote fane navn
        /// </summary>
        public string NewTabNoteName
        {
            get => _dataGridModel._newTabNoteName;
            set => Set(ref _dataGridModel._newTabNoteName, value);
        }

        /// <summary>
        /// Omdøber scriptnote fanernes navne
        /// </summary>
        public void RenameTabHeader()
        {
            var TabRenaming = new RenameTabHeaderMainDataGridWindow();
            TabRenaming.DataContext = this;

            if (TabRenaming.ShowDialog() == true)
            {
                TabItems[SelectedTabIndex].Header = NewTabNoteName;
            }
        }

        /// <summary>
        /// Tildeler et nummer til et ekstra Query vindue fane
        /// </summary>
        public int xTabIndexesQuery
        {
            get => _dataGridModel._xTabIndexesQuery;
            set => Set(ref _dataGridModel._xTabIndexesQuery, value);
        }

        /// <summary>
        /// Tildeler et nummer til et ekstra Compare vindue fane
        /// </summary>
        public int xTabIndexesCompare
        {
            get => _dataGridModel._xTabIndexesCompare;
            set => Set(ref _dataGridModel._xTabIndexesCompare, value);
        }

        /// <summary>
        /// Selve valgte tab til tabcontrollen
        /// </summary>
        public int SelectedTabIndex
        {
            get => _dataGridModel._selectedTabIndex;
            set => Set(ref _dataGridModel._selectedTabIndex, value);
        }

        //-- Tilføje ekstra tabs
        //-- Laver en ny tab med indhold af en usercontrol
        //-- https://stackoverflow.com/questions/5650812/how-do-i-bind-a-tabcontrol-to-a-collection-of-viewmodels/5651542#5651542
        //-- https://www.codeproject.com/Articles/351300/MVVM-TabControl 
        //-- https://www.technical-recipes.com/2017/adding-tab-items-dynamically-in-wpf/
        public void CreateNewTabWindow()
        {
            xTabIndexesQuery++;
            var header = $"Query Window {xTabIndexesQuery}";
            var item = new NewTabItemModel { Header = header, Content = new NewQueryTabItem() };
            TabItems.Add(item);
        }

        /// <summary>
        /// Laver ny compare vindue
        /// </summary>
        public void CreatenewCompareWindow()
        {
            xTabIndexesCompare++;
            var header = $"Compare window {xTabIndexesCompare}";
            var item = new NewTabItemModel { Header = header, Content = new Compare_tabItem() };
            TabItems.Add(item);
        }

        /// <summary>
        /// Laver ny Relations vindue
        /// </summary>
        public void CreateNewRelationWindow()
        {
            var header = "Relations window";
            var item = new NewTabItemModel { Header = header, Content = new MainRelationsTabItem() };
            if(!TabItems.Contains(item))
            {
                TabItems.Add(item);
            }
            else
            {
                MessageBox.Show("Current version only supports 1 relation tab for main tab",
                    "DataMergeEditor - Relation tab message",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///  Fjerne valgte tab
        /// Fjerner sidste tab først
        /// </summary>
        public void RemoveSelectedtab()
        {
            if (!TabItems.Count.Equals(2) && SelectedTabIndex != 0 && SelectedTabIndex != 1)
            {
                TabItems.RemoveAt(SelectedTabIndex);
                xTabIndexesQuery--;
            }
            else
            {
                MessageBox.Show("It's not possible to remove the default taps",
                    "DataMergeEditor - Removing tabs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
