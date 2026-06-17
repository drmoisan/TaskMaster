using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Seam tests for the Folder-only <see cref="IFolderPredictor"/> accessor
    /// (<see cref="OlFolderClassifierGroup.GetFolderPredictorAsync"/>). With
    /// <c>UseLcppnPredictor</c> off (default) the accessor returns the flat
    /// <see cref="BayesianClassifierGroup"/> awaited from the unchanged <c>Manager["Folder"]</c>
    /// entry and its behavior is unchanged (AC13); with the flag on it returns the held
    /// <see cref="LcppnFolderPredictor"/>. Both predictors are reachable as
    /// <see cref="IFolderPredictor"/> through the accessor the callers route through (AC14). All
    /// tests are deterministic, in-memory, and use no Outlook COM instances or temporary files.
    /// </summary>
    [TestClass]
    public class FolderPredictorSeam_Tests
    {
        private static readonly string[] InboxTokens = ["invoice", "payment", "due", "invoice"];
        private static readonly string[] ArchiveTokens =
        [
            "newsletter",
            "weekly",
            "digest",
            "newsletter",
        ];

        // Builds a deterministic two-folder flat classifier mirroring the production build path.
        private static BayesianClassifierGroup CreateFlatGroup()
        {
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(InboxTokens.Concat(ArchiveTokens)),
            };
            group.Train("Inbox", InboxTokens, 1);
            group.Train("Archive", ArchiveTokens, 1);
            return group;
        }

        // Builds a deterministic LCPPN predictor with the flag enabled.
        private static LcppnFolderPredictor CreateLcppnPredictor()
        {
            var config = LcppnFolderPredictorConfig.Create(
                useLcppnPredictor: true,
                beamWidth: 3,
                minimumPathProbability: 0.01,
                shrinkageLambda: 0.7,
                minColdStartExamples: 0
            );
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = config.BeamWidth,
                MinimumPathProbability = config.MinimumPathProbability,
                ShrinkageLambda = config.ShrinkageLambda,
                MinColdStartExamples = config.MinColdStartExamples,
            };
            predictor.Train(@"Projects\Alpha", new[] { "alpha", "spec" }, 1);
            predictor.Train(@"Clients\Acme", new[] { "invoice", "billing" }, 1);
            return predictor;
        }

        // Builds a mock globals whose AF.Manager["Folder"] resolves to the supplied flat group.
        private static Mock<IApplicationGlobals> CreateMockGlobalsWithFolder(
            BayesianClassifierGroup folderGroup
        )
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object)
            {
                ["Folder"] = folderGroup.ToAsyncLazy(),
            };
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            // Real backing store for the shared Folder-only holder so SetLcppnPredictor (which now
            // writes Globals.AF.FolderPredictor) and the accessor observe the same value, matching
            // the production AppAutoFileObjects.FolderPredictor auto-property semantics.
            mockAf.SetupProperty(x => x.FolderPredictor);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);
            return mockGlobals;
        }

        // AC13: with the flag off (default), the accessor returns the flat BayesianClassifierGroup
        // awaited from Manager["Folder"].
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOff_ReturnsFlatManagerGroup()
        {
            // Arrange
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            // Act
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: the seam returns the exact flat instance held by Manager["Folder"].
            group.FolderPredictorConfig.UseLcppnPredictor.Should().BeFalse("flag defaults to off");
            predictor.Should().BeOfType<BayesianClassifierGroup>();
            predictor.Should().BeSameAs(flat);
        }

        // AC13: classification through the flag-off seam is byte-for-byte identical to calling the
        // flat group directly.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOff_ClassifyUnchanged()
        {
            // Arrange
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            // Act
            var seamResults = (await group.GetFolderPredictorAsync())
                .Classify(InboxTokens)
                .ToArray();
            var directResults = flat.Classify(InboxTokens).ToArray();

            // Assert: identical ordering and probabilities
            seamResults.Select(p => p.Class).Should().Equal(directResults.Select(p => p.Class));
            seamResults
                .Select(p => p.Probability)
                .Should()
                .Equal(directResults.Select(p => p.Probability));
            seamResults[0].Class.Should().Be("Inbox", "the query tokens match the Inbox corpus");
        }

        // AC13: Train and UnTrain through the flag-off seam mutate the flat group exactly as a
        // direct call would, leaving the seam path observationally identical to the prior path.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOff_TrainAndUnTrainAffectFlatGroup()
        {
            // Arrange
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var group = new OlFolderClassifierGroup(mockGlobals.Object);
            var newTokens = new[] { "ticket", "support" };

            // Act: train a new tag through the seam
            (await group.GetFolderPredictorAsync()).Train("Support", newTokens, 1);

            // Assert: the flat group now contains the new tag
            flat.Classifiers.Should().ContainKey("Support");

            // Act: untrain through the seam
            (await group.GetFolderPredictorAsync()).UnTrain("Support", newTokens, 1);

            // Assert: the tag is removed (MatchEmailCount returns to zero and the tag is dropped)
            flat.Classifiers.Should().NotContainKey("Support");
        }

        // AC14: with the flag on and a held predictor, the accessor returns the LcppnFolderPredictor.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOn_ReturnsHeldLcppnPredictor()
        {
            // Arrange
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var group = new OlFolderClassifierGroup(mockGlobals.Object)
            {
                FolderPredictorConfig = LcppnFolderPredictorConfig.Create(useLcppnPredictor: true),
            };
            var lcppn = CreateLcppnPredictor();
            group.SetLcppnPredictor(lcppn);

            // Act
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: the LCPPN predictor is returned, not the flat group.
            predictor.Should().BeOfType<LcppnFolderPredictor>();
            predictor.Should().BeSameAs(lcppn);
        }

        // AC14: both predictors are reachable as IFolderPredictor through the same accessor.
        [TestMethod]
        public async Task GetFolderPredictorAsync_BothPredictors_AreReachableAsIFolderPredictor()
        {
            // Arrange: flag-off instance returns the flat predictor; flag-on instance returns LCPPN.
            var flat = CreateFlatGroup();
            var offGlobals = CreateMockGlobalsWithFolder(flat);
            var offGroup = new OlFolderClassifierGroup(offGlobals.Object);

            var onGlobals = CreateMockGlobalsWithFolder(CreateFlatGroup());
            var onGroup = new OlFolderClassifierGroup(onGlobals.Object)
            {
                FolderPredictorConfig = LcppnFolderPredictorConfig.Create(useLcppnPredictor: true),
            };
            onGroup.SetLcppnPredictor(CreateLcppnPredictor());

            // Act
            IFolderPredictor flatSeam = await offGroup.GetFolderPredictorAsync();
            IFolderPredictor lcppnSeam = await onGroup.GetFolderPredictorAsync();

            // Assert: both are usable through the shared IFolderPredictor contract.
            flatSeam.Should().BeAssignableTo<IFolderPredictor>();
            lcppnSeam.Should().BeAssignableTo<IFolderPredictor>();
            flatSeam.Classify(InboxTokens).Should().NotBeNull();
            lcppnSeam.Classify(new[] { "alpha", "spec" }).Should().NotBeNull();
        }

        // AC13: when the flag is on but no predictor has been built/held, the accessor falls back
        // to the flat Manager["Folder"] entry rather than returning null.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOnButNoHeldPredictor_FallsBackToFlat()
        {
            // Arrange
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var group = new OlFolderClassifierGroup(mockGlobals.Object)
            {
                FolderPredictorConfig = LcppnFolderPredictorConfig.Create(useLcppnPredictor: true),
            };

            // Act
            var predictor = await group.GetFolderPredictorAsync();

            // Assert
            predictor.Should().BeSameAs(flat);
        }

        // F1 regression: the flag-on LCPPN predictor is held on the shared Globals.AF.FolderPredictor
        // holder, so it is reachable by the fresh per-call OlFolderClassifierGroup instances that
        // production callers (EmailFiler, SortEmail, FolderScorer) construct - not only by the
        // build-time instance. Two separate instances over the same shared globals must both return
        // the same held LCPPN predictor.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance()
        {
            // Arrange: a single shared globals (the production pattern: one globals, many fresh
            // OlFolderClassifierGroup instances). The predictor is held on Globals.AF.FolderPredictor.
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            var lcppn = CreateLcppnPredictor();
            mockGlobals.Object.AF.FolderPredictor = lcppn;

            var flagOnConfig = LcppnFolderPredictorConfig.Create(useLcppnPredictor: true);

            // Act: two independent per-call instances, mirroring the production per-call construction.
            var firstInstance = new OlFolderClassifierGroup(mockGlobals.Object)
            {
                FolderPredictorConfig = flagOnConfig,
            };
            var secondInstance = new OlFolderClassifierGroup(mockGlobals.Object)
            {
                FolderPredictorConfig = flagOnConfig,
            };
            var firstPredictor = await firstInstance.GetFolderPredictorAsync();
            var secondPredictor = await secondInstance.GetFolderPredictorAsync();

            // Assert: both fresh instances resolve the same shared LCPPN predictor.
            firstPredictor.Should().BeOfType<LcppnFolderPredictor>();
            firstPredictor.Should().BeSameAs(lcppn, "the held predictor is reachable per-call");
            secondPredictor.Should().BeSameAs(lcppn, "every fresh per-call instance resolves it");
            firstPredictor.Should().BeSameAs(secondPredictor);
        }

        // AC13 regression: with the flag off and no held predictor, a fresh per-call instance returns
        // the flat Manager["Folder"] group byte-for-byte (the same instance), confirming flag-off
        // behavior is unchanged across the per-call construction pattern.
        [TestMethod]
        public async Task GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat()
        {
            // Arrange: shared globals with no held predictor (Globals.AF.FolderPredictor is null).
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobalsWithFolder(flat);
            mockGlobals.Object.AF.FolderPredictor.Should().BeNull("no predictor has been built");

            // Act: a fresh per-call instance with the flag off (default).
            var freshInstance = new OlFolderClassifierGroup(mockGlobals.Object);
            var predictor = await freshInstance.GetFolderPredictorAsync();

            // Assert: returns the exact flat Manager["Folder"] instance, unchanged.
            freshInstance
                .FolderPredictorConfig.UseLcppnPredictor.Should()
                .BeFalse("flag defaults to off");
            predictor.Should().BeOfType<BayesianClassifierGroup>();
            predictor.Should().BeSameAs(flat);
        }
    }
}
