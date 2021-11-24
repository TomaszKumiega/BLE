using System;
using System.Collections.Generic;

namespace ERGBLE.Services
{
    public interface IDataParser
    {
        void ParseMeasurementDataAndAddToDictionary(SortedDictionary<DateTime, List<string>> dictionary, List<byte[]> data, string deviceName);

        List<string> ParseDictionaryToText(SortedDictionary<DateTime, List<string>> dictionary);
    }
}