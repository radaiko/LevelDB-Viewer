using System;

namespace LevelDBViewer.Models;

public class LevelDbEntry
{
    private byte[]? _keyBytes;
    private byte[]? _valueBytes;
    private string? _keyHex;
    private string? _valueHex;

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public string KeyHex
    {
        get
        {
            if (_keyHex == null && _keyBytes != null)
            {
                _keyHex = BitConverter.ToString(_keyBytes).Replace("-", " ");
            }
            return _keyHex ?? string.Empty;
        }
        set => _keyHex = value;
    }
    
    public string ValueHex
    {
        get
        {
            if (_valueHex == null && _valueBytes != null)
            {
                _valueHex = BitConverter.ToString(_valueBytes).Replace("-", " ");
            }
            return _valueHex ?? string.Empty;
        }
        set => _valueHex = value;
    }

    // Internal properties to store raw bytes for lazy hex conversion
    internal byte[]? KeyBytes
    {
        get => _keyBytes;
        set => _keyBytes = value;
    }

    internal byte[]? ValueBytes
    {
        get => _valueBytes;
        set => _valueBytes = value;
    }
}
