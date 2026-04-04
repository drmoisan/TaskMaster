using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            Action act = () => new FileInfoWrapper((FileInfo)null);

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

        [TestMethod]
        public void PropertyDelegates_ShouldMirrorMockedIFileInfo()
        {
            var directory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            directory.SetupGet(x => x.FullName).Returns(@"C:\Repo");

            var attributes = FileAttributes.Normal;
            var creationTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);
            var creationTimeUtc = new DateTime(2024, 1, 2, 8, 4, 5, DateTimeKind.Utc);
            var lastAccessTime = new DateTime(2024, 2, 3, 4, 5, 6, DateTimeKind.Local);
            var lastAccessTimeUtc = new DateTime(2024, 2, 3, 9, 5, 6, DateTimeKind.Utc);
            var lastWriteTime = new DateTime(2024, 3, 4, 5, 6, 7, DateTimeKind.Local);
            var lastWriteTimeUtc = new DateTime(2024, 3, 4, 10, 6, 7, DateTimeKind.Utc);
            var isReadOnly = false;

            var fileInfo = new Mock<IFileInfo>(MockBehavior.Strict);
            fileInfo.SetupGet(x => x.Attributes).Returns(() => attributes);
            fileInfo
                .SetupSet(x => x.Attributes = It.IsAny<FileAttributes>())
                .Callback<FileAttributes>(value => attributes = value);
            fileInfo.SetupGet(x => x.CreationTime).Returns(() => creationTime);
            fileInfo
                .SetupSet(x => x.CreationTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => creationTime = value);
            fileInfo.SetupGet(x => x.CreationTimeUtc).Returns(() => creationTimeUtc);
            fileInfo
                .SetupSet(x => x.CreationTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => creationTimeUtc = value);
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.Extension).Returns(".txt");
            fileInfo.SetupGet(x => x.FullName).Returns(@"C:\Repo\file.txt");
            fileInfo.SetupGet(x => x.LastAccessTime).Returns(() => lastAccessTime);
            fileInfo
                .SetupSet(x => x.LastAccessTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastAccessTime = value);
            fileInfo.SetupGet(x => x.LastAccessTimeUtc).Returns(() => lastAccessTimeUtc);
            fileInfo
                .SetupSet(x => x.LastAccessTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastAccessTimeUtc = value);
            fileInfo.SetupGet(x => x.LastWriteTime).Returns(() => lastWriteTime);
            fileInfo
                .SetupSet(x => x.LastWriteTime = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastWriteTime = value);
            fileInfo.SetupGet(x => x.LastWriteTimeUtc).Returns(() => lastWriteTimeUtc);
            fileInfo
                .SetupSet(x => x.LastWriteTimeUtc = It.IsAny<DateTime>())
                .Callback<DateTime>(value => lastWriteTimeUtc = value);
            fileInfo.SetupGet(x => x.Name).Returns("file.txt");
            fileInfo.SetupGet(x => x.Directory).Returns(directory.Object);
            fileInfo.SetupGet(x => x.DirectoryName).Returns(@"C:\Repo");
            fileInfo.SetupGet(x => x.IsReadOnly).Returns(() => isReadOnly);
            fileInfo
                .SetupSet(x => x.IsReadOnly = It.IsAny<bool>())
                .Callback<bool>(value => isReadOnly = value);
            fileInfo.SetupGet(x => x.Length).Returns(123L);

            var wrapper = new FileInfoWrapper(fileInfo.Object);

            wrapper.Attributes.Should().Be(FileAttributes.Normal);
            wrapper.CreationTime.Should().Be(creationTime);
            wrapper.CreationTimeUtc.Should().Be(creationTimeUtc);
            wrapper.Exists.Should().BeTrue();
            wrapper.Extension.Should().Be(".txt");
            wrapper.FullName.Should().Be(@"C:\Repo\file.txt");
            wrapper.LastAccessTime.Should().Be(lastAccessTime);
            wrapper.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            wrapper.LastWriteTime.Should().Be(lastWriteTime);
            wrapper.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);
            wrapper.Name.Should().Be("file.txt");
            wrapper.Directory.Should().BeSameAs(directory.Object);
            wrapper.DirectoryName.Should().Be(@"C:\Repo");
            wrapper.IsReadOnly.Should().BeFalse();
            wrapper.Length.Should().Be(123L);

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
            wrapper.IsReadOnly = true;

            attributes.Should().Be(FileAttributes.ReadOnly);
            creationTime.Should().Be(nextLocal);
            creationTimeUtc.Should().Be(nextUtc);
            lastAccessTime.Should().Be(nextAccessLocal);
            lastAccessTimeUtc.Should().Be(nextAccessUtc);
            lastWriteTime.Should().Be(nextWriteLocal);
            lastWriteTimeUtc.Should().Be(nextWriteUtc);
            isReadOnly.Should().BeTrue();
        }

        [TestMethod]
        public void StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo()
        {
            using var appendStream = new MemoryStream();
            using var createTextStream = new MemoryStream();
            using var openTextStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("text"));
            using var createStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var openModeStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var openModeAccessStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var openModeAccessShareStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var openReadStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var openWriteStream = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var appendWriter = new StreamWriter(
                appendStream,
                System.Text.Encoding.UTF8,
                1024,
                leaveOpen: true
            );
            using var createWriter = new StreamWriter(
                createTextStream,
                System.Text.Encoding.UTF8,
                1024,
                leaveOpen: true
            );
            using var textReader = new StreamReader(
                openTextStream,
                System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true
            );

            var copyTarget = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var copyOverwriteTarget = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var replaceTarget = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var replaceIgnoreTarget = new Mock<IFileInfo>(MockBehavior.Strict).Object;

            var fileInfo = new Mock<IFileInfo>(MockBehavior.Strict);
            fileInfo.Setup(x => x.AppendText()).Returns(appendWriter);
            fileInfo.Setup(x => x.CopyTo("copy.txt")).Returns(copyTarget);
            fileInfo.Setup(x => x.CopyTo("copy-overwrite.txt", true)).Returns(copyOverwriteTarget);
            fileInfo.Setup(x => x.Create()).Returns(createStream);
            fileInfo.Setup(x => x.CreateText()).Returns(createWriter);
            fileInfo.Setup(x => x.Open(FileMode.Open)).Returns(openModeStream);
            fileInfo
                .Setup(x => x.Open(FileMode.Open, FileAccess.Read))
                .Returns(openModeAccessStream);
            fileInfo
                .Setup(x => x.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(openModeAccessShareStream);
            fileInfo.Setup(x => x.OpenRead()).Returns(openReadStream);
            fileInfo.Setup(x => x.OpenText()).Returns(textReader);
            fileInfo.Setup(x => x.OpenWrite()).Returns(openWriteStream);
            fileInfo.Setup(x => x.Replace("dest.txt", "backup.txt")).Returns(replaceTarget);
            fileInfo
                .Setup(x => x.Replace("dest.txt", "backup.txt", true))
                .Returns(replaceIgnoreTarget);

            var wrapper = new FileInfoWrapper(fileInfo.Object);

            wrapper.AppendText().Should().BeSameAs(appendWriter);
            wrapper.CopyTo("copy.txt").Should().BeSameAs(copyTarget);
            wrapper
                .CopyTo("copy-overwrite.txt", overwrite: true)
                .Should()
                .BeSameAs(copyOverwriteTarget);
            wrapper.Create().Should().BeSameAs(createStream);
            wrapper.CreateText().Should().BeSameAs(createWriter);
            wrapper.Open(FileMode.Open).Should().BeSameAs(openModeStream);
            wrapper.Open(FileMode.Open, FileAccess.Read).Should().BeSameAs(openModeAccessStream);
            wrapper
                .Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                .Should()
                .BeSameAs(openModeAccessShareStream);
            wrapper.OpenRead().Should().BeSameAs(openReadStream);
            wrapper.OpenText().Should().BeSameAs(textReader);
            wrapper.OpenWrite().Should().BeSameAs(openWriteStream);
            wrapper.Replace("dest.txt", "backup.txt").Should().BeSameAs(replaceTarget);
            wrapper
                .Replace("dest.txt", "backup.txt", ignoreMetadataErrors: true)
                .Should()
                .BeSameAs(replaceIgnoreTarget);
        }

        [TestMethod]
        public void AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo()
        {
            var fileSecurity = new FileSecurity();
            var includeSectionsSecurity = new FileSecurity();
            var objectDataInfo = new SerializationInfo(
                typeof(FileInfoWrapper),
                new FormatterConverter()
            );
            var streamingContext = new StreamingContext(StreamingContextStates.All);

            var fileInfo = new Mock<IFileInfo>(MockBehavior.Strict);
            fileInfo.Setup(x => x.Decrypt());
            fileInfo.Setup(x => x.Delete());
            fileInfo.Setup(x => x.Encrypt());
            fileInfo.Setup(x => x.GetAccessControl()).Returns(fileSecurity);
            fileInfo
                .Setup(x => x.GetAccessControl(AccessControlSections.Access))
                .Returns(includeSectionsSecurity);
            fileInfo.Setup(x => x.GetObjectData(objectDataInfo, streamingContext));
            fileInfo.Setup(x => x.MoveTo("moved.txt"));
            fileInfo.Setup(x => x.Refresh());
            fileInfo.Setup(x => x.SetAccessControl(fileSecurity));
            fileInfo.Setup(x => x.ToString()).Returns("wrapped-file");

            var wrapper = new FileInfoWrapper(fileInfo.Object);

            wrapper.Decrypt();
            wrapper.Delete();
            wrapper.Encrypt();
            wrapper.GetAccessControl().Should().BeSameAs(fileSecurity);
            wrapper
                .GetAccessControl(AccessControlSections.Access)
                .Should()
                .BeSameAs(includeSectionsSecurity);
            wrapper.GetObjectData(objectDataInfo, streamingContext);
            wrapper.MoveTo("moved.txt");
            wrapper.Refresh();
            wrapper.SetAccessControl(fileSecurity);
            wrapper.ToString().Should().Be("wrapped-file");
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
