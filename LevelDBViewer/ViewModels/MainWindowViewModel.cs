using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelDBViewer.Models;
using LevelDBViewer.Services;

namespace LevelDBViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LevelDbService _levelDbService = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "No database opened";

    [ObservableProperty]
    private bool _isDatabaseOpen;

    [ObservableProperty]
    private int _totalEntries;

    private ObservableCollection<LevelDbEntry> _allEntries = new();
    
    [ObservableProperty]
    private ObservableCollection<LevelDbEntry> _filteredEntries = new();

    partial void OnSearchTextChanged(string value)
    {
        FilterEntries();
    }

    [RelayCommand]
    private async Task OpenDatabaseAsync()
    {
        try
        {
            if (FolderPickerFunc == null)
            {
                StatusMessage = "Error: Folder picker not initialized";
                return;
            }

            var folder = await FolderPickerFunc();
            if (folder == null)
                return;

            var path = folder.Path.LocalPath;
            StatusMessage = $"Opening database: {path}...";

            await Task.Run(() =>
            {
                _levelDbService.OpenDatabase(path);
                _allEntries = new ObservableCollection<LevelDbEntry>(_levelDbService.GetAllEntries());
            });

            IsDatabaseOpen = true;
            TotalEntries = _allEntries.Count;
            StatusMessage = $"Database opened: {path} ({TotalEntries} entries)";
            
            FilterEntries();
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsDatabaseOpen = false;
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
        StatusMessage = "Database closed";
    }

    private void FilterEntries()
    {
        FilteredEntries.Clear();
        
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var entry in _allEntries)
            {
                FilteredEntries.Add(entry);
            }
        }
        else
        {
            var searchLower = SearchText.ToLower();
            var filtered = _allEntries.Where(e =>
                e.Key.ToLower().Contains(searchLower) ||
                e.Value.ToLower().Contains(searchLower) ||
                e.KeyHex.ToLower().Contains(searchLower) ||
                e.ValueHex.ToLower().Contains(searchLower)
            );
            
            foreach (var entry in filtered)
            {
                FilteredEntries.Add(entry);
            }
        }
    }

    // Property to store folder picker function from view
    public System.Func<Task<IStorageFolder?>>? FolderPickerFunc { get; set; }
}
