using Plugin.BLE.Abstractions.Contracts;
using System.Threading.Tasks;

namespace ERGBLE.Services
{
    public interface IDeviceConnector
    {
        Task ClearOutRecordsAndSetCurrentTimeAsync(ICharacteristic eepromControlCharacteristic);
        Task EnableNotificationsAsync(IDescriptor clientConfigDescriptor);
        Task<byte[]> FindAddressOfTheLastWrittenRecordAsync(ICharacteristic eepromControlCharacteristic);
        Task<byte[]> FindNumberOfWrittenRecordsAsync(ICharacteristic eepromControlCharacteristic);
        Task<ICharacteristic> GetEEPROMControlCharacteristicAsync(IService service);
        Task<ICharacteristic> GetEEPROMPageCharacteristicAsync(IService service);
        Task<IService> GetEepromServiceAsync(IDevice device);
        Task SelectReverseReadOutModeAsync(ICharacteristic eepromControlCharacteristic);
        Task SendCurrentTimeAsync(ICharacteristic eepromControlCharacteristic);
        Task SendReadOutCommandAsync(ICharacteristic eepromControlCharacteristic);
        Task SetNumberOfBytesAndAddressAsync(ICharacteristic eepromControlCharacteristic, byte[] startAddr, byte[] numberOfBytes);
        Task TurnDataLoggingOffAsync(ICharacteristic eepromControlCharacteristic);
        Task TurnDataLoggingOnAsync(ICharacteristic eepromControlCharacteristic);
    }
}