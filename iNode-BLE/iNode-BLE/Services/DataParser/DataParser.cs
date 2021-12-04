using ERGBLE.Models;
using ERGBLE.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERGBLE.Services
{
    public class DataParser : IDataParser
    {
        #region Private methods
 
        private List<DataFrame> GetDataFrames(List<byte[]> records, out int leftOverData)
        {
            List<DataFrame> dataFrames = new List<DataFrame>();
            var frame = new byte[8];
            int frameIndex = 0;
            int i = 0;
            
            foreach (var record in records)
            {
                foreach (var @byte in record)
                {
                    frameIndex = i % 8;
                    
                    if (i != 0 && frameIndex == 0)
                    {
                        dataFrames.Add(new DataFrame(frame));
                        frame = new byte[8];
                    }

                    frame[frameIndex] = @byte;

                    ++i;
                }
            }

            leftOverData = frameIndex > 4 ? (frameIndex - 2) / 2 : (frameIndex - 1) / 2;

            return dataFrames;
        }

        private List<string> ParseDataFrames(List<DataFrame> dataFrames, string deviceName, int leftoverData)
        {
            var dictionary = new SortedDictionary<DateTime, string>();

            dictionary.Add(new DateTime(0), "Data");

            var temperatureDataFrames = dataFrames.Where(x => x.IsTemperatureDataFrame()).ToList();
            var humidityDataFrames = dataFrames.Where(x => x.IsHumidityDataFrame()).ToList();
            
            if (temperatureDataFrames.Count > 0)
            {
                dictionary[new DateTime(0)] += $",,{deviceName}  -  temperatura";
                AddValuesToDictionary(dictionary, temperatureDataFrames, leftoverData, 1);
            }
                

            if (humidityDataFrames.Count > 0)
            {
                dictionary[new DateTime(0)] += $",,{deviceName}  -  wilgotność";
                AddValuesToDictionary(dictionary, humidityDataFrames, leftoverData, 2);
            }

            return dictionary.Values.ToList();
        }

        private void AddValuesToDictionary(SortedDictionary<DateTime, string> dictionary, List<DataFrame> dataFrames, int leftoverData, int columnNumber)
        {
            var valuesDictionary = dataFrames.Count > 0 ? GetMeasurementsDictionary(dataFrames, leftoverData) : null;

            var datesToAdd = valuesDictionary.Keys.Where(x => !dictionary.Keys.Contains(x)).ToList();

            foreach (var date in datesToAdd)
            {
                dictionary.Add(date, date.ToString("s"));
            }

            foreach (var pair in valuesDictionary)
            {
                if (dictionary.ContainsKey(pair.Key))
                {
                    dictionary[pair.Key] += columnNumber == 1 ? $",,{pair.Value:F02}" : $",,{pair.Value:F01}";
                }
                else
                {
                    var date = pair.Key.ToString("s");
                    var value = columnNumber == 1 ? $"{pair.Value:F02}" : $"{pair.Value:F01}";
                    
                    var sb = new StringBuilder();
                    sb.Append(date);

                    for (int i = 0; i < columnNumber; ++i)
                    {
                        sb.Append(",,");
                    }

                    sb.Append(value);
                    
                    dictionary.Add(pair.Key, sb.ToString());
                }
            }
        }

        private Dictionary<DateTime, float> GetMeasurementsDictionary(List<DataFrame> dataFrames, int leftoverData)
        {
            var dictionary = new Dictionary<DateTime, float>();
            int indexOfFirstFrameWithTime = dataFrames.FindIndex(x => x.ContainsTime());
            DateTime? dateTimeOfFirstTimeRecord = dataFrames.FirstOrDefault(x => x.ContainsTime())?.GetTime();

            DateTime lastCalculatedDateTime = dateTimeOfFirstTimeRecord.HasValue
               ? dateTimeOfFirstTimeRecord.Value.AddMinutes(-(indexOfFirstFrameWithTime * 3)).Trim(TimeSpan.TicksPerMinute)
               : DateTime.Now.AddMinutes(-leftoverData - (dataFrames.Count * 3)).Trim(TimeSpan.TicksPerMinute);

            foreach (var dataFrame in dataFrames)
            {
                if (dataFrame.ContainsTime())
                    lastCalculatedDateTime = dataFrame.GetTime().Trim(TimeSpan.TicksPerMinute);

                List<float> values = dataFrame.GetMeasurementValues();

                foreach (var val in values)
                {
                    dictionary.Add(lastCalculatedDateTime, val);
                    lastCalculatedDateTime = lastCalculatedDateTime.AddMinutes(1);
                }
            }

            return dictionary;
        }
        #endregion

        public List<string> ParseMeasurementData(List<byte[]> data, string deviceName)
        {
            var dataFrames = GetDataFrames(data, out int leftoverData);

            return ParseDataFrames(dataFrames, deviceName, leftoverData);
        }
    }
}
