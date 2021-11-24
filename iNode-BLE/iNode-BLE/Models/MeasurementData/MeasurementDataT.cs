using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models
{
    public class MeasurementDataT : IMeasurementData
    {
        public SensorType SensorType => SensorType.T;

        public float Temperature { get; private set; }

        private float CalculateTemperature(int msbT, int lsbT)
        {
            float temperature = (float)(msbT * 0.0625) + (16 * (lsbT & 0x0F));

            if ((lsbT & 0x10) != 0)
            {
                return (float)(temperature - 256);
            }
            else if (temperature < -30)
            {
                return -30;
            }
            else if (temperature > 70)
            {
                return 70;
            }

            return temperature;
        }

        public void ReadFromManufacturerSpecificData(byte[] data)
        {
            var msbT = data[8];
            var lsbT = data[9];

            Temperature = CalculateTemperature(msbT, lsbT);
        }

        public override string ToString()
        {
            return $"{Temperature:F02}°C".Trim();
        }
    }
}
