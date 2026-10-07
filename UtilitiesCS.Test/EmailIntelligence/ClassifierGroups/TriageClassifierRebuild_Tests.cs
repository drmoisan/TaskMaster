using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class TriageClassifierRebuild_Tests
    {
        [TestMethod]
        public async Task RebuildFromMinedMailAsync_ValidTriageLabels_RebuildsAllClassifierState()
        {
            // Arrange
            var triage = new UtilitiesCS.EmailIntelligence.Triage(
                new Mock<IApplicationGlobals>().Object
            );
            var minedMail = new[]
            {
                new MinedMailInfo { Triage = "A", Tokens = new[] { "alpha", "shared" } },
                new MinedMailInfo { Triage = "B", Tokens = new[] { "bravo", "shared" } },
                new MinedMailInfo { Triage = "C", Tokens = new[] { "charlie", "shared" } },
            };

            // Act
            var rebuilt = await triage.RebuildFromMinedMailAsync(
                minedMail,
                _ => Task.CompletedTask,
                _ => { }
            );

            // Assert
            rebuilt.Should().BeTrue();
            triage.ClassifierGroup.TotalEmailCount.Should().Be(3);
            triage.ClassifierGroup.SharedTokenBase.TokenFrequency["shared"].Should().Be(3);
            triage.ClassifierGroup.Classifiers["A"].MatchEmailCount.Should().Be(1);
            triage.ClassifierGroup.Classifiers["B"].MatchEmailCount.Should().Be(1);
            triage.ClassifierGroup.Classifiers["C"].MatchEmailCount.Should().Be(1);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("D")]
        public async Task RebuildFromMinedMailAsync_InvalidTriageLabel_DoesNotMutateAggregateState(
            string triageLabel
        )
        {
            // Arrange
            var triage = new UtilitiesCS.EmailIntelligence.Triage(
                new Mock<IApplicationGlobals>().Object
            );
            await triage.RebuildFromMinedMailAsync(
                new[]
                {
                    new MinedMailInfo { Triage = "A", Tokens = new[] { "seed" } },
                    new MinedMailInfo { Triage = "B", Tokens = new[] { "shared" } },
                    new MinedMailInfo { Triage = "C", Tokens = new[] { "shared" } },
                },
                _ => Task.CompletedTask,
                _ => { }
            );
            var existingGroup = triage.ClassifierGroup;
            var existingEmailCount = existingGroup.TotalEmailCount;
            var existingSharedTokenCount = existingGroup.SharedTokenBase.TokenFrequency["shared"];

            // Act
            var rebuilt = await triage.RebuildFromMinedMailAsync(
                new[]
                {
                    new MinedMailInfo { Triage = triageLabel, Tokens = new[] { "invalid" } },
                },
                _ => Task.CompletedTask,
                _ => { }
            );

            // Assert
            rebuilt.Should().BeFalse();
            triage.ClassifierGroup.Should().BeSameAs(existingGroup);
            triage.ClassifierGroup.TotalEmailCount.Should().Be(existingEmailCount);
            triage
                .ClassifierGroup.SharedTokenBase.TokenFrequency["shared"]
                .Should()
                .Be(existingSharedTokenCount);
            triage.ClassifierGroup.SharedTokenBase.TokenFrequency.Should().NotContainKey("invalid");
        }

        [TestMethod]
        public async Task RebuildFromMinedMailAsync_ValidTrainingData_PersistsAndReplacesManagerOnce()
        {
            // Arrange
            var triage = new UtilitiesCS.EmailIntelligence.Triage(
                new Mock<IApplicationGlobals>().Object
            );
            var persistenceCount = 0;
            var managerReplacementCount = 0;
            BayesianClassifierGroup replacement = null;

            // Act
            var rebuilt = await triage.RebuildFromMinedMailAsync(
                new[]
                {
                    new MinedMailInfo { Triage = "A", Tokens = new[] { "alpha" } },
                    new MinedMailInfo { Triage = "B", Tokens = new[] { "bravo" } },
                    new MinedMailInfo { Triage = "C", Tokens = new[] { "charlie" } },
                },
                _ =>
                {
                    persistenceCount++;
                    return Task.CompletedTask;
                },
                classifierGroup =>
                {
                    managerReplacementCount++;
                    replacement = classifierGroup;
                }
            );

            // Assert
            rebuilt.Should().BeTrue();
            persistenceCount.Should().Be(1);
            managerReplacementCount.Should().Be(1);
            replacement.Should().BeSameAs(triage.ClassifierGroup);
        }

        [TestMethod]
        public async Task RebuildFromMinedMailAsync_NoValidTrainingData_DoesNotPersistOrReplaceManager()
        {
            // Arrange
            var triage = new UtilitiesCS.EmailIntelligence.Triage(
                new Mock<IApplicationGlobals>().Object
            )
            {
                ClassifierGroup = UtilitiesCS.EmailIntelligence.Triage.CreateClassifier(),
            };
            var existingGroup = triage.ClassifierGroup;
            var persistenceCount = 0;
            var managerReplacementCount = 0;

            // Act
            var rebuilt = await triage.RebuildFromMinedMailAsync(
                new[]
                {
                    new MinedMailInfo { Triage = null, Tokens = new[] { "null" } },
                    new MinedMailInfo { Triage = "d", Tokens = new[] { "lowercase" } },
                    new MinedMailInfo { Triage = "D", Tokens = new[] { "invalid" } },
                },
                _ =>
                {
                    persistenceCount++;
                    return Task.CompletedTask;
                },
                _ => managerReplacementCount++
            );

            // Assert
            rebuilt.Should().BeFalse();
            persistenceCount.Should().Be(0);
            managerReplacementCount.Should().Be(0);
            triage.ClassifierGroup.Should().BeSameAs(existingGroup);
        }

        [TestMethod]
        public async Task RebuildFromStagedMinedMailAsync_StagedTrainingData_PersistsAndReplacesManager()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var specialFolders = new ConcurrentDictionary<string, string>
            {
                ["AppData"] = "app-data",
            };
            var mockFileSystem = new Mock<IFileSystemFolderPaths>();
            mockFileSystem
                .SetupGet(fileSystem => fileSystem.SpecialFolders)
                .Returns(specialFolders);

            var manager = new ConfigurableManagerAsyncLazy(mockGlobals.Object);
            var loader = new SmartSerializableLoader(mockGlobals.Object) { Name = "Triage" };
            manager.SetConfiguration(
                new ConcurrentDictionary<string, SmartSerializableLoader> { ["Triage"] = loader }
            );

            var mockAutoFiles = new Mock<IAppAutoFileObjects>();
            mockAutoFiles.SetupGet(autoFiles => autoFiles.Manager).Returns(manager);
            mockGlobals.SetupGet(globals => globals.FS).Returns(mockFileSystem.Object);
            mockGlobals.SetupGet(globals => globals.AF).Returns(mockAutoFiles.Object);
            var triage = new UtilitiesCS.EmailIntelligence.Triage(mockGlobals.Object);
            string stagingPath = null;

            // Act
            var rebuilt = await triage.RebuildFromStagedMinedMailAsync(path =>
            {
                stagingPath = path;
                return Task.FromResult(
                    new[]
                    {
                        new MinedMailInfo { Triage = "A", Tokens = new[] { "alpha" } },
                        new MinedMailInfo { Triage = "B", Tokens = new[] { "bravo" } },
                        new MinedMailInfo { Triage = "C", Tokens = new[] { "charlie" } },
                    }
                );
            });

            // Assert
            rebuilt.Should().BeTrue();
            stagingPath.Should().Be(System.IO.Path.Combine("app-data", "Bayesian"));
            triage.ClassifierGroup.Config.Should().BeSameAs(loader.Config);
            manager.TryGetValue("Triage", out var rebuiltGroup).Should().BeTrue();
            (await rebuiltGroup).Should().BeSameAs(triage.ClassifierGroup);
        }

        [TestMethod]
        public async Task RebuildFromStagedMinedMailAsync_MissingAppData_DoesNotLoadStagedMail()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockFileSystem = new Mock<IFileSystemFolderPaths>();
            mockFileSystem
                .SetupGet(fileSystem => fileSystem.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            mockGlobals.SetupGet(globals => globals.FS).Returns(mockFileSystem.Object);
            var triage = new UtilitiesCS.EmailIntelligence.Triage(mockGlobals.Object);
            var loadCalled = false;

            // Act
            var rebuilt = await triage.RebuildFromStagedMinedMailAsync(_ =>
            {
                loadCalled = true;
                return Task.FromResult(Array.Empty<MinedMailInfo>());
            });

            // Assert
            rebuilt.Should().BeFalse();
            loadCalled.Should().BeFalse();
        }

        private sealed class ConfigurableManagerAsyncLazy : ManagerAsyncLazy
        {
            public ConfigurableManagerAsyncLazy(IApplicationGlobals globals)
                : base(globals) { }

            public void SetConfiguration(
                ConcurrentDictionary<string, SmartSerializableLoader> configuration
            ) =>
                Configuration = new AsyncLazy<
                    ConcurrentDictionary<string, SmartSerializableLoader>
                >(() => Task.FromResult(configuration));
        }
    }
}
