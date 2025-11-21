# LevelDB Viewer

A modern, cross-platform desktop application for viewing and searching LevelDB database files, built with Avalonia UI and C#.

## Features

- 🗂️ **Open LevelDB databases** - Browse any LevelDB database directory
- 🔍 **Search functionality** - Search through keys, values, and hex representations
- 🌙 **Dark mode support** - Modern dark theme by default (supports light mode too)
- 📊 **Data grid view** - View key-value pairs with both string and hex representations
- 🏗️ **MVVM architecture** - Clean architecture using CommunityToolkit.Mvvm
- 🖥️ **Cross-platform** - Works on Windows, macOS, and Linux

## Screenshots

The application features a clean, modern interface with:
- Toolbar with Open/Close database buttons
- Search bar for filtering entries
- DataGrid displaying keys and values (both as strings and hex)
- Status bar showing database information

## Requirements

- .NET 9.0 or later
- Works on Windows, macOS, and Linux

## Building the Application

```bash
cd LevelDBViewer
dotnet build
```

## Running the Application

```bash
cd LevelDBViewer
dotnet run
```

Or build and run the published version:

```bash
cd LevelDBViewer
dotnet publish -c Release
# Navigate to bin/Release/net9.0/publish and run the executable
```

## Usage

1. **Open a Database**: Click the "Open Database" button and select a LevelDB database directory
2. **Browse Data**: View all key-value pairs in the data grid
3. **Search**: Use the search box to filter entries by key, value, or hex representation
4. **View Formats**: Each entry shows both string and hexadecimal representations
5. **Close Database**: Click "Close Database" when done

## Architecture

The application follows the MVVM (Model-View-ViewModel) pattern:

- **Models**: `LevelDbEntry` - Represents a key-value pair with string and hex representations
- **ViewModels**: `MainWindowViewModel` - Contains the business logic, observable properties, and commands
- **Views**: `MainWindow` - The UI layout defined in AXAML
- **Services**: `LevelDbService` - Handles LevelDB operations

### Key Technologies

- **Avalonia UI 11.3.9** - Cross-platform XAML-based UI framework
- **CommunityToolkit.Mvvm 8.2.1** - Modern MVVM framework with source generators
- **LevelDB.Standard 2.1.6.1** - LevelDB database library for .NET
- **FluentTheme** - Modern UI theme with dark mode support

## Project Structure

```
LevelDBViewer/
├── App.axaml                      # Application entry point and theme configuration
├── App.axaml.cs                   # Application code-behind
├── Program.cs                     # Main entry point
├── ViewLocator.cs                 # View-ViewModel locator
├── Models/
│   └── LevelDbEntry.cs            # Data model for LevelDB entries
├── ViewModels/
│   ├── ViewModelBase.cs           # Base class for all ViewModels
│   └── MainWindowViewModel.cs     # Main window ViewModel
├── Views/
│   ├── MainWindow.axaml           # Main window UI definition
│   └── MainWindow.axaml.cs        # Main window code-behind
└── Services/
    └── LevelDbService.cs          # LevelDB operations service
```

## Development

### Adding Features

The application uses CommunityToolkit.Mvvm source generators for cleaner code:

- Use `[ObservableProperty]` attribute for properties that need change notification
- Use `[RelayCommand]` attribute for command methods
- All ViewModels inherit from `ViewModelBase` which extends `ObservableObject`

### Theme Customization

To change the theme, edit `App.axaml`:
- `RequestedThemeVariant="Dark"` - Dark mode
- `RequestedThemeVariant="Light"` - Light mode  
- `RequestedThemeVariant="Default"` - Follow system theme

## Creating a Test Database

You can create a test LevelDB database using the LevelDB.Standard library:

```csharp
using LevelDB;

var options = new Options { CreateIfMissing = true };
using var db = new DB(options, "/path/to/database");

db.Put("key1", "value1");
db.Put("key2", "value2");
```

## Troubleshooting

### macOS: "Unable to load shared library 'leveldb.dll'"

If you encounter this error on macOS, try one of these solutions:

**Option 1: Run from published build**
```bash
cd LevelDBViewer
dotnet publish -c Release -r osx-x64 --self-contained false
cd bin/Release/net9.0/osx-x64/publish
./LevelDBViewer
```

**Option 2: Run with explicit runtime**
```bash
cd LevelDBViewer
dotnet run --runtime osx-x64
```

**Option 3: For Apple Silicon Macs**

The LevelDB.Standard library currently only provides x64 binaries for macOS. On Apple Silicon Macs:
- Install Rosetta 2 if not already installed: `softwareupdate --install-rosetta`
- Use the x64 runtime as shown above

**Note**: The native libraries are located in `bin/Debug/net9.0/runtimes/osx-x64/native/leveldb.dll` after building.

### Linux: Missing Native Libraries

If you encounter library loading issues on Linux:
```bash
cd LevelDBViewer
dotnet publish -c Release -r linux-x64 --self-contained false
cd bin/Release/net9.0/linux-x64/publish
./LevelDBViewer
```

## License

See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.