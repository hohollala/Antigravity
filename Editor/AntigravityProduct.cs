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

        internal static string ResolveLaunchPath(string selectedPath)
        {
            return ResolveLaunchPath(selectedPath, IdeInstallCandidates(), PathExists);
        }

        internal static string ResolveLaunchPath(string selectedPath, IEnumerable<string> ideCandidates, Func<string, bool> exists)
        {
            if (!IsLegacyPath(selectedPath))
                return selectedPath;

            var preferredIdePath = ideCandidates.FirstOrDefault(exists);
            return string.IsNullOrEmpty(preferredIdePath) ? selectedPath : preferredIdePath;
        }

        internal static IEnumerable<string> IdeInstallCandidates()
        {
#if UNITY_EDITOR_OSX
            yield return "/Applications/Antigravity IDE.app";

            var applications = "/Applications";
            if (Directory.Exists(applications))
            {
                foreach (var path in Directory.EnumerateDirectories(applications, "Antigravity IDE*.app").OrderBy(path => path))
                    yield return path;
            }
#elif UNITY_EDITOR_WIN
            var localAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
            var programFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            foreach (var basePath in new[] { localAppPath, programFiles })
            {
                yield return Path.Combine(basePath, "Antigravity IDE", "antigravity-ide.exe");
                yield return Path.Combine(basePath, "Antigravity IDE", "Antigravity IDE.exe");
                yield return Path.Combine(basePath, "antigravity-ide", "antigravity-ide.exe");
            }
#else
            yield return "/usr/bin/antigravity-ide";
            yield return "/bin/antigravity-ide";
            yield return "/usr/local/bin/antigravity-ide";
            yield return "/usr/bin/agy-ide";
            yield return "/bin/agy-ide";
            yield return "/usr/local/bin/agy-ide";

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return Path.Combine(home, ".antigravity-ide", "antigravity-ide", "bin", "antigravity-ide");
#endif
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

        private static bool IsLegacyPath(string path)
        {
            var name = LastPathSegment(path);
            return ContainsIgnoreCase(name, LegacyDisplayName) &&
                !ContainsIgnoreCase(name, DisplayName) &&
                !ContainsIgnoreCase(name, ApplicationName) &&
                !ContainsIgnoreCase(name, AliasName);
        }

        private static bool PathExists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
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
