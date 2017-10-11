namespace NucLedCli
{
    public enum Mode : byte
    {
        Off = 0,
        On = 4,
        Blink1Hz = 1,
        BlinkPoint5Hz = 5,
        BlinkPoint25Hz = 2,
        Fade1Hz = 3,
        FadePoint5Hz = 7,
        FadePoint25Hz = 6
    }
}