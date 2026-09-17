using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Bootstrap
{
    /// <summary>
    /// Guards the invariant that every <c>FSharp.Core</c> HintPath in the solution selects the
    /// netstandard2.0 flavour of the package.
    /// </summary>
    /// <remarks>
    /// The package ships both a netstandard2.0 and a netstandard2.1 flavour under the same
    /// assembly identity. Only the netstandard2.0 flavour is loadable on .NET Framework, and the
    /// identity is the same for both, so no build-time conflict reports the difference. These
    /// assertions read the project files directly, so they need neither a build nor a restored
    /// package folder and they cover every configuration rather than one built configuration.
    /// </remarks>
    [TestClass]
    public class FSharpCoreHintPathAlignmentTests
    {
        private const string ExpectedHintPathSuffix = @"\lib\netstandard2.0\FSharp.Core.dll";
        private const int ExpectedHintPathCount = 6;
        private const string SolutionFileName = "TaskMaster.sln";

        private static readonly string[] SkippedDirectoryNames =
        {
            "packages",
            "bin",
            "obj",
            "node_modules",
        };

        /// <summary>
        /// Walks up from the test assembly's base directory to the first directory holding the
        /// solution file and returns its full path.
        /// </summary>
        /// <returns>The full path of the repository root.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown, naming the directory the walk started from, when no ancestor holds the
        /// solution file. Failing loudly is deliberate: a silent skip would make every assertion
        /// in this class vacuous.
        /// </exception>
        internal static string FindRepositoryRoot()
        {
            string startDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(startDirectory);
            while (
                directory is not null
                && !File.Exists(Path.Combine(directory.FullName, SolutionFileName))
            )
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException(
                    "Could not locate the repository root: no "
                        + SolutionFileName
                        + " was found walking up from "
                        + startDirectory
                        + "."
                );
        }

        /// <summary>
        /// Enumerates every project file under the repository root and returns each HintPath that
        /// selects an <c>FSharp.Core</c> binary, paired with the project file that declares it.
        /// </summary>
        /// <returns>
        /// The discovered pairs, ordered by project path with an ordinal comparison so the result
        /// does not depend on directory enumeration order.
        /// </returns>
        /// <remarks>
        /// Directories whose name begins with a dot are skipped, which excludes the agent
        /// worktrees nested under the tooling directory. Each of those worktrees holds a full copy
        /// of every project file, so without the exclusion the count would be inflated well past
        /// the real one.
        /// </remarks>
        internal static IReadOnlyList<(
            string ProjectPath,
            string HintPathValue
        )> DiscoverFSharpCoreHintPaths()
        {
            string root = FindRepositoryRoot();
            List<(string ProjectPath, string HintPathValue)> discovered =
                new List<(string ProjectPath, string HintPathValue)>();
            Stack<string> pending = new Stack<string>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                string current = pending.Pop();

                foreach (string child in Directory.GetDirectories(current))
                {
                    string name = Path.GetFileName(child);
                    if (name.StartsWith(".", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (SkippedDirectoryNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    pending.Push(child);
                }

                foreach (string projectPath in Directory.GetFiles(current, "*.csproj"))
                {
                    XDocument document = XDocument.Load(projectPath);
                    foreach (XElement element in document.Descendants())
                    {
                        if (
                            !string.Equals(
                                element.Name.LocalName,
                                "HintPath",
                                StringComparison.Ordinal
                            )
                        )
                        {
                            continue;
                        }

                        string value = element.Value;
                        if (value.EndsWith("FSharp.Core.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            discovered.Add((projectPath, value));
                        }
                    }
                }
            }

            return discovered.OrderBy(pair => pair.ProjectPath, StringComparer.Ordinal).ToList();
        }

        private static string RelativeToRoot(string root, string path)
        {
            return path.Substring(root.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Asserts that the solution declares exactly six <c>FSharp.Core</c> HintPath entries, so
        /// the flavour assertion below is measured over the whole set rather than a shrunken one.
        /// </summary>
        [TestMethod]
        public void SolutionHasExactlySixFSharpCoreHintPaths()
        {
            // Arrange
            string root = FindRepositoryRoot();

            // Act
            IReadOnlyList<(string ProjectPath, string HintPathValue)> hintPaths =
                DiscoverFSharpCoreHintPaths();

            // Assert
            hintPaths
                .Select(pair => RelativeToRoot(root, pair.ProjectPath))
                .Should()
                .HaveCount(
                    ExpectedHintPathCount,
                    "a deleted HintPath, or one moved under a renamed package folder, would "
                        + "otherwise let the flavour assertion pass vacuously over a smaller set"
                );
        }

        /// <summary>
        /// Asserts that every discovered <c>FSharp.Core</c> HintPath ends in the netstandard2.0
        /// flavour path, naming any project file that does not.
        /// </summary>
        [TestMethod]
        public void EveryFSharpCoreHintPath_SelectsNetstandard20()
        {
            // Arrange
            string root = FindRepositoryRoot();
            IReadOnlyList<(string ProjectPath, string HintPathValue)> hintPaths =
                DiscoverFSharpCoreHintPaths();

            // Act
            List<string> offenders = hintPaths
                .Where(pair =>
                    !pair.HintPathValue.EndsWith(
                        ExpectedHintPathSuffix,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(pair => RelativeToRoot(root, pair.ProjectPath) + ": " + pair.HintPathValue)
                .ToList();

            // Assert
            // The offending entries are projected into the reason text rather than left to the
            // assertion renderer, which names only one representative element of a non-empty
            // collection. Without this the failure would identify one project file even when
            // several are misaligned.
            offenders
                .Should()
                .BeEmpty(
                    "the other flavour of this package references netstandard 2.1.0.0, an "
                        + "identity that does not exist on .NET Framework, so any copy deployed "
                        + "from it is unloadable wherever it lands; misaligned entries: "
                        + string.Join(" | ", offenders)
                );
        }
    }
}
