using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models
{
    public interface IMeasurementData
    {
        SensorType SensorType { get; }

        void ReadFromManufacturerSpecificData(byte[] data);
        string ToString();
    }
}
