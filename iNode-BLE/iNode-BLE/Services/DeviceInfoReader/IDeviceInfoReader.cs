using ERGBLE.Models;
using Plugin.BLE.Abstractions.Contracts;

namespace ERGBLE.Services
{
    public interface IDeviceInfoReader
    {
        SensorInfo GetSensorInfo(IDevice device);
        SensorType GetSensorType(IDevice device);
    }
}