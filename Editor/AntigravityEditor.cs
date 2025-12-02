/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

[assembly: InternalsVisibleTo("Antigravity.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Antigravity.Editor
{
	[InitializeOnLoad]
	internal class AntigravityExternalEditor : IExternalCodeEditor
	{
		[MenuItem("Antigravity/Settings")]
		private static void OpenAntigravitySettings()
		{
			CreateVscodeLaunchConfiguration();
		}

		private static void CreateVscodeLaunchConfiguration()
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
				string launchJsonContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Attach to Unity"",
            ""type"": ""vstuc"",
            ""request"": ""attach""
        }
    ]
}";

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

		CodeEditor.Installation[] IExternalCodeEditor.Installations
        {
            get
            {
                var installations = new List<CodeEditor.Installation>();

                // Use discovered installations if available
                if (_discoverInstallations != null && _discoverInstallations.Result != null && _discoverInstallations.Result.Count > 0)
                {
                    installations.AddRange(_discoverInstallations.Result.Values.Select(v => v.ToCodeEditorInstallation()));
                }
                else
                {
                    // Always provide Antigravity as a default option so it appears in the dropdown,
                    // even if not installed. Users can then browse for the installation path.
                    installations.Add(new CodeEditor.Installation
                    {
                        Name = "Antigravity",
                        Path = AntigravityInstallation.DefaultInstallPath()
                    });
                }

                return installations.ToArray();
            }
        }

		private static readonly AsyncOperation<Dictionary<string, IAntigravityBaseInstallation>> _discoverInstallations;

		static AntigravityExternalEditor()
		{
			if (!UnityInstallation.IsMainUnityEditorProcess)
				return;

			Discovery.Initialize();
			CodeEditor.Register(new AntigravityExternalEditor());

			_discoverInstallations = AsyncOperation<Dictionary<string, IAntigravityBaseInstallation>>.Run(DiscoverInstallations);
		}

#if UNITY_2019_4_OR_NEWER && !UNITY_2020
		[InitializeOnLoadMethod]
		static void LegacyVisualStudioCodePackageDisabler()
		{
			// disable legacy Visual Studio Code packages
			var editor = CodeEditor.Editor.GetCodeEditorForPath("code.cmd");
			if (editor == null)
				return;

			if (editor is AntigravityExternalEditor)
				return;

			// only disable the com.unity.ide.vscode package
			var assembly = editor.GetType().Assembly;
			var assemblyName = assembly.GetName().Name;
			if (assemblyName != "Unity.VSCode.Editor")
				return;

			CodeEditor.Unregister(editor);
		}
#endif

		private static Dictionary<string, IAntigravityBaseInstallation> DiscoverInstallations()
		{
			try
			{
				var dict = new Dictionary<string, IAntigravityBaseInstallation>();
				foreach (var installation in Discovery.GetAntigravityBaseInstallations())
				{
					var fullPath = Path.GetFullPath(installation.Path);
					if (!dict.ContainsKey(fullPath))
					{
						dict[fullPath] = installation;
					}
				}
				return dict;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error detecting Visual Studio installations: {ex}");
				return new Dictionary<string, IAntigravityBaseInstallation>();
			}
		}

		internal static bool IsEnabled => CodeEditor.CurrentEditor is AntigravityExternalEditor && UnityInstallation.IsMainUnityEditorProcess;

		// this one seems legacy and not used anymore
		// keeping it for now given it is public, so we need a major bump to remove it 
		internal void CreateIfDoesntExist()
		{
			if (!TryGetAntigravityBaseInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation)) 
				return;

			var generator = installation.ProjectGenerator;
			if (!generator.HasSolutionBeenGenerated())
				generator.Sync();
		}

		public void Initialize(string editorInstallationPath)
		{
		}

		internal virtual bool TryGetAntigravityBaseInstallationForPath(string editorPath, bool lookupDiscoveredInstallations, out IAntigravityBaseInstallation installation)
		{
			editorPath = Path.GetFullPath(editorPath);

			// lookup for well known installations
			if (lookupDiscoveredInstallations && _discoverInstallations.Result.TryGetValue(editorPath, out installation))
				return true;

			return Discovery.TryDiscoverInstallation(editorPath, out installation);
		}

		public virtual bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
		{
			var result = TryGetAntigravityBaseInstallationForPath(editorPath, lookupDiscoveredInstallations: false, out var vsi);
			installation = vsi?.ToCodeEditorInstallation() ?? default;
			return result;
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (!TryGetAntigravityBaseInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
				return;

			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(GetType().Assembly);

			var style = new GUIStyle
			{
				richText = true,
				margin = new RectOffset(0, 4, 0, 0)
			};

			GUILayout.Label($"<size=10><color=grey>{package.displayName} v{package.version} enabled</color></size>", style);
			GUILayout.EndHorizontal();

			if (installation is AntigravityInstallation)
			{
				var reuseWindow = EditorPrefs.GetBool(AntigravityInstallation.ReuseExistingWindowKey, false);
				var newReuseWindow = EditorGUILayout.Toggle(new GUIContent("Reuse existing Antigravity window", "When enabled, opens files in an existing Antigravity window if found. When disabled, always opens a new window."), reuseWindow);
				if (newReuseWindow != reuseWindow)
					EditorPrefs.SetBool(AntigravityInstallation.ReuseExistingWindowKey, newReuseWindow);
				
				EditorGUILayout.Space();
			}

			EditorGUILayout.LabelField("Generate .csproj files for:");
			EditorGUI.indentLevel++;
			SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Local, "Local packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Git, "Git packages", "", installation);
			SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "", installation);
			SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "", installation);
			SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "", installation);
			SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project-player.csproj'", installation);
			RegenerateProjectFiles(installation);
			EditorGUI.indentLevel--;
		}

		private static void RegenerateProjectFiles(IAntigravityBaseInstallation installation)
		{
			var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
			rect.width = 252;
			if (GUI.Button(rect, "Regenerate project files"))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		private static void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip, IAntigravityBaseInstallation installation)
		{
			var generator = installation.ProjectGenerator;
			var prevValue = generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);

			var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
			if (newValue != prevValue)
				generator.AssemblyNameProvider.ToggleProjectGeneration(preference);
		}

		public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
		{
			if (TryGetAntigravityBaseInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles), importedFiles);
			}

			foreach (var file in importedFiles.Where(a => Path.GetExtension(a) == ".pdb"))
			{
				var pdbFile = FileUtility.GetAssetFullPath(file);

				// skip Unity packages like com.unity.ext.nunit
				if (pdbFile.IndexOf($"{Path.DirectorySeparatorChar}com.unity.", StringComparison.OrdinalIgnoreCase) > 0)
					continue;

				var asmFile = Path.ChangeExtension(pdbFile, ".dll");
				if (!File.Exists(asmFile) || !Image.IsAssembly(asmFile))
					continue;

				if (Symbols.IsPortableSymbolFile(pdbFile))
					continue;

				Debug.LogWarning($"Unity is only able to load mdb or portable-pdb symbols. {file} is using a legacy pdb format.");
			}
		}

		public void SyncAll()
		{
			if (TryGetAntigravityBaseInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		private static bool IsSupportedPath(string path, IGenerator generator)
		{
			// Path is empty with "Open C# Project", as we only want to open the solution without specific files
			if (string.IsNullOrEmpty(path))
				return true;

			// cs, uxml, uss, shader, compute, cginc, hlsl, glslinc, template are part of Unity builtin extensions
			// txt, xml, fnt, cd are -often- par of Unity user extensions
			// asdmdef is mandatory included
			return generator.IsSupportedFile(path);
		}

		public bool OpenProject(string path, int line, int column)
		{
			var editorPath = CodeEditor.CurrentEditorInstallation;

			if (!Discovery.TryDiscoverInstallation(editorPath, out var installation)) {
				Debug.LogWarning($"Visual Studio executable {editorPath} is not found. Please change your settings in Edit > Preferences > External Tools.");
				return false;
			}

			var generator = installation.ProjectGenerator;
			if (!IsSupportedPath(path, generator))
				return false;

			if (!IsProjectGeneratedFor(path, generator, out var missingFlag))
				Debug.LogWarning($"You are trying to open {path} outside a generated project. This might cause problems with IntelliSense and debugging. To avoid this, you can change your .csproj preferences in Edit > Preferences > External Tools and enable {GetProjectGenerationFlagDescription(missingFlag)} generation.");

			var solution = GetOrGenerateSolutionFile(generator);
			return installation.Open(path, line, column, solution);
		}

		private static string GetProjectGenerationFlagDescription(ProjectGenerationFlag flag)
		{
			switch (flag)
			{
				case ProjectGenerationFlag.BuiltIn:
					return "Built-in packages";
				case ProjectGenerationFlag.Embedded:
					return "Embedded packages";
				case ProjectGenerationFlag.Git:
					return "Git packages";
				case ProjectGenerationFlag.Local:
					return "Local packages";
				case ProjectGenerationFlag.LocalTarBall:
					return "Local tarball";
				case ProjectGenerationFlag.PlayerAssemblies:
					return "Player projects";
				case ProjectGenerationFlag.Registry:
					return "Registry packages";
				case ProjectGenerationFlag.Unknown:
					return "Packages from unknown sources";
				default:
					return string.Empty;
			}
		}

		private static bool IsProjectGeneratedFor(string path, IGenerator generator, out ProjectGenerationFlag missingFlag)
		{
			missingFlag = ProjectGenerationFlag.None;

			// No need to check when opening the whole solution
			if (string.IsNullOrEmpty(path))
				return true;

			// We only want to check for cs scripts
			if (ProjectGeneration.ScriptingLanguageForFile(path) != ScriptingLanguage.CSharp)
				return true;

			// Even on windows, the package manager requires relative path + unix style separators for queries
			var basePath = generator.ProjectDirectory;
			var relativePath = path
				.NormalizeWindowsToUnix()
				.Replace(basePath, string.Empty)
				.Trim(FileUtility.UnixSeparator);

			var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(relativePath);
			if (packageInfo == null)
				return true;

			var source = packageInfo.source;
			if (!Enum.TryParse<ProjectGenerationFlag>(source.ToString(), out var flag))
				return true;

			if (generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(flag))
				return true;

			// Return false if we found a source not flagged for generation
			missingFlag = flag;
			return false;
		}

		private static string GetOrGenerateSolutionFile(IGenerator generator)
		{
			generator.Sync();
			return generator.SolutionFile();
		}
	}
}
