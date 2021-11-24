using ERGBLE.Models;
using ERGBLE.Models.Builders;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERGBLE.Services
{
    public class DeviceInfoReader : IDeviceInfoReader
    {
        private SensorInfoBuilder SensorInfoBuilder { get; }
        private MeasurementDataBuilder MeasurementDataBuilder { get; }

        public DeviceInfoReader(SensorInfoBuilder sensorInfoBuilder, MeasurementDataBuilder measurementDataBuilder)
        {
            SensorInfoBuilder = sensorInfoBuilder;
            MeasurementDataBuilder = measurementDataBuilder;
        }

        #region Private methods
        private byte[] GetManufacturerSpecificData(IDevice device)
        {
            var advertismentRecords = device?.AdvertisementRecords?.ToList();
            var manufacturerSpecificData = advertismentRecords?.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);

            return manufacturerSpecificData?.Data;
        }

        private int GetBatteryLevel(int groupsAndBatteryData)
        {
            var battery = (groupsAndBatteryData >> 12) & 0x0F;

            if (battery == 1)
            {
                return 100;
            }
            else
            {
                return 10 * (Math.Min(battery, 11) - 1);
            }
        }

        private double GetBatteryVoltage(int groupsAndBatteryData)
        {
            var batteryLevel = GetBatteryLevel(groupsAndBatteryData);

            return ((batteryLevel - 10) * 1.2 / 100) + 1.8;
        }
        #endregion

        public SensorInfo GetSensorInfo(IDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException(nameof(device));
            }

            byte[] data = GetManufacturerSpecificData(device);

            if (data?.Length != 24)
            {
                return null;
            }

            SensorType sensorType = GetSensorType(device);
            int groupsAndBatteryData = (data[3] << 8) + data[4];
            int batteryLevel = GetBatteryLevel(groupsAndBatteryData);
            double batteryVoltage = GetBatteryVoltage(groupsAndBatteryData);

            IMeasurementData measurement = MeasurementDataBuilder
                .OfType(sensorType)
                .FromManufacturerSpecificData(data)
                .Build();

            SensorInfo sensorInfo = SensorInfoBuilder
                .OfType(sensorType)
                .WithName(device.Name)
                .WithBatteryLevel(batteryLevel)
                .WithBatteryVoltage(batteryVoltage)
                .WithMeasurement(measurement)
                .Build();

            return sensorInfo;
        }

        public SensorType GetSensorType(IDevice device)
        {
            byte[] data = GetManufacturerSpecificData(device);

            return data.Length != 24
                ? SensorType.Unknown
                : Enum.IsDefined(typeof(SensorType), data[1]) ? (SensorType)data[1] : SensorType.Unknown;
        }
    }
}
