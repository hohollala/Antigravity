using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class AntigravityMenus
    {
        [MenuItem("Antigravity/launch.json")]
        public static void GenerateLaunchJson()
        {
            try
            {
                // 프로젝트 루트 경로 가져오기
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string vscodeDir = Path.Combine(projectRoot, ".vscode");
                string launchJsonPath = Path.Combine(vscodeDir, "launch.json");

                // .vscode 폴더 생성 (없으면)
                if (!Directory.Exists(vscodeDir))
                {
                    Directory.CreateDirectory(vscodeDir);
                }

                // launch.json 콘텐츠
                string launchJsonContent;
#if UNITY_EDITOR_OSX
                launchJsonContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Unity Editor"",
            ""type"": ""unity"",
            ""request"": ""attach""
        }
    ]
}";
#else
                // 플랫폼별 Unity Editor 경로 설정
                string unityEditorPath;
#if UNITY_EDITOR_WIN
                unityEditorPath = "${workspaceFolder}/Library/UnityEditor.exe";
#else
                unityEditorPath = "${workspaceFolder}/Library/UnityEditor";
#endif

                launchJsonContent = @$"{{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {{
            ""name"": ""Unity Editor"",
            ""type"": ""unity"",
            ""request"": ""launch"",
            ""program"": ""{unityEditorPath}""
        }}
    ]
}}";
#endif

                // 파일 생성 또는 덮어쓰기
                File.WriteAllText(launchJsonPath, launchJsonContent);

                Debug.Log($"Antigravity launch.json created successfully at: {launchJsonPath}");
                EditorUtility.DisplayDialog("Success", $".vscode/launch.json 파일이 생성되었습니다.\n\n경로: {launchJsonPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create launch.json: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"launch.json 생성 실패:\n{ex.Message}", "OK");
            }
        }

        [MenuItem("Antigravity/settings.json")]
        public static void GenerateSettingsJson()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string vscodeDir = Path.Combine(projectRoot, ".vscode");
                string settingsJsonPath = Path.Combine(vscodeDir, "settings.json");

                if (!Directory.Exists(vscodeDir))
                {
                    Directory.CreateDirectory(vscodeDir);
                }

                string solutionName = Path.GetFileName(projectRoot) + ".sln";
                var slnFiles = Directory.GetFiles(projectRoot, "*.sln");
                if (slnFiles.Length > 0)
                {
                    solutionName = Path.GetFileName(slnFiles[0]);
                }

                string settingsJsonContent = $@"{{
    ""dotnet.preferCSharpExtension"": true,
    ""files.exclude"": {{
        ""**/.DS_Store"": true,
        ""**/.git"": true,
        ""**/.vs"": true,
        ""**/.gitmodules"": true,
        ""**/.vsconfig"": true,
        ""**/*.booproj"": true,
        ""**/*.pidb"": true,
        ""**/*.suo"": true,
        ""**/*.user"": true,
        ""**/*.userprefs"": true,
        ""**/*.unityproj"": true,
        ""**/*.dll"": true,
        ""**/*.exe"": true,
        ""**/*.pdf"": true,
        ""**/*.mid"": true,
        ""**/*.midi"": true,
        ""**/*.wav"": true,
        ""**/*.gif"": true,
        ""**/*.ico"": true,
        ""**/*.jpg"": true,
        ""**/*.jpeg"": true,
        ""**/*.png"": true,
        ""**/*.psd"": true,
        ""**/*.tga"": true,
        ""**/*.tif"": true,
        ""**/*.tiff"": true,
        ""**/*.3ds"": true,
        ""**/*.3DS"": true,
        ""**/*.fbx"": true,
        ""**/*.FBX"": true,
        ""**/*.lxo"": true,
        ""**/*.LXO"": true,
        ""**/*.ma"": true,
        ""**/*.MA"": true,
        ""**/*.obj"": true,
        ""**/*.OBJ"": true,
        ""**/*.asset"": true,
        ""**/*.cubemap"": true,
        ""**/*.flare"": true,
        ""**/*.mat"": true,
        ""**/*.meta"": true,
        ""**/*.prefab"": true,
        ""**/*.unity"": true,
        ""build/"": true,
        ""Build/"": true,
        ""Library/"": true,
        ""library/"": true,
        ""obj/"": true,
        ""Obj/"": true,
        ""Logs/"": true,
        ""logs/"": true,
        ""ProjectSettings/"": true,
        ""UserSettings/"": true,
        ""temp/"": true,
        ""Temp/"": true,
        ""**/*.log"": true,
        ""**/*.unitypackage"": true,
        ""**/*.txt"": true,
        ""**/*.asmdef"": true,
        ""**/*.tmp"": true,
        ""node_modules"": true,
        ""dist"": true,
        ""*.meta"": true,
        ""secret_file.txt"": true,
        ""Build"": true,
        ""Library"": true,
        ""*.gitignore"": true,
        ""*.vsconfig"": true,
        ""*.user"": true,
        ""*.editorconfig"": true,
        ""ProjectSettings"": true,
        ""Temp"": true,
        ""UserSettings"": true,
        ""Logs"": true,
        ""obj"": true,
        ""Packages"": true,
        "".vs"": true,
        "".idea"": true,
        ""*.claude"": true,
        ""*.md"": true,
        ""*.csproj"": true,
        ""*.sln"": true
    }},
    ""dotnet.defaultSolution"": ""{solutionName}"",
    ""terminal.integrated.allowChords"": true
}}";

                File.WriteAllText(settingsJsonPath, settingsJsonContent);
                Debug.Log($"Antigravity settings.json created successfully at: {settingsJsonPath}");
                EditorUtility.DisplayDialog("Success", $".vscode/settings.json 파일이 생성되었습니다.\n\n경로: {settingsJsonPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create settings.json: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"settings.json 생성 실패:\n{ex.Message}", "OK");
            }
        }

        [MenuItem("Antigravity/DotRush")]
        public static void GenerateDotRushLaunchJson()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string vscodeDir = Path.Combine(projectRoot, ".vscode");
                string launchJsonPath = Path.Combine(vscodeDir, "launch.json");

                if (!Directory.Exists(vscodeDir))
                {
                    Directory.CreateDirectory(vscodeDir);
                }

                string launchJsonContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Unity Editor"",
            ""type"": ""unity"",
            ""request"": ""attach""
        }
    ]
}";

                File.WriteAllText(launchJsonPath, launchJsonContent);
                Debug.Log($"Antigravity DotRush launch.json created successfully at: {launchJsonPath}");
                EditorUtility.DisplayDialog("Success", $"DotRush launch.json 파일이 생성되었습니다.\n\n경로: {launchJsonPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create DotRush launch.json: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"DotRush launch.json 생성 실패:\n{ex.Message}", "OK");
            }
        }

        /// <summary>
        /// keybindings.json 파일을 생성하거나 업데이트합니다.
        /// </summary>
        [MenuItem("Antigravity/keybindings.json")]
        public static void GenerateKeybindingsJson()
        {
            try
            {
                string keybindingsPath = "/Users/supermac/Library/Application Support/Antigravity/User/keybindings.json";
                string keybindingsDir = Path.GetDirectoryName(keybindingsPath);

                if (!Directory.Exists(keybindingsDir))
                {
                    Directory.CreateDirectory(keybindingsDir);
                }

                string keybindingsContent = @"// Place your key bindings in this file to override the defaults
[
    {
        ""key"": ""cmd+i"",
        ""command"": ""composerMode.agent""
    },
    {
        ""key"": ""alt+f8"",
        ""command"": ""editor.action.formatDocument"",
        ""when"": ""editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""alt+f8"",
        ""command"": ""editor.action.formatSelection"",
        ""when"": ""editorHasSelection && editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""ctrl+alt+o"",
        ""command"": ""editor.action.organizeImports"",
        ""when"": ""editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""alt+f3"",
        ""command"": ""revealFileInOS"",
    },
    {
        ""key"": ""alt+f2"",
        ""command"": ""workbench.files.action.showActiveFileInExplorer""
    },
    {
        ""key"": ""cmd+shift+space"",
        ""command"": ""workbench.action.quickOpen""
    },
    {
        ""key"": ""ctrl+/"",
        ""command"": ""editor.action.commentLine"",
        ""when"": ""editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""ctrl+shift+/"",
        ""command"": ""editor.action.blockComment"",
        ""when"": ""editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""ctrl+l"",
        ""command"": ""editor.action.deleteLines"",
        ""when"": ""editorTextFocus && !editorReadonly""
    },
    {
        ""key"": ""alt+1"",
        ""command"": ""workbench.view.explorer""
    },
    {
        ""key"": ""alt+2"",
        ""command"": ""workbench.view.search""
    },
    {
        ""key"": ""alt+3"",
        ""command"": ""workbench.view.debug""
    },
    {
        ""key"": ""alt+4"",
        ""command"": ""workbench.view.extensions""
    }
]";

                File.WriteAllText(keybindingsPath, keybindingsContent);
                Debug.Log($"Antigravity keybindings.json updated successfully at: {keybindingsPath}");
                EditorUtility.DisplayDialog("Success", $"keybindings.json 파일이 업데이트되었습니다.\n\n경로: {keybindingsPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update keybindings.json: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"keybindings.json 업데이트 실패:\n{ex.Message}", "OK");
            }
        }
    }
}
