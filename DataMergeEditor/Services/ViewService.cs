using DataMergeEditor.Interfaces;
using DataMergeEditor.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataMergeEditor.Services
{
    public class ViewService : IViewService
    {
        private IDataService _dataService;

        public ViewService(IDataService dataService)
        {
            _dataService = dataService;
        }
        /// <summary>
        /// Takes any Window parameter from the View/Windows to open it.
        /// Every ViewModel attached will be applyed when shown.
        /// </summary>
        /// <param name="window"></param>
        public void CreateWindow(Window window)
        {
            window.Show();
        }

        /// <summary>
        /// Takes any Window parameter from the View/Windows to open it.
        /// Takes the datacontext from the viewmodel which is refered to 'this' - etc. (window, this)
        /// Every ViewModel attached will be applyed when shown.
        /// Is an overload method
        /// </summary>
        /// <param name="window"></param>
        /// <param name="datacontext"></param>
        public void CreateWindow(Window window, object datacontext)
        {
            window.DataContext = datacontext;
            window.Show();
        }

    }
}
