using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models.Builders
{
    public class SensorInfoBuilder
    {
        private SensorInfo SensorInfo { get; set; }

        public SensorInfoBuilder OfType(SensorType type)
        {
            SensorInfo = new SensorInfo();
            SensorInfo.SensorType = type;

            return this;
        }

        public SensorInfoBuilder WithName(string name)
        {
            SensorInfo.Name = name;
            
            return this;
        }

        public SensorInfoBuilder WithBatteryLevel(int batteryLevel)
        {
            SensorInfo.BatteryLevel = batteryLevel;

            return this;
        }

        public SensorInfoBuilder WithBatteryVoltage(double batteryVoltage)
        {
            SensorInfo.BatteryVoltage = batteryVoltage;

            return this;
        }

        public SensorInfoBuilder WithMeasurement(IMeasurementData measurement)
        {
            SensorInfo.Measurement = measurement;

            return this;
        }

        public SensorInfo Build()
        {
            return SensorInfo;
        }
    }
}
