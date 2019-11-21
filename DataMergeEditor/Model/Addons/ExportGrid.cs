using Microsoft.Win32;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Grid;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace DataMergeEditor.Model.Exports
{
    public class ExportGrid
    {
        /// <summary>
        /// Export to PDF
        /// </summary>
        /// <param name="datatable"></param>
        public void ExportToPDF(DataTable datatable)
        {
            if (datatable != null && datatable.Columns.Count != 0)
            {
                datatable.AcceptChanges();
                //Create a new PDF document.
                PdfDocument document = new PdfDocument();
                //Add a page.
                PdfPage page = document.Pages.Add();
                //Create a PdfGrid.
                PdfGrid pdfGrid = new PdfGrid();
                //Adds the datatable
                pdfGrid.DataSource = datatable;
                //--DrawThegrid
                pdfGrid.Draw(page, new PointF(10, 10));
                //-- Open dialog
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "PDF Files(*.pdf)|*.pdf"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (Stream stream = sfd.OpenFile())
                    {
                        document.Save(stream);
                    }

                    //Message box confirmation to view the created Pdf file.
                    if (MessageBox.Show("Do you want to view the Pdf file?", "DataMergeEditor - Exporting to pdf",
                                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        //Launching the Pdf file using the default Application.
                        System.Diagnostics.Process.Start(sfd.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Export to CSV
        /// </summary>
        /// <param name="datatable"></param>
        public void exportToCSV(DataTable datatable)
        {
            if (datatable != null && datatable.Columns.Count != 0)
            {
                datatable.AcceptChanges();

                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = datatable.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(";", columnNames));

                foreach (DataRow row in datatable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(";", fields));
                }

                //-- Open dialog
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV Files(.csv)|.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, sb.ToString());
                    //Message box confirmation to view the created Pdf file.
                    if (MessageBox.Show("Do you want to view the file?", "DataMergeEditor - Exporting to csv",
                                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(sfd.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// export to txt file
        /// </summary>
        /// <param name="datatable"></param>
        public void exportToTXT(DataTable datatable)
        {
            if (datatable != null && datatable.Columns.Count != 0)
            {
                datatable.AcceptChanges();
                //-- Open dialog
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "TXT Files(.txt)|.txt"
                };

                if (sfd.ShowDialog() == true)
                {
                    StreamWriter streamWriter = new StreamWriter(sfd.FileName, true);
                    int i = 0;

                    for (i = 0; i < datatable.Columns.Count - 1; i++)
                    {

                        streamWriter.Write(datatable.Columns[i].ColumnName + ";");

                    }
                    streamWriter.Write(datatable.Columns[i].ColumnName);
                    streamWriter.WriteLine();

                    foreach (DataRow row in datatable.Rows)
                    {
                        object[] array = row.ItemArray;

                        for (i = 0; i < array.Length - 1; i++)
                        {
                            streamWriter.Write(array[i].ToString() + ";");
                        }
                        streamWriter.Write(array[i].ToString());
                        streamWriter.WriteLine();
                    }

                    streamWriter.Close();

                    if (MessageBox.Show("Do you want to view the file?", "DataMergeEditor - Exporting to txt",
                                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(sfd.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Exportere til XML
        /// </summary>
        /// <param name="datatable"></param>
        public void ExportToXML(DataTable datatable)
        {
            datatable.AcceptChanges();
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(datatable);
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "XML Files(.xml)|.xml"
            };

            if (sfd.ShowDialog() == true)
            {

                // Save to a file
                dataSet.WriteXml(sfd.FileName);

                if (MessageBox.Show("Do you want to view the file?", "DataMergeEditor - Exporting to XML",
                       MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
            }

        }
    }
}
