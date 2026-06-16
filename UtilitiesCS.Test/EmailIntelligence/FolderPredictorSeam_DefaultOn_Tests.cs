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
    /// Default-ON production-enablement tests for the Folder predictor seam (AC21, AC22). A
    /// production-style <see cref="OlFolderClassifierGroup"/> resolves its
    /// <see cref="LcppnFolderPredictorConfig"/> from the persisted
    /// <see cref="IAppAutoFileObjects.UseLcppnPredictor"/> accessor (default ON) without any caller
    /// hand-setting the flag. When ON with a held predictor it selects LCPPN; when ON without a held
    /// predictor it falls back to the flat group (AC22); when OFF it preserves flat-only selection
    /// (AC13 parity). All tests are deterministic, in-memory, and use no Outlook COM or temporary
    /// files.
    /// </summary>
    [TestClass]
    public class FolderPredictorSeam_DefaultOn_Tests
    {
        private static readonly string[] InboxTokens = ["invoice", "payment", "due", "invoice"];
        private static readonly string[] ArchiveTokens =
        [
            "newsletter",
            "weekly",
            "digest",
            "newsletter",
        ];

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

        private static LcppnFolderPredictor CreateLcppnPredictor()
        {
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 3,
                MinimumPathProbability = 0.01,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train(@"Projects\Alpha", new[] { "alpha", "spec" }, 1);
            predictor.Train(@"Clients\Acme", new[] { "invoice", "billing" }, 1);
            return predictor;
        }

        // Builds mock globals whose persisted UseLcppnPredictor accessor returns the supplied value
        // and whose Manager["Folder"] resolves to the supplied flat group. FolderPredictor is a real
        // backing property so the holder set/read is observed consistently (production semantics).
        private static Mock<IApplicationGlobals> CreateMockGlobals(
            BayesianClassifierGroup folderGroup,
            bool persistedUseLcppn
        )
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object)
            {
                ["Folder"] = folderGroup.ToAsyncLazy(),
            };
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            mockAf.SetupGet(x => x.UseLcppnPredictor).Returns(persistedUseLcppn);
            mockAf.SetupProperty(x => x.FolderPredictor);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);
            return mockGlobals;
        }

        // AC21: with the persisted setting ON (default) and a held LCPPN predictor, a production-style
        // construction (no explicit flag) resolves UseLcppnPredictor==true and returns the LCPPN
        // predictor.
        [TestMethod]
        public async Task DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld()
        {
            // Arrange: persisted ON, no caller-set FolderPredictorConfig.
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobals(flat, persistedUseLcppn: true);
            var lcppn = CreateLcppnPredictor();
            mockGlobals.Object.AF.FolderPredictor = lcppn;
            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            // Act
            var resolvedFlag = group.FolderPredictorConfig.UseLcppnPredictor;
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: the persisted default flowed through; LCPPN is selected without hand-setting.
            resolvedFlag.Should().BeTrue("the persisted production default is ON");
            predictor.Should().BeOfType<LcppnFolderPredictor>();
            predictor.Should().BeSameAs(lcppn);
        }

        // AC22: with the persisted setting ON (default) but no held predictor, the accessor falls back
        // to the flat group and does not throw.
        [TestMethod]
        public async Task DefaultOn_NoHeldPredictor_FallsBackToFlat()
        {
            // Arrange: persisted ON, holder null.
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobals(flat, persistedUseLcppn: true);
            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            // Act
            group.FolderPredictorConfig.UseLcppnPredictor.Should().BeTrue();
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: flat group returned, never null, never thrown.
            predictor.Should().NotBeNull();
            predictor.Should().BeOfType<BayesianClassifierGroup>();
            predictor.Should().BeSameAs(flat);
        }

        // AC21 toggle-OFF / AC13 parity: with the persisted setting OFF, a production-style
        // construction resolves UseLcppnPredictor==false and returns the flat group (same instance),
        // even if a predictor happened to be held.
        [TestMethod]
        public async Task ToggleOff_ResolvesFlatOnly_PreservingAc13()
        {
            // Arrange: persisted OFF, even with a held predictor present.
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobals(flat, persistedUseLcppn: false);
            mockGlobals.Object.AF.FolderPredictor = CreateLcppnPredictor();
            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            // Act
            var resolvedFlag = group.FolderPredictorConfig.UseLcppnPredictor;
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: OFF restores flat-only selection.
            resolvedFlag.Should().BeFalse("the persisted setting is OFF");
            predictor.Should().BeOfType<BayesianClassifierGroup>();
            predictor.Should().BeSameAs(flat);
        }

        // AC21: an explicitly-set FolderPredictorConfig still wins over the persisted default (the
        // injectable seam is preserved for tests/overrides).
        [TestMethod]
        public async Task ExplicitConfig_OverridesPersistedDefault()
        {
            // Arrange: persisted ON, but caller injects an OFF config.
            var flat = CreateFlatGroup();
            var mockGlobals = CreateMockGlobals(flat, persistedUseLcppn: true);
            mockGlobals.Object.AF.FolderPredictor = CreateLcppnPredictor();
            var group = new OlFolderClassifierGroup(mockGlobals.Object)
            {
                FolderPredictorConfig = LcppnFolderPredictorConfig.Create(useLcppnPredictor: false),
            };

            // Act
            var predictor = await group.GetFolderPredictorAsync();

            // Assert: the explicit OFF config is honored over the persisted ON default.
            group.FolderPredictorConfig.UseLcppnPredictor.Should().BeFalse();
            predictor.Should().BeSameAs(flat);
        }
    }
}
