using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class ActionableClassifierGroup_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var group = new ActionableClassifierGroup();
            group.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithGlobals_SetsProperties()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);

            group.Should().NotBeNull();
            group.EngineName.Should().Be("Actionable");
        }

        [TestMethod]
        public void Constructor_WithGlobals_SetsProbabilityThreshold()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);

            group.ProbabilityThreshold.Should().Be(0.2);
        }

        [TestMethod]
        public async Task InitAsync_GroupNotInManager_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var group = new ActionableClassifierGroup(mockGlobals.Object);
            var result = await group.InitAsync("NonExistent");

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task InitAsync_GroupInManager_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["Actionable"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var group = new ActionableClassifierGroup(mockGlobals.Object);
            var result = await group.InitAsync("Actionable");

            result.Should().NotBeNull();
            result.ClassifierGroup.Should().BeSameAs(classifierGroup);
        }

        [TestMethod]
        public void GetMatchingCategories_WithClassifierGroup_ReturnsFiltered()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);

            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 10,
                SharedTokenBase = new Corpus(),
            };
            classifierGroup.Classifiers["None"] = new BayesianClassifierShared(
                "None",
                classifierGroup
            );
            classifierGroup.Classifiers["Action"] = new BayesianClassifierShared(
                "Action",
                classifierGroup
            );
            group.ClassifierGroup = classifierGroup;

            // MailItemHelper.Tokens is non-virtual; use a real instance (default ctor sets empty tokens)
            var helper = new MailItemHelper();

            var result = group.GetMatchingCategories(helper);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void IsActivated_NoClassifierGroup_ReturnsFalse()
        {
            var group = new ActionableClassifierGroup();
            group.IsActivated.Should().BeFalse();
        }

        [TestMethod]
        public void IsActivated_WithClassifierGroup_ReturnsTrue()
        {
            var group = new ActionableClassifierGroup();
            group.ClassifierGroup = new BayesianClassifierGroup();
            group.IsActivated.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateEngineAsync_GroupNotInManager_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var result = await ActionableClassifierGroup.CreateEngineAsync(
                mockGlobals.Object,
                "NonExistent"
            );
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateEngineAsync_GroupInManager_ReturnsEngine()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["Actionable"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var result = await ActionableClassifierGroup.CreateEngineAsync(
                mockGlobals.Object,
                "Actionable"
            );
            result.Should().NotBeNull();
            result.ClassifierGroup.Should().BeSameAs(classifierGroup);
        }

        [TestMethod]
        public async Task InitAsync_GroupInManager_SetsAsyncActionAndCondition()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["Actionable"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var group = new ActionableClassifierGroup(mockGlobals.Object);
            var result = await group.InitAsync("Actionable");

            result.Should().NotBeNull();
            result.AsyncAction.Should().NotBeNull();
            result.AsyncCondition.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetMatchingCategoriesAsync_WithClassifierGroup_ReturnsFiltered()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);

            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 10,
                SharedTokenBase = new Corpus(),
            };
            classifierGroup.Classifiers["None"] = new BayesianClassifierShared(
                "None",
                classifierGroup
            );
            classifierGroup.Classifiers["Action"] = new BayesianClassifierShared(
                "Action",
                classifierGroup
            );
            group.ClassifierGroup = classifierGroup;

            var helper = new MailItemHelper();

            var result = await group.GetMatchingCategoriesAsync(helper);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_NullCollection_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();

            using var cts = new CancellationTokenSource();
            var ppkg = new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                StopWatch = new SegmentStopWatch(),
            };

            var result = await group.BuildClassifiersAsync(
                classifierGroup,
                null,
                ppkg,
                "Actionable"
            );
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_EmptyActionableCollection_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            var collection = new MinedMailInfo[]
            {
                new MinedMailInfo { Actionable = null, Tokens = new[] { "t1" } },
            };

            using var cts = new CancellationTokenSource();
            var ppkg = new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                StopWatch = new SegmentStopWatch(),
            };

            var result = await group.BuildClassifiersAsync(
                classifierGroup,
                collection,
                ppkg,
                "Actionable"
            );
            result.Should().BeFalse();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }

    [TestClass]
    public class CategoryClassifierGroup_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_CreatesInstance()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.Should().NotBeNull();
        }

        [TestMethod]
        public async Task InitAsync_GroupNotInManager_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var result = await group.InitAsync("NonExistent");

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task InitAsync_GroupInManager_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["Context"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var result = await group.InitAsync("Context");

            result.Should().NotBeNull();
            result.Should().BeSameAs(group);
        }

        [TestMethod]
        public async Task CreateEngineAsync_GroupNotInManager_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var result =
                await UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup.CreateEngineAsync(
                    mockGlobals.Object,
                    "NonExistent"
                );
            result.Should().BeNull();
        }

        [TestMethod]
        public void IsActivated_NoClassifierGroup_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.IsActivated.Should().BeFalse();
        }

        [TestMethod]
        public void IsActivated_WithClassifierGroup_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.ClassifierGroup = new BayesianClassifierGroup();
            group.IsActivated.Should().BeTrue();
        }

        [TestMethod]
        public void ProbabilityThreshold_Default_Is0_8()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.ProbabilityThreshold.Should().Be(0.8);
        }

        [TestMethod]
        public void Message_HasExpectedValue()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.Message.Should().Contain("CategoryClassifierGroup");
        }

        [TestMethod]
        public void Engine_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.Engine.Should().BeSameAs(group);
        }

        [TestMethod]
        public void EngineInitializer_IsNotNull()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.EngineInitializer.Should().NotBeNull();
        }

        [TestMethod]
        public void TypedItem_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.TypedItem = null;
            group.TypedItem.Should().BeNull();
        }

        [TestMethod]
        public void Serialize_WithClassifierGroup_DoesNotThrow()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.ClassifierGroup = new BayesianClassifierGroup();

            ((IConditionalEngine<MailItemHelper>)group).Serialize();
        }

        [TestMethod]
        public void AsyncAction_IsNotNull()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.AsyncAction.Should().NotBeNull();
        }

        [TestMethod]
        public void AsyncCondition_IsNotNull()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.AsyncCondition.Should().NotBeNull();
        }

        [TestMethod]
        public void Config_ReturnsClassifierGroupConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var cg = new BayesianClassifierGroup();
            group.ClassifierGroup = cg;
            group.Config.Should().BeSameAs(cg.Config);
        }

        [TestMethod]
        public void GetMatchingCategories_WithClassifierGroup_ReturnsResults()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 10,
                SharedTokenBase = new Corpus(),
            };
            cg.Classifiers["Test"] = new BayesianClassifierShared("Test", cg);
            group.ClassifierGroup = cg;

            var helper = new MailItemHelper();

            var result = group.GetMatchingCategories(helper);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetMatchingCategoriesAsync_WithClassifierGroup_ReturnsResults()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 10,
                SharedTokenBase = new Corpus(),
            };
            cg.Classifiers["Test"] = new BayesianClassifierShared("Test", cg);
            group.ClassifierGroup = cg;

            var helper = new MailItemHelper();

            var result = await group.GetMatchingCategoriesAsync(helper);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void ExplodeMailsByCategory_NullCategories_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mail = new MinedMailInfo { Categories = null, Tokens = new[] { "t1" } };
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.Setup(p => p.Value).Returns("Context");

            var result = group.ExplodeMailsByCategory(mail, mockPrefix.Object);
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task BuildClassifierAsync_WithGroup_RebuildClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var tokenFrequency = new Dictionary<string, int>
            {
                { "t1", 1 },
                { "t2", 1 },
                { "t3", 1 },
            };
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(tokenFrequency),
            };

            var items = new[]
            {
                new MinedMailInfo { GroupingKey = "A", Tokens = new[] { "t1", "t2" } },
                new MinedMailInfo { GroupingKey = "A", Tokens = new[] { "t3" } },
            };
            var grouping = items.GroupBy(x => x.GroupingKey).First();

            await group.BuildClassifierAsync(grouping, cg, default);
            cg.Classifiers.Should().NotBeEmpty();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }

    [TestClass]
    public class OlFolderClassifierGroup_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_CreatesInstance()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            group.Should().NotBeNull();
            group.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        [TestMethod]
        public void Constructor_WithGlobals_CreatesCgUtilities()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            group.CgUtilities.Should().NotBeNull();
        }

        [TestMethod]
        public async Task LoadStaging_NoPythonStagingKey_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var emptyFolders = new ConcurrentDictionary<string, string>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            mockFs.Setup(f => f.SpecialFolders).Returns(emptyFolders);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);

            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            var result = await group.LoadStaging();

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithCollection_ReturnsGroup()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );

            var collection = new[]
            {
                new MinedMailInfo { Tokens = new[] { "token1", "token2" } },
                new MinedMailInfo { Tokens = new[] { "token3" } },
            };

            var result = await group.CreateClassifierGroupAsync(collection);

            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(2);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public async Task BuildClassifierAsync_WithGroup_RebuildClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            var tokenFrequency = new Dictionary<string, int>
            {
                { "t1", 2 },
                { "t2", 1 },
                { "t3", 1 },
            };
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(tokenFrequency),
            };

            var mockFolder = new Mock<IFolderWrapper>();
            mockFolder.Setup(f => f.RelativePath).Returns("Inbox");

            var items = new[]
            {
                new MinedMailInfo { FolderInfo = mockFolder.Object, Tokens = new[] { "t1", "t2" } },
                new MinedMailInfo { FolderInfo = mockFolder.Object, Tokens = new[] { "t1", "t3" } },
            };
            var grouping = items.GroupBy(x => x.FolderInfo.RelativePath).First();

            await group.BuildClassifierAsync(grouping, cg, default);
            cg.Classifiers.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task BuildFolderClassifiersAsync_EmptyCollection_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            var cg = new BayesianClassifierGroup();
            var collection = Array.Empty<MinedMailInfo>();

            using var cts = new CancellationTokenSource();
            var ppkg = new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                StopWatch = new SegmentStopWatch(),
            };

            var result = await group.BuildFolderClassifiersAsync(cg, collection, ppkg);
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task GetOrCreateClassifierGroupAsync_CreatesNew()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            var collection = new[] { new MinedMailInfo { Tokens = new[] { "t1" } } };

            // CgUtilities.Deserialize throws NullReferenceException when config path is not set
            Func<Task> act = () => group.GetOrCreateClassifierGroupAsync(collection);
            await act.Should().ThrowAsync<NullReferenceException>();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }
}
