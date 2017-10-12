namespace NucLedCli
{
    public enum Error : byte
    {
        None = 0,
        FunctionNotSupported = 0xe1,
        UndefinedDevice = 0xe2,
        NoECResponse = 0xe3,
        InvalidParameter = 0xe4,
        UnexpectedError = 0xef
    }
}