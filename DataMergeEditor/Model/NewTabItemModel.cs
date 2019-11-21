using GalaSoft.MvvmLight;
using System.Windows.Controls;

namespace DataMergeEditor.Model
{
    /// <summary>
    /// ObservableObject skal haves til ObservableCollection, hvor den bruges, så den kan Opdatere til UI
    /// Bruges via. GallaSoft
    /// Header = Tab navnet
    /// Content = UserControllen man ønsker. Se View -> UserControls
    /// </summary>
    public class NewTabItemModel : ObservableObject
    {
        private string _header;
        public string Header
        {
            get => _header;
            set => Set(ref _header, value);
        }
        public UserControl Content { get; set; }     
    }
}
