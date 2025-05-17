namespace CRM.FileStorage.Infrastructure.Settings;

public class QrCodeSettings
{
    public int DefaultSize { get; set; } = 300;
    public int DarkColorR { get; set; } = 0;
    public int DarkColorG { get; set; } = 0;
    public int DarkColorB { get; set; } = 0;
    public int LightColorR { get; set; } = 255;
    public int LightColorG { get; set; } = 255;
    public int LightColorB { get; set; } = 255;
}