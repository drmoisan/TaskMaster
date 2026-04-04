using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            Action act = () => new DirectoryInfoWrapper((DirectoryInfo)null);

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

        [TestMethod]
        public void PropertyDelegates_ShouldMirrorMockedIDirectoryInfo()
        {
            var parent = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var root = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;

            var attributes = FileAttributes.Directory;
            var creationTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);
            var creationTimeUtc = new DateTime(2024, 1, 2, 8, 4, 5, DateTimeKind.Utc);
            var lastAccessTime = new DateTime(2024, 2, 3, 4, 5, 6, DateTimeKind.Local);
            var lastAccessTimeUtc = new DateTime(2024, 2, 3, 9, 5, 6, DateTimeKind.Utc);
            var lastWriteTime = new DateTime(2024, 3, 4, 5, 6, 7, DateTimeKind.Local);
            var lastWriteTimeUtc = new DateTime(2024, 3, 4, 10, 6, 7, DateTimeKind.Utc);

            var directory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            directory.SetupGet(x => x.Attributes).Returns(() => attributes);
            directory
                .SetupSet(x => x.Attributes = It.IsAny<FileAttributes>())
                .Callback<FileAttributes>(value => attributes = value);
            directory.SetupGet(x => x.CreationTime).Returns(() => creationTime);
            directory
                .SetupSet(x => x.CreationTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => creationTime = value);
            directory.SetupGet(x => x.CreationTimeUtc).Returns(() => creationTimeUtc);
            directory
                .SetupSet(x => x.CreationTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => creationTimeUtc = value);
            directory.SetupGet(x => x.Exists).Returns(true);
            directory.SetupGet(x => x.Extension).Returns(string.Empty);
            directory.SetupGet(x => x.FullName).Returns(@"C:\Repo");
            directory.SetupGet(x => x.LastAccessTime).Returns(() => lastAccessTime);
            directory
                .SetupSet(x => x.LastAccessTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastAccessTime = value);
            directory.SetupGet(x => x.LastAccessTimeUtc).Returns(() => lastAccessTimeUtc);
            directory
                .SetupSet(x => x.LastAccessTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastAccessTimeUtc = value);
            directory.SetupGet(x => x.LastWriteTime).Returns(() => lastWriteTime);
            directory
                .SetupSet(x => x.LastWriteTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastWriteTime = value);
            directory.SetupGet(x => x.LastWriteTimeUtc).Returns(() => lastWriteTimeUtc);
            directory
                .SetupSet(x => x.LastWriteTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastWriteTimeUtc = value);
            directory.SetupGet(x => x.Name).Returns("Repo");
            directory.SetupGet(x => x.Parent).Returns(parent);
            directory.SetupGet(x => x.Root).Returns(root);

            var wrapper = new DirectoryInfoWrapper(directory.Object);

            wrapper.Attributes.Should().Be(FileAttributes.Directory);
            wrapper.CreationTime.Should().Be(creationTime);
            wrapper.CreationTimeUtc.Should().Be(creationTimeUtc);
            wrapper.Exists.Should().BeTrue();
            wrapper.Extension.Should().BeEmpty();
            wrapper.FullName.Should().Be(@"C:\Repo");
            wrapper.LastAccessTime.Should().Be(lastAccessTime);
            wrapper.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            wrapper.LastWriteTime.Should().Be(lastWriteTime);
            wrapper.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);
            wrapper.Name.Should().Be("Repo");
            wrapper.Parent.Should().BeSameAs(parent);
            wrapper.Root.Should().BeSameAs(root);

            var nextLocal = creationTime.AddDays(1);
            var nextUtc = creationTimeUtc.AddDays(1);
            var nextAccessLocal = lastAccessTime.AddDays(1);
            var nextAccessUtc = lastAccessTimeUtc.AddDays(1);
            var nextWriteLocal = lastWriteTime.AddDays(1);
            var nextWriteUtc = lastWriteTimeUtc.AddDays(1);

            wrapper.Attributes = FileAttributes.ReadOnly;
            wrapper.CreationTime = nextLocal;
            wrapper.CreationTimeUtc = nextUtc;
            wrapper.LastAccessTime = nextAccessLocal;
            wrapper.LastAccessTimeUtc = nextAccessUtc;
            wrapper.LastWriteTime = nextWriteLocal;
            wrapper.LastWriteTimeUtc = nextWriteUtc;

            attributes.Should().Be(FileAttributes.ReadOnly);
            creationTime.Should().Be(nextLocal);
            creationTimeUtc.Should().Be(nextUtc);
            lastAccessTime.Should().Be(nextAccessLocal);
            lastAccessTimeUtc.Should().Be(nextAccessUtc);
            lastWriteTime.Should().Be(nextWriteLocal);
            lastWriteTimeUtc.Should().Be(nextWriteUtc);
        }

        [TestMethod]
        public void EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo()
        {
            var childDirectory = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var childFile = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var childInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict).Object;
            var subdirectory = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var secureSubdirectory = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var security = new DirectorySecurity();

            var directory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            directory.Setup(x => x.CreateSubdirectory("child")).Returns(subdirectory);
            directory
                .Setup(x => x.CreateSubdirectory("child", security))
                .Returns(secureSubdirectory);
            directory.Setup(x => x.EnumerateDirectories()).Returns(new[] { childDirectory });
            directory.Setup(x => x.EnumerateDirectories("src")).Returns(new[] { childDirectory });
            directory
                .Setup(x => x.EnumerateDirectories("src", SearchOption.AllDirectories))
                .Returns(new[] { childDirectory });
            directory.Setup(x => x.EnumerateFiles()).Returns(new[] { childFile });
            directory.Setup(x => x.EnumerateFiles("*.cs")).Returns(new[] { childFile });
            directory
                .Setup(x => x.EnumerateFiles("*.cs", SearchOption.AllDirectories))
                .Returns(new[] { childFile });
            directory.Setup(x => x.EnumerateFileSystemInfos()).Returns(new[] { childInfo });
            directory.Setup(x => x.EnumerateFileSystemInfos("*.cs")).Returns(new[] { childInfo });
            directory
                .Setup(x => x.EnumerateFileSystemInfos("*.cs", SearchOption.AllDirectories))
                .Returns(new[] { childInfo });
            directory.Setup(x => x.GetDirectories()).Returns(new[] { childDirectory });
            directory.Setup(x => x.GetDirectories("src")).Returns(new[] { childDirectory });
            directory
                .Setup(x => x.GetDirectories("src", SearchOption.AllDirectories))
                .Returns(new[] { childDirectory });
            directory.Setup(x => x.GetFiles()).Returns(new[] { childFile });
            directory.Setup(x => x.GetFiles("*.cs")).Returns(new[] { childFile });
            directory
                .Setup(x => x.GetFiles("*.cs", SearchOption.AllDirectories))
                .Returns(new[] { childFile });
            directory.Setup(x => x.GetFileSystemInfos()).Returns(new[] { childInfo });
            directory.Setup(x => x.GetFileSystemInfos("*.cs")).Returns(new[] { childInfo });
            directory
                .Setup(x => x.GetFileSystemInfos("*.cs", SearchOption.AllDirectories))
                .Returns(new[] { childInfo });

            var wrapper = new DirectoryInfoWrapper(directory.Object);

            wrapper.CreateSubdirectory("child").Should().BeSameAs(subdirectory);
            wrapper.CreateSubdirectory("child", security).Should().BeSameAs(secureSubdirectory);
            wrapper
                .EnumerateDirectories()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper
                .EnumerateDirectories("src")
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper
                .EnumerateDirectories("src", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper.EnumerateFiles().Should().ContainSingle().Which.Should().BeSameAs(childFile);
            wrapper
                .EnumerateFiles("*.cs")
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childFile);
            wrapper
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childFile);
            wrapper
                .EnumerateFileSystemInfos()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
            wrapper
                .EnumerateFileSystemInfos("*.cs")
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
            wrapper
                .EnumerateFileSystemInfos("*.cs", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
            wrapper
                .GetDirectories()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper
                .GetDirectories("src")
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper
                .GetDirectories("src", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childDirectory);
            wrapper.GetFiles().Should().ContainSingle().Which.Should().BeSameAs(childFile);
            wrapper.GetFiles("*.cs").Should().ContainSingle().Which.Should().BeSameAs(childFile);
            wrapper
                .GetFiles("*.cs", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childFile);
            wrapper
                .GetFileSystemInfos()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
            wrapper
                .GetFileSystemInfos("*.cs")
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
            wrapper
                .GetFileSystemInfos("*.cs", SearchOption.AllDirectories)
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeSameAs(childInfo);
        }

        [TestMethod]
        public void LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo()
        {
            var objectDataInfo = new SerializationInfo(
                typeof(DirectoryInfoWrapper),
                new FormatterConverter()
            );
            var streamingContext = new StreamingContext(StreamingContextStates.All);
            var directorySecurity = new DirectorySecurity();
            var includeSectionsSecurity = new DirectorySecurity();

            var directory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            directory.Setup(x => x.Create());
            directory.Setup(x => x.Create(directorySecurity));
            directory.Setup(x => x.Delete());
            directory.Setup(x => x.Delete(true));
            directory.Setup(x => x.GetAccessControl()).Returns(directorySecurity);
            directory
                .Setup(x => x.GetAccessControl(AccessControlSections.Access))
                .Returns(includeSectionsSecurity);
            directory.Setup(x => x.GetObjectData(objectDataInfo, streamingContext));
            directory.Setup(x => x.MoveTo("moved"));
            directory.Setup(x => x.Refresh());
            directory.Setup(x => x.SetAccessControl(directorySecurity));
            directory.Setup(x => x.ToString()).Returns("wrapped-directory");

            var wrapper = new DirectoryInfoWrapper(directory.Object);

            wrapper.Create();
            wrapper.Create(directorySecurity);
            wrapper.Delete();
            wrapper.Delete(recursive: true);
            wrapper.GetAccessControl().Should().BeSameAs(directorySecurity);
            wrapper
                .GetAccessControl(AccessControlSections.Access)
                .Should()
                .BeSameAs(includeSectionsSecurity);
            wrapper.GetObjectData(objectDataInfo, streamingContext);
            wrapper.MoveTo("moved");
            wrapper.Refresh();
            wrapper.SetAccessControl(directorySecurity);
            wrapper.ToString().Should().Be("wrapped-directory");
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
