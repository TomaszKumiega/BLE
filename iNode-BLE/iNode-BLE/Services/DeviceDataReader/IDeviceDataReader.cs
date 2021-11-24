using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERGBLE.Services
{
    public interface IDeviceDataReader
    {
        bool ClearOutRecordsAndSetTime { set; }
        Action<bool> SetProcessing { set; }
        Action<float, float> SetProgress { set; }
        List<string> ListOfFailedConnections { get; }

        event EventHandler FinishedProcessing;

        Task SaveRecords(List<IDevice> devices);
    }
}