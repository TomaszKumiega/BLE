using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.ViewModels.Commands
{
    public class SaveRecordsCommand : IDevicesVMCommand
    {
        public IDevicesViewModel ViewModel { private get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return ViewModel.Devices?.Count > 0 && !ViewModel.Processing;
        }

        public async void Execute(object parameter)
        {
            await ViewModel.SaveRecords(true);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
