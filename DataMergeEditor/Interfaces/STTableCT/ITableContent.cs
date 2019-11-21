using System.Data;
using System.Threading.Tasks;

namespace DataMergeEditor.Interfaces.STTableCT
{
    /// <summary>
    /// ITableContent indeholder alle de relevante ting som alle grids skal
    /// minimum have for at opfylde kravende for en virkende tabel
    /// </summary>
    interface ITableContent
    {
        DataTable Maintable { get; set; }
        string QueryTXT { get; set; }
        int Progress { get; set; }

        Task<DataTable> RunSqlQuery(string query);
    }
}
