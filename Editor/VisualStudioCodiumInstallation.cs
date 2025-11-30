/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Antigravity.Editor
{
	internal class VisualStudioCodiumInstallation : AntigravityBaseInstallation
	{
		public override bool SupportsAnalyzers => false;

		public override Version LatestLanguageVersionSupported => new Version(11, 0);

		public override IGenerator ProjectGenerator => null;

		public static bool TryDiscoverInstallation(string editorPath, out IAntigravityBaseInstallation installation)
		{
			installation = null;
			return false;
		}

		public static IEnumerable<IAntigravityBaseInstallation> GetAntigravityBaseInstallations()
		{
			return Enumerable.Empty<IAntigravityBaseInstallation>();
		}

		public override string[] GetAnalyzers()
		{
			return Array.Empty<string>();
		}

		public override void CreateExtraFiles(string projectDirectory)
		{
		}

		public override bool Open(string path, int line, int column, string solution)
		{
			return false;
		}

		public static void Initialize()
		{
		}
	}
}
