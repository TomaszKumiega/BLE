using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Services
{
    public class DataConverter : IDataConverter
    {
        public byte[] ConvertNumberOfBytesToByteArray(int numberOfBytes)
        {
            return new byte[] { (byte)numberOfBytes, (byte)(numberOfBytes >> 8) };
        }

        public byte[] ConvertStartAddressToByteArray(int startAddr)
        {
            return new byte[] { (byte)startAddr, (byte)(startAddr >> 8) };
        }

        public int ConvertAddressOfLastWrittenRecordToInt(byte[] data)
        {
            return (data[1] << 8) + data[0];
        }

        public int ConvertNumberOfRecordsToInt(byte[] data)
        {
            return (data[1] << 8) + data[0];
        }

        public int CalculateNumberOfBytesToRead(int numberOfRecords)
        {
            return numberOfRecords > 8192
                ? 65536
                : 8 * numberOfRecords % 65536;
        }

        public int CalculateTheStartAddress(int lastAddr, int numberOfBytes)
        {
            return (lastAddr - numberOfBytes) & 0xFFFF;
        }

        public double CalculateTemperature(byte lsbT, byte msbT)
        {
            var raw = (msbT << 8) + lsbT;
            return (175.72 * raw * 4 / 65536) - 46.85;
        }

        public double CalculateHumidity(byte lsbH, byte msbH)
        {
            double rawHumidity = (msbH << 8) + lsbH;
            double humidity = (125 * rawHumidity * 4 / 65536) - 6;

            if (humidity < 1)
                return 1;
            else if (humidity > 100)
                return 100;

            return humidity;
        }
    }
}
