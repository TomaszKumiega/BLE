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

        private void ParseDataFramesAndAddToDictionary(SortedDictionary<DateTime, List<string>> dictionary, List<DataFrame> dataFrames, string deviceName, int leftoverData)
        {
            var temperatureDataFrames = dataFrames.Where(x => x.IsTemperatureDataFrame()).ToList();
            var humidityDataFrames = dataFrames.Where(x => x.IsHumidityDataFrame()).ToList();
            
            if (temperatureDataFrames.Count > 0)
                AddValuesToDictionary(dictionary, temperatureDataFrames, $"{deviceName}  -  temperatura", leftoverData);

            if (humidityDataFrames.Count > 0)
                AddValuesToDictionary(dictionary, humidityDataFrames, $"{deviceName}  -  wilgotność", leftoverData);

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

        private void AddValuesToDictionary(SortedDictionary<DateTime, List<string>> dictionary, List<DataFrame> dataFrames, string columnName, int leftoverData)
        {
            var valuesDictionary = dataFrames.Count > 0 ? GetMeasurementsDictionary(dataFrames, leftoverData) : null;

            var listOfColumns = dictionary[new DateTime(0)].Count;
            dictionary[new DateTime(0)].Add(columnName);
            var datesToAdd = valuesDictionary.Keys.Where(x => !dictionary.Keys.Contains(x)).ToList();

            foreach (var date in datesToAdd)
            {
                if (listOfColumns == 0)
                    dictionary.Add(date, new List<string>());
                else
                    dictionary.Add(date, new List<string>(new string[listOfColumns]));
            }

            foreach (var pair in dictionary)
            {
                if (valuesDictionary.TryGetValue(pair.Key, out float value))
                {
                    pair.Value.Add($"{value:F02}");
                }
            }
        }
        #endregion

        public void ParseMeasurementDataAndAddToDictionary(SortedDictionary<DateTime, List<string>> dictionary, List<byte[]> data, string deviceName)
        {
            if (dictionary.Count == 0)
                dictionary.Add(new DateTime(0), new List<string>());

            var dataFrames = GetDataFrames(data, out int leftoverData);

            ParseDataFramesAndAddToDictionary(dictionary, dataFrames, deviceName, leftoverData);
        }

        public List<string> ParseDictionaryToText(SortedDictionary<DateTime, List<string>> dictionary)
        {
            var lines = new List<string>();
            StringBuilder sb = new StringBuilder();

            #region Column headers
            var first = dictionary.First();
            
            sb.Append("Data;");

            foreach (var t in first.Value)
            {
                sb.Append(t + ';');
            }

            sb.AppendLine();
            
            lines.Add(sb.ToString());
            
            sb.Clear();
            #endregion

            foreach (var pair in dictionary.Skip(1))
            {
                sb.Append(pair.Key.ToString("s") + ';');
                
                foreach (var t in pair.Value)
                {
                    sb.Append(t + ';');
                }

                sb.AppendLine();
                lines.Add(sb.ToString());
                
                sb.Clear();
            }

            return lines;
        }
    }
}
