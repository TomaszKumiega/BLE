using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models
{
    public enum SensorType : byte
    {
        Unknown,
        T = 0x9A,
        HT = 0x9B
    }

    public class SensorInfo
    {
        public SensorType SensorType { get; set; }
        public string Name { get; set; }
        public int BatteryLevel { get; set; }
        public string BatteryLevelText => BatteryLevel.ToString() + "%";
        public double BatteryVoltage { get; set; }
        public IMeasurementData Measurement { get; set; }
        public bool FailedToSaveRecords { get; set; }
    }
}
