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
using System.Text;
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
        private List<string> FilePaths { get; set; }

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
            FilePaths = new List<string>();

            NextDevice += OnNextDevice;
        }

        private void Reset()
        {
            ListOfFailedConnections = new List<string>();
            FilePaths = new List<string>();
            CurrentDeviceIndex = 0;
        }

        private async Task SaveRecords(List<byte[]> listOfRecords, string deviceName)
        {
            var date = DateTime.Now.ToString().Replace('/', '-');
            var path = Path.Combine(FileSystem.AppDataDirectory, date + ".txt");

            var lines = await Task.Run(() => DataParser.ParseMeasurementData(listOfRecords, deviceName));

            File.AppendAllLines(path, lines);

            FilePaths.Add(path);
        }

        private async Task ShareRecordsInTextFile()
        {
            var fileShareList = new List<ShareFile>();

            foreach (var filePath in FilePaths)
            {
                fileShareList.Add(new ShareFile(filePath));
            }

            await Share.RequestAsync(new ShareMultipleFilesRequest()
            {
                Title = "Udostępnij pliki z pomiarami",
                Files = fileShareList
            });
        }

        private async void OnNextDevice(object sender, EventArgs args)
        {
            if (CurrentDeviceIndex >= Devices.Count)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Zapisano dane z {Devices.Count - ListOfFailedConnections.Count}/{Devices.Count} urządzeń.");

                if (ListOfFailedConnections.Count > 0)
                {
                    sb.AppendLine("Nie udało się zapisać danych z urządzeń:");

                    foreach (var failedConnName in ListOfFailedConnections)
                    {
                        sb.AppendLine($"- {failedConnName}");
                    }
                }

                App.Current.Dispatcher.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Zapisywanie zakończone", sb.ToString(), "Kontynuuj"));
                await ShareRecordsInTextFile();

                FinishedProcessing?.Invoke(this, EventArgs.Empty);
                SetProcessing(false);
                SetProgress(0, 1);

                return;
            }

            SetProgress(CurrentDeviceIndex, Devices.Count);
            
            try
            {
                var success = await SaveRecords(Devices[CurrentDeviceIndex]);
                
                if (!success)
                    ListOfFailedConnections.Add(Devices[CurrentDeviceIndex].Name);
            }
            catch (Exception)
            {
                ListOfFailedConnections.Add(Devices[CurrentDeviceIndex].Name);
            }
        }

        public async Task SaveRecords(List<IDevice> devices)
        {
            if (devices == null)
                throw new ArgumentException(nameof(devices));
            if (devices.Count == 0)
                throw new ArgumentException(nameof(devices));

            if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
                throw new PermissionNotGranted(typeof(Permissions.StorageWrite));

            Reset();

            Devices = devices;
            SetProcessing(true);

            try
            {
                var success = await SaveRecords(Devices[0]);

                if (!success)
                    ListOfFailedConnections.Add(Devices[0].Name);
            }
            catch (Exception)
            {
                ListOfFailedConnections.Add(Devices[0].Name);
            }
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

                    await SaveRecords(listOfRecords, device.Name);
                   
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
