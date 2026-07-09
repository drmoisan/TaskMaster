using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster.Logging;

namespace TaskMaster.Test.Logging
{
    /// <summary>
    /// Deterministic verification of the pure log-directory resolve/ensure unit (issue #208,
    /// <see cref="TaskMaster.Logging.LogDirectoryInitializer"/>). The filesystem boundary is a Moq
    /// stub of <see cref="ILogDirectoryFileSystem"/>, so every scenario — missing directory
    /// (positive), directory already exists (edge), and invalid/unwritable path (error handling) —
    /// is exercised without a live log4net appender, a live Outlook process, the real filesystem,
    /// or any temporary file.
    /// </summary>
    [TestClass]
    public class LogDirectoryInitializerTests
    {
        [TestMethod]
        public void Constructor_NullFileSystem_ThrowsArgumentNullException()
        {
            // Act / Assert: the filesystem seam is a required collaborator.
            Action act = () => new LogDirectoryInitializer(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystem");
        }

        [TestMethod]
        public void ResolveLogDirectory_RelativePath_CombinesWithBaseAndNormalizes()
        {
            // Arrange: a rooted base directory and the configured relative "logs" path.
            var baseDirectory = @"C:\deployed\addin";

            // Act
            var resolved = LogDirectoryInitializer.ResolveLogDirectory(baseDirectory, "logs");

            // Assert: the resolved directory is the base combined with "logs", absolute, no trailing
            // separator.
            var expected = Path.GetFullPath(Path.Combine(baseDirectory, "logs"))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            resolved.Should().Be(expected);
            Path.IsPathRooted(resolved).Should().BeTrue();
        }

        [TestMethod]
        public void ResolveLogDirectory_TrailingSeparator_StripsToDirectory()
        {
            // Arrange: the log4net config uses a directory-prefix value ending with a separator.
            var baseDirectory = @"C:\deployed\addin";

            // Act
            var resolved = LogDirectoryInitializer.ResolveLogDirectory(
                baseDirectory,
                "logs" + Path.DirectorySeparatorChar
            );

            // Assert: the trailing separator is removed; the result is the directory itself.
            resolved.Should().EndWith("logs");
            resolved.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
        }

        [TestMethod]
        public void ResolveLogDirectory_RootedPath_ReturnsNormalizedPathIgnoringBase()
        {
            // Arrange: an absolute configured path should ignore the base directory.
            var rooted = @"D:\logstore\taskmaster";

            // Act
            var resolved = LogDirectoryInitializer.ResolveLogDirectory(@"C:\ignored", rooted);

            // Assert
            resolved
                .Should()
                .Be(
                    Path.GetFullPath(rooted)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                );
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void ResolveLogDirectory_NullOrBlankPath_ThrowsArgumentException(string logPath)
        {
            // Act / Assert: a blank configured path is an invariant violation and fails fast.
            Action act = () => LogDirectoryInitializer.ResolveLogDirectory(@"C:\base", logPath);
            act.Should().Throw<ArgumentException>().WithParameterName("logPath");
        }

        [TestMethod]
        public void EnsureLogDirectory_MissingDirectory_CreatesItAndReturnsTrue()
        {
            // Arrange: the directory does not exist yet (the reported issue #208 scenario).
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            var directory = @"C:\deployed\addin\logs";
            fileSystem.Setup(fs => fs.DirectoryExists(directory)).Returns(false);
            fileSystem.Setup(fs => fs.CreateDirectory(directory));
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act
            var created = initializer.EnsureLogDirectory(directory);

            // Assert: the directory is created exactly once and the method reports creation.
            created.Should().BeTrue();
            fileSystem.Verify(fs => fs.CreateDirectory(directory), Times.Once);
            fileSystem.VerifyAll();
        }

        [TestMethod]
        public void EnsureLogDirectory_DirectoryAlreadyExists_DoesNotCreateAndReturnsFalse()
        {
            // Arrange: the directory already exists (edge case — no creation should occur).
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            var directory = @"C:\deployed\addin\logs";
            fileSystem.Setup(fs => fs.DirectoryExists(directory)).Returns(true);
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act
            var created = initializer.EnsureLogDirectory(directory);

            // Assert: no creation attempt, method reports the directory already existed.
            created.Should().BeFalse();
            fileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void EnsureLogDirectory_NullOrBlankPath_ThrowsArgumentException(string directory)
        {
            // Arrange
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act / Assert: a blank directory is invalid and fails fast before touching the seam.
            Action act = () => initializer.EnsureLogDirectory(directory);
            act.Should().Throw<ArgumentException>().WithParameterName("logDirectory");
            fileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void EnsureLogDirectory_UnwritablePath_PropagatesCreateFailure()
        {
            // Arrange: the directory is missing and creation fails (for example an unwritable path).
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            var directory = @"C:\Windows\System32\config\taskmaster-logs";
            fileSystem.Setup(fs => fs.DirectoryExists(directory)).Returns(false);
            fileSystem
                .Setup(fs => fs.CreateDirectory(directory))
                .Throws(new UnauthorizedAccessException("Access to the path is denied."));
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act / Assert: the failure is propagated to the caller, not silently swallowed.
            Action act = () => initializer.EnsureLogDirectory(directory);
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [TestMethod]
        public void EnsureLogDirectoryForPath_MissingDirectory_ResolvesThenCreates()
        {
            // Arrange: combine resolution and creation for the relative "logs" path.
            var baseDirectory = @"C:\deployed\addin";
            var expected = Path.GetFullPath(Path.Combine(baseDirectory, "logs"))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            fileSystem.Setup(fs => fs.DirectoryExists(expected)).Returns(false);
            fileSystem.Setup(fs => fs.CreateDirectory(expected));
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act
            var created = initializer.EnsureLogDirectoryForPath(baseDirectory, "logs");

            // Assert: the resolved absolute directory was ensured exactly once.
            created.Should().BeTrue();
            fileSystem.Verify(fs => fs.CreateDirectory(expected), Times.Once);
            fileSystem.VerifyAll();
        }

        [TestMethod]
        public void EnsureLogDirectoryForPath_DirectoryExists_ResolvesAndReportsNoCreation()
        {
            // Arrange: the resolved directory already exists.
            var baseDirectory = @"C:\deployed\addin";
            var expected = Path.GetFullPath(Path.Combine(baseDirectory, "logs"))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fileSystem = new Mock<ILogDirectoryFileSystem>(MockBehavior.Strict);
            fileSystem.Setup(fs => fs.DirectoryExists(expected)).Returns(true);
            var initializer = new LogDirectoryInitializer(fileSystem.Object);

            // Act
            var created = initializer.EnsureLogDirectoryForPath(baseDirectory, "logs");

            // Assert
            created.Should().BeFalse();
            fileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
        }
    }
}
