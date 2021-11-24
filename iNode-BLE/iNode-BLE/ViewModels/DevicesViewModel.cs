using ERGBLE.Exceptions;
using ERGBLE.Models;
using ERGBLE.Services;
using ERGBLE.ViewModels.Commands;
using ERGBLE.ViewModels.Commands.Builders;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace ERGBLE.ViewModels
{
    public class DevicesViewModel : IDevicesViewModel
    {
        private IDeviceScanner DeviceScanner { get; }
        private IDeviceInfoReader DeviceInfoReader { get; }
        private IDeviceDataReader DeviceDataReader { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Public properies
        public ObservableCollection<SensorInfo> SensorInfos { get; }
        public IDevicesVMCommand ScanForDevicesCommand { get; }
        public IDevicesVMCommand SaveRecordsCommand { get; }

        private float _progress;
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        private bool _processing;
        public bool Processing
        {
            get => _processing;
            set
            {
                _processing = value;
                SaveRecordsCommand.RaiseCanExecuteChanged();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Processing)));
            }
        }

        public List<IDevice> Devices => DeviceScanner.Devices;
        #endregion

        #region Constructor
        public DevicesViewModel(
            IDeviceScanner deviceScanner, 
            CommandsBuilder<ScanForDevicesCommand, IDevicesViewModel> scanForDevicesCommandsBuilder,
            CommandsBuilder<SaveRecordsCommand, IDevicesViewModel> saveRecordsCommandBuilder,
            IDeviceInfoReader deviceInfoReader,
            IDeviceDataReader deviceDataReader)
        {
            SensorInfos = new ObservableCollection<SensorInfo>();

            ScanForDevicesCommand = scanForDevicesCommandsBuilder
                .WithViewModel(this)
                .Build();
            SaveRecordsCommand = saveRecordsCommandBuilder
                .WithViewModel(this)
                .Build();

            DeviceInfoReader = deviceInfoReader;
            DeviceScanner = deviceScanner;
            DeviceDataReader = deviceDataReader;

            InitializeProgressActions();
            InitializeEventHandlers();
        }

        private void InitializeProgressActions()
        {
            DeviceScanner.SetProcessing = value =>
            {
                Processing = value;
            };
            DeviceScanner.SetProgress = (actual, max) =>
            {
                Progress = actual / max;
            };

            DeviceDataReader.SetProcessing = value =>
            {
                Processing = value;
            };
            DeviceDataReader.SetProgress = (actual, max) =>
            {
                Progress = actual / max;
            };
        }
        private void InitializeEventHandlers()
        {
            DeviceScanner.ScanTimeoutElapsed += OnScanTimeoutElapsed;
            DeviceScanner.PropertyChanged += OnPropertyChanged;
            DeviceDataReader.FinishedProcessing += OnSavingFinished;
        }
        #endregion

        #region EventHandler methods
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(DeviceScanner.Devices))
            {
                SaveRecordsCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnSavingFinished(object sender, EventArgs args)
        {
            foreach (var s in SensorInfos)
            {
                if (DeviceDataReader.ListOfFailedConnections.Contains(s.Name))
                {
                    s.FailedToSaveRecords = true;
                }
            }
        }

        public void OnScanTimeoutElapsed(object sender, EventArgs args)
        {
            SensorInfos.Clear();

            var sensorInfos = new List<SensorInfo>();

            foreach (var device in DeviceScanner.Devices)
            {
                if (DeviceInfoReader.GetSensorType(device) != SensorType.Unknown)
                {
                    var sensorInfo = DeviceInfoReader.GetSensorInfo(device);
                    sensorInfos.Add(sensorInfo);
                }
            }

            sensorInfos = sensorInfos.OrderBy(x => x.Name).ToList();

            foreach (var sensorInfo in sensorInfos)
            {
                sensorInfo.Name = sensorInfo.Name.Replace("W-264-", "");
                SensorInfos.Add(sensorInfo);
            }
        }
        #endregion

        public async Task ScanForDevices()
        {
            try
            {
                DeviceScanner.DeviceFilter = device =>
                {
                    return device?.Name?.StartsWith("W-264") ?? false;
                };

                await DeviceScanner.ScanForDevices(10000);
            }
            catch (PermissionNotGranted e)
            {
                if (e.PermissionType == typeof(Permissions.LocationWhenInUse))
                {
                    PermissionStatus permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (permissionStatus == PermissionStatus.Granted)
                    {
                        await ScanForDevices();
                    }
                }
            }
            catch (BluetoothNotEnabledException e)
            {
                //TODO: user prompt
            }
        }

        public async Task SaveRecords(bool clearRecordsAndSetCurrentDate)
        {
            DeviceDataReader.ClearOutRecordsAndSetTime = clearRecordsAndSetCurrentDate;
            try
            {
                await DeviceDataReader.SaveRecords(Devices);
            }
            catch (PermissionNotGranted e)
            {
                if (e.PermissionType == typeof(Permissions.StorageWrite))
                {
                    var permissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (permissionStatus == PermissionStatus.Granted)
                        await SaveRecords(clearRecordsAndSetCurrentDate);
                }
            }
        }
    }
}
