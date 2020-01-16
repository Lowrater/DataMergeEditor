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
        void CreateWindow(Window window);
        void CreateWindow(Window window, object datacontext);

    }
}
