using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LevelDBViewer.ViewModels;

namespace LevelDBViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Wire up the pickers when DataContext is set
        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.FolderPickerFunc = PickFolderAsync;
                viewModel.FilePickerFunc = PickFileAsync;
            }
        };
    }

    private async Task<IStorageFolder?> PickFolderAsync()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select LevelDB Database Folder",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0] : null;
    }

    private async Task<IStorageFolder?> PickFileAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Any File in LevelDB Database Directory",
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            // Get the parent folder of the selected file
            var file = files[0];
            var parentPath = System.IO.Path.GetDirectoryName(file.Path.LocalPath);
            if (!string.IsNullOrEmpty(parentPath))
            {
                return await StorageProvider.TryGetFolderFromPathAsync(new System.Uri(parentPath));
            }
        }

        return null;
    }
}