using System;
using System.Collections.Generic;

namespace ERGBLE.Models
{
    public class DataFrame
    {
        public byte[] Bytes { get; }

        public DataFrame(byte[] bytes)
        {
            if (bytes?.Length != 8)
                throw new ArgumentException();

            Bytes = bytes;
        }

        public bool ContainsTime()
        {
            return ((Bytes[0] >> 4) == 0b1010) && (Bytes[5] == 0x00 || (Bytes[5] > 0x81 && Bytes[5] < 0x89) || Bytes[5] == 0x8C || Bytes[5] == 0x8D || Bytes[5] == 0x08);
        }

        public bool IsTemperatureDataFrame()
        {
            return Bytes[5] == 1 || Bytes[5] == 0x82 || Bytes[5] == 0x83;
        }

        public bool IsHumidityDataFrame()
        {
            return Bytes[5] == 2 || Bytes[5] == 0x84 || Bytes[5] == 0x85;
        }

        public List<float> GetMeasurementValues()
        {
            var values = new List<float>();
            
            if (IsTemperatureDataFrame())
            {
                if (!ContainsTime())
                {
                    values.Add(GetTemperature(Bytes[1], Bytes[2]));
                    values.Add(GetTemperature(Bytes[3], Bytes[4]));
                }

                values.Add(GetTemperature(Bytes[6], Bytes[7]));
            }
            else if (IsHumidityDataFrame())
            {
                if (!ContainsTime())
                {
                    values.Add(GetHumidity(Bytes[1], Bytes[2]));
                    values.Add(GetHumidity(Bytes[3], Bytes[4]));
                }

                values.Add(GetHumidity(Bytes[6], Bytes[7]));
            }

            return values;
        }

        public DateTime GetTime()
        {
            long timestamp = ((uint)Bytes[4] << 24) + ((uint)Bytes[3] << 16) + ((uint)Bytes[2] << 8) + Bytes[1];
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp);
        }

        #region Helper methods
        private float GetTemperature(byte lsbT, byte msbT)
        {
            var raw = (msbT << 8) + lsbT;
            return (float)((175.72 * raw * 4 / 65536) - 46.85);
        }

        private float GetHumidity(byte lsbH, byte msbH)
        {
            double rawHumidity = (msbH << 8) + lsbH;
            double humidity = (125 * rawHumidity * 4 / 65536) - 6;

            if (humidity < 1)
                return 1;
            else if (humidity > 100)
                return 100;

            return (float)humidity;
        }
        #endregion
    }
}
