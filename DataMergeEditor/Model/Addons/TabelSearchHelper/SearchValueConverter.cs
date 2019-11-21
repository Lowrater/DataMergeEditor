using System;
using System.Windows.Data;
//-- guide: https://stackoverflow.com/questions/15467553/proper-datagrid-search-from-textbox-in-wpf-using-mvvm
//-- rettet linje 20 til 'Contains' i stedet for 'startwidth' for at give et bedre overblik
namespace DataMergeEditor.TabelSearchHelper
{
    public class SearchValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string cellText = values[0] == null ? string.Empty : values[0].ToString();
            string searchText = values[1] as string;

            if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(cellText))
            {
                
                if(searchText.StartsWith("=") && !searchText.Equals("="))
                {
                    //DET ER MARTIn
                    //-- =Martin
                    searchText = searchText.Substring(1);
                    return cellText.ToLower().Equals(searchText.ToLower());
                }
                else if(searchText.StartsWith("*") && !searchText.Equals("*"))
                {
                    // SLUTTER MED MARTIN
                    //-- *Martin
                    searchText = searchText.Substring(1);
                    return cellText.ToLower().EndsWith(searchText.ToLower());
                }
                else if(searchText.EndsWith("*") && !searchText.Equals("*"))
                {
                    // STARTER MED MARTIN
                    //-- Martin*
                    searchText = searchText.Remove(searchText.Length - 1);
                    return cellText.ToLower().StartsWith(searchText.ToLower());
                }
                else
                {
                    // INDEHOLDER MARTIN
                    //-- Martin
                    return cellText.ToLower().Contains(searchText.ToLower());
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
