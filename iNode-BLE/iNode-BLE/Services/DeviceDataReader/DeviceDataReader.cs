using ERGBLE.Exceptions;
using ERGBLE.Models;
using ERGBLE.Models.Selectors;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ERGBLE.Services
{
    public class DeviceDataReader : IDeviceDataReader
    {
        private IAdapter Adapter { get; }
        private IDeviceConnector DeviceConnector { get; }
        private IDataConverter DataConverter { get; }
        private IDataParser DataParser { get; }
        private int CurrentDeviceIndex { get; set; }
        private List<IDevice> Devices { get; set; }
        private SortedDictionary<DateTime, List<string>> DataDictionary { get; }

        private event EventHandler NextDevice;

        public Action<float, float> SetProgress { private get; set; }
        public Action<bool> SetProcessing { private get; set; }
        public bool ClearOutRecordsAndSetTime { private get; set; }
        public List<string> ListOfFailedConnections { get; private set; }
        
        public event EventHandler FinishedProcessing;

        public DeviceDataReader(IDeviceConnector deviceConnector, IDataConverter dataConverter, IDataParser dataParser)
        {
            ListOfFailedConnections = new List<string>();

            Adapter = CrossBluetoothLE.Current.Adapter;
            DeviceConnector = deviceConnector;
            DataConverter = dataConverter;
            DataParser = dataParser;
            DataDictionary = new SortedDictionary<DateTime, List<string>>();

            NextDevice += OnNextDevice;
        }

        private async Task ParseRecords(List<byte[]> listOfRecords, string deviceName)
        {
            await Task.Run(() => DataParser.ParseMeasurementDataAndAddToDictionary(DataDictionary, listOfRecords, deviceName));
        }

        private async Task ShareRecordsInTextFile()
        {
            var date = DateTime.Now.ToString().Replace('/', '-');
            var path = Path.Combine(FileSystem.AppDataDirectory, date + ".txt");

            var lines = DataParser.ParseDictionaryToText(DataDictionary);
            File.AppendAllLines(path, lines);
            
            await Share.RequestAsync(new ShareFileRequest()
            {
                Title = "Udostępnij plik z pomiarami",
                File = new ShareFile(path)
            });

            //TODO usuwanie pliku po udostępnieniu
        }

        private async void OnNextDevice(object sender, EventArgs args)
        {
            if (CurrentDeviceIndex >= Devices.Count)
            {
                await ShareRecordsInTextFile();

                CurrentDeviceIndex = 0;
                FinishedProcessing?.Invoke(this, EventArgs.Empty);
                SetProcessing(false);
                SetProgress(0, 1);

                return;
            }

            SetProgress(CurrentDeviceIndex, Devices.Count);
            await SaveRecords(Devices[CurrentDeviceIndex]);
        }

        public async Task SaveRecords(List<IDevice> devices)
        {
            if (devices == null)
                throw new ArgumentException(nameof(devices));
            if (devices.Count == 0)
                throw new ArgumentException(nameof(devices));

            if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
                throw new PermissionNotGranted(typeof(Permissions.StorageWrite));

            Devices = devices;
            SetProcessing(true);
            await SaveRecords(Devices[0]);
        }

        private async Task<bool> SaveRecords(IDevice device)
        {
            try
            {
                await Adapter.ConnectToDeviceAsync(device);
            }
            catch (DeviceConnectionException)
            {
                return false;
            }

            var eepromService = await DeviceConnector.GetEepromServiceAsync(device);
            var eepromControlCharacteristic = await DeviceConnector.GetEEPROMControlCharacteristicAsync(eepromService);

            await DeviceConnector.SendCurrentTimeAsync(eepromControlCharacteristic);

            await DeviceConnector.TurnDataLoggingOffAsync(eepromControlCharacteristic);
            await DeviceConnector.SelectReverseReadOutModeAsync(eepromControlCharacteristic);
            var lastAddrArray = await DeviceConnector.FindAddressOfTheLastWrittenRecordAsync(eepromControlCharacteristic);
            var numberOfRecordsArray = await DeviceConnector.FindNumberOfWrittenRecordsAsync(eepromControlCharacteristic);

            var lastAddrInt = DataConverter.ConvertAddressOfLastWrittenRecordToInt(lastAddrArray);
            var numberOfRecordsInt = DataConverter.ConvertNumberOfRecordsToInt(numberOfRecordsArray);

            var numberOfBytesToRead = DataConverter.CalculateNumberOfBytesToRead(numberOfRecordsInt);
            var startAddress = DataConverter.CalculateTheStartAddress(lastAddrInt, numberOfBytesToRead);

            var numberOfBytesArray = DataConverter.ConvertNumberOfBytesToByteArray(numberOfBytesToRead);
            var startAddressArray = DataConverter.ConvertStartAddressToByteArray(startAddress);

            await DeviceConnector.SetNumberOfBytesAndAddressAsync(eepromControlCharacteristic, startAddressArray, numberOfBytesArray);

            var eepromPageCharacteristic = await DeviceConnector.GetEEPROMPageCharacteristicAsync(eepromService);
            var descriptors = await eepromPageCharacteristic.GetDescriptorsAsync();
            var clientConfigDescr = descriptors.Last();

            await DeviceConnector.EnableNotificationsAsync(clientConfigDescr);

            var listOfRecords = new List<byte[]>();
            int numberOfBytesRead = 0;

            eepromPageCharacteristic.ValueUpdated += async (s, a) =>
            {
                numberOfBytesRead += a.Characteristic.Value.Length;
                listOfRecords.Add(a.Characteristic.Value);

                if (numberOfBytesRead >= numberOfBytesToRead)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await eepromPageCharacteristic.StopUpdatesAsync();
                    });

                    await ParseRecords(listOfRecords, device.Name);
                   
                    if (ClearOutRecordsAndSetTime)
                        await DeviceConnector.ClearOutRecordsAndSetCurrentTimeAsync(eepromControlCharacteristic);

                    await DeviceConnector.TurnDataLoggingOnAsync(eepromControlCharacteristic);

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Adapter.DisconnectDeviceAsync(device);
                    });
                    
                    CurrentDeviceIndex++;
                    NextDevice?.Invoke(this, EventArgs.Empty);
                }
            };

            await MainThread.InvokeOnMainThreadAsync(async () => 
            {
                await eepromPageCharacteristic.StartUpdatesAsync();
            });

            await DeviceConnector.SendReadOutCommandAsync(eepromControlCharacteristic);

            return true;
        }
    }
}
