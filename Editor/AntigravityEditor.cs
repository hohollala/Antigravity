using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AntigravityEditor : IExternalCodeEditor
{
    const string EditorName = "Antigravity";
    const string EditorPathKey = "AntigravityEditorPath";

    static AntigravityEditor()
    {
        CodeEditor.Register(new AntigravityEditor());
    }

    public CodeEditor.Installation[] Installations => new[]
    {
        new CodeEditor.Installation
        {
            Name = EditorName,
            Path = GetAntigravityPath()
        }
    };

    /// <summary>
    /// Antigravity 실행 파일의 경로를 자동으로 감지하거나 반환합니다.
    /// 1. EditorPrefs에 저장된 경로 확인
    /// 2. 시스템 PATH에서 자동 감지
    /// 3. 공통 설치 경로 확인
    /// </summary>
    private static string GetAntigravityPath()
    {
        // 1. 이전에 저장된 경로 확인
        string savedPath = EditorPrefs.GetString(EditorPathKey, "");
        if (!string.IsNullOrEmpty(savedPath))
        {
            return savedPath;
        }

        // 2. 시스템 PATH에서 자동 감지
        string detectedPath = DetectAntigravityInPath();
        if (!string.IsNullOrEmpty(detectedPath))
        {
            EditorPrefs.SetString(EditorPathKey, detectedPath);
            return detectedPath;
        }

        // 3. 공통 설치 경로 확인
        string[] commonPaths = GetCommonInstallPaths();
        foreach (string path in commonPaths)
        {
            if (File.Exists(path))
            {
                EditorPrefs.SetString(EditorPathKey, path);
                return path;
            }
        }

        // 저장된 경로 반환
        string saved = EditorPrefs.GetString(EditorPathKey, "");
        if (!string.IsNullOrEmpty(saved))
        {
            return saved;
        }

        // 감지 실패 시에도 목록에 표시되도록 기본 경로 반환
        if (IsWindows()) return "C:\\Program Files\\Antigravity\\antigravity.exe";
        if (IsMac()) return "/Applications/Antigravity.app/Contents/MacOS/Antigravity"; // Mac 기본 경로 수정
        return "/usr/bin/antigravity"; // Linux 기본 경로
    }

    /// <summary>
    /// 시스템 PATH에서 Antigravity를 자동으로 감지합니다.
    /// </summary>
    private static string DetectAntigravityInPath()
    {
        try
        {
            string command = IsWindows() ? "where" : "which";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "antigravity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                string firstLine = output.Split('\n')[0].Trim();
                if (File.Exists(firstLine))
                {
                    return firstLine;
                }
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// 운영체제별 공통 Antigravity 설치 경로를 반환합니다.
    /// </summary>
    private static string[] GetCommonInstallPaths()
    {
        if (IsWindows())
        {
            return new[]
            {
                "C:\\Program Files\\Antigravity\\antigravity.exe",
                "C:\\Program Files (x86)\\Antigravity\\antigravity.exe",
                Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "AppData\\Local\\Programs\\Antigravity\\antigravity.exe"),
                Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "scoop\\apps\\antigravity\\current\\antigravity.exe"),
                Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "scoop\\shims\\antigravity.exe"),
            };
        }
        else if (IsMac())
        {
            return new[]
            {
                "/usr/local/bin/antigravity",
                "/opt/homebrew/bin/antigravity",
                "/usr/bin/antigravity",
                Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), ".cargo/bin/antigravity"),
            };
        }
        else // Linux
        {
            return new[]
            {
                "/usr/local/bin/antigravity",
                "/usr/bin/antigravity",
                Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), ".cargo/bin/antigravity"),
                Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), ".local/bin/antigravity"),
            };
        }
    }

    private static bool IsWindows() => Application.platform == RuntimePlatform.WindowsEditor;
    private static bool IsMac() => Application.platform == RuntimePlatform.OSXEditor;

    public void Initialize(string editorInstallationPath)
    {
        // 사용자가 선택한 경로 저장
        if (!string.IsNullOrEmpty(editorInstallationPath))
        {
            EditorPrefs.SetString(EditorPathKey, editorInstallationPath);
            Debug.Log($"Antigravity Editor path saved: {editorInstallationPath}");
        }
    }

    public void OnGUI()
    {
        // Preferences > External Tools의 UI 섹션
        EditorGUILayout.LabelField("Antigravity Editor Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        string currentPath = EditorPrefs.GetString(EditorPathKey, "");

        // 경로 표시
        EditorGUILayout.LabelField("Executable Path", EditorStyles.label);
        EditorGUILayout.SelectableLabel(currentPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        EditorGUILayout.Space();

        // 경로 변경 버튼
        if (GUILayout.Button("Browse for Antigravity Executable", GUILayout.Width(250)))
        {
            string browseStartPath = string.IsNullOrEmpty(currentPath) ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) : Path.GetDirectoryName(currentPath);
            string filter = IsWindows() ? "exe" : "";
            string newPath = EditorUtility.OpenFilePanel("Select Antigravity Executable", browseStartPath, filter);

            if (!string.IsNullOrEmpty(newPath))
            {
                EditorPrefs.SetString(EditorPathKey, newPath);
                Debug.Log($"Antigravity Editor path updated: {newPath}");
            }
        }

        EditorGUILayout.Space();

        // 자동 감지 버튼
        if (GUILayout.Button("Auto-detect Antigravity Path", GUILayout.Width(250)))
        {
            string detectedPath = DetectAntigravityInPath();
            if (!string.IsNullOrEmpty(detectedPath))
            {
                EditorPrefs.SetString(EditorPathKey, detectedPath);
                Debug.Log($"Antigravity Editor auto-detected at: {detectedPath}");
            }
            else
            {
                EditorUtility.DisplayDialog("Auto-detection Failed", "Could not automatically find Antigravity. Please browse manually.", "OK");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("The path to the Antigravity executable. Leave empty to auto-detect.", MessageType.Info);
    }

    public bool OpenProject(string filePath, int line, int column)
    {
        // 파일을 Antigravity에서 열기
        string arguments = ParseArguments(filePath, line, column);
        return OpenApp(arguments);
    }

    public void SyncAll()
    {
        // 필요시 프로젝트 파일 동기화 (예: .csproj 생성)
    }

    public void SyncIfNeeded(string[] added, string[] deleted, string[] moved, string[] movedFrom, string[] imported)
    {
        // 파일 변경 처리
    }

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
        if (!string.IsNullOrEmpty(editorPath) && editorPath.ToLower().Contains("antigravity"))
        {
            installation = new CodeEditor.Installation
            {
                Name = EditorName,
                Path = editorPath
            };
            return true;
        }

        installation = default;
        return false;
    }

    /// <summary>
    /// Antigravity CLI의 인자 형식을 생성합니다.
    /// 형식: "file_path":line:column
    /// 필요시 이 메서드를 수정하여 다른 형식을 지원할 수 있습니다.
    /// </summary>
    private string ParseArguments(string filePath, int line, int column)
    {
        // Antigravity 기본 형식: "file.cs":10:5
        return $"\"{filePath}\":{line}:{column}";

        // 다른 형식 예시들:
        // return $"open \"{filePath}\" --line {line} --column {column}";
        // return $"--open \"{filePath}\" --goto {line}:{column}";
    }

    /// <summary>
    /// Antigravity 프로세스를 시작합니다.
    /// </summary>
    private bool OpenApp(string arguments)
    {
        string appPath = EditorPrefs.GetString(EditorPathKey);

        if (string.IsNullOrEmpty(appPath))
        {
            Debug.LogError("Antigravity executable path not set. Please configure it in Preferences > External Tools > Antigravity Editor Settings");
            return false;
        }

        if (!File.Exists(appPath))
        {
            Debug.LogError($"Antigravity executable not found at: {appPath}");
            return false;
        }

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to launch Antigravity: {e.Message}");
            return false;
        }
    }
}
