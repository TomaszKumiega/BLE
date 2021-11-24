using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ERGBLE.Services
{
    public interface IDeviceScanner : INotifyPropertyChanged
    {
        Func<IDevice, bool> DeviceFilter { get; set; }
        List<IDevice> Devices { get; }
        Action<float, float> SetProgress { set; }
        Action<bool> SetProcessing { set; }
        event EventHandler ScanTimeoutElapsed;

        Task ScanForDevices(int scanTimeoutMs);
    }
}