/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;

namespace Antigravity.Editor
{
	internal static class Discovery
	{
		public static IEnumerable<IAntigravityBaseInstallation> GetAntigravityBaseInstallations()
		{
			foreach (var installation in AntigravityInstallation.GetAntigravityBaseInstallations())
				yield return installation;
			foreach (var installation in VisualStudioCodiumInstallation.GetAntigravityBaseInstallations())
				yield return installation;
		}

		public static bool TryDiscoverInstallation(string editorPath, out IAntigravityBaseInstallation installation)
		{
			try
			{
				if (AntigravityInstallation.TryDiscoverInstallation(editorPath, out installation))
					return true;
				if (VisualStudioCodiumInstallation.TryDiscoverInstallation(editorPath, out installation))
					return true;
			}
			catch (IOException)
			{
				installation = null;
			}

			return false;
		}

		public static void Initialize()
		{
            AntigravityInstallation.Initialize();
            VisualStudioCodiumInstallation.Initialize();
		}
	}
}
