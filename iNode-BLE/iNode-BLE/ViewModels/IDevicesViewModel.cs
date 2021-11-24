using ERGBLE.Models;
using ERGBLE.Services;
using ERGBLE.ViewModels.Commands;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERGBLE.ViewModels
{
    public interface IDevicesViewModel : INotifyPropertyChanged
    {
        float Progress { get; }
        bool Processing { get; }
        List<IDevice> Devices { get; }
        ObservableCollection<SensorInfo> SensorInfos { get; }
        IDevicesVMCommand ScanForDevicesCommand { get; }
        IDevicesVMCommand SaveRecordsCommand { get; }

        Task ScanForDevices();
        Task SaveRecords(bool clearRecordsAndSetCurrentDate);
    }
}