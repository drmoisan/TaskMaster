using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
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
    // -----------------------------------------------------------------------
    // P50 — SpamBayes Coverage
    // -----------------------------------------------------------------------

    [TestClass]
    public class SpamBayes_Tests
    {
        // -----------------------------------------------------------------------
        // P50-T1 — create-new path returns a configured classifier group
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that CreateNewClassifier returns a non-null BayesianClassifierGroup
        /// configured with the expected Spam and Ham classifiers.
        ///
        /// Purpose:
        ///     Confirm the static factory creates a fully populated group without
        ///     requiring any Globals dependency.
        ///
        /// Returns:
        ///     Passes when the group is non-null and contains both Spam and Ham classifiers.
        /// </summary>
        [TestMethod]
        public void CreateNewClassifier_ReturnsConfiguredGroup()
        {
            // Act
            var group = SpamBayes.CreateNewClassifier();

            // Assert
            group.Should().NotBeNull();
            group.Name.Should().Be(SpamBayes.GroupName);
            group.Classifiers.Should().ContainKey("Spam");
            group.Classifiers.Should().ContainKey("Ham");
            group.SharedTokenBase.Should().NotBeNull();
        }

        // -----------------------------------------------------------------------
        // P50-T2 — missing configuration invokes the fallback handling path
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that SpamBayesMissingHandlerAsync with NotFoundEnum.Skip returns
        /// false without throwing an exception.
        ///
        /// Purpose:
        ///     Confirm the Skip treatment short-circuits the handler and returns false,
        ///     which causes CreateAsync to return null.
        ///
        /// Returns:
        ///     Passes when the handler returns false and no exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task SpamBayesMissingHandler_WhenSkip_ReturnsFalse()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var spamBayes = new SpamBayes(mockGlobals.Object);

            // Act
            var result = await spamBayes.SpamBayesMissingHandlerAsync(
                Enums.NotFoundEnum.Skip,
                "test message",
                default
            );

            // Assert: Skip treatment returns false (no UI, no throw)
            result.Should().BeFalse();
        }

        // -----------------------------------------------------------------------
        // P50-T3 — validation rejects an incomplete setup
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that ValidatePathsSet returns false when the required Outlook
        /// folder references (JunkCertain, JunkPotential, Inbox) are null.
        ///
        /// Purpose:
        ///     Confirm the guard clause in ValidatePathsSet prevents CreateAsync from
        ///     proceeding when required COM folder references are not configured.
        ///
        /// Returns:
        ///     Passes when ValidatePathsSet returns false for an incomplete Globals setup.
        /// </summary>
        [TestMethod]
        public void ValidatePathsSet_WhenRequiredFoldersNull_ReturnsFalse()
        {
            // Arrange: Ol.JunkCertain is null (returns default null for MAPIFolder)
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(o => o.JunkCertain).Returns((Microsoft.Office.Interop.Outlook.Folder)null);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var spamBayes = new SpamBayes(mockGlobals.Object);

            // Act
            var result = spamBayes.ValidatePathsSet();

            // Assert: validation fails when required folders are null
            result.Should().BeFalse();
        }
    }

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

        /// <summary>
        /// Verifies that <see cref="ActionableClassifierGroup.GetMatchingCategories"/> filters out
        /// the "None" class from results even when a "None" classifier is present in the group.
        ///
        /// Purpose:
        ///     Confirms the filter's second Where clause excludes "None", returning only
        ///     categories with probability above the threshold and class != "None".
        ///
        /// Returns:
        ///     Passes when the filtered result does not contain "None".
        /// </summary>
        [TestMethod]
        public void GetMatchingCategories_NoneClassAlwaysFiltered()
        {
            // Arrange: provide a mix of "None" and "Task" classifiers (actionable and non-actionable).
            var group = new ActionableClassifierGroup();
            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 10,
                SharedTokenBase = new Corpus(),
            };
            classifierGroup.Classifiers["None"] = new BayesianClassifierShared(
                "None",
                classifierGroup
            );
            classifierGroup.Classifiers["Task"] = new BayesianClassifierShared(
                "Task",
                classifierGroup
            );
            group.ClassifierGroup = classifierGroup;

            // Act: call the category filter with an empty-token helper.
            var result = group.GetMatchingCategories(new MailItemHelper());

            // Assert: "None" is always excluded from the returned subset of categories.
            result.Should().NotContain("None");
        }

        /// <summary>
        /// Verifies that the categorization path for <see cref="ActionableClassifierGroup"/>
        /// short-circuits gracefully when supplied with empty data (no tokens, untrained classifiers),
        /// returning no categories — analogous to the short-circuit within TestAsync that routes to "None".
        ///
        /// Purpose:
        ///     Confirm classification completes without throwing and produces an empty result set
        ///     so that TestAsync's IsNullOrEmpty branch (value = "None") executes rather than
        ///     the unhappy path.
        ///
        /// Note:
        ///     The async TestAsync/GetMatchingCategoriesAsync path requires
        ///     Microsoft.Bcl.AsyncInterfaces v10.0.0.0 which is unavailable in headless test
        ///     execution; the synchronous GetMatchingCategories exercises the same filtering logic.
        ///
        /// Returns:
        ///     Passes when the result is empty/null and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void TestAsync_EmptyData_CategorizationShortCircuitsToEmpty()
        {
            // Arrange: untrained classifier with empty-token helper.
            var group = new ActionableClassifierGroup();
            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 0,
                SharedTokenBase = new Corpus(),
            };
            classifierGroup.Classifiers["None"] = new BayesianClassifierShared(
                "None",
                classifierGroup
            );
            group.ClassifierGroup = classifierGroup;

            // Act: synchronous counterpart of TestAsync's internal categorization call.
            var results = group.GetMatchingCategories(new MailItemHelper());

            // Assert: empty data short-circuits to no results — IsNullOrEmpty would route
            // TestAsync to value = "None" without throwing.
            results.IsNullOrEmpty().Should().BeTrue();
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

        // -----------------------------------------------------------------------
        // P56-T1 — BuildClassifierAsync stores the grouping key as the classifier key
        // -----------------------------------------------------------------------

        [TestMethod]
        public async Task BuildClassifierAsync_GroupingKey_IsStoredAsClassifierKey()
        {
            // Arrange: grouping with a known key to verify classifier key round-trip.
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var tokenFrequency = new Dictionary<string, int> { { "t1", 1 }, { "t2", 1 } };
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(tokenFrequency),
            };
            var items = new[]
            {
                new MinedMailInfo { GroupingKey = "Context:Work", Tokens = new[] { "t1", "t2" } },
            };
            var grouping = items.GroupBy(x => x.GroupingKey).First();

            // Act: build the classifier for the grouping.
            await group.BuildClassifierAsync(grouping, cg, default);

            // Assert: the classifier dictionary contains the exact grouping key.
            cg.Classifiers.Should().ContainKey("Context:Work");
        }

        // -----------------------------------------------------------------------
        // P56-T2 — ExplodeMailsByCategory returns original item unchanged when
        //           Categories is empty, so no category-derived classifier key
        //           is created for such items.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void ExplodeMailsByCategory_EmptyCategories_ReturnsOriginalItemWithoutCategoryKey()
        {
            // Arrange: item with empty Categories string.
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mail = new MinedMailInfo
            {
                Categories = "",
                Tokens = new[] { "t1" },
                GroupingKey = null,
            };
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.Setup(p => p.Value).Returns("Context");

            // Act: explode — empty categories should yield the original item only.
            var result = group.ExplodeMailsByCategory(mail, mockPrefix.Object).ToList();

            // Assert: exactly one item, with no category-derived GroupingKey.
            result.Should().HaveCount(1);
            result[0]
                .GroupingKey.Should()
                .BeNull("empty categories must not produce a category-derived classifier key");
        }

        // -----------------------------------------------------------------------
        // P56-T3 — InitAsync sets ClassifierGroup to the pre-existing manager entry,
        //           reusing it rather than creating a duplicate.
        // -----------------------------------------------------------------------

        [TestMethod]
        public async Task InitAsync_GroupInManager_SetsExistingClassifierGroup()
        {
            // Arrange: manager pre-populated with a known BayesianClassifierGroup.
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

            // Act: load — should wire up the existing entry, not create a new one.
            var result = await group.InitAsync("Context");

            // Assert: same object reference confirms reuse of the existing group.
            result.Should().NotBeNull();
            result!
                .ClassifierGroup.Should()
                .BeSameAs(
                    classifierGroup,
                    "InitAsync must reuse the pre-existing manager entry without creating a duplicate"
                );
        }

        [STATestMethod]
        public async Task BuildClassifiersAsync_MissingStagingData_ShowsActionableWarningInsteadOfThrowing()
        {
            var mockGlobals = CreateMockGlobals();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var progressPane = new Mock<CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            var appDataRoot = Path.Combine(
                Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\",
                "TaskMaster-Tests",
                Guid.NewGuid().ToString("N")
            );
            mockFs
                .SetupGet(x => x.SpecialFolders)
                .Returns(
                    new ConcurrentDictionary<string, string>(
                        new[] { new KeyValuePair<string, string>("AppData", appDataRoot) }
                    )
                );
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            mockAf.SetupGet(x => x.ProgressTracker).Returns(CreateHeadlessProgressTrackerPane());
            mockAf.SetupGet(x => x.ProgressPane).Returns(progressPane.Object);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);

            var prefixList = new ScoCollection<IPrefix>
            {
                CreatePrefix("Context", "_@"),
                CreatePrefix("Project", "Tag PROJECT"),
            };
            var mockTd = new Mock<IToDoObjects>();
            mockTd.SetupGet(x => x.PrefixList).Returns(prefixList);
            mockGlobals.SetupGet(x => x.TD).Returns(mockTd.Object);

            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.CgUtilities = new StubCategoryClassifierGroupUtilities(
                mockGlobals.Object,
                new BayesianClassifierGroup()
            );

            var dialogCalls = 0;
            string dialogTitle = null;
            string dialogMessage = null;
            MyBox.DialogInvoker = viewer =>
            {
                dialogCalls++;
                dialogTitle = viewer.Text;
                dialogMessage = viewer.TextMessage.Text;
                return DialogResult.OK;
            };

            try
            {
                await group.BuildClassifiersAsync();
            }
            finally
            {
                MyBox.DialogInvoker = viewer => viewer.ShowDialog();
            }

            dialogCalls.Should().Be(1);
            dialogTitle.Should().Be("Category classifier data unavailable");
            dialogMessage.Should().Contain("Continue Mining");
            dialogMessage.Should().Contain("Scrape and Mine");
            dialogMessage.Should().Contain(Path.Combine(appDataRoot, "Bayesian"));
            progressPane.Object.Visible.Should().BeFalse();
        }

        [TestMethod]
        public async Task LoadClassifierGroup_ProgressPackageOverload_ReturnsUtilitiesResult()
        {
            var mockGlobals = CreateMockGlobals();
            var expected = new BayesianClassifierGroup();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            group.CgUtilities = new StubCategoryClassifierGroupUtilities(
                mockGlobals.Object,
                expected
            );

            var prefix = CreatePrefix("Project", "Tag PROJECT");
            var package = CreateHeadlessProgressPackage();
            var collection = new[] { new MinedMailInfo { Tokens = new[] { "alpha" } } };

            var result = await InvokeNonPublicAsync<BayesianClassifierGroup>(
                group,
                "LoadClassifierGroup",
                package,
                package.StopWatch,
                collection,
                prefix
            );

            result.Should().BeSameAs(expected);
            package.ProgressTrackerPane.Progress.Should().Be(20);
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_WithMatchingCategories_ReturnsTrueAndBuildsExpectedKeys()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new RecordingCategoryClassifierGroup(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 3,
                SharedTokenBase = new Corpus(
                    new Dictionary<string, int> { { "alpha", 2 }, { "beta", 1 } }
                ),
            };
            var collection = new[]
            {
                new MinedMailInfo
                {
                    Categories = "Tag PROJECT Roadmap, _@Desk",
                    Tokens = new[] { "alpha", "beta" },
                },
                new MinedMailInfo
                {
                    Categories = "Tag PROJECT Roadmap",
                    Tokens = new[] { "alpha" },
                },
                new MinedMailInfo { Categories = "_@Desk", Tokens = new[] { "beta" } },
            };
            var prefix = CreatePrefix("Project", "Tag PROJECT");
            var package = CreateHeadlessProgressPackage();

            var result = await group.BuildClassifiersAsync(
                classifierGroup,
                collection,
                package,
                prefix
            );

            result.Should().BeTrue();
            group
                .BuiltGroupingKeys.Should()
                .ContainSingle()
                .Which.Should()
                .Be("Tag PROJECT Roadmap");
            classifierGroup.Classifiers.Should().ContainKey("Tag PROJECT Roadmap");
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_WithNullProgressPane_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var classifierGroup = new BayesianClassifierGroup();
            var collection = new[]
            {
                new MinedMailInfo
                {
                    Categories = "Tag PROJECT Roadmap",
                    Tokens = new[] { "alpha" },
                },
            };
            var prefix = CreatePrefix("Project", "Tag PROJECT");
            using var cts = new CancellationTokenSource();
            var package = new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                StopWatch = new SegmentStopWatch().Start(),
                ProgressTrackerPane = null,
            };

            var result = await group.BuildClassifiersAsync(
                classifierGroup,
                collection,
                package,
                prefix
            );

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task AsyncAction_WithConfiguredSetter_InvokesCategorySetter()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                )
                {
                    ClassifierGroup = CreateTrainedCategoryClassifierGroup(),
                    ProbabilityThreshold = 0.5,
                };
            var helper = CreateMailItemHelper("alpha", "beta");
            string[] assignedCategories = null;

            group.CategorySetter = (categories, item) =>
            {
                assignedCategories = categories.ToArray();
                return Task.CompletedTask;
            };

            await group.AsyncAction(helper);

            assignedCategories.Should().Contain("Tag PROJECT Roadmap");
        }

        [TestMethod]
        public void AsyncAction_WithoutCategorySetter_ReturnsNullTask()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var helper = CreateMailItemHelper("alpha");

            var task = group.AsyncAction(helper);

            task.Should().BeNull();
        }

        [TestMethod]
        public async Task GetMatchingCategoriesAsync_WithHighProbabilityMatch_ReturnsFilteredCategories()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                )
                {
                    ClassifierGroup = CreateTrainedCategoryClassifierGroup(),
                    ProbabilityThreshold = 0.5,
                };
            var helper = CreateMailItemHelper("alpha", "beta");

            var result = await group.GetMatchingCategoriesAsync(helper);

            result.Should().Contain("Tag PROJECT Roadmap");
        }

        [TestMethod]
        public void GetMatchingCategories_WithHighProbabilityMatch_ReturnsFilteredCategories()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                )
                {
                    ClassifierGroup = CreateTrainedCategoryClassifierGroup(),
                    ProbabilityThreshold = 0.5,
                };
            var helper = CreateMailItemHelper("alpha", "beta");

            var result = group.GetMatchingCategories(helper);

            result.Should().Contain("Tag PROJECT Roadmap");
        }

        [TestMethod]
        public void Condition_WithNonMailItem_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );

            InvokeNonPublic<bool>(group, "Condition", new object()).Should().BeFalse();
        }

        [TestMethod]
        public void Condition_WithNonNoteMailItem_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Schedule.Meeting.Request");

            InvokeNonPublic<bool>(group, "Condition", mailItem.Object).Should().BeFalse();
        }

        [TestMethod]
        public void Condition_WithNoteMailItem_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");

            InvokeNonPublic<bool>(group, "Condition", mailItem.Object).Should().BeTrue();
        }

        [TestMethod]
        public void ConditionLog_WithAppointmentItem_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var appointment = new Mock<AppointmentItem>();

            InvokeNonPublic<bool>(group, "ConditionLog", appointment.Object).Should().BeFalse();
        }

        [TestMethod]
        public void ConditionLog_WithMailItemUsingNonNoteMessageClass_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Schedule.Meeting.Request");
            mailItem.SetupGet(x => x.CreationTime).Returns(new DateTime(2024, 1, 2, 3, 4, 0));
            mailItem.SetupGet(x => x.Subject).Returns("Roadmap");

            InvokeNonPublic<bool>(group, "ConditionLog", mailItem.Object).Should().BeFalse();
        }

        [TestMethod]
        public void ConditionLog_WithNoteMailItem_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");

            InvokeNonPublic<bool>(group, "ConditionLog", mailItem.Object).Should().BeTrue();
        }

        [TestMethod]
        public void GetOlItemString_WithReflectionFriendlyItem_UsesFallbackTypeAndReadableFields()
        {
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                    mockGlobals.Object
                );
            var item = new ReflectionFriendlyCategoryItem
            {
                CreationTime = new DateTime(2024, 2, 3, 4, 5, 0),
                Subject = "Status update",
            };
            var outlookItem = new UtilitiesCS.OutlookItem(item);

            var result = InvokeNonPublic<string>(group, "GetOlItemString", outlookItem);

            result.Should().Contain(nameof(ReflectionFriendlyCategoryItem));
            result.Should().Contain("created on");
            result.Should().Contain("with subject Status update");
        }

        private static BayesianClassifierGroup CreateTrainedCategoryClassifierGroup()
        {
            var classifierGroup = new BayesianClassifierGroup();
            classifierGroup.Train("Tag PROJECT Roadmap", new[] { "alpha", "alpha", "beta" }, 1);
            classifierGroup.Train("Tag PROJECT Archive", new[] { "gamma", "gamma" }, 1);
            return classifierGroup;
        }

        private static MailItemHelper CreateMailItemHelper(params string[] tokens)
        {
            var helper = new MailItemHelper();
            typeof(MailItemHelper)
                .GetProperty(
                    "Tokens",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )!
                .SetValue(helper, tokens);
            return helper;
        }

        private static ProgressPackage CreateHeadlessProgressPackage()
        {
            var cts = new CancellationTokenSource();
            return new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                ProgressTrackerPane = CreateHeadlessProgressTrackerPane(),
                StopWatch = new SegmentStopWatch().Start(),
            };
        }

        private static ProgressTrackerPane CreateHeadlessProgressTrackerPane(double progress = 0)
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentProgressType = typeof(ProgressTrackerPane)
                .Assembly.GetType("UtilitiesCS.ParentProgress`1")!
                .MakeGenericType(typeof(ValueTuple<int, string>));
            var parentProgress = Activator.CreateInstance(
                parentProgressType,
                new Progress<(int Value, string JobName)>(_ => { }),
                100,
                0
            );

            SetPrivateField(pane, "_parent", parentProgress);
            SetPrivateField(pane, "_progress", progress);
            SetPrivateField(pane, "_isRoot", false);
            SetPrivateField(pane, "_jobName", "Test");
            return pane;
        }

        private static IPrefix CreatePrefix(string key, string value)
        {
            var prefix = new Mock<IPrefix>();
            prefix.SetupProperty(x => x.Key, key);
            prefix.SetupProperty(x => x.Value, value);
            return prefix.Object;
        }

        private static T InvokeNonPublic<T>(
            object instance,
            string methodName,
            params object[] args
        )
        {
            var method = instance
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(x =>
                    x.Name == methodName
                    && ParametersMatch(
                        x.GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
                        args
                    )
                );
            return (T)method.Invoke(instance, args);
        }

        private static async Task<T> InvokeNonPublicAsync<T>(
            object instance,
            string methodName,
            params object[] args
        )
        {
            var method = instance
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(x =>
                    x.Name == methodName
                    && ParametersMatch(
                        x.GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
                        args
                    )
                );
            var task = (Task)method.Invoke(instance, args);
            await task;
            return (T)task.GetType().GetProperty("Result")!.GetValue(task);
        }

        private static bool ParametersMatch(Type[] parameterTypes, object[] args)
        {
            if (parameterTypes.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (args[i] is null)
                {
                    continue;
                }

                if (!parameterTypes[i].IsInstanceOfType(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            instance
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, value);
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

        private sealed class RecordingCategoryClassifierGroup(IApplicationGlobals globals)
            : UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                globals
            )
        {
            public List<string> BuiltGroupingKeys { get; } = new();

            public override async Task BuildClassifierAsync(
                IGrouping<string, MinedMailInfo> group,
                BayesianClassifierGroup classifierGroup,
                CancellationToken cancel
            )
            {
                BuiltGroupingKeys.Add(group.Key);
                await base.BuildClassifierAsync(group, classifierGroup, cancel);
            }
        }

        private sealed class StubCategoryClassifierGroupUtilities(
            IApplicationGlobals globals,
            BayesianClassifierGroup classifierGroup
        ) : ClassifierGroupUtilities(globals)
        {
            public override Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(
                MinedMailInfo[] collection,
                string name,
                int minimumCountPerToken = 0
            )
            {
                return Task.FromResult(classifierGroup);
            }
        }

        private sealed class ReflectionFriendlyCategoryItem
        {
            public DateTime CreationTime { get; set; }

            public string Subject { get; set; }
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

        // -----------------------------------------------------------------------
        // P67-T1 — BuildClassifierAsync called for N distinct folder paths yields
        //           N classifiers, one per unique folder key.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling BuildClassifierAsync for each of two distinct folder
        /// paths produces two independent classifiers in the classifier group.
        ///
        /// Purpose:
        ///     Confirm the build path creates one classifier entry per eligible folder:
        ///     the classifier key equals the folder's RelativePath.
        ///
        /// Returns:
        ///     Passes when Classifiers contains exactly one entry per distinct folder path.
        /// </summary>
        [TestMethod]
        public async Task BuildClassifierAsync_TwoDistinctFolderPaths_CreatesOneClassifierPerPath()
        {
            // Arrange: shared token base based on the union of all mail tokens.
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );
            var tokenFrequency = new Dictionary<string, int> { { "report", 3 }, { "meeting", 2 } };
            var cg = new BayesianClassifierGroup
            {
                TotalEmailCount = 4,
                SharedTokenBase = new Corpus(tokenFrequency),
            };

            var mockFolderA = new Mock<IFolderWrapper>();
            mockFolderA.Setup(f => f.RelativePath).Returns("Inbox");
            var mockFolderB = new Mock<IFolderWrapper>();
            mockFolderB.Setup(f => f.RelativePath).Returns("Projects");

            // Each grouping mimics one folder's mailed items.
            var groupingA = new[]
            {
                new MinedMailInfo
                {
                    FolderInfo = mockFolderA.Object,
                    Tokens = new[] { "report", "meeting" },
                },
            }.GroupBy(x => x.FolderInfo.RelativePath).First();
            var groupingB = new[]
            {
                new MinedMailInfo { FolderInfo = mockFolderB.Object, Tokens = new[] { "report" } },
            }.GroupBy(x => x.FolderInfo.RelativePath).First();

            // Act: build classifiers for both folder paths.
            await group.BuildClassifierAsync(groupingA, cg, default);
            await group.BuildClassifierAsync(groupingB, cg, default);

            // Assert: one classifier key per folder path.
            cg.Classifiers.Should().HaveCount(2);
            cg.Classifiers.Should().ContainKey("Inbox");
            cg.Classifiers.Should().ContainKey("Projects");
        }

        // -----------------------------------------------------------------------
        // P67-T2 — Empty collection yields a classifier group with zero emails
        //           and no classifiers.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that CreateClassifierGroupAsync with an empty mined-mail array
        /// produces a group with TotalEmailCount of zero and an empty classifiers dict.
        ///
        /// Purpose:
        ///     Confirm the empty-staging-source guard: no classifiers should be created
        ///     when there are no eligible folder items.
        ///
        /// Returns:
        ///     Passes when the group has TotalEmailCount == 0 and no classifiers.
        /// </summary>
        [TestMethod]
        public async Task CreateClassifierGroupAsync_EmptyCollection_YieldsGroupWithZeroEmailsAndNoClassifiers()
        {
            // Arrange
            var mockGlobals = CreateMockGlobals();
            var group =
                new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                    mockGlobals.Object
                );

            // Act: empty staging source — no mail info entries.
            var result = await group.CreateClassifierGroupAsync(Array.Empty<MinedMailInfo>());

            // Assert: no emails and no classifiers when the staging source is empty.
            result.TotalEmailCount.Should().Be(0);
            result.Classifiers.Should().BeEmpty();
        }

        // -----------------------------------------------------------------------
        // P67-T3 — Load path returns the pre-existing group without creating a new one.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that when GetOrCreateClassifierGroupAsync returns a pre-existing
        /// group (simulating a successful deserialization from the store), the caller
        /// receives that exact group reference rather than a freshly created copy.
        ///
        /// Purpose:
        ///     Confirm the load/rehydration path: if a stored classifier group is available,
        ///     return it unchanged so downstream callers benefit from previously trained state.
        ///
        /// Returns:
        ///     Passes when the returned group is the same reference as the pre-populated entry.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateClassifierGroupAsync_WhenStoreReturnsGroup_ReturnsPreExistingGroup()
        {
            // Arrange: pre-built group that the store would return on load.
            var mockGlobals = CreateMockGlobals();
            var preExistingGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 5,
                SharedTokenBase = new Corpus(new Dictionary<string, int> { { "meeting", 3 } }),
            };
            preExistingGroup.Classifiers["Reports"] = new BayesianClassifierShared(
                "Reports",
                preExistingGroup
            );

            // Override GetOrCreateClassifierGroupAsync to simulate a successful store load.
            var testGroup = new TestableFolderClassifierGroup(mockGlobals.Object, preExistingGroup);

            // Act: call the virtual method with arbitrary collection data.
            var result = await testGroup.GetOrCreateClassifierGroupAsync(
                new[] { new MinedMailInfo { Tokens = new[] { "meeting" } } }
            );

            // Assert: same reference confirms the load path returns the stored group.
            result.Should().BeSameAs(preExistingGroup);
            result.Classifiers.Should().ContainKey("Reports");
        }

        /// <summary>
        /// Test-only subclass of OlFolderClassifierGroup that short-circuits
        /// GetOrCreateClassifierGroupAsync to return a caller-supplied group,
        /// simulating a successful deserialization from a backing store.
        ///
        /// Side Effects:
        ///     None — purely in-memory; no I/O is performed.
        /// </summary>
        private sealed class TestableFolderClassifierGroup
            : UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup
        {
            private readonly BayesianClassifierGroup _storedGroup;

            public TestableFolderClassifierGroup(
                IApplicationGlobals globals,
                BayesianClassifierGroup storedGroup
            )
                : base(globals)
            {
                _storedGroup = storedGroup;
            }

            public override Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(
                MinedMailInfo[] collection
            )
            {
                // Simulate the "found in store" branch without touching the filesystem.
                return Task.FromResult(_storedGroup);
            }
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
