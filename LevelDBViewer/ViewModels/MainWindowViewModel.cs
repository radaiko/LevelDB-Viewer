using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelDBViewer.Models;
using LevelDBViewer.Services;

namespace LevelDBViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LevelDbService _levelDbService = new();
    private CancellationTokenSource? _filterCancellationTokenSource;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "No database opened";

    [ObservableProperty]
    private bool _isDatabaseOpen;

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private bool _isCorruptionDetected;

    [ObservableProperty]
    private string _lastDatabasePath = string.Empty;

    private List<LevelDbEntry> _allEntries = new();
    
    [ObservableProperty]
    private ObservableCollection<LevelDbEntry> _filteredEntries = new();

    partial void OnSearchTextChanged(string value)
    {
        // Debounce search: cancel previous filter and start new one with delay
        _filterCancellationTokenSource?.Cancel();
        _filterCancellationTokenSource?.Dispose();
        _filterCancellationTokenSource = new CancellationTokenSource();
        var token = _filterCancellationTokenSource.Token;
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token); // 300ms debounce
                if (!token.IsCancellationRequested)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => FilterEntries());
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when search is cancelled, ignore
            }
        }, token);
    }

    [RelayCommand]
    private async Task OpenDatabaseAsync()
    {
        if (FolderPickerFunc == null)
        {
            StatusMessage = "Error: Folder picker not initialized";
            return;
        }

        var folder = await FolderPickerFunc();
        if (folder == null)
            return;

        await OpenDatabaseFromFolderAsync(folder);
    }

    [RelayCommand]
    private async Task OpenDatabaseFromFileAsync()
    {
        if (FilePickerFunc == null)
        {
            StatusMessage = "Error: File picker not initialized";
            return;
        }

        var folder = await FilePickerFunc();
        if (folder == null)
            return;

        await OpenDatabaseFromFolderAsync(folder);
    }

    private async Task OpenDatabaseFromFolderAsync(IStorageFolder folder)
    {
        var path = folder.Path.LocalPath;
        LastDatabasePath = path;
        IsCorruptionDetected = false;
        StatusMessage = $"Opening database: {path}...";

        try
        {
            // Load entries on background thread - keep as List (not ObservableCollection)
            _allEntries = await Task.Run(() =>
            {
                _levelDbService.OpenDatabase(path);
                return _levelDbService.GetAllEntries();
            });
            
            IsDatabaseOpen = true;
            TotalEntries = _allEntries.Count;
            
            // Create FilteredEntries collection on UI thread
            FilterEntries();
            UpdateDatabaseStatusMessage();
        }
        catch (System.Exception ex)
        {
            IsDatabaseOpen = false;
            IsCorruptionDetected = IsCorruptionError(ex);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CloseDatabase()
    {
        _levelDbService.Close();
        _allEntries.Clear();
        FilteredEntries.Clear();
        IsDatabaseOpen = false;
        TotalEntries = 0;
        SearchText = string.Empty;
        IsCorruptionDetected = false;
        StatusMessage = "Database closed";
    }

    [RelayCommand]
    private async Task RepairDatabaseAsync()
    {
        if (string.IsNullOrEmpty(LastDatabasePath))
        {
            StatusMessage = "Error: No database path available for repair";
            return;
        }

        try
        {
            StatusMessage = $"Repairing database: {LastDatabasePath}...";
            
            await Task.Run(() =>
            {
                _levelDbService.RepairDatabase(LastDatabasePath);
            });

            StatusMessage = $"Database repaired successfully. Attempting to reopen...";
            IsCorruptionDetected = false;

            // Try to open the repaired database - keep as List
            _allEntries = await Task.Run(() =>
            {
                _levelDbService.OpenDatabase(LastDatabasePath);
                return _levelDbService.GetAllEntries();
            });
            
            IsDatabaseOpen = true;
            TotalEntries = _allEntries.Count;
            
            FilterEntries();
            var message = $"Database repaired and reopened: {LastDatabasePath} ({TotalEntries} entries)";
            if (FilteredEntries.Count != TotalEntries)
            {
                message += $" - Showing {FilteredEntries.Count} filtered";
            }
            StatusMessage = message;
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Repair failed: {ex.Message}";
            IsDatabaseOpen = false;
            IsCorruptionDetected = IsCorruptionError(ex);
        }
    }

    private void FilterEntries()
    {
        IEnumerable<LevelDbEntry> filtered;
        
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = _allEntries;
        }
        else
        {
            var searchLower = SearchText.ToLower();
            filtered = _allEntries.Where(e =>
                e.Key.ToLower().Contains(searchLower) ||
                e.Value.ToLower().Contains(searchLower)
            );
        }
        
        // Create new ObservableCollection to trigger property change notification
        FilteredEntries = new ObservableCollection<LevelDbEntry>(filtered);
        
        // Update status when filtering
        if (IsDatabaseOpen)
        {
            UpdateDatabaseStatusMessage();
        }
    }

    private void UpdateDatabaseStatusMessage()
    {
        var message = $"Database opened: {LastDatabasePath} ({TotalEntries} entries)";
        if (FilteredEntries.Count != TotalEntries)
        {
            message += $" - Showing {FilteredEntries.Count} filtered";
        }
        StatusMessage = message;
    }

    private static bool IsCorruptionError(System.Exception ex)
    {
        return ex.Message.Contains("Corruption", System.StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("corrupted", System.StringComparison.OrdinalIgnoreCase);
    }

    // Properties to store picker functions from view
    public System.Func<Task<IStorageFolder?>>? FolderPickerFunc { get; set; }
    public System.Func<Task<IStorageFolder?>>? FilePickerFunc { get; set; }
}
