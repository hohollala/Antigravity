# Development Commands for Antigravity Editor

## Code Style & Conventions
- **Language**: C# with .NET/Unity conventions
- **Naming**: PascalCase for classes/methods, camelCase for locals, CONSTANT_CASE for constants
- **Indentation**: 4 spaces
- **Type Hints**: Always use explicit types (Unity Editor coding standard)
- **Docstrings**: XML documentation comments (`///`) for public members
- **Pattern**: Use Unity Editor coding style from official documentation

## Build & Compilation
```bash
# Unity automatically compiles scripts when changes detected
# No explicit build command needed for Editor scripts

# Verify syntax (in Unity Editor)
# Edit > Preferences > External Tools > check for compilation errors
```

## Testing & Validation
```bash
# Manual testing in Unity Editor:
1. Create empty Unity project (2019.4+)
2. Install package: Window > TextAsset > Package Manager > Git URL
3. Set path: Edit > Preferences > External Tools > Antigravity > Auto-detect
4. Double-click any C# script to verify it opens in Antigravity

# Cross-platform testing checklist:
- [ ] Test on Windows (Scoop installation path detection)
- [ ] Test on macOS (Homebrew/Cargo path detection)
- [ ] Test on Linux (APT/Cargo path detection)
- [ ] Test auto-detection vs manual Browse
- [ ] Test with missing executable (error handling)
- [ ] Test with empty EditorPrefs (first-time setup)
```

## Documentation & Formatting
```bash
# Documentation files (already created):
- README.md              # Main user guide
- QUICK_START.md         # 5-minute setup
- INSTALL.md             # Detailed troubleshooting
- CHANGELOG.md           # Version history
- PROJECT_SUMMARY.md     # Project overview

# Update docs when:
- Adding new feature
- Fixing bug (add to CHANGELOG.md)
- Changing version (update package.json version)
```

## Git Workflow
```bash
# Repository is at: /Users/supermac/Dev/aiProjects/Antigravity/com.antigravity.editor

# Status check
git status
git log --oneline

# Making changes
git add Editor/
git commit -m "Fix: description or Feature: description"

# Tags for releases
git tag v1.0.0
git tag v1.0.1

# Note: Repository URL placeholder in package.json:
# Replace "yourusername" with actual GitHub username before publishing
```

## Linting & Code Quality
```bash
# C# code style validation:
- Use Visual Studio / Rider for real-time linting
- Follow .editorconfig if present (none currently)
- Check Unity coding standards:
  * https://docs.unity3d.com/Manual/CodingStandardsAndConventions.html

# No automated linting tools configured
# Manual review checklist:
- [ ] All public methods have XML documentation
- [ ] Error messages are user-friendly
- [ ] Edge cases handled (null paths, missing executables)
- [ ] Cross-platform paths use Path.Combine()
- [ ] UseShellExecute = false for security
```

## Common Development Tasks

### Debugging
```csharp
// Debug output in Unity Editor Console:
Debug.Log("Info message");           // Info
Debug.LogWarning("Warning message"); // Yellow
Debug.LogError("Error message");     // Red

// Access EditorPrefs values:
string path = EditorPrefs.GetString("AntigravityEditorPath", "");
EditorPrefs.SetString("AntigravityEditorPath", newPath);
EditorPrefs.DeleteKey("AntigravityEditorPath");
```

### Adding Platform Support
```csharp
// In GetCommonInstallPaths():
// Add platform check
if (IsWindows()) { /* Windows paths */ }
else if (IsMac()) { /* macOS paths */ }
else { /* Linux paths */ }

// Helper methods already exist:
private static bool IsWindows() => Application.platform == RuntimePlatform.WindowsEditor;
private static bool IsMac() => Application.platform == RuntimePlatform.OSXEditor;
```

### Customizing CLI Arguments
```csharp
// In ParseArguments() method:
// Default: "file.cs":10:5
private string ParseArguments(string filePath, int line, int column)
{
    // Modify format as needed for Antigravity CLI
    return $"\\\"{filePath}\\\":{line}:{column}\";
}
```

## Deployment Checklist
Before releasing new version:
- [ ] Update version in package.json
- [ ] Update CHANGELOG.md with new features/fixes
- [ ] Test on all supported platforms
- [ ] Run documentation verification
- [ ] Create git tag: `git tag vX.Y.Z`
- [ ] Create GitHub Release with version notes
- [ ] Test installation via git URL in fresh Unity project
