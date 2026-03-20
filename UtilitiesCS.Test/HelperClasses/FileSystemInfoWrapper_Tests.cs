using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.FileSystem;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class FileSystemInfoWrapper_Tests
    {
        [TestMethod]
        public void Constructor_WhenNull_ShouldThrowArgumentNullException()
        {
            Action act = () => new FileSystemInfoWrapper(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Name_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            // Use a real DirectoryInfo (cwd always exists) as the underlying FileSystemInfo
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.Name.Should().Be(dirInfo.Name);
        }

        [TestMethod]
        public void FullName_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.FullName.Should().Be(dirInfo.FullName);
        }

        [TestMethod]
        public void Extension_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.Extension.Should().Be(dirInfo.Extension);
        }

        [TestMethod]
        public void Exists_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.Exists.Should().Be(dirInfo.Exists);
        }

        [TestMethod]
        public void Attributes_Getter_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.Attributes.Should().Be(dirInfo.Attributes);
        }

        [TestMethod]
        public void CreationTime_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.CreationTime.Should().Be(dirInfo.CreationTime);
        }

        [TestMethod]
        public void LastAccessTime_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.LastAccessTime.Should().Be(dirInfo.LastAccessTime);
        }

        [TestMethod]
        public void LastWriteTime_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.LastWriteTime.Should().Be(dirInfo.LastWriteTime);
        }

        [TestMethod]
        public void Refresh_ShouldNotThrow()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            Action act = () => wrapper.Refresh();

            act.Should().NotThrow();
        }
    }
}
