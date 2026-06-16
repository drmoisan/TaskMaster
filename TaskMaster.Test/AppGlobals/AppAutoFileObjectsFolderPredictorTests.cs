using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// AC23 load-on-startup tests for <see cref="AppAutoFileObjects.LoadFolderPredictorAsync"/>. The
    /// deserialization is exercised through the internal <c>FolderPredictorDeserializer</c> seam so
    /// the rehydration and fail-soft paths are deterministic and use no filesystem or temporary
    /// files. The persisted <c>UseLcppnPredictor</c> setting is saved and restored per test.
    /// </summary>
    [TestClass]
    public class AppAutoFileObjectsFolderPredictorTests
    {
        private bool _originalUseLcppn;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalUseLcppn = TaskMaster.Properties.Settings.Default.UseLcppnPredictor;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = _originalUseLcppn;
        }

        // Builds a mock IApplicationGlobals whose FS.SpecialFolders resolves AppData, so the load
        // path can build the dedicated store config without a real filesystem.
        private static Mock<IApplicationGlobals> CreateMockGlobalsWithAppData()
        {
            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders[@"AppData"] = @"C:\Users\test\AppData";
            var mockFs = new Mock<IFileSystemFolderPaths>();
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            return mockGlobals;
        }

        // AC23: setting ON + a persisted (deserializable) predictor -> the holder is populated with
        // an LcppnFolderPredictor.
        [TestMethod]
        public async Task LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder()
        {
            // Arrange
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = true;
            var mockGlobals = CreateMockGlobalsWithAppData();
            var sut = new AppAutoFileObjects(mockGlobals.Object);
            var persisted = new LcppnFolderPredictor();
            persisted.Train(@"Projects\Alpha", new[] { "alpha" }, 1);
            sut.FolderPredictorDeserializer = _ => Task.FromResult(persisted);

            // Act
            await sut.LoadFolderPredictorAsync();

            // Assert
            sut.FolderPredictor.Should().NotBeNull();
            sut.FolderPredictor.Should().BeOfType<LcppnFolderPredictor>();
            sut.FolderPredictor.Should().BeSameAs(persisted);
        }

        // AC23/AC22 fail-soft: setting ON but the dedicated file is absent (deserializer returns
        // null) -> the holder stays null and LoadFolderPredictorAsync does not throw.
        [TestMethod]
        public async Task LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull()
        {
            // Arrange
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = true;
            var mockGlobals = CreateMockGlobalsWithAppData();
            var sut = new AppAutoFileObjects(mockGlobals.Object);
            sut.FolderPredictorDeserializer = _ => Task.FromResult<LcppnFolderPredictor>(null);

            // Act
            var act = async () => await sut.LoadFolderPredictorAsync();

            // Assert: no throw, holder stays null (flat fallback).
            await act.Should().NotThrowAsync();
            sut.FolderPredictor.Should().BeNull();
        }

        // AC23/AC22 fail-soft: setting ON but the read/parse throws -> caught, holder stays null, no
        // throw out of the load path.
        [TestMethod]
        public async Task LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull()
        {
            // Arrange
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = true;
            var mockGlobals = CreateMockGlobalsWithAppData();
            var sut = new AppAutoFileObjects(mockGlobals.Object);
            sut.FolderPredictorDeserializer = _ =>
                throw new System.IO.IOException("simulated unreadable file");

            // Act
            var act = async () => await sut.LoadFolderPredictorAsync();

            // Assert
            await act.Should().NotThrowAsync();
            sut.FolderPredictor.Should().BeNull();
        }

        // AC13: setting OFF -> no load attempted, holder stays null, deserializer not invoked.
        [TestMethod]
        public async Task LoadFolderPredictorAsync_SettingOff_DoesNotLoad()
        {
            // Arrange
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = false;
            var mockGlobals = CreateMockGlobalsWithAppData();
            var sut = new AppAutoFileObjects(mockGlobals.Object);
            var invoked = false;
            sut.FolderPredictorDeserializer = _ =>
            {
                invoked = true;
                return Task.FromResult(new LcppnFolderPredictor());
            };

            // Act
            await sut.LoadFolderPredictorAsync();

            // Assert
            invoked.Should().BeFalse("OFF must not attempt a load");
            sut.FolderPredictor.Should().BeNull();
        }

        // AC23 fail-soft: setting ON but AppData special folder unresolved -> holder stays null, no
        // throw, deserializer not invoked.
        [TestMethod]
        public async Task LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull()
        {
            // Arrange: empty special folders (no AppData).
            TaskMaster.Properties.Settings.Default.UseLcppnPredictor = true;
            var mockFs = new Mock<IFileSystemFolderPaths>();
            mockFs
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            var sut = new AppAutoFileObjects(mockGlobals.Object);
            var invoked = false;
            sut.FolderPredictorDeserializer = _ =>
            {
                invoked = true;
                return Task.FromResult(new LcppnFolderPredictor());
            };

            // Act
            var act = async () => await sut.LoadFolderPredictorAsync();

            // Assert
            await act.Should().NotThrowAsync();
            sut.FolderPredictor.Should().BeNull();
            invoked.Should().BeFalse("no AppData means no deserialize attempt");
        }
    }
}
