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
        
        // Wire up the folder picker when DataContext is set
        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.FolderPickerFunc = PickFolderAsync;
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
}