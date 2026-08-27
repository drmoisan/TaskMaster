using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Issue #614 tests for defect D7: the OneDrive root resolution in
    /// <c>AppFileSystemFolderPaths.LoadFolders</c>. Resolution now reads the environment through
    /// an injectable delegate seam and fails explicitly instead of silently falling back to
    /// AppData or to an arbitrary first entry. No test mutates process environment state.
    /// </summary>
    [TestClass]
    public class AppFileSystemFolderPathsOneDriveResolutionTests
    {
        private const string Commercial = @"C:\Users\testuser\OneDrive - Contoso";
        private const string Consumer = @"C:\Users\testuser\OneDrive";
        private const string Personal = @"C:\Users\testuser\OneDrive - Personal";

        [TestMethod]
        public void ResolveOneDriveRoot_AllThreeVariablesSet_PicksTheHighestPriority()
        {
            // Arrange
            Func<string, string> reader = Reader(Commercial, Consumer, Personal);

            // Act
            string actual = AppFileSystemFolderPaths.ResolveOneDriveRoot(reader);

            // Assert
            actual.Should().Be(Commercial, "OneDriveCommercial has the highest priority");
        }

        [TestMethod]
        public void ResolveOneDriveRoot_CommercialUnset_FallsToTheSecondPriority()
        {
            // Arrange
            Func<string, string> reader = Reader(null, Consumer, Personal);

            // Act
            string actual = AppFileSystemFolderPaths.ResolveOneDriveRoot(reader);

            // Assert
            actual.Should().Be(Consumer);
        }

        [TestMethod]
        public void ResolveOneDriveRoot_OnlyPersonalSet_FallsToTheThirdPriority()
        {
            // Arrange
            Func<string, string> reader = Reader(null, string.Empty, Personal);

            // Act
            string actual = AppFileSystemFolderPaths.ResolveOneDriveRoot(reader);

            // Assert
            actual.Should().Be(Personal);
        }

        [TestMethod]
        public void ResolveOneDriveRoot_WhitespaceOnlyValues_AreTreatedAsUnset()
        {
            // Arrange
            Func<string, string> reader = Reader("   ", "  ", Personal);

            // Act
            string actual = AppFileSystemFolderPaths.ResolveOneDriveRoot(reader);

            // Assert
            actual.Should().Be(Personal);
        }

        [TestMethod]
        public void ResolveOneDriveRoot_NoVariableSet_FailsExplicitlyWithARedactedDiagnostic()
        {
            // Arrange
            Func<string, string> reader = Reader(null, null, string.Empty);
            Action act = () => AppFileSystemFolderPaths.ResolveOneDriveRoot(reader);

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert: no AppData fallback, no arbitrary first entry, no leaked profile path.
            thrown.Message.Should().Contain("No OneDrive root is set in the environment");
            thrown.Message.Should().NotContain("testuser");
            thrown.Message.Should().NotContain("Contoso");
        }

        [TestMethod]
        public void ResolveOneDriveRoot_NullReader_ThrowsArgumentNullException()
        {
            // Arrange
            Action act = () => AppFileSystemFolderPaths.ResolveOneDriveRoot(null);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("readEnvironmentVariable");
        }

        [TestMethod]
        public void OneDriveVariablesInPriorityOrder_IsTheDocumentedThreeVariableSequence()
        {
            // Arrange / Act / Assert
            AppFileSystemFolderPaths
                .OneDriveVariablesInPriorityOrder.Should()
                .Equal(new[] { "OneDriveCommercial", "OneDrive", "OneDrivePersonal" });
        }

        /// <summary>Builds a reader over an in-memory map; process environment is never touched.</summary>
        private static Func<string, string> Reader(
            string commercial,
            string consumer,
            string personal
        )
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "OneDriveCommercial", commercial },
                { "OneDrive", consumer },
                { "OneDrivePersonal", personal },
            };
            return name => values.TryGetValue(name, out string value) ? value : null;
        }
    }
}
