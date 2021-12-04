using ERGBLE.Exceptions;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;

namespace ERGBLE.Services
{
    public class DeviceScanner : IDeviceScanner
    {
        private IBluetoothLE BLE { get; }
        private IAdapter Adapter { get; }
        private ISecondsTimer Timer { get; }
        private int ScanTimeoutMs { get; set; } = 1;

        public List<IDevice> Devices { get; }
        public Func<IDevice, bool> DeviceFilter { get; set; }
        public Action<float, float> SetProgress { private get; set; }
        public Action<bool> SetProcessing { private get; set; }

        public event EventHandler ScanTimeoutElapsed;
        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceScanner(ISecondsTimer timer)
        {
            Devices = new List<IDevice>();

            Timer = timer;
            BLE = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;

            Timer.IntervalPassed += OnIntervalPassed;
            Timer.TimeoutElapsed += OnTimeoutElapsed;
            Adapter.DeviceDiscovered += DeviceFound;
            Adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
        }

        #region EventHandlers
        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            Devices.Add(args.Device);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Devices)));
        }

        private async void OnScanTimeoutElapsed(object sender, EventArgs args)
        {
            await Adapter.StopScanningForDevicesAsync();
            ScanTimeoutElapsed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public async Task ScanForDevices(int scanTimeoutMs)
        {
            if (BLE.State != BluetoothState.On)
            {
                throw new BluetoothNotEnabledException();
            }

            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                throw new PermissionNotGranted(typeof(Permissions.LocationWhenInUse));
            }

            if (Adapter.IsScanning)
            {
                return;
            }

            Devices.Clear();

            Adapter.ScanMode = ScanMode.Balanced;

            SetProcessing(true);
            Adapter.ScanTimeout = scanTimeoutMs;
            ScanTimeoutMs = scanTimeoutMs;

            Timer.TimeoutMs = scanTimeoutMs;
            Timer.IntervalMs = 50;
            Timer.Start();

            await Adapter.StartScanningForDevicesAsync(
                deviceFilter: DeviceFilter);
        }

        private void OnIntervalPassed(object sender, EventArgs args)
        {
            SetProgress(Timer.TimePassedMs, ScanTimeoutMs);
        }

        private void OnTimeoutElapsed(object sender, EventArgs args)
        {
            SetProcessing(false);
            SetProgress(0, 1);
        }
    }
}
