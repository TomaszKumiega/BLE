using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ERGBLE.ViewModels.Commands
{
    public interface ICommand<TViewModel> : ICommand
    {
        TViewModel ViewModel { set; }
    }
}
