using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianSerializationHelper_Tests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IFileSystemFolderPaths> _mockFileSystem;
        private string _appDataRoot;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());

            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockGlobals = _mockRepository.Create<IApplicationGlobals>();
            _mockGlobals.SetupAllProperties();
            _mockFileSystem = _mockRepository.Create<IFileSystemFolderPaths>();
            _appDataRoot = @"C:\UnitTests\BayesianSerializationHelperTests";

            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders["AppData"] = _appDataRoot;
            _mockFileSystem.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            _mockGlobals.SetupGet(x => x.FS).Returns(_mockFileSystem.Object);
        }

        [TestMethod]
        public void Constructor_SetsGlobals()
        {
            // Act
            var sut = new BayesianSerializationHelper(_mockGlobals.Object);

            // Assert
            sut.Globals.Should().BeSameAs(_mockGlobals.Object);
        }

        [TestMethod]
        public void Deserialize_WhenAppDataMissing_ReturnsDefault()
        {
            // Arrange
            var mockGlobals = _mockRepository.Create<IApplicationGlobals>();
            var fileSystem = _mockRepository.Create<IFileSystemFolderPaths>();
            fileSystem
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            mockGlobals.SetupGet(x => x.FS).Returns(fileSystem.Object);
            var sut = new BayesianSerializationHelper(mockGlobals.Object);

            // Act
            var result = sut.Deserialize<SerializationFixture>("missing");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_WithExistingFile_ReturnsDeserializedObject()
        {
            // Arrange
            var sut = CreateHelper();
            var expected = new SerializationFixture { Name = "alpha", Value = 3 };
            sut.StoreJson("fixture.json", expected);

            // Act
            var result = sut.Deserialize<SerializationFixture>("fixture");

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithExistingFile_ReturnsDeserializedObject()
        {
            // Arrange
            var sut = CreateHelper();
            var expected = new SerializationFixture { Name = "beta", Value = 5 };
            sut.StoreJson("fixtureAsync.json", expected);

            // Act
            var result = await sut.DeserializeAsync<SerializationFixture>("fixtureAsync");

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithProgressAndExistingFile_ReturnsDeserializedObject()
        {
            // Arrange
            var sut = CreateHelper();
            var expected = new SerializationFixture { Name = "gamma", Value = 7 };
            sut.StoreJson("fixtureProgress.json", expected);

            // Act
            var result = await sut.DeserializeAsync<SerializationFixture>(
                BayesianPerformanceMeasurement_Tests.CreateFakeProgressTrackerPane(),
                "fixtureProgress"
            );

            // Assert
            result.Should().BeEquivalentTo(expected);
            sut.LastProgressReadPath.Should().Be(GetBayesianPath("fixtureProgress.json"));
            sut.LastProgressReadPrefix.Should().Be("Reading fixtureProgress.json Async: ");
        }

        [TestMethod]
        public void NonPublicHelpers_ReturnExpectedDiskAndJsonSettings()
        {
            // Arrange
            var sut = new BayesianSerializationHelper(_mockGlobals.Object);

            // Act
            var disk = (FilePathHelper)InvokeNonPublic(sut, "GetDisk", "scores", "daily", ".txt");
            var jsonSettings = (JsonSerializerSettings)InvokeNonPublic(sut, "GetJsonSettings");

            // Assert
            disk.FilePath.Should().Be(GetBayesianPath("scores_daily.txt"));
            jsonSettings.Converters.Should().ContainSingle(x => x is AppGlobalsConverter);
            jsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public async Task SaveTextsAsync_WritesUnicodeTextAndOverwritesExistingFile()
        {
            // Arrange
            var sut = CreateHelper();
            sut.StoreText("notes.txt", "stale");

            // Act
            await sut.SaveTextsAsync(new[] { "line one", "line two" }, "notes");

            // Assert
            sut.ReadStoredLines("notes.txt").Should().ContainInOrder("line one", "line two");
            sut.DeletedFiles.Should().Contain(GetBayesianPath("notes.txt"));
        }

        [TestMethod]
        public async Task SaveCsvAsync_WritesCommaSeparatedText()
        {
            // Arrange
            var sut = CreateHelper();

            // Act
            await sut.SaveCsvAsync([new[] { "A", "B" }, new[] { "1", "2" }], "matrix");

            // Assert
            sut.ReadStoredLines("matrix.csv").Should().ContainInOrder("A,B", "1,2");
        }

        [TestMethod]
        public void SerializeAndSave_WritesJsonToInMemoryStore()
        {
            // Arrange
            var sut = CreateHelper();
            var fixture = new SerializationFixture { Name = "delta", Value = 9 };

            // Act
            sut.SerializeAndSave(fixture, "savedFixture");

            // Assert
            sut.ReadStoredText("savedFixture.json").Should().Contain("delta");
        }

        [TestMethod]
        public async Task SerializeAndSaveAsync_WritesJsonUsingProgressPath()
        {
            // Arrange
            var sut = CreateHelper();
            var fixture = new SerializationFixture { Name = "epsilon", Value = 11 };

            // Act
            await sut.SerializeAndSaveAsync(
                fixture,
                BayesianPerformanceMeasurement_Tests.CreateFakeProgressTrackerPane(),
                "asyncSavedFixture"
            );

            // Assert
            sut.ReadStoredText("asyncSavedFixture.json").Should().Contain("epsilon");
            sut.LastProgressSerializedPath.Should().Be(GetBayesianPath("asyncSavedFixture.json"));
        }

        [TestMethod]
        public async Task WriteTextsAsync_AppendsEachLine()
        {
            // Arrange
            var sut = CreateHelper();
            var targetFile = Path.Combine(_appDataRoot, "write-texts.txt");

            // Act
            await sut.WriteTextsAsync(targetFile, new[] { "alpha", "beta" });

            // Assert
            sut.ReadStoredLinesByPath(targetFile).Should().ContainInOrder("alpha", "beta");
        }

        [TestMethod]
        public void InternalSerializeAndSave_UsesProvidedSerializerAndDisk()
        {
            // Arrange
            var sut = CreateHelper();
            var disk = new FilePathHelper("internal.json", GetBayesianFolder());
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());

            // Act
            InvokeGenericNonPublic(
                sut,
                "SerializeAndSave",
                typeof(SerializationFixture),
                new SerializationFixture { Name = "zeta", Value = 13 },
                serializer,
                disk
            );

            // Assert
            sut.ReadStoredText("internal.json").Should().Contain("zeta");
        }

        private TestableBayesianSerializationHelper CreateHelper()
        {
            return new TestableBayesianSerializationHelper(_mockGlobals.Object, _appDataRoot);
        }

        private string GetBayesianFolder()
        {
            return Path.Combine(_appDataRoot, "Bayesian");
        }

        private string GetBayesianPath(string fileName)
        {
            return Path.Combine(GetBayesianFolder(), fileName);
        }

        private static object InvokeNonPublic(
            object target,
            string methodName,
            params object[] args
        )
        {
            var method = target
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Should().NotBeNull();
            return method.Invoke(target, args);
        }

        private static object InvokeGenericNonPublic(
            object target,
            string methodName,
            Type genericType,
            params object[] args
        )
        {
            var method = target
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Single(x =>
                    x.Name == methodName
                    && x.IsGenericMethodDefinition
                    && x.GetParameters().Length == args.Length
                    && x.GetParameters()[1].ParameterType == typeof(JsonSerializer)
                );
            var closedMethod = method.MakeGenericMethod(genericType);
            return closedMethod.Invoke(target, args);
        }

        private sealed class SerializationFixture
        {
            public string Name { get; set; }

            public int Value { get; set; }
        }

        private sealed class TestableBayesianSerializationHelper : BayesianSerializationHelper
        {
            private readonly string _appDataRoot;
            private readonly Dictionary<string, string> _storedFiles = new(
                StringComparer.OrdinalIgnoreCase
            );

            public TestableBayesianSerializationHelper(
                IApplicationGlobals globals,
                string appDataRoot
            )
                : base(globals)
            {
                _appDataRoot = appDataRoot;
            }

            public List<string> DeletedFiles { get; } = new();
            public string LastProgressReadPath { get; private set; }
            public string LastProgressReadPrefix { get; private set; }
            public string LastProgressSerializedPath { get; private set; }

            public void StoreJson(string fileName, object value)
            {
                _storedFiles[GetBayesianPath(fileName)] = JsonConvert.SerializeObject(value);
            }

            public void StoreText(string fileName, string text)
            {
                _storedFiles[GetBayesianPath(fileName)] = text;
            }

            public string ReadStoredText(string fileName)
            {
                return _storedFiles[GetBayesianPath(fileName)];
            }

            public string[] ReadStoredLines(string fileName)
            {
                return ReadStoredLinesByPath(GetBayesianPath(fileName));
            }

            public string[] ReadStoredLinesByPath(string filePath)
            {
                return _storedFiles[filePath]
                    .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            }

            protected override bool FileExists(string filePath)
            {
                return _storedFiles.ContainsKey(filePath);
            }

            protected override void DeleteFile(string filePath)
            {
                DeletedFiles.Add(filePath);
                _storedFiles.Remove(filePath);
            }

            protected override string ReadAllText(string filePath)
            {
                return _storedFiles[filePath];
            }

            protected override Task<string> ReadAllTextAsync(string filePath)
            {
                return Task.FromResult(_storedFiles[filePath]);
            }

            protected override Task<string> ReadTextWithProgressAsync(
                FilePathHelper disk,
                ProgressTrackerPane progress,
                string messagePrefix
            )
            {
                LastProgressReadPath = disk.FilePath;
                LastProgressReadPrefix = messagePrefix;
                return Task.FromResult(_storedFiles[disk.FilePath]);
            }

            protected override Stream CreateTextWriteStream(string filePath)
            {
                var existingBytes = _storedFiles.TryGetValue(filePath, out var existingText)
                    ? Encoding.Unicode.GetBytes(existingText)
                    : [];
                return new CapturingMemoryStream(
                    existingBytes,
                    bytes => _storedFiles[filePath] = Encoding.Unicode.GetString(bytes)
                );
            }

            protected override Task SerializeWithProgressAsync<T>(
                JsonSerializer serializer,
                T obj,
                FilePathHelper disk,
                ProgressTrackerPane progress,
                CancellationToken cancel,
                string progressPrefix
            )
            {
                LastProgressSerializedPath = disk.FilePath;
                using var writer = new StringWriter();
                serializer.Serialize(writer, obj);
                _storedFiles[disk.FilePath] = writer.ToString();
                return Task.CompletedTask;
            }

            protected internal override void SerializeAndSave<T>(
                T obj,
                JsonSerializer serializer,
                FilePathHelper disk
            )
            {
                using var writer = new StringWriter();
                serializer.Serialize(writer, obj);
                _storedFiles[disk.FilePath] = writer.ToString();
            }

            private string GetBayesianPath(string fileName)
            {
                return Path.Combine(_appDataRoot, "Bayesian", fileName);
            }
        }

        private sealed class CapturingMemoryStream : MemoryStream
        {
            private readonly Action<byte[]> _onDispose;

            public CapturingMemoryStream(byte[] existingBytes, Action<byte[]> onDispose)
            {
                _onDispose = onDispose;
                if (existingBytes.Length > 0)
                {
                    Write(existingBytes, 0, existingBytes.Length);
                }

                Position = Length;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _onDispose(ToArray());
                }

                base.Dispose(disposing);
            }
        }
    }
}
