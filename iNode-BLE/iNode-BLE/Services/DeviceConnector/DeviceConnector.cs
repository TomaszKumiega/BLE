using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ERGBLE.Services
{
    public class DeviceConnector : IDeviceConnector
    {
        #region Get Service/Characteristic/Descriptor
        public async Task<IService> GetEepromServiceAsync(IDevice device)
        {
            return await device.GetServiceAsync(Guid.Parse("0000CB4A-5EFD-45BE-B5BE-158DF376D8AD"));
        }

        public async Task<ICharacteristic> GetEEPROMControlCharacteristicAsync(IService service)
        {
            return await service.GetCharacteristicAsync(Guid.Parse("0000CB4C-5EFD-45BE-B5BE-158DF376D8AD"));
        }

        public async Task<ICharacteristic> GetEEPROMPageCharacteristicAsync(IService service)
        {
            return await service.GetCharacteristicAsync(Guid.Parse("0000CB4D-5EFD-45BE-B5BE-158DF376D8AD"));
        }
        #endregion

        #region Communication
        private byte[] GetCurrentTime()
        {
            var unixTimeStamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            byte[] arrayNow = new byte[] { (byte)unixTimeStamp, (byte)(unixTimeStamp >> 8), (byte)(unixTimeStamp >> 16), (byte)(unixTimeStamp >> 24) };

            return arrayNow;
        }

        public async Task SendCurrentTimeAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x04);
            byteList.Add(0x01);
            byteList.AddRange(GetCurrentTime());

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });   
        }

        public async Task TurnDataLoggingOffAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x02);
            byteList.Add(0x01);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }

        public async Task SelectReverseReadOutModeAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x0B);
            byteList.Add(0x01);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }

        public async Task<byte[]> FindAddressOfTheLastWrittenRecordAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x07);
            byteList.Add(0x01);
            byteList.Add(0x10);
            byteList.Add(0x00);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });

            byte[] lastAddrArray = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await eepromControlCharacteristic.ReadAsync();
            });
             
            return lastAddrArray;
        }

        public async Task<byte[]> FindNumberOfWrittenRecordsAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x07);
            byteList.Add(0x01);
            byteList.Add(0x12);
            byteList.Add(0x00);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });

            byte[] numberOfRecordsArray = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await eepromControlCharacteristic.ReadAsync();
            });

            return numberOfRecordsArray;
        }

        public async Task SetNumberOfBytesAndAddressAsync(ICharacteristic eepromControlCharacteristic, byte[] startAddr, byte[] numberOfBytes)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x03);
            byteList.Add(0x01);
            byteList.Add(startAddr[0]);
            byteList.Add(startAddr[1]);
            byteList.Add(numberOfBytes[0]);
            byteList.Add(numberOfBytes[1]);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }

        public async Task EnableNotificationsAsync(IDescriptor clientConfigDescriptor)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x01);
            byteList.Add(0x00);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await clientConfigDescriptor.WriteAsync(byteList.ToArray());
            });
        }

        public async Task SendReadOutCommandAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x05);
            byteList.Add(0x01);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }

        public async Task ClearOutRecordsAndSetCurrentTimeAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x04);
            byteList.Add(0x02);
            byteList.AddRange(GetCurrentTime());

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }

        public async Task TurnDataLoggingOnAsync(ICharacteristic eepromControlCharacteristic)
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(0x09);
            byteList.Add(0x01);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });

            byteList.Clear();
            byteList.Add(0x01);
            byteList.Add(0x01);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await eepromControlCharacteristic.WriteAsync(byteList.ToArray());
            });
        }
        #endregion
    }
}
