using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Coverage expansion for AppAutoFileObjects initialization and property decision paths that can
    /// run without Outlook automation or filesystem temporary files. Every system under test is built
    /// over its own seeded <see cref="InMemorySettingsStore"/>, so the scalar settings are read from
    /// and saved to the fake and no shared settings instance or settings file is touched.
    /// </summary>
    [TestClass]
    public class AppAutoFileObjectsCoverageExpansionTests
    {
        private const int SeedLngConvCtPwr = 2;
        private const int SeedConversationWeight = 10;
        private const int SeedMatchScore = 3;
        private const int SeedMismatchScore = -1;
        private const int SeedGapPenalty = -2;
        private const int SeedMaxRecents = 15;

        private static InMemorySettingsStore CreateScalarStore()
        {
            var store = new InMemorySettingsStore();
            store[nameof(TaskMaster.Properties.Settings.ConversationExponent)] = SeedLngConvCtPwr;
            store[nameof(TaskMaster.Properties.Settings.ConversationWeight)] =
                SeedConversationWeight;
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_MatchScore)] =
                SeedMatchScore;
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_MismatchScore)] =
                SeedMismatchScore;
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_GapPenalty)] =
                SeedGapPenalty;
            store[nameof(TaskMaster.Properties.Settings.MaxRecents)] = SeedMaxRecents;
            return store;
        }

        [TestMethod]
        public void Constructor_WhenCreated_InitializesDefaultStateAndCancelToken()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var firstToken = sut.CancelToken;
            var secondToken = sut.CancelToken;

            // Assert
            sut.SuggestionFilesLoaded.Should().BeFalse();
            sut.MaximizeQuickFileWindow.Should().BeNull();
            sut.FolderPredictor.Should().BeNull();
            firstToken.Should().Be(sut.CancelSource.Token);
            secondToken.Should().Be(firstToken);
        }

        [TestMethod]
        public void PropertyDecisions_WhenAssigned_RetainProvidedValues()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());
            var invoked = false;
            var predictor = new Mock<IFolderPredictor>().Object;

            // Act
            sut.SuggestionFilesLoaded = true;
            sut.MaximizeQuickFileWindow = () => invoked = true;
            sut.FolderPredictor = predictor;
            sut.MaximizeQuickFileWindow();

            // Assert
            sut.SuggestionFilesLoaded.Should().BeTrue();
            invoked.Should().BeTrue();
            sut.FolderPredictor.Should().BeSameAs(predictor);
        }

        [TestMethod]
        public void FileBackedProperties_WhenPythonStagingMissing_ReturnNullWithoutFilesystemAccess()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var movedMails = sut.MovedMails;
            var ctfMapPropertyValue = sut.CtfMap;
            var loadedCtfMap = sut.LoadCtfMap();
            var commonWords = sut.CommonWords;

            // Assert
            movedMails.Should().BeNull();
            ctfMapPropertyValue.Should().BeNull();
            loadedCtfMap.Should().BeNull();
            commonWords.Should().BeNull();
        }

        [TestMethod]
        public void FileBackedProperties_WhenOnlyFlowFolderExists_StillSkipPythonStagingLoads()
        {
            // Arrange
            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders.TryAdd("Flow", @"C:\TaskMaster\Flow");
            var sut = CreateSutWithSpecialFolders(specialFolders);

            // Act
            var movedMails = sut.MovedMails;
            var ctfMapPropertyValue = sut.CtfMap;
            var loadedCtfMap = sut.LoadCtfMap();
            var commonWords = sut.CommonWords;

            // Assert
            movedMails.Should().BeNull();
            ctfMapPropertyValue.Should().BeNull();
            loadedCtfMap.Should().BeNull();
            commonWords.Should().BeNull();
        }

        [TestMethod]
        public void ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues()
        {
            // Arrange
            var store = CreateScalarStore();
            var sut = CreateSutWithSpecialFolders(
                new ConcurrentDictionary<string, string>(),
                store: store
            );

            // Act
            var saves = SettingsSaveProbe.CountSavesOnCurrentThread(
                TaskMaster.Properties.Settings.Default,
                () =>
                {
                    sut.LngConvCtPwr = SeedLngConvCtPwr + 1;
                    sut.Conversation_Weight = SeedConversationWeight + 1;
                    sut.SmithWatterman_MatchScore = SeedMatchScore + 1;
                    sut.SmithWatterman_MismatchScore = SeedMismatchScore + 1;
                    sut.SmithWatterman_GapPenalty = SeedGapPenalty + 1;
                    sut.MaxRecents = SeedMaxRecents + 1;
                }
            );

            // Assert
            sut.LngConvCtPwr.Should().Be(SeedLngConvCtPwr + 1);
            sut.Conversation_Weight.Should().Be(SeedConversationWeight + 1);
            sut.SmithWatterman_MatchScore.Should().Be(SeedMatchScore + 1);
            sut.SmithWatterman_MismatchScore.Should().Be(SeedMismatchScore + 1);
            sut.SmithWatterman_GapPenalty.Should().Be(SeedGapPenalty + 1);
            sut.MaxRecents.Should().Be(SeedMaxRecents + 1);
            store[nameof(TaskMaster.Properties.Settings.ConversationExponent)]
                .Should()
                .Be(SeedLngConvCtPwr + 1);
            store[nameof(TaskMaster.Properties.Settings.ConversationWeight)]
                .Should()
                .Be(SeedConversationWeight + 1);
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_MatchScore)]
                .Should()
                .Be(SeedMatchScore + 1);
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_MismatchScore)]
                .Should()
                .Be(SeedMismatchScore + 1);
            store[nameof(TaskMaster.Properties.Settings.SmithWatterman_GapPenalty)]
                .Should()
                .Be(SeedGapPenalty + 1);
            store[nameof(TaskMaster.Properties.Settings.MaxRecents)]
                .Should()
                .Be(SeedMaxRecents + 1);
            store.SaveCount.Should().Be(6, "each scalar setter saves through the store once");
            saves
                .Should()
                .Be(0, "the test persisted settings to the shared per-host settings file");
        }

        [TestMethod]
        public void Constructor_ParentOnly_UsesApplicationSettingsStore()
        {
            // Arrange
            var parent = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            var field = typeof(AppAutoFileObjects).GetField(
                "_settingsStore",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act: inspect the default store without reading or writing any setting.
            var store = field!.GetValue(new AppAutoFileObjects(parent.Object));

            // Assert
            store
                .Should()
                .BeOfType<ApplicationSettingsStore>(
                    "the production constructor persists through TaskMaster's own settings"
                );
        }

        [TestMethod]
        public void Constructor_NullSettingsStore_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            Action act = () => _ = new AppAutoFileObjects(parent.Object, null!);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("settingsStore");
        }

        [TestMethod]
        public void CommonWordsSetter_WhenFlowFolderMissing_DoesNotSerializeOrAssignFolder()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());
            var commonWords = new SerializableList<string>();

            // Act
            sut.CommonWords = commonWords;

            // Assert
            sut.CommonWords.Should().BeSameAs(commonWords);
            commonWords.Folderpath.Should().BeEmpty();
            commonWords.Filename.Should().BeEmpty();
        }

        [TestMethod]
        public async Task PrivateAsyncLoaders_WhenConfigurationMissing_FailDeterministically()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(
                new ConcurrentDictionary<string, string>(),
                new ConcurrentDictionary<string, SmartSerializableLoader>()
            );

            // Act
            await InvokePrivateTaskAsync(sut, "LoadRecentsListAsync");
            await InvokePrivateTaskAsync(sut, "LoadMovedMailsAsync");
            await InvokePrivateTaskAsync(sut, "LoadCtfMapAsync");
            await InvokePrivateTaskAsync(sut, "LoadCommonWordsAsync");
            Func<Task> loadFilters = async () =>
                await InvokePrivateTaskAsync(sut, "LoadFiltersAsync");
            Func<Task> loadSubjectMapAndEncoder = async () =>
                await InvokePrivateTaskAsync(sut, "LoadSubjectMapAndEncoderAsync");

            // Assert
            sut.RecentsList.Should().BeNull();
            sut.MovedMails.Should().BeNull();
            sut.CtfMap.Should().BeNull();
            sut.CommonWords.Should().BeNull();
            await loadFilters.Should().ThrowAsync<NullReferenceException>();
            await loadSubjectMapAndEncoder.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public void BackupLoaders_WithMissingFiles_ReturnEmptyCollections()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var commonWords = InvokePrivate<IList<string>>(
                sut,
                "CommonWordsBackupLoader",
                @"C:\TaskMaster\missing-common-words.csv"
            );
            var subjectMapEntries = InvokePrivate<IList<SubjectMapEntry>>(
                sut,
                "SubjectMapBackupLoader",
                @"C:\TaskMaster\missing-subject-map.csv"
            );

            // Assert
            commonWords.Should().BeEmpty();
            subjectMapEntries.Should().BeEmpty();
        }

        private static AppAutoFileObjects CreateSutWithSpecialFolders(
            ConcurrentDictionary<string, string> specialFolders,
            ConcurrentDictionary<string, SmartSerializableLoader> config = null,
            InMemorySettingsStore store = null
        )
        {
            var mockFs = new Mock<IFileSystemFolderPaths>(MockBehavior.Strict);
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            if (config is not null)
            {
                mockGlobals
                    .SetupGet(x => x.IntelRes)
                    .Returns(new StubIntelligenceConfig(mockGlobals.Object, config));
            }
            return new AppAutoFileObjects(mockGlobals.Object, store ?? CreateScalarStore());
        }

        private static async Task InvokePrivateTaskAsync(AppAutoFileObjects sut, string methodName)
        {
            var task = (Task)InvokePrivate<object>(sut, methodName);
            await task;
        }

        private static T InvokePrivate<T>(
            AppAutoFileObjects sut,
            string methodName,
            params object[] arguments
        )
        {
            var method = typeof(AppAutoFileObjects).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method.Should().NotBeNull();
            return (T)method.Invoke(sut, arguments);
        }

        private sealed class StubIntelligenceConfig : IntelligenceConfig
        {
            internal StubIntelligenceConfig(
                IApplicationGlobals globals,
                ConcurrentDictionary<string, SmartSerializableLoader> config
            )
                : base(globals)
            {
                Config = config;
            }
        }
    }
}
