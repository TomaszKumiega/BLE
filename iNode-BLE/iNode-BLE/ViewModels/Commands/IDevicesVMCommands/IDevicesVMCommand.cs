using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.ViewModels.Commands
{
    public interface IDevicesVMCommand : ICommand<IDevicesViewModel>
    {
        void RaiseCanExecuteChanged();
    }
}
