/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using IOPath = System.IO.Path;
using Debug = UnityEngine.Debug;

namespace Antigravity.Editor
{
	internal class AntigravityInstallation : AntigravityBaseInstallation
	{
		private static readonly IGenerator _generator = new SdkStyleProjectGeneration();
		internal const string ReuseExistingWindowKey = "antigravity_reuse_existing_window";

		public override bool SupportsAnalyzers
		{
			get
			{
				return true;
			}
		}

		public override Version LatestLanguageVersionSupported
		{
			get
			{
				return new Version(11, 0);
			}
		}

		private string GetExtensionPath()
		{
			var vscode = IsPrerelease ? ".vscode-insiders" : ".vscode";
			var extensionsPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), vscode, "extensions");
			if (!Directory.Exists(extensionsPath))
				return null;

			return Directory
				.EnumerateDirectories(extensionsPath, $"{MicrosoftUnityExtensionId}*") // publisherid.extensionid
				.OrderByDescending(n => n)
				.FirstOrDefault();
		}

		public override string[] GetAnalyzers()
		{
			var vstuPath = GetExtensionPath();
			if (string.IsNullOrEmpty(vstuPath))
				return Array.Empty<string>();

			return GetAnalyzers(vstuPath);
		}

		public override IGenerator ProjectGenerator
		{
			get
			{
				return _generator;
			}
		}

		private static bool IsCandidateForDiscovery(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

#if UNITY_EDITOR_OSX
			// macOS: .app 번들 확인 및 실행 가능성 검증
			if (!Directory.Exists(path))
				return false;

			// Regex 정확도 향상: Antigravity로 정확히 시작하고 .app으로 끝남
			if (!Regex.IsMatch(path, @"^.*/Antigravity.*\.app/?$", RegexOptions.IgnoreCase))
				return false;

			// 실행 가능한 바이너리 확인
			var executablePath = IOPath.Combine(path, "Contents", "MacOS", "Antigravity");
			return File.Exists(executablePath);

#elif UNITY_EDITOR_WIN
			// Windows: .exe 파일 확인 및 검증
			if (!File.Exists(path))
				return false;

			// 정확한 파일명 검증: Antigravity*.exe
			var fileName = IOPath.GetFileName(path).ToLowerInvariant();
			if (!fileName.StartsWith("antigravity") || !fileName.EndsWith(".exe"))
				return false;

			// 파일 접근 가능성 확인
			try
			{
				using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
				{
					return true;
				}
			}
			catch
			{
				return false;
			}

#else
			// Linux: 실행 파일 확인
			if (!File.Exists(path))
				return false;

			// 정확한 파일명 검증: "antigravity"로 정확히 끝남
			var fileName = IOPath.GetFileName(path).ToLowerInvariant();
			if (fileName != "antigravity")
				return false;

			// 실행 권한 확인
			try
			{
				var fileInfo = new FileInfo(path);
				// Unix 권한: 소유자, 그룹, 기타 중 누구든 실행 가능하면 true
				var unixFileMode = (int)fileInfo.Attributes;
				// 간단한 확인: 파일 크기 > 0 && 실행 가능 확인
				return fileInfo.Length > 0 && IsExecutable(path);
			}
			catch
			{
				return false;
			}
#endif
		}

		// Linux: 파일 실행 권한 확인 헬퍼
#if UNITY_EDITOR_LINUX
		[System.Runtime.InteropServices.DllImport("libc")]
		private static extern int access(string path, int mode);

		private static bool IsExecutable(string path)
		{
			const int X_OK = 1; // Execute permission
			try
			{
				return access(path, X_OK) == 0;
			}
			catch
			{
				return false;
			}
		}
#endif

		[Serializable]
		internal class VisualStudioCodeManifest
		{
			public string name;
			public string version;
		}

		public static bool TryDiscoverInstallation(string editorPath, out IAntigravityBaseInstallation installation)
		{
			installation = null;

			if (string.IsNullOrEmpty(editorPath))
				return false;

			if (!IsCandidateForDiscovery(editorPath))
				return false;

			Version version = null;
			var isPrerelease = false;

			try
			{
				var manifestBase = GetRealPath(editorPath);

#if UNITY_EDITOR_WIN
				// on Windows, editorPath is a file, resources as subdirectory
				manifestBase = IOPath.GetDirectoryName(manifestBase);
#elif UNITY_EDITOR_OSX
				// on Mac, editorPath is a directory
				manifestBase = IOPath.Combine(manifestBase, "Contents");
#else
				// on Linux, editorPath is a file, in a bin sub-directory
				var parent = Directory.GetParent(manifestBase);
				// but we can link to [vscode]/code or [vscode]/bin/code
				manifestBase = parent?.Name == "bin" ? parent.Parent?.FullName : parent?.FullName;
#endif

				if (manifestBase == null)
					return false;

				var manifestFullPath = IOPath.Combine(manifestBase, "resources", "app", "package.json");
				if (File.Exists(manifestFullPath))
				{
					var manifest = JsonUtility.FromJson<VisualStudioCodeManifest>(File.ReadAllText(manifestFullPath));
					Version.TryParse(manifest.version.Split('-').First(), out version);
					isPrerelease = manifest.version.ToLower().Contains("insider");
				}
			}
			catch (Exception)
			{
				// do not fail if we are not able to retrieve the exact version number
			}

			isPrerelease = isPrerelease || editorPath.ToLower().Contains("insider");
			installation = new AntigravityInstallation()
			{
				IsPrerelease = isPrerelease,
				Name = "Antigravity" + (isPrerelease ? " - Insider" : string.Empty) + (version != null ? $" [{version.ToString(3)}]" : string.Empty),
				Path = editorPath,
				Version = version ?? new Version()
			};

			return true;
		}

		public static IEnumerable<IAntigravityBaseInstallation> GetAntigravityBaseInstallations()\n\t\t{\n\t\t\tvar candidates = new List<string>();\n\n#if UNITY_EDITOR_WIN\n\t\t\t// Windows: Standard installation locations\n\t\t\tvar localAppPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), \"Programs\");\n\t\t\tvar programFiles = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));\n\t\t\tvar programFilesX86 = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));\n\n\t\t\tforeach (var basePath in new[] { localAppPath, programFiles, programFilesX86 }) {\n\t\t\t\tif (Directory.Exists(basePath))\n\t\t\t\t{\n\t\t\t\t\tcandidates.Add(IOPath.Combine(basePath, \"antigravity\", \"antigravity.exe\"));\n\t\t\t\t}\n\t\t\t}\n\n\t\t\t// Check LOCALAPPDATA environment variable\n\t\t\tvar localAppData = Environment.GetEnvironmentVariable(\"LOCALAPPDATA\");\n\t\t\tif (!string.IsNullOrEmpty(localAppData))\n\t\t\t{\n\t\t\t\tcandidates.Add(IOPath.Combine(localAppData, \"Programs\", \"antigravity\", \"antigravity.exe\"));\n\t\t\t}\n\n#elif UNITY_EDITOR_OSX\n\t\t\t// macOS: Standard /Applications directory\n\t\t\tvar appPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));\n\t\t\tif (Directory.Exists(appPath))\n\t\t\t{\n\t\t\t\tcandidates.AddRange(Directory.EnumerateDirectories(appPath, \"Antigravity*.app\"));\n\t\t\t}\n\n\t\t\t// macOS: Homebrew locations (standard paths)\n\t\t\tvar homebrewLocations = new[]\n\t\t\t{\n\t\t\t\t\"/usr/local/opt/antigravity\",  // Intel Macs\n\t\t\t\t\"/opt/homebrew/opt/antigravity\", // Apple Silicon Macs\n\t\t\t\t\"/usr/local/Cellar/antigravity\", // Homebrew default for Intel\n\t\t\t\t\"/opt/homebrew/Cellar/antigravity\" // Homebrew for Apple Silicon\n\t\t\t};\n\n\t\t\tvar userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);\n\t\t\tforeach (var location in homebrewLocations)\n\t\t\t{\n\t\t\t\tif (Directory.Exists(location))\n\t\t\t\t{\n\t\t\t\t\t// Find .app bundles in Homebrew location\n\t\t\t\t\tvar appBundles = Directory.EnumerateDirectories(location, \"*.app\", SearchOption.AllDirectories);\n\t\t\t\t\tcandidates.AddRange(appBundles);\n\n\t\t\t\t\t// Also try direct executable path\n\t\t\t\t\tvar execPath = IOPath.Combine(location, \"bin\", \"Antigravity\");\n\t\t\t\t\tif (File.Exists(execPath))\n\t\t\t\t\t{\n\t\t\t\t\t\tcandidates.Add(execPath);\n\t\t\t\t\t}\n\t\t\t\t}\n\t\t\t}\n\n\t\t\t// Check home directory for custom installations\n\t\t\tvar customAppPath = IOPath.Combine(userHome, \"Applications\", \"Antigravity.app\");\n\t\t\tif (Directory.Exists(customAppPath))\n\t\t\t{\n\t\t\t\tcandidates.Add(customAppPath);\n\t\t\t}\n\n#elif UNITY_EDITOR_LINUX\n\t\t\t// Linux: Well known locations\n\t\t\tcandidates.Add(\"/usr/bin/antigravity\");\n\t\t\tcandidates.Add(\"/bin/antigravity\");\n\t\t\tcandidates.Add(\"/usr/local/bin/antigravity\");\n\n\t\t\t// Check environment PATH variable for additional locations\n\t\t\tvar pathEnv = Environment.GetEnvironmentVariable(\"PATH\");\n\t\t\tif (!string.IsNullOrEmpty(pathEnv))\n\t\t\t{\n\t\t\t\tvar pathDirs = pathEnv.Split(':');\n\t\t\t\tforeach (var dir in pathDirs)\n\t\t\t\t{\n\t\t\t\t\tif (!Directory.Exists(dir))\n\t\t\t\t\t\tcontinue;\n\n\t\t\t\t\tvar antigravityPath = IOPath.Combine(dir, \"antigravity\");\n\t\t\t\t\tif (File.Exists(antigravityPath))\n\t\t\t\t\t{\n\t\t\t\t\t\tcandidates.Add(antigravityPath);\n\t\t\t\t\t}\n\t\t\t\t}\n\t\t\t}\n\n\t\t\t// Check XDG_DATA_DIRS for desktop entries\n\t\t\tvar envdirs = Environment.GetEnvironmentVariable(\"XDG_DATA_DIRS\");\n\t\t\tif (!string.IsNullOrEmpty(envdirs))\n\t\t\t{\n\t\t\t\tvar dirs = envdirs.Split(':');\n\t\t\t\tforeach (var dir in dirs)\n\t\t\t\t{\n\t\t\t\t\tif (!Directory.Exists(dir))\n\t\t\t\t\t\tcontinue;\n\n\t\t\t\t\tvar appsDir = IOPath.Combine(dir, \"applications\");\n\t\t\t\t\tif (!Directory.Exists(appsDir))\n\t\t\t\t\t\tcontinue;\n\n\t\t\t\t\tcandidates.AddRange(Directory.EnumerateDirectories(appsDir, \"antigravity*\"));\n\t\t\t\t}\n\t\t\t}\n\n\t\t\t// Preference ordered base directories relative to which desktop files should be searched\n\t\t\tcandidates.AddRange(GetXdgCandidates());\n#endif

			var foundPaths = new HashSet<string>();
			foreach (var candidate in candidates.Distinct())
			{
				if (TryDiscoverInstallation(candidate, out var installation))
				{
					foundPaths.Add(installation.Path);
					yield return installation;
				}
			}

            // Fallback: Provide default installation only if not already found
            var defaultPath = DefaultInstallPath();
            if (!foundPaths.Contains(defaultPath))
            {
                if (TryDiscoverInstallation(defaultPath, out var defaultInstallation))
                {
                    yield return defaultInstallation;
                }
                else
                {
                    // Force return a default installation object even if file doesn't exist,
                    // so the user can see "Antigravity" in the list and browse manually.
                    yield return new AntigravityInstallation
                    {
                        Name = "Antigravity",
                        Path = defaultPath,
                        Version = new Version()
                    };
                }
            }
		}

        public static string DefaultInstallPath()
        {
#if UNITY_EDITOR_WIN
            return "C:\\Program Files\\Antigravity\\antigravity.exe";
#elif UNITY_EDITOR_OSX
            return "/Applications/Antigravity.app";
#else
            return "/usr/bin/antigravity";
#endif
        }

#if UNITY_EDITOR_LINUX
		private static readonly Regex DesktopFileExecEntry = new Regex(@"Exec=(\S+)", RegexOptions.Singleline | RegexOptions.Compiled);

		private static IEnumerable<string> GetXdgCandidates()
		{
			var envdirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
			if (string.IsNullOrEmpty(envdirs))
				yield break;

			var dirs = envdirs.Split(':');
			foreach(var dir in dirs)
			{
				Match match = null;

				try
				{
					var desktopFile = IOPath.Combine(dir, "applications/code.desktop");
					if (!File.Exists(desktopFile))
						continue;

					var content = File.ReadAllText(desktopFile);
					match = DesktopFileExecEntry.Match(content);
				}
				catch
				{
					// do not fail if we cannot read desktop file
				}

				if (match == null || !match.Success)
					continue;

				yield return match.Groups[1].Value;
				break;
			}
		}

		[System.Runtime.InteropServices.DllImport ("libc")]
		private static extern int readlink(string path, byte[] buffer, int buflen);

		internal static string GetRealPath(string path)
		{
			byte[] buf = new byte[512];
			int ret = readlink(path, buf, buf.Length);
			if (ret == -1) return path;
			char[] cbuf = new char[512];
			int chars = System.Text.Encoding.Default.GetChars(buf, 0, ret, cbuf, 0);
			return new String(cbuf, 0, chars);
		}
#else
		internal static string GetRealPath(string path)
		{
			return path;
		}
#endif

		public override void CreateExtraFiles(string projectDirectory)
		{
			try
			{
				var vscodeDirectory = IOPath.Combine(projectDirectory.NormalizePathSeparators(), ".vscode");
				Directory.CreateDirectory(vscodeDirectory);

				var enablePatch = !File.Exists(IOPath.Combine(vscodeDirectory, ".vstupatchdisable"));

				CreateRecommendedExtensionsFile(vscodeDirectory, enablePatch);
				CreateSettingsFile(vscodeDirectory, enablePatch);
				CreateLaunchFile(vscodeDirectory, enablePatch);
			}
			catch (IOException)
			{
			}
		}

		private const string DefaultLaunchFileContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Attach to Unity"",
            ""type"": ""unity"",
            ""request"": ""attach""
        }
     ]
}";

		private static void CreateLaunchFile(string vscodeDirectory, bool enablePatch)
		{
			var launchFile = IOPath.Combine(vscodeDirectory, "launch.json");
			if (File.Exists(launchFile))
			{
				if (enablePatch)
					PatchLaunchFile(launchFile);

				return;
			}

			// 업데이트된 기본 launch.json with debugServer port
			var defaultLaunchFileContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Attach to Unity"",
            ""type"": ""unity"",
            ""request"": ""attach"",
            ""debugServer"": 55555
        },
        {
            ""name"": ""Attach to Unity (Alternative Port)"",
            ""type"": ""unity"",
            ""request"": ""attach"",
            ""debugServer"": 55556
        }
    ]
}";

			File.WriteAllText(launchFile, defaultLaunchFileContent);
		}

		private static void PatchLaunchFile(string launchFile)
		{
			try
			{
				const string configurationsKey = "configurations";
				const string typeKey = "type";
				const string debugServerKey = "debugServer";

				var content = File.ReadAllText(launchFile);
				var launch = JSONNode.Parse(content);

				if (launch == null)
				{
					Debug.LogWarning("[Antigravity] Failed to parse launch.json, skipping patch");
					return;
				}

				var configurations = launch[configurationsKey] as JSONArray;
				if (configurations == null)
				{
					configurations = new JSONArray();
					launch.Add(configurationsKey, configurations);
				}

				// Unity debugger 설정이 있는지 확인
				bool hasUnityDebugger = configurations.Linq.Any(entry => entry.Value[typeKey]?.Value == "unity");
				
				if (!hasUnityDebugger)
				{
					// Unity debugger 설정 추가
					var unityConfig = JSONNode.Parse(@"{
        ""name"": ""Attach to Unity"",
        ""type"": ""unity"",
        ""request"": ""attach"",
        ""debugServer"": 55555
    }");
					configurations.Add(unityConfig);
				}
				else
				{
					// 기존 Unity debugger 설정에 debugServer 포트 추가
					foreach (var config in configurations.Linq.Where(e => e.Value[typeKey]?.Value == "unity"))
					{
						if (config.Value[debugServerKey] == null)
						{
							config.Value[debugServerKey] = 55555;
						}
					}
				}

				WriteAllTextFromJObject(launchFile, launch);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[Antigravity] Error patching launch.json: {ex.Message}");
			}
		}

		private void CreateSettingsFile(string vscodeDirectory, bool enablePatch)
		{
			var settingsFile = IOPath.Combine(vscodeDirectory, "settings.json");
			if (File.Exists(settingsFile))
			{
				if (enablePatch)
					PatchSettingsFile(settingsFile);

				return;
			}

			const string excludes = @"    ""files.exclude"": {
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
        ""Temp/"": true
    }";

			var content = @"{
" + excludes + @",
    ""dotnet.defaultSolution"": """ + IOPath.GetFileName(ProjectGenerator.SolutionFile()) + @"""
}";

			File.WriteAllText(settingsFile, content);
		}

		private void PatchSettingsFile(string settingsFile)
		{
			try
			{
				const string excludesKey = "files.exclude";
				const string solutionKey = "dotnet.defaultSolution";

				var content = File.ReadAllText(settingsFile);
				var settings = JSONNode.Parse(content);

				var excludes = settings[excludesKey] as JSONObject;
				if (excludes == null)
					return;

				var patchList = new List<string>();
				var patched = false;

				// Remove files.exclude for solution+project files in the project root
				foreach (var exclude in excludes)
				{
					if (!bool.TryParse(exclude.Value, out var exc) || !exc)
						continue;

					var key = exclude.Key;

					if (!key.EndsWith(".sln") && !key.EndsWith(".csproj"))
						continue;

					if (!Regex.IsMatch(key, "^(\\*\\*[\\\\\\/])?\\*\\.(sln|csproj)$"))
						continue;

					patchList.Add(key);
					patched = true;
				}

				// Check default solution
				var defaultSolution = settings[solutionKey];
				var solutionFile = IOPath.GetFileName(ProjectGenerator.SolutionFile());
				if (defaultSolution == null || defaultSolution.Value != solutionFile)
				{
					settings[solutionKey] = solutionFile;
					patched = true;
				}

				if (!patched)
					return;

				foreach (var patch in patchList)
					excludes.Remove(patch);

				WriteAllTextFromJObject(settingsFile, settings);
			}
			catch (Exception)
			{
				// do not fail if we cannot patch the settings.json file
			}
		}

		private const string MicrosoftUnityExtensionId = "antigravity.tools";
		private const string DefaultRecommendedExtensionsContent = @"{
    ""recommendations"": [
      """ + MicrosoftUnityExtensionId + @"""
    ]
}
";

		private static void CreateRecommendedExtensionsFile(string vscodeDirectory, bool enablePatch)
		{
			// see https://tattoocoder.com/recommending-vscode-extensions-within-your-open-source-projects/
			var extensionFile = IOPath.Combine(vscodeDirectory, "extensions.json");
			if (File.Exists(extensionFile))
			{
				if (enablePatch)
					PatchRecommendedExtensionsFile(extensionFile);

				return;
			}

			File.WriteAllText(extensionFile, DefaultRecommendedExtensionsContent);
		}

		private static void PatchRecommendedExtensionsFile(string extensionFile)
		{
			try
			{
				const string recommendationsKey = "recommendations";

				var content = File.ReadAllText(extensionFile);
				var extensions = JSONNode.Parse(content);

				var recommendations = extensions[recommendationsKey] as JSONArray;
				if (recommendations == null)
				{
					recommendations = new JSONArray();
					extensions.Add(recommendationsKey, recommendations);
				}

				if (recommendations.Linq.Any(entry => entry.Value.Value == MicrosoftUnityExtensionId))
					return;

				recommendations.Add(MicrosoftUnityExtensionId);
				WriteAllTextFromJObject(extensionFile, extensions);
			}
			catch (Exception)
			{
				// do not fail if we cannot patch the extensions.json file
			}
		}

		private static void WriteAllTextFromJObject(string file, JSONNode node)
		{
			using (var fs = File.Open(file, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				// Keep formatting/indent in sync with default contents
				sw.Write(node.ToString(aIndent: 4));
			}
		}

		private Process FindRunningAntigravityWithSolution(string solutionPath)
		{
			if (string.IsNullOrWhiteSpace(solutionPath))
				return null;

			try
			{
				// 경로 정규화: 완전한 절대 경로로 변환
				var targetPath = IOPath.GetFullPath(solutionPath);
				var normalizedTargetPath = NormalizePath(targetPath);

#if UNITY_EDITOR_WIN
				// Windows: drive letter 소문자로 통일
				if (normalizedTargetPath.Length > 1 && normalizedTargetPath[1] == ':')
				{
					normalizedTargetPath = char.ToLowerInvariant(normalizedTargetPath[0]) + normalizedTargetPath.Substring(1);
				}
#else
				// macOS/Linux: 반드시 /로 시작
				if (!normalizedTargetPath.StartsWith("/"))
				{
					normalizedTargetPath = "/" + normalizedTargetPath;
				}
#endif

				var processes = new List<Process>();

				// 플랫폼별 프로세스명 검색
#if UNITY_EDITOR_OSX
				processes.AddRange(Process.GetProcessesByName("Antigravity"));
				processes.AddRange(Process.GetProcessesByName("Antigravity Helper"));
#elif UNITY_EDITOR_LINUX
				processes.AddRange(Process.GetProcessesByName("antigravity"));
				processes.AddRange(Process.GetProcessesByName("Antigravity"));
#else
				processes.AddRange(Process.GetProcessesByName("antigravity"));
#endif

				foreach (var process in processes)
				{
					try
					{
						var workspaces = ProcessRunner.GetProcessWorkspaces(process);
						if (workspaces == null || workspaces.Length == 0)
							continue;

						foreach (var workspace in workspaces)
						{
							var normalizedWorkspace = NormalizePath(workspace);

#if UNITY_EDITOR_WIN
							// Windows: drive letter 소문자로 통일
							if (normalizedWorkspace.Length > 1 && normalizedWorkspace[1] == ':')
							{
								normalizedWorkspace = char.ToLowerInvariant(normalizedWorkspace[0]) + normalizedWorkspace.Substring(1);
							}
#else
							// macOS/Linux: 반드시 /로 시작
							if (!normalizedWorkspace.StartsWith("/"))
							{
								normalizedWorkspace = "/" + normalizedWorkspace;
							}
#endif

							// 정확한 경로 일치 또는 디렉토리 포함 관계 확인
							if (PathsMatch(normalizedWorkspace, normalizedTargetPath))
							{
								return process;
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[Antigravity] Error checking process: {ex.Message}");
						continue;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Antigravity] Error finding running Antigravity instance: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// 경로를 정규화: 백슬래시를 슬래시로, 중복 슬래시 제거, 대소문자 통일
		/// </summary>
		private static string NormalizePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return string.Empty;

			// 1. 백슬래시를 슬래시로 변환
			var normalized = path.Replace('\\', '/');

			// 2. 중복 슬래시 제거
			while (normalized.Contains("//"))
			{
				normalized = normalized.Replace("//", "/");
			}

			// 3. 뒤의 슬래시 제거
			normalized = normalized.TrimEnd('/');

			// 4. 소문자로 통일 (경로 비교용)
#if !UNITY_EDITOR_WIN
			// Windows가 아닌 경우에만 소문자 통일 (Windows는 case-insensitive지만 경로에 문자가 있을 수 있음)
			normalized = normalized.ToLowerInvariant();
#else
			// Windows: 드라이브 레터만 소문자, 나머지는 유지
			if (normalized.Length > 1 && normalized[1] == ':')
			{
				normalized = normalized.Substring(0, 2).ToLowerInvariant() + normalized.Substring(2);
			}
			else
			{
				normalized = normalized.ToLowerInvariant();
			}
#endif

			return normalized;
		}

		/// <summary>
		/// 두 경로가 일치하는지 확인 (완전 일치 또는 부모-자식 관계)
		/// </summary>
		private static bool PathsMatch(string workspacePath, string targetPath)
		{
			if (string.IsNullOrEmpty(workspacePath) || string.IsNullOrEmpty(targetPath))
				return false;

			// 1. 완전 일치
			if (string.Equals(workspacePath, targetPath, StringComparison.OrdinalIgnoreCase))
				return true;

			// 2. 대상 경로가 워크스페이스의 자식 디렉토리
			if (targetPath.StartsWith(workspacePath + "/", StringComparison.OrdinalIgnoreCase))
				return true;

			// 3. 워크스페이스가 대상 경로의 자식 디렉토리
			if (workspacePath.StartsWith(targetPath + "/", StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		private static string TryFindWorkspace(string directory)
		{
			var files = Directory.GetFiles(directory, "*.code-workspace", SearchOption.TopDirectoryOnly);
			if (files.Length == 0 || files.Length > 1)
				return null;

			return files[0];
		}

		public override bool Open(string path, int line, int column, string solution)
		{
			line = Math.Max(1, line);
			column = Math.Max(0, column);

			var directory = IOPath.GetDirectoryName(solution);
			var application = Path;

			var workspace = TryFindWorkspace(directory);
			workspace ??= directory;
			directory = workspace;

			if (EditorPrefs.GetBool(ReuseExistingWindowKey, false))
			{
				var existingProcess = FindRunningAntigravityWithSolution(directory);
				if (existingProcess != null)
				{
					try
					{
						var args = string.IsNullOrEmpty(path) ?
							$"--reuse-window \"{directory}\"" :
							$"--reuse-window -g \"{path}\":{line}:{column}";

						ProcessRunner.Start(ProcessStartInfoFor(application, args));
						return true;
					}
					catch (Exception ex)
					{
						Debug.LogError($"[Antigravity] Error using existing instance: {ex}");
					}
				}
			}

			var newArgs = string.IsNullOrEmpty(path) ?
				$"--new-window \"{directory}\"" :
				$"--new-window \"{directory}\" -g \"{path}\":{line}:{column}";

			ProcessRunner.Start(ProcessStartInfoFor(application, newArgs));
			return true;
		}

		private static ProcessStartInfo ProcessStartInfoFor(string application, string arguments)
		{
#if UNITY_EDITOR_OSX
			// wrap with built-in OSX open feature
			arguments = $"-n \"{application}\" --args {arguments}";
			application = "open";
			return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect:false, shell: true);
#else
			return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect: false);
#endif
		}

		public static void Initialize()
		{
		}
	}
}
