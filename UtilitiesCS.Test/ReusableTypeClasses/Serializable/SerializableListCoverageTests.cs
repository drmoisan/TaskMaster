using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace UtilitiesCS.Test.ReusableTypeClasses.Serializable
{
    [TestClass]
    public class SerializableListCoverageTests
    {
        [TestMethod]
        public void ConstructorsAndListOperations_HandleConstructionAddRemoveAndEmptyLists()
        {
            // Arrange
            var defaultList = new SerializableList<int>();
            var seededList = new SerializableList<int>(new List<int> { 3, 1, 2 });
            var lazyList = new SerializableList<int>(Enumerable.Range(4, 2));

            // Act
            defaultList.Add(10);
            defaultList.Insert(1, 20);
            defaultList.Remove(10);
            seededList.Sort();
            lazyList.RemoveAt(0);

            // Assert
            defaultList.Should().Equal(20);
            seededList.ToList().Should().Equal(1, 2, 3);
            lazyList.ToList().Should().Equal(5);
            new SerializableList<int>().Count.Should().Be(0);
        }

        [TestMethod]
        public void PropertyChangedAndCollectionMembers_ReportExpectedState()
        {
            // Arrange
            var list = new SerializableList<string>();
            var propertyNames = new List<string>();
            list.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);

            // Act
            list.Add("alpha");
            list.Add("beta");
            string[] copy = new string[3];
            list.CopyTo(copy, 1);
            bool removed = list.Remove("alpha");

            // Assert
            propertyNames
                .Should()
                .Equal(nameof(SerializableList<string>.Add), nameof(SerializableList<string>.Add));
            copy.Should().Equal(null, "alpha", "beta");
            removed.Should().BeTrue();
            list.Contains("beta").Should().BeTrue();
            list.IndexOf("beta").Should().Be(0);
            list.IsReadOnly.Should().BeFalse();
        }

        [TestMethod]
        public void FilepathFilenameAndFolderpath_ComposePathsAndRejectFolderPathInput()
        {
            // Arrange
            var list = new SerializableList<string>();
            string folder = @"C:\Lists";

            // Act
            list.Folderpath = folder;
            list.Filename = "items.json";
            string composedFromFolderFirst = list.Filepath;
            list.Filepath = Path.Combine(folder, "other.json");
            Action folderPathAssignment = () =>
                new SerializableList<string>().Filepath =
                    AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

            // Assert
            composedFromFolderFirst.Should().Be(Path.Combine(folder, "items.json"));
            list.Folderpath.Should().Be(folder);
            list.Filename.Should().Be("other.json");
            folderPathAssignment.Should().Throw<ArgumentException>().WithMessage("*Folder Path*");
        }

        [TestMethod]
        public void JsonSerializationShape_RoundTripsWithoutDiskIo()
        {
            // Arrange
            var source = new SerializableList<int>(new List<int> { 5, 6, 7 });
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            // Act
            string json = JsonConvert.SerializeObject(source, settings);
            var roundTrip = JsonConvert.DeserializeObject<SerializableList<int>>(json, settings);

            // Assert
            json.Should().Contain("5");
            json.Should().Contain("6");
            json.Should().Contain("7");
            roundTrip.Should().NotBeNull();
            roundTrip.ToList().Should().Equal(5, 6, 7);
        }

        [TestMethod]
        public void SerializeThreadSafe_WritesJsonThroughInjectedFileSystem()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2 });
            using var stream = new MemoryStream();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.CreateText("memory.json"))
                .Returns(() => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.SerializeThreadSafe("memory.json");
            stream.Position = 0;
            string json = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

            // Assert
            json.Should().Contain("1");
            json.Should().Contain("2");
        }

        [TestMethod]
        public void SerializeAndDeserialize_WithNoConfiguredPath_DoNotChangeList()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 8, 9 });

            // Act
            list.Serialize();
            list.Deserialize();
            list.Deserialize(askUserOnError: false);

            // Assert
            list.ToList().Should().Equal(8, 9);
        }

        [TestMethod]
        public void Deserialize_WithValidInjectedJson_LoadsItemsWithoutPrompt()
        {
            // Arrange
            var list = new SerializableList<int>();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText("valid.json"))
                .Returns("[3,4,5]");
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            list.Deserialize("valid.json", askUserOnError: false);

            // Assert
            list.ToList().Should().Equal(3, 4, 5);
            list.Filepath.Should().Be("valid.json");
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndBackupLoader_UsesBackupWithoutDiskIo()
        {
            // Arrange
            var list = new SerializableList<int>();
            string observedBackupPath = null;
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(@"C:\Lists\items.json"))
                .Throws(new FileNotFoundException("missing"));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(
                @"C:\Lists\items.json",
                path =>
                {
                    observedBackupPath = path;
                    return new List<int> { 11, 12 };
                },
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(11, 12);
            observedBackupPath.Should().Be(@"C:\Lists\items.csv");
        }

        [TestMethod]
        public void Deserialize_WithPromptNoThenYes_CreatesEmptyListWithoutBackupLoader()
        {
            // Arrange
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText("missing.json"))
                .Throws(new FileNotFoundException("missing"));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .SetupSequence(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.No)
                .Returns(System.Windows.Forms.DialogResult.Yes);
            var list = new SerializableList<int>();

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            list.Deserialize("missing.json", askUserOnError: true);

            // Assert
            list.Should().BeEmpty();
        }

        [TestMethod]
        public void FromListAndFindIndex_ReplaceStateAndFindExpectedItems()
        {
            // Arrange
            var list = new SerializableList<string>();

            // Act
            list.FromList(new List<string> { "ant", "bear", "cat", "dog" });

            // Assert
            list.FindIndex(value => value.Length == 3).Should().Be(0);
            list.FindIndex(1, value => value.Length == 3).Should().Be(2);
            list.FindIndex(1, 2, value => value.StartsWith("b", StringComparison.Ordinal))
                .Should()
                .Be(1);
            list.ToArray().Should().Equal("ant", "bear", "cat", "dog");
        }

        private static Mock<ISerializableListFileSystem> CreateFileSystemMock()
        {
            var fileSystemMock = new Mock<ISerializableListFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(fileSystem => fileSystem.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(Stream.Null));
            return fileSystemMock;
        }

        private sealed class SerializableListDependencyScope<T> : IDisposable
            where T : IComparable<T>
        {
            private readonly ISerializableListFileSystem originalFileSystem;
            private readonly ISerializableListPrompt originalPrompt;

            public SerializableListDependencyScope(
                ISerializableListFileSystem fileSystem,
                ISerializableListPrompt prompt = null
            )
            {
                originalFileSystem = SerializableList<T>.FileSystem;
                originalPrompt = SerializableList<T>.Prompt;
                SerializableList<T>.FileSystem = fileSystem;
                if (prompt is not null)
                {
                    SerializableList<T>.Prompt = prompt;
                }
            }

            public void Dispose()
            {
                SerializableList<T>.FileSystem = originalFileSystem;
                SerializableList<T>.Prompt = originalPrompt;
            }
        }
    }
}
