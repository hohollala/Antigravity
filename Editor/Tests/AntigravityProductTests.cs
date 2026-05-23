using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Antigravity.Editor.Tests
{
    public class AntigravityProductTests
    {
        [Test]
        public void DefaultInstallPathUsesAntigravityIde()
        {
#if UNITY_EDITOR_OSX
            Assert.AreEqual("/Applications/Antigravity IDE.app", AntigravityInstallation.DefaultInstallPath());
#elif UNITY_EDITOR_WIN
            StringAssert.Contains("Antigravity IDE", AntigravityInstallation.DefaultInstallPath());
            StringAssert.EndsWith("antigravity-ide.exe", AntigravityInstallation.DefaultInstallPath());
#else
            Assert.AreEqual("/usr/bin/antigravity-ide", AntigravityInstallation.DefaultInstallPath());
#endif
        }

        [Test]
        public void TryDiscoverInstallationUsesAntigravityIdeDisplayName()
        {
#if UNITY_EDITOR_OSX
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var appPath = Path.Combine(tempRoot, "Antigravity IDE.app");
            var manifestDirectory = Path.Combine(appPath, "Contents", "resources", "app");
            Directory.CreateDirectory(manifestDirectory);
            File.WriteAllText(
                Path.Combine(manifestDirectory, "package.json"),
                "{\"name\":\"Antigravity IDE\",\"version\":\"1.107.0\"}");

            try
            {
                Assert.IsTrue(AntigravityInstallation.TryDiscoverInstallation(appPath, out var installation));
                Assert.AreEqual("Antigravity IDE [1.107.0]", installation.Name);
            }
            finally
            {
                Directory.Delete(tempRoot, true);
            }
#else
            Assert.Pass("Bundle discovery is platform-specific and covered on macOS.");
#endif
        }

        [Test]
        public void UserDataRootCandidatesPreferAntigravityIdeBeforeLegacyAntigravity()
        {
            var roots = AntigravityProduct.UserDataRootCandidates("/Users/example").ToArray();

            Assert.IsTrue(roots[0].Contains("Antigravity IDE") || roots[0].Contains(".antigravity-ide"));
            Assert.IsTrue(roots.Any(root => root.Contains("Antigravity")));
        }

        [Test]
        public void WorkspaceStorageCandidatesUseUserWorkspaceStorage()
        {
            var candidates = AntigravityProduct.WorkspaceStorageCandidates("/Users/example").ToArray();

            Assert.That(candidates[0], Does.EndWith(Path.Combine("User", "workspaceStorage")));
        }

        [Test]
        public void ResolveLaunchPathMapsLegacyAntigravitySelectionToInstalledIde()
        {
            var resolvedPath = AntigravityProduct.ResolveLaunchPath(
                "/Applications/Antigravity.app",
                new[] { "/Applications/Antigravity IDE.app" },
                path => path == "/Applications/Antigravity IDE.app");

            Assert.AreEqual("/Applications/Antigravity IDE.app", resolvedPath);
        }

        [Test]
        public void ResolveLaunchPathKeepsLegacySelectionWhenIdeIsNotInstalled()
        {
            var resolvedPath = AntigravityProduct.ResolveLaunchPath(
                "/Applications/Antigravity.app",
                new[] { "/Applications/Antigravity IDE.app" },
                path => false);

            Assert.AreEqual("/Applications/Antigravity.app", resolvedPath);
        }

        [Test]
        public void ResolveLaunchPathKeepsExplicitAntigravityIdeSelection()
        {
            var resolvedPath = AntigravityProduct.ResolveLaunchPath(
                "/Applications/Antigravity IDE.app",
                new[] { "/Applications/Other IDE.app" },
                path => true);

            Assert.AreEqual("/Applications/Antigravity IDE.app", resolvedPath);
        }
    }
}
