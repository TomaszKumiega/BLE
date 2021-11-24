using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models
{
    public class MeasurementDataHT : IMeasurementData
    {
        public SensorType SensorType => SensorType.HT;

        public float Temperature { get; private set; }
        public float Humidity { get; private set; }

        private double CalculateTemperature(double rawTemperature)
        {
            double temperature = (175.72 * rawTemperature * 4 / 65536) - 46.85;
            
            if (temperature < -30)
            {
                return -30;
            }
            else if (temperature > 70)
            {
                return 70;
            }
                
            return temperature;
        }

        private double CalculateHumidity(double rawHumidity)
        {
            double humidity = (125 * rawHumidity * 4 / 65536) - 6;
           
            if (humidity < 1)
            {
                return 1;
            }
            else if (humidity > 100)
            {
                return 100;
            }

            return humidity;
        }

        public void ReadFromManufacturerSpecificData(byte[] data)
        {
            var rawTemperature = (data[9] << 8) + data[8];
            Temperature = (float)CalculateTemperature(rawTemperature);

            var rawHumidity = (data[11] << 8) + data[10];
            Humidity = (float)CalculateHumidity(rawHumidity);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{Temperature:F02}°C");
            
            if (Humidity != 1)
                sb.AppendLine($"{Humidity:F01}%");
            
            return sb.ToString().Trim();
        }
    }
}
