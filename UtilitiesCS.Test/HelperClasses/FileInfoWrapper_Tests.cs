using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.FileSystem;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class FileInfoWrapper_Tests
    {
        [TestMethod]
        public void Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new FileInfoWrapper(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileInfo");
        }

        [TestMethod]
        public void Properties_ShouldMirrorWrappedFileInfo()
        {
            // Arrange
            var file = GetSolutionFile();
            var wrapper = new FileInfoWrapper(file);

            // Assert
            wrapper.Exists.Should().Be(file.Exists);
            wrapper.FullName.Should().Be(file.FullName);
            wrapper.Name.Should().Be(file.Name);
            wrapper.Extension.Should().Be(".sln");
            wrapper.DirectoryName.Should().Be(file.DirectoryName);
            wrapper.Directory.FullName.Should().Be(file.Directory.FullName);
        }

        [TestMethod]
        public void ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory()
        {
            // Arrange
            var wrapper = new FileInfoWrapper(GetSolutionFile());

            // Act
            var directoryWrapper = (DirectoryInfoWrapper)wrapper;

            // Assert
            directoryWrapper.FullName.Should().Be(wrapper.Directory.FullName);
            directoryWrapper.Name.Should().Be(wrapper.Directory.Name);
        }

        [TestMethod]
        public void OpenRead_ShouldReturnReadableStreamForWrappedFile()
        {
            // Arrange
            var wrapper = new FileInfoWrapper(GetSolutionFile());

            // Act
            using var stream = wrapper.OpenRead();

            // Assert
            stream.CanRead.Should().BeTrue();
            stream.Length.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void ToString_ShouldDelegateToWrappedFileInfo()
        {
            // Arrange
            var file = GetSolutionFile();
            var wrapper = new FileInfoWrapper(file);

            // Act
            var result = wrapper.ToString();

            // Assert
            result.Should().Be(file.ToString());
        }

        private static FileInfo GetSolutionFile()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (current is not null)
            {
                var solutionPath = Path.Combine(current.FullName, "TaskMaster.sln");
                if (File.Exists(solutionPath))
                {
                    return new FileInfo(solutionPath);
                }

                current = current.Parent;
            }

            throw new InvalidOperationException(
                "The TaskMaster solution file could not be located from the test assembly path."
            );
        }
    }
}
