using System;
using System.Collections.Generic;

namespace ERGBLE.Services
{
    public interface IDataParser
    {
        List<string> ParseMeasurementData(List<byte[]> data, string deviceName);
    }
}