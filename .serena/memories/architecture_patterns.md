# Antigravity Editor - Architecture & Design Patterns

## High-Level Architecture

### Component Interaction Flow
```
Unity Editor
    ↓
IExternalCodeEditor Interface
    ↓
AntigravityEditor Class (Singleton via [InitializeOnLoad])
    ├── Path Detection Layer
    │   ├── EditorPrefs (cached settings)
    │   ├── PATH Detection (which/where)
    │   └── Common Paths Search (OS-specific)
    ├── UI Layer
    │   ├── Preferences Rendering
    │   ├── Browse Button Handler
    │   └── Auto-detect Button Handler
    └── Execution Layer
        ├── Process Management
        ├── Argument Parsing
        └── Error Handling
```

## Design Patterns

### 1. Singleton Pattern
**Implementation**: `[InitializeOnLoad]` static constructor
```csharp
[InitializeOnLoad]
public class AntigravityEditor : IExternalCodeEditor
{
    static AntigravityEditor()
    {
        CodeEditor.Register(new AntigravityEditor());
    }
}
```
**Purpose**: Ensures single instance auto-registers with Unity's CodeEditor system on editor load

### 2. Registry Pattern
**Implementation**: `CodeEditor.Register()` call in static constructor
**Purpose**: Integrates with Unity's external editor discovery system

### 3. Strategy Pattern
**Implementation**: OS-specific path detection methods
- `GetCommonInstallPaths()` - Strategy based on platform
- `DetectAntigravityInPath()` - Unified "which/where" strategy
**Purpose**: Encapsulates platform-specific behavior

### 4. Template Method Pattern
**Implementation**: `IExternalCodeEditor` interface methods
**Purpose**: Defines skeleton of editor integration, Unity fills in specifics

### 5. Facade Pattern
**Implementation**: `AntigravityEditor` wraps Process/EditorPrefs complexity
**Purpose**: Provides simple UI while hiding process management details

## Key Architectural Decisions

### 1. EditorPrefs Caching
**Decision**: Store user-selected path in EditorPrefs
**Rationale**: Persistent across editor sessions, faster than repeated detection
**Trade-off**: Stale paths if Antigravity moved; mitigated by auto-detect button

### 2. PATH-First Detection
**Decision**: Check system PATH before common paths
**Rationale**: Respects user's environment configuration (Homebrew, Cargo, etc.)
**Trade-off**: Requires process spawning (slight performance cost); negligible for editor startup

### 3. Editor-Only Assembly
**Decision**: Separate `.asmdef` with `includePlatforms: [Editor]`
**Rationale**: Code excluded from player builds, smaller package size
**Trade-off**: Cannot use in runtime scripts; acceptable for editor-only integration

### 4. UseShellExecute = false
**Decision**: Launch process without shell
**Rationale**: Security best practice, prevents injection attacks
**Trade-off**: More complex environment variable handling; worth it

## Extension Points

### 1. Custom CLI Arguments
**File**: `AntigravityEditor.cs:ParseArguments()`
**Current Format**: `"file.cs":line:column`
**Customization**: Change return value for different CLI syntax

### 2. Additional Installation Paths
**File**: `AntigravityEditor.cs:GetCommonInstallPaths()`
**Current**: Windows/macOS/Linux paths
**Extension**: Add Flatpak, Snap, AppImage, or other distribution methods

### 3. UI Enhancement
**File**: `AntigravityEditor.cs:OnGUI()`
**Current**: Path field + 2 buttons
**Extension**: Add version detection, health check, advanced options

### 4. Platform Detection
**File**: `AntigravityEditor.cs:IsWindows(), IsMac()`
**Enhancement**: Could add more granular platform checks if needed

## Dependencies & Integration Points

### Unity APIs Used
- `IExternalCodeEditor` - CodeEditor integration interface
- `EditorPrefs` - Persistent editor preferences
- `EditorGUILayout` - Preferences UI rendering
- `EditorUtility.OpenFilePanel()` - File browser dialog
- `Application.platform` - Platform detection
- `Process` & `ProcessStartInfo` - External process management
- `[InitializeOnLoad]` - Auto-initialization on editor load

### External Dependencies
- None (uses only Unity built-in APIs)

### File System Integration
- Checks PATH environment variable
- Scans file system for common installation paths
- No file modification (read-only)

## Performance Considerations

### Path Detection Timing
1. **First time**: Full detection (PATH scan + common paths) → ~100-200ms
2. **Cached**: Direct EditorPrefs lookup → ~1ms
3. **Auto-detect button click**: Re-scan PATH → ~100-200ms

**Optimization**: EditorPrefs caching prevents repeated detection

### Process Startup
- `UseShellExecute = false` + `CreateNoWindow = true` optimizes launch
- File existence checks before launching prevent error dialogs

### Memory Impact
- Minimal: Single class instance, small UI state
- No memory leaks: Proper process cleanup via .NET GC

## Security Considerations

### Input Validation
- File paths: Quoted in CLI arguments to prevent injection
- EditorPrefs: User-selected via file dialog only
- Executable check: `File.Exists()` validation before launch

### Process Execution
- `UseShellExecute = false` prevents shell injection
- `RedirectStandardOutput/Error = true` captures process output
- `CreateNoWindow = true` prevents console window

### Data Privacy
- No telemetry or external communication
- EditorPrefs stored locally only
- No network access

## Future Enhancement Opportunities

1. **Version Detection**: Query Antigravity `--version` to validate installation
2. **Health Check**: Verify executable is actually Antigravity
3. **Multi-Version Support**: Allow different Antigravity versions per project
4. **Integration Testing**: Automated test suite for cross-platform paths
5. **Performance Profiling**: UI startup time analysis
6. **Advanced Options**: Custom CLI arguments, working directory configuration
