/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Debug = UnityEngine.Debug;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;

namespace Antigravity.Editor
{
	internal class ProcessRunnerResult
	{
		public bool Success { get; set; }
		public string Output { get; set; }
		public string Error { get; set; }
	}

	internal static class ProcessRunner
	{
		public const int DefaultTimeoutInMilliseconds = 300000;

		public static ProcessStartInfo ProcessStartInfoFor(string filename, string arguments, bool redirect = true, bool shell = false)
		{
			return new ProcessStartInfo
			{
				UseShellExecute = shell,
				CreateNoWindow = true, 
				RedirectStandardOutput = redirect,
				RedirectStandardError = redirect,
				FileName = filename,
				Arguments = arguments
			};
		}

		public static void Start(string filename, string arguments)
		{
			Start(ProcessStartInfoFor(filename, arguments, false));
		}

		public static void Start(ProcessStartInfo processStartInfo)
		{
			var process = new Process { StartInfo = processStartInfo };

			using (process)
			{
				process.Start();
			}
		}

		public static ProcessRunnerResult StartAndWaitForExit(string filename, string arguments, int timeoutms = DefaultTimeoutInMilliseconds, Action<string> onOutputReceived = null)
		{
			return StartAndWaitForExit(ProcessStartInfoFor(filename, arguments), timeoutms, onOutputReceived);
		}

		public static ProcessRunnerResult StartAndWaitForExit(ProcessStartInfo processStartInfo, int timeoutms = DefaultTimeoutInMilliseconds, Action<string> onOutputReceived = null)
		{
			var process = new Process { StartInfo = processStartInfo };

			using (process)
			{
				var sbOutput = new StringBuilder();
				var sbError = new StringBuilder();

				var outputSource = new TaskCompletionSource<bool>();
				var errorSource = new TaskCompletionSource<bool>();

				const int MaxBufferSize = 1024 * 1024; // 1MB 최대 버퍼 크기

				process.OutputDataReceived += (_, e) =>
				{
					// 버퍼 오버플로우 방지
					if (sbOutput.Length < MaxBufferSize)
					{
						Append(sbOutput, e.Data, outputSource);
						if (onOutputReceived != null && e.Data != null)
						{
							try
							{
								onOutputReceived(e.Data);
							}
							catch (Exception ex)
							{
								Debug.LogWarning($"[ProcessRunner] Error in output callback: {ex.Message}");
							}
						}
					}
					else if (e.Data == null)
					{
						outputSource.TrySetResult(true);
					}
				};

				process.ErrorDataReceived += (_, e) =>
				{
					if (sbError.Length < MaxBufferSize)
					{
						Append(sbError, e.Data, errorSource);
					}
					else if (e.Data == null)
					{
						errorSource.TrySetResult(true);
					}
				};

				try
				{
					process.Start();
				}
				catch (Exception ex)
				{
					return new ProcessRunnerResult 
					{ 
						Success = false, 
						Error = $"Failed to start process: {ex.Message}", 
						Output = string.Empty
					};
				}

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				var waitTask = Task.Run(() =>
				{
					try
					{
						return process.WaitForExit(timeoutms);
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[ProcessRunner] Process wait error: {ex.Message}");
						return false;
					}
				});

				var delayTask = Task.Delay(timeoutms);
				var completedTask = Task.WhenAny(waitTask, delayTask).Result;

				if (completedTask == delayTask)
				{
					try
					{
						process.Kill();
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[ProcessRunner] Failed to kill process on timeout: {ex.Message}");
					}

					return new ProcessRunnerResult
					{
						Success = false,
						Error = $"Process timeout after {timeoutms}ms",
						Output = sbOutput.ToString()
					};
				}

				if (waitTask.Result)
				{
					var outputTimeout = Task.Delay(2000);
					var allOutputTask = Task.WhenAll(outputSource.Task, errorSource.Task);
					var outputCompleted = Task.WhenAny(allOutputTask, outputTimeout).Result;

					if (outputCompleted == outputTimeout)
					{
						Debug.LogWarning("[ProcessRunner] Output reading timeout, continuing with partial output");
					}

					return new ProcessRunnerResult
					{
						Success = true,
						Error = sbError.ToString(),
						Output = sbOutput.ToString()
					};
				}
				else
				{
					try
					{
						process.Kill();
					}
					catch { }

					return new ProcessRunnerResult
					{
						Success = false,
						Error = sbError.ToString(),
						Output = sbOutput.ToString()
					};
				}
			}
		}

		private static void Append(StringBuilder sb, string data, TaskCompletionSource<bool> taskSource)
		{
			if (data == null)
			{
				taskSource.SetResult(true);
				return;
			}

			sb?.Append(data);
		}

		public static string[] GetProcessWorkspaces(Process process)
		{
			if (process == null)
				return null;

			try
			{
				var workspaces = new List<string>();
				var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				string antigravityStoragePath;

#if UNITY_EDITOR_OSX
				antigravityStoragePath = Path.Combine(userProfile, "Library", "Application Support", "antigravity", "User", "workspaceStorage");
#elif UNITY_EDITOR_LINUX
				antigravityStoragePath = Path.Combine(userProfile, ".config", "Antigravity", "User", "workspaceStorage");
#else
				antigravityStoragePath = Path.Combine(userProfile, "AppData", "Roaming", "antigravity", "User", "workspaceStorage");
#endif

				if (!Directory.Exists(antigravityStoragePath))
				{
					Debug.LogWarning($"[Antigravity] Workspace storage directory not found: {antigravityStoragePath}");
					return new string[0];
				}

				foreach (var workspaceDir in Directory.GetDirectories(antigravityStoragePath))
				{
					try
					{
						// workspace.json 처리
						var workspaceStatePath = Path.Combine(workspaceDir, "workspace.json");
						if (File.Exists(workspaceStatePath))
						{
							var workspacePath = TryGetWorkspacePath(workspaceStatePath, "folder");
							if (!string.IsNullOrEmpty(workspacePath))
							{
								workspaces.Add(workspacePath);
								continue; // workspace.json에서 찾으면 window.json은 스킵
							}
						}

						// window.json 처리
						var windowStatePath = Path.Combine(workspaceDir, "window.json");
						if (File.Exists(windowStatePath))
						{
							var workspacePath = TryGetWorkspacePath(windowStatePath, "workspace");
							if (!string.IsNullOrEmpty(workspacePath))
							{
								workspaces.Add(workspacePath);
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[Antigravity] Error reading workspace state file in {workspaceDir}: {ex.Message}");
						continue;
					}
				}

				return workspaces.Distinct().ToArray();
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Antigravity] Error getting workspace directory: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// JSON 파일에서 워크스페이스 경로를 추출 (오류 처리 강화)
		/// </summary>
		private static string TryGetWorkspacePath(string jsonFilePath, string pathKey)
		{
			if (!File.Exists(jsonFilePath))
				return null;

			try
			{
				var content = File.ReadAllText(jsonFilePath);
				
				// 파일이 비어있거나 공백만 있으면 반환
				if (string.IsNullOrWhiteSpace(content))
				{
					Debug.LogWarning($"[Antigravity] Empty workspace state file: {jsonFilePath}");
					return null;
				}

				// JSON 파싱 시도
				var jsonNode = JSONNode.Parse(content);
				if (jsonNode == null)
				{
					Debug.LogWarning($"[Antigravity] Failed to parse JSON file: {jsonFilePath}");
					return null;
				}

				// pathKey로 경로 추출
				var pathNode = jsonNode[pathKey];
				if (pathNode == null)
				{
					Debug.LogWarning($"[Antigravity] Key '{pathKey}' not found in {jsonFilePath}");
					return null;
				}

				var pathValue = pathNode.Value;
				if (string.IsNullOrEmpty(pathValue))
				{
					Debug.LogWarning($"[Antigravity] Empty value for key '{pathKey}' in {jsonFilePath}");
					return null;
				}

				// file:/// URI scheme 처리
				if (pathValue.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						// URI 디코딩: %20 -> space, 기타 인코딩 처리
						pathValue = Uri.UnescapeDataString(pathValue.Substring(8));
						
						// Windows의 경우 드라이브 문자 처리
#if UNITY_EDITOR_WIN
						// /C:/Users/... → C:/Users/...
						if (pathValue.Length > 2 && pathValue[0] == '/' && pathValue[2] == ':')
						{
							pathValue = pathValue.Substring(1);
						}
#endif
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[Antigravity] Error decoding file URI: {ex.Message}");
						return null;
					}
				}

				// 경로 유효성 확인 (최소한 문자열로는 유효)
				if (pathValue.Length < 2)
				{
					Debug.LogWarning($"[Antigravity] Invalid path format: {pathValue}");
					return null;
				}

				return pathValue;
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[Antigravity] Unexpected error in TryGetWorkspacePath: {ex.Message}");
				return null;
			}
		}
	}
}
