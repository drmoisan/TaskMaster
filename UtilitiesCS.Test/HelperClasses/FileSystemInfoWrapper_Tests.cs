using System;
using System.IO;
using System.Runtime.Serialization;
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

        [TestMethod]
        public void MetadataSetters_WithCurrentValues_DelegateWithoutChangingState()
        {
            var dirInfo = new DirectoryInfo(
                Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "docs")
            );
            var wrapper = new FileSystemInfoWrapper(dirInfo);

            wrapper.Attributes = dirInfo.Attributes;
            wrapper.CreationTime = dirInfo.CreationTime;
            wrapper.CreationTimeUtc = dirInfo.CreationTimeUtc;
            wrapper.LastAccessTime = dirInfo.LastAccessTime;
            wrapper.LastAccessTimeUtc = dirInfo.LastAccessTimeUtc;
            wrapper.LastWriteTime = dirInfo.LastWriteTime;
            wrapper.LastWriteTimeUtc = dirInfo.LastWriteTimeUtc;

            wrapper.Attributes.Should().Be(dirInfo.Attributes);
            wrapper.CreationTimeUtc.Should().Be(dirInfo.CreationTimeUtc);
            wrapper.LastAccessTimeUtc.Should().Be(dirInfo.LastAccessTimeUtc);
            wrapper.LastWriteTimeUtc.Should().Be(dirInfo.LastWriteTimeUtc);
        }

        [TestMethod]
        public void DeleteAndGetObjectData_ShouldDelegateToUnderlyingFileSystemInfo()
        {
            var info = new RecordingFileSystemInfo();
            var wrapper = new FileSystemInfoWrapper(info);
            var serializationInfo = new SerializationInfo(
                typeof(RecordingFileSystemInfo),
                new FormatterConverter()
            );

            wrapper.Delete();
            wrapper.GetObjectData(
                serializationInfo,
                new StreamingContext(StreamingContextStates.All)
            );

            info.DeleteCalled.Should().BeTrue();
            info.GetObjectDataCalled.Should().BeTrue();
            serializationInfo.GetString("Marker").Should().Be("Recorded");
        }

        private sealed class RecordingFileSystemInfo : FileSystemInfo
        {
            public bool DeleteCalled { get; private set; }

            public bool GetObjectDataCalled { get; private set; }

            public override bool Exists => true;

            public override string Name => "recording";

            public override string FullName => @"C:\recording";

            public override void Delete()
            {
                DeleteCalled = true;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                GetObjectDataCalled = true;
                info.AddValue("Marker", "Recorded");
            }
        }
    }
}
