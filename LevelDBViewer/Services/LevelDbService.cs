using System;
using System.Collections.Generic;
using System.Text;
using LevelDB;
using LevelDBViewer.Models;

namespace LevelDBViewer.Services;

public class LevelDbService : IDisposable
{
    private DB? _database;
    private bool _disposed;

    public bool IsOpen => _database != null;

    public void OpenDatabase(string path)
    {
        Close();

        var options = new Options { CreateIfMissing = false };
        _database = new DB(options, path);
    }

    public List<LevelDbEntry> GetAllEntries()
    {
        if (_database == null)
            return new List<LevelDbEntry>();

        var entries = new List<LevelDbEntry>();
        using var iterator = _database.CreateIterator();
        iterator.SeekToFirst();

        while (iterator.IsValid())
        {
            var keyBytes = iterator.Key();
            var valueBytes = iterator.Value();

            entries.Add(new LevelDbEntry
            {
                Key = TryDecodeUtf8(keyBytes),
                Value = TryDecodeUtf8(valueBytes),
                KeyHex = BitConverter.ToString(keyBytes).Replace("-", " "),
                ValueHex = BitConverter.ToString(valueBytes).Replace("-", " ")
            });

            iterator.Next();
        }

        return entries;
    }

    private static string TryDecodeUtf8(byte[] bytes)
    {
        try
        {
            var str = Encoding.UTF8.GetString(bytes);
            // Check if the string contains only printable characters
            if (IsPrintable(str))
                return str;
            
            return $"[Binary: {bytes.Length} bytes]";
        }
        catch
        {
            return $"[Binary: {bytes.Length} bytes]";
        }
    }

    private static bool IsPrintable(string str)
    {
        foreach (var c in str)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                return false;
        }
        return true;
    }

    public void Close()
    {
        _database?.Dispose();
        _database = null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Close();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
