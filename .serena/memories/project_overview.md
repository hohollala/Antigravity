# Antigravity Editor Integration - Project Overview

## Purpose
A professional UPM (Unity Package Manager) package that integrates **Antigravity** as an external code editor for Unity. Implements `IExternalCodeEditor` interface to allow developers to use Antigravity as their default C# editor in Unity projects.

## Tech Stack
- **Language**: C# (.NET/Unity)
- **Framework**: Unity (2019.4 LTS or newer)
- **Package Format**: UPM (Unity Package Manager)
- **Assembly Type**: Editor-only (.asmdef)
- **Platform Support**: Windows, macOS, Linux
- **Runtime**: .NET Framework (via Unity)

## Project Structure
```
com.antigravity.editor/
├── Editor/
│   ├── AntigravityEditor.cs          # Main implementation (~270 lines)
│   └── Antigravity.Editor.asmdef     # Editor-only assembly definition
├── package.json                       # UPM metadata (v1.0.0)
├── README.md                          # Main documentation (Korean + English)
├── QUICK_START.md                     # 5-minute quickstart guide
├── INSTALL.md                         # Detailed installation & troubleshooting
├── CHANGELOG.md                       # Version history
├── LICENSE                            # MIT License
└── .gitignore                         # Git exclusions
```

## Core Architecture

### AntigravityEditor.cs - Main Class
Implements `IExternalCodeEditor` interface with 5 key responsibilities:

1. **Path Detection** (`GetAntigravityPath()`):
   - Checks EditorPrefs for saved path
   - Auto-detects via system PATH (`which`/`where`)
   - Scans common installation directories per OS
   
2. **Platform Support** (`GetCommonInstallPaths()`):
   - Windows: Program Files, AppData, Scoop
   - macOS: /usr/local/bin, /opt/homebrew, ~/.cargo
   - Linux: /usr/local/bin, /usr/bin, ~/.local/bin
   
3. **Preferences UI** (`OnGUI()`):
   - Shows current path
   - "Browse" button for manual selection
   - "Auto-detect" button for automatic discovery
   
4. **File Opening** (`OpenProject()`):
   - Opens files with line:column info
   - Parses arguments in format: "file.cs":line:column
   
5. **Process Management** (`OpenApp()`):
   - Launches Antigravity process
   - Error handling with Debug.LogError

### Design Patterns Used
- **Singleton** via `[InitializeOnLoad]` static constructor
- **Strategy** for OS-specific path detection
- **Template Method** for IExternalCodeEditor interface
- **Registry** pattern through Unity's CodeEditor.Register()
