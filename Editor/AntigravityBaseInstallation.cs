/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Unity.CodeEditor;

namespace Antigravity.Editor
{
	internal interface IAntigravityBaseInstallation
	{
		string Name { get; }
		string Path { get; }
		bool SupportsAnalyzers { get; }
		Version LatestLanguageVersionSupported { get; }
		IGenerator ProjectGenerator { get; }
		string[] GetAnalyzers();
		CodeEditor.Installation ToCodeEditorInstallation();
		void CreateExtraFiles(string projectDirectory);
		bool Open(string path, int line, int column, string solution);
	}

	internal abstract class AntigravityBaseInstallation : IAntigravityBaseInstallation
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public Version Version { get; set; }
		public bool IsPrerelease { get; set; }

		public abstract bool SupportsAnalyzers { get; }
		public abstract Version LatestLanguageVersionSupported { get; }
		public abstract IGenerator ProjectGenerator { get; }

		public abstract string[] GetAnalyzers();
		public abstract void CreateExtraFiles(string projectDirectory);
		public abstract bool Open(string path, int line, int column, string solution);

		protected string[] GetAnalyzers(string analyzersPath)
		{
			try
			{
				if (string.IsNullOrEmpty(analyzersPath))
					return Array.Empty<string>();

				var extensions = System.IO.Directory.GetDirectories(analyzersPath, "CSharpAnalyzers");
				if (extensions.Length == 0)
					return Array.Empty<string>();

				return System.IO.Directory.GetFiles(extensions[0], "*.dll");
			}
			catch
			{
				return Array.Empty<string>();
			}
		}

		public virtual CodeEditor.Installation ToCodeEditorInstallation()
		{
			return new CodeEditor.Installation
			{
				Name = Name,
				Path = Path
			};
		}
	}
}
