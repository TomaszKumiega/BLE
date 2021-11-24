using ERGBLE.Models.Selectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models.Builders
{
    public class MeasurementDataBuilder
    {
        private IImplementationSelector<SensorType, IMeasurementData> Selector { get; }
        private IMeasurementData MeasurementData { get; set; }

        public MeasurementDataBuilder(IImplementationSelector<SensorType, IMeasurementData> selector)
        {
            Selector = selector;
        }

        public MeasurementDataBuilder OfType(SensorType sensorType)
        {
            MeasurementData = Selector.Select(sensorType);

            return this;
        }

        public MeasurementDataBuilder FromManufacturerSpecificData(byte[] data)
        {
            MeasurementData.ReadFromManufacturerSpecificData(data);

            return this;
        }

        public IMeasurementData Build()
        {
            return MeasurementData;
        }
    }
}
