using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Bootstrap
{
    /// <summary>
    /// Guards the invariant that the <c>FSharp.Core</c> binary deployed into every build output
    /// directory references a netstandard version that exists on .NET Framework.
    /// </summary>
    /// <remarks>
    /// The referenced-assembly table is read through the metadata reader rather than by loading
    /// the assembly. No assembly enters any application domain, which is what makes it possible to
    /// inspect fifteen files that all carry the same assembly identity in a single process. The
    /// alternative, a reflection-only load, deduplicates by identity and would need one child
    /// application domain per directory.
    /// </remarks>
    [TestClass]
    public class FSharpCoreDeployedIdentityTests
    {
        private const string NetstandardAssemblyName = "netstandard";
        private const string FSharpCoreFileName = "FSharp.Core.dll";
        private const string ControlFlavourDirectory = "netstandard2.1";

        private static readonly Version Netstandard20 = new Version(2, 0, 0, 0);
        private static readonly Version Netstandard21 = new Version(2, 1, 0, 0);

        /// <summary>
        /// Reads the referenced-assembly table of the assembly at the given path.
        /// </summary>
        /// <param name="assemblyPath">Full path of the assembly file to inspect.</param>
        /// <returns>One name and version pair per referenced assembly.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown, carrying the full path, when no file exists at that path. A missing output
        /// directory or an unrestored package folder is a broken precondition, so it fails loudly
        /// rather than being skipped.
        /// </exception>
        internal static IReadOnlyList<(string Name, Version Version)> ReadAssemblyReferences(
            string assemblyPath
        )
        {
            if (!File.Exists(assemblyPath))
            {
                throw new InvalidOperationException(
                    "Expected an assembly at "
                        + assemblyPath
                        + " but no file exists there. Build the solution and restore packages "
                        + "before running this test."
                );
            }

            List<(string Name, Version Version)> references =
                new List<(string Name, Version Version)>();
            using FileStream stream = new FileStream(
                assemblyPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            using var reader = new PEReader(stream);
            MetadataReader metadata = reader.GetMetadataReader();
            foreach (AssemblyReferenceHandle handle in metadata.AssemblyReferences)
            {
                AssemblyReference reference = metadata.GetAssemblyReference(handle);
                references.Add((metadata.GetString(reference.Name), reference.Version));
            }

            return references;
        }

        private static string DeployedFSharpCorePath(string projectDirectoryName)
        {
            return Path.Combine(
                FSharpCoreHintPathAlignmentTests.FindRepositoryRoot(),
                projectDirectoryName,
                "bin",
                "Debug",
                FSharpCoreFileName
            );
        }

        /// <summary>
        /// Asserts that the copy deployed into one build output directory references netstandard
        /// at the version that exists on .NET Framework and references no later one.
        /// </summary>
        /// <param name="projectDirectoryName">
        /// Name of the project directory whose <c>bin\Debug</c> output is inspected.
        /// </param>
        [DataTestMethod]
        [DataRow(
            "QuickFiler",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler]"
        )]
        [DataRow(
            "QuickFiler.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler.Test]"
        )]
        [DataRow(
            "ToDoModel",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel]"
        )]
        [DataRow(
            "UtilitiesCS",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS]"
        )]
        [DataRow(
            "UtilitiesCS.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS.Test]"
        )]
        [DataRow(
            "ToDoModel.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel.Test]"
        )]
        [DataRow("Tags", DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [Tags]")]
        [DataRow(
            "Tags.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [Tags.Test]"
        )]
        [DataRow(
            "VBFunctions.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [VBFunctions.Test]"
        )]
        [DataRow("TaskTree", DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskTree]")]
        [DataRow(
            "TaskTree.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskTree.Test]"
        )]
        [DataRow(
            "TaskVisualization",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization]"
        )]
        [DataRow(
            "TaskVisualization.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization.Test]"
        )]
        [DataRow(
            "TaskMaster",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster]"
        )]
        [DataRow(
            "TaskMaster.Test",
            DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster.Test]"
        )]
        public void DeployedFSharpCore_ReferencesNetstandard20(string projectDirectoryName)
        {
            // Arrange
            string deployedPath = DeployedFSharpCorePath(projectDirectoryName);

            // Act
            IReadOnlyList<(string Name, Version Version)> references = ReadAssemblyReferences(
                deployedPath
            );
            List<Version> netstandardVersions = references
                .Where(reference => reference.Name == NetstandardAssemblyName)
                .Select(reference => reference.Version)
                .ToList();

            // Assert
            netstandardVersions
                .Should()
                .ContainSingle(
                    "the copy deployed into "
                        + projectDirectoryName
                        + " must name netstandard exactly once, otherwise the version assertion "
                        + "below has nothing to measure"
                )
                .Which.Should()
                .Be(
                    Netstandard20,
                    "only netstandard 2.0.0.0 resolves on .NET Framework, so the copy deployed "
                        + "into "
                        + projectDirectoryName
                        + " must reference that version"
                );
            references
                .Select(reference => reference.Version)
                .Should()
                .NotContain(
                    Netstandard21,
                    "netstandard 2.1.0.0 does not exist on .NET Framework, so nothing deployed "
                        + "into "
                        + projectDirectoryName
                        + " may request it"
                );
        }

        /// <summary>
        /// Positive control over the package flavour this fix moves away from. Without it, a
        /// reader that never found a netstandard reference at all would report every directory as
        /// compliant.
        /// </summary>
        [TestMethod]
        public void Detector_OnPackageNetstandard21Binary_Reports21()
        {
            // Arrange
            IReadOnlyList<(string ProjectPath, string HintPathValue)> hintPaths =
                FSharpCoreHintPathAlignmentTests.DiscoverFSharpCoreHintPaths();
            hintPaths
                .Should()
                .NotBeEmpty(
                    "the control resolves the package folder from a discovered HintPath rather "
                        + "than hard-coding the package version"
                );

            (string ProjectPath, string HintPathValue) first = hintPaths[0];
            string resolvedHintPath = Path.GetFullPath(
                Path.Combine(Path.GetDirectoryName(first.ProjectPath), first.HintPathValue)
            );
            string libDirectory = Path.GetDirectoryName(Path.GetDirectoryName(resolvedHintPath));
            string controlPath = Path.Combine(
                libDirectory,
                ControlFlavourDirectory,
                FSharpCoreFileName
            );

            // Act
            IReadOnlyList<(string Name, Version Version)> references = ReadAssemblyReferences(
                controlPath
            );

            // Assert
            references
                .Where(reference => reference.Name == NetstandardAssemblyName)
                .Select(reference => reference.Version)
                .Should()
                .ContainSingle(
                    "the control binary names netstandard exactly once, so a single version is "
                        + "under test"
                )
                .Which.Should()
                .Be(
                    Netstandard21,
                    "this is the flavour whose netstandard reference cannot be satisfied on .NET "
                        + "Framework, so a reader that cannot see 2.1.0.0 here cannot see it "
                        + "anywhere"
                );
        }
    }
}
