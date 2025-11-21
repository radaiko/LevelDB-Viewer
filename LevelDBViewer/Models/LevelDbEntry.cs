namespace LevelDBViewer.Models;

public class LevelDbEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string KeyHex { get; set; } = string.Empty;
    public string ValueHex { get; set; } = string.Empty;
}
