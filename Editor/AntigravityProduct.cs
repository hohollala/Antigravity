using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Antigravity.Editor
{
    internal static class AntigravityProduct
    {
        internal const string DisplayName = "Antigravity IDE";
        internal const string LegacyDisplayName = "Antigravity";
        internal const string ApplicationName = "antigravity-ide";
        internal const string AliasName = "agy-ide";
        internal const string LegacyApplicationName = "antigravity";

        internal static bool IsKnownBundleName(string path)
        {
            var name = LastPathSegment(path);
            return EndsWithIgnoreCase(name, ".app") &&
                (ContainsIgnoreCase(name, DisplayName) || ContainsIgnoreCase(name, LegacyDisplayName));
        }

        internal static bool IsKnownExecutableName(string path)
        {
            var name = LastPathSegment(path);
            if (!EndsWithIgnoreCase(name, ".exe"))
                return false;

            var withoutExtension = Path.GetFileNameWithoutExtension(name);
            return IsKnownCommandName(withoutExtension) ||
                ContainsIgnoreCase(withoutExtension, DisplayName) ||
                ContainsIgnoreCase(withoutExtension, LegacyDisplayName);
        }

        internal static bool IsKnownCommandName(string path)
        {
            var name = LastPathSegment(path);
            return EqualsIgnoreCase(name, ApplicationName) ||
                EqualsIgnoreCase(name, AliasName) ||
                EqualsIgnoreCase(name, LegacyApplicationName);
        }

        internal static string DisplayNameForPath(string path, string manifestName)
        {
            if (ContainsIgnoreCase(manifestName, DisplayName))
                return DisplayName;

            var name = LastPathSegment(path);
            if (ContainsIgnoreCase(name, DisplayName) ||
                ContainsIgnoreCase(name, ApplicationName) ||
                ContainsIgnoreCase(name, AliasName))
            {
                return DisplayName;
            }

            return LegacyDisplayName;
        }

        internal static IEnumerable<string> ExtensionRootCandidates(string userProfile, bool isPrerelease)
        {
            yield return Path.Combine(userProfile, ".antigravity-ide", "extensions");

            var legacyCodeFolder = isPrerelease ? ".vscode-insiders" : ".vscode";
            yield return Path.Combine(userProfile, legacyCodeFolder, "extensions");
        }

        internal static IEnumerable<string> UserDataRootCandidates(string userProfile)
        {
#if UNITY_EDITOR_OSX
            var applicationSupport = Path.Combine(userProfile, "Library", "Application Support");
            yield return Path.Combine(applicationSupport, DisplayName);
            yield return Path.Combine(applicationSupport, LegacyDisplayName);
            yield return Path.Combine(applicationSupport, LegacyApplicationName);
#elif UNITY_EDITOR_LINUX
            var configRoot = Path.Combine(userProfile, ".config");
            yield return Path.Combine(configRoot, DisplayName);
            yield return Path.Combine(configRoot, ApplicationName);
            yield return Path.Combine(configRoot, LegacyDisplayName);
            yield return Path.Combine(configRoot, LegacyApplicationName);
#else
            var roaming = Path.Combine(userProfile, "AppData", "Roaming");
            yield return Path.Combine(roaming, DisplayName);
            yield return Path.Combine(roaming, ApplicationName);
            yield return Path.Combine(roaming, LegacyDisplayName);
            yield return Path.Combine(roaming, LegacyApplicationName);
#endif
        }

        internal static IEnumerable<string> WorkspaceStorageCandidates(string userProfile)
        {
            return UserDataRootCandidates(userProfile)
                .Select(root => Path.Combine(root, "User", "workspaceStorage"));
        }

        internal static string UserKeybindingsPath(string userProfile)
        {
            return Path.Combine(ResolveUserDataRoot(userProfile), "User", "keybindings.json");
        }

        private static string ResolveUserDataRoot(string userProfile)
        {
            foreach (var root in UserDataRootCandidates(userProfile))
            {
                if (Directory.Exists(root))
                    return root;
            }

            return UserDataRootCandidates(userProfile).First();
        }

        private static string LastPathSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        private static bool ContainsIgnoreCase(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool EndsWithIgnoreCase(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.EndsWith(search, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EqualsIgnoreCase(string value, string search)
        {
            return string.Equals(value, search, StringComparison.OrdinalIgnoreCase);
        }
    }
}
