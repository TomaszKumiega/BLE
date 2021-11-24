namespace ERGBLE.Services
{
    public interface IDataConverter
    {
        int ConvertAddressOfLastWrittenRecordToInt(byte[] data);
        byte[] ConvertNumberOfBytesToByteArray(int numberOfBytes);
        int ConvertNumberOfRecordsToInt(byte[] data);
        byte[] ConvertStartAddressToByteArray(int startAddr);
        int CalculateNumberOfBytesToRead(int numberOfRecords);
        int CalculateTheStartAddress(int lastAddr, int numberOfBytes);
        double CalculateTemperature(byte lsbT, byte msbT);
        double CalculateHumidity(byte lsbH, byte msbH);
    }
}