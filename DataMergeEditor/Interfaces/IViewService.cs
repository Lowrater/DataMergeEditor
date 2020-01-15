using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataMergeEditor.Interfaces
{
   public interface IViewService
    {
        void CreateWindowWithDataContext(Window window, object datacontext);
        void CreateWindow(Window window);
    }
}
