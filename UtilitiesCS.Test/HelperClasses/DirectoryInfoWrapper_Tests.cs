using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.FileSystem;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class DirectoryInfoWrapper_Tests
    {
        [TestMethod]
        public void Constructor_WhenDirectoryInfoIsNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new DirectoryInfoWrapper(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("directoryInfo");
        }

        [TestMethod]
        public void Properties_ShouldMirrorWrappedDirectoryInfo()
        {
            // Arrange
            var directory = GetRepositoryRoot();
            var wrapper = new DirectoryInfoWrapper(directory);

            // Assert
            wrapper.Exists.Should().Be(directory.Exists);
            wrapper.FullName.Should().Be(directory.FullName);
            wrapper.Name.Should().Be(directory.Name);
            wrapper.Extension.Should().Be(directory.Extension);
            wrapper.Parent.FullName.Should().Be(directory.Parent.FullName);
            wrapper.Root.FullName.Should().Be(directory.Root.FullName);
        }

        [TestMethod]
        public void GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries()
        {
            // Arrange
            var directory = GetRepositoryRoot();
            var wrapper = new DirectoryInfoWrapper(directory);

            // Act
            var directories = wrapper.GetDirectories();
            var files = wrapper.GetFiles();

            // Assert
            directories.Should().NotBeEmpty();
            directories.Should().OnlyContain(item => item is DirectoryInfoWrapper);
            directories.Select(item => item.Name).Should().Contain("UtilitiesCS");

            files.Should().NotBeEmpty();
            files.Should().OnlyContain(item => item is FileInfoWrapper);
            files.Select(item => item.Name).Should().Contain("TaskMaster.sln");
        }

        [TestMethod]
        public void EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles()
        {
            // Arrange
            var directory = GetRepositoryRoot();
            var wrapper = new DirectoryInfoWrapper(directory);

            // Act
            var fileSystemInfos = wrapper.EnumerateFileSystemInfos().ToArray();

            // Assert
            fileSystemInfos
                .Should()
                .Contain(item => item is DirectoryInfoWrapper && item.Name == "UtilitiesCS");
            fileSystemInfos
                .Should()
                .Contain(item => item is FileInfoWrapper && item.Name == "TaskMaster.sln");
        }

        [TestMethod]
        public void ToString_ShouldDelegateToWrappedDirectoryInfo()
        {
            // Arrange
            var directory = GetRepositoryRoot();
            var wrapper = new DirectoryInfoWrapper(directory);

            // Act
            var result = wrapper.ToString();

            // Assert
            result.Should().Be(directory.ToString());
        }

        private static DirectoryInfo GetRepositoryRoot()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (
                current is not null
                && !File.Exists(Path.Combine(current.FullName, "TaskMaster.sln"))
            )
            {
                current = current.Parent;
            }

            current
                .Should()
                .NotBeNull("the test assembly should run inside the TaskMaster repository");
            return current;
        }
    }
}
