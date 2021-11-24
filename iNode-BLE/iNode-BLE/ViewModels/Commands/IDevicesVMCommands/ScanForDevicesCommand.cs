using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ERGBLE.ViewModels.Commands
{
    public class ScanForDevicesCommand : IDevicesVMCommand
    {
        public IDevicesViewModel ViewModel { private get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await ViewModel.ScanForDevices();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
