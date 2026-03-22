using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using C;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Test.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class BayesianClassifierSharedTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            //this.mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
        }

        #region Helper Functions and Classes

        private static SubBayesianClassifier CreateSimpleClassifier(
            int matchEmailCount = 2,
            int totalEmailCount = 10
        )
        {
            return new SubBayesianClassifier
            {
                Tag = "tag",
                Match = new SubCorpus(new Dictionary<string, int>()),
                MatchEmailCount = matchEmailCount,
                Parent = new SubClassifierGroup
                {
                    SharedTokenBase = new SubCorpus(new Dictionary<string, int>()),
                    TotalEmailCount = totalEmailCount,
                },
                Prob = new ConcurrentDictionary<string, double>(),
            };
        }

        #endregion Helper Functions and Classes

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Act
            var classifier = new BayesianClassifierShared();

            // Assert
            classifier.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTag_InitializesProperties()
        {
            // Act
            var classifier = new BayesianClassifierShared("test-tag");

            // Assert
            classifier.Tag.Should().Be("test-tag");
            classifier.Match.Should().NotBeNull();
            classifier.Prob.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTagAndParent_SetsParent()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();

            // Act
            var classifier = new BayesianClassifierShared("tag", parent);

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Parent.Should().BeSameAs(parent);
        }

        [TestMethod]
        public void Tag_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifierShared();

            // Act
            classifier.Tag = "new-tag";

            // Assert
            classifier.Tag.Should().Be("new-tag");
        }

        [TestMethod]
        public void MatchEmailCount_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifierShared("tag");

            // Act
            classifier.MatchEmailCount = 42;

            // Assert
            classifier.MatchEmailCount.Should().Be(42);
        }

        [TestMethod]
        public void FromTokenBase_WithNullParent_ThrowsArgumentNullException()
        {
            // Arrange
            var matches = new Dictionary<string, int> { ["hello"] = 1 };

            // Act
            Action act = () =>
                BayesianClassifierShared.FromTokenBase(null, "tag", matches, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithNullTag_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var matches = new Dictionary<string, int> { ["hello"] = 1 };

            // Act
            Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, null, matches, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithNullMatches_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();

            // Act
            Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, "tag", null, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithZeroEmailCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var matches = new Dictionary<string, int> { ["hello"] = 1 };

            // Act
            Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, "tag", matches, 0, false);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void FromTokenBase_WithValidParams_CreatesClassifier()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            parent.SharedTokenBase.TokenFrequency["hello"] = 5;
            parent.TotalEmailCount = 6;
            var matches = new Dictionary<string, int> { ["hello"] = 3 };

            // Act
            var classifier = BayesianClassifierShared.FromTokenBase(
                parent,
                "tag",
                matches,
                1,
                false
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.MatchEmailCount.Should().Be(1);
            classifier.Parent.Should().BeSameAs(parent);
            classifier.Match.TokenFrequency["hello"].Should().Be(3);
        }

        [TestMethod]
        public void FromTokenBase_AddToParent_UpdatesSharedTokenBase()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            parent.TotalEmailCount = 4;
            var matches = new Dictionary<string, int> { ["hello"] = 3 };

            // Act
            var classifier = BayesianClassifierShared.FromTokenBase(
                parent,
                "tag",
                matches,
                1,
                true
            );

            // Assert
            parent
                .SharedTokenBase.TokenFrequency.Should()
                .Contain(new KeyValuePair<string, int>("hello", 3));
            classifier.Prob.Should().ContainKey("hello");
        }

        [TestMethod]
        public void Train_AddTokensToClassifier_UpdatesMatchCount()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var classifier = new BayesianClassifierShared("tag", parent);
            classifier.MatchEmailCount = 1;
            parent.TotalEmailCount = 2;
            var tokens = new Dictionary<string, int> { ["word"] = 2 };

            // Act
            classifier.Train(tokens, 1);

            // Assert
            classifier.MatchEmailCount.Should().Be(2);
            classifier
                .Match.TokenFrequency.Should()
                .Contain(new KeyValuePair<string, int>("word", 2));
        }

        [TestMethod]
        public void GetMatchProbability_StateUnderTest_ExpectedBehavior()
        {
            Console.WriteLine(
                "Integration test of GetMatchProbability method which \n"
                    + "calls GetProbabilityList and CombineProbabilities"
            );

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            classifier
                .Prob.OrderBy(x => x.Key)
                .ToDictionary()
                .LogProbabilities("Source probabilities");

            // Set up tokens in the Prob list
            Dictionary<string, int> input = new Dictionary<string, int>
            {
                ["shared1"] = 2,
                ["dedicated8"] = 1,
                ["shared4"] = 2,
                ["shared2"] = 1,
                ["shared7"] = 1,
            };

            double expected = 0.391816521680729;

            // Act
            double actual = classifier.GetMatchProbability(input);

            // Assert
            Console.WriteLine($"Expected: {expected:N5}");
            Console.WriteLine($"Actual:   {actual:N5}");
            Assert.AreEqual(Math.Round(expected, 5), Math.Round(actual, 5));
        }

        [TestMethod]
        public void GetInterestingList_MultiCase_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine(
                $"Tests several conditions:\n1) A subset of tokens are found in the probability list."
                    + $"\n2) A subset of tokens are not found in the probability list but are found in either the shared "
                    + $"token list or the dedicated token list, and\n3) Some of the tokens found in those lists do not meet "
                    + $"the minimum threshhold for inclusion and are excluded from the list. \n   "
                    + $"When included, they should carry the minimum probability of "
                    + $"a match to the current classifier because they are important to other classifiers\n"
                    + $"4) There is one new token, which should be excluded\n"
                    + $"5) There are two duplicated tokens which should have two entries"
            );

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1();

            // Set up tokens in the Prob list
            var inputTokens = Enumerable
                .Range(8, 4)
                .Select(i => SampleTestSets.alphabet[i].ToString())
                .ToList();

            // Add two duplicate tokens in the Prob list
            inputTokens.AddRange(
                Enumerable.Range(9, 2).Select(i => SampleTestSets.alphabet[i].ToString())
            );

            // Add Shared and Dedicated tokens that are NOT in the Prob list
            inputTokens.AddRange([
                "dedicated2",
                "dedicated3",
                "shared1",
                "shared2",
                "shared3",
                "new1",
            ]);

            var input = inputTokens
                .GroupBy(x => x)
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .ToDictionary();

            Console.WriteLine($"\nInput Tokens: \n[{string.Join(", ", inputTokens)}]\n");
            input.LogTokenFrequency("Input Token Frequency");

            // Set up the expected output
            var expected = new SortedList<string, double>();

            Enumerable
                .Range(8, 4)
                .ForEach(i =>
                    expected.Add(
                        $".{40 - i:00}000{SampleTestSets.alphabet[i]}0",
                        i / (double)100 + 0.6
                    )
                );
            Enumerable
                .Range(9, 2)
                .ForEach(i =>
                    expected.Add(
                        $".{40 - i:00}000{SampleTestSets.alphabet[i]}1",
                        i / (double)100 + 0.6
                    )
                );
            expected.Add($".01100dedicated20", 0.011);
            expected.Add($".01100shared10", 0.011);
            expected.Add($".01100shared20", 0.011);
            //expected.Add($".50000dedicated30", 0.500);
            expected.Add($".50000new10", 0.500);
            //expected.Add($".50000shared30", 0.500);

            //Console.WriteLine("Expected list should exclude:\n" +
            //    "dedicated3: does not meet minimum token count\n" +
            //    "shared3:    does not meet minimum token count\n" +
            //    "new1:       does not exist in any list\n");

            expected.LogProbabilities("Expected Probability List");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            actual.LogProbabilities("Actual Probability List");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetInterestingList_NullCase_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests null input");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = null;
            Dictionary<string, int> input = null;

            // Set up the expected output
            var expected = new SortedList<string, double>();
            expected.LogProbabilities("Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            actual.LogProbabilities("Actual Output");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetInterestingList_EmptyCase_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests empty input");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = [];
            Dictionary<string, int> input = new Dictionary<string, int>();

            // Set up the expected output
            var expected = new SortedList<string, double>();
            expected.LogProbabilities("Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            actual.LogProbabilities("Actual Output");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetInterestingList_AllNew_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests all new tokens");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = ["new1", "new2", "new3"];
            Dictionary<string, int> input = new Dictionary<string, int>
            {
                ["new1"] = 1,
                ["new2"] = 1,
                ["new3"] = 1,
            };

            // Set up the expected output
            var expected = new SortedList<string, double>()
            {
                { ".50000new10", 0.5000 },
                { ".50000new20", 0.5000 },
                { ".50000new30", 0.5000 },
            };
            expected.LogProbabilities("Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            actual.LogProbabilities("Actual Output");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void CombineProbabilities_01ExcludeEntriesAfterInterestingWordCount_ExpectedBehavior()
        {
            Console.WriteLine(
                "Tests whether the cutoff for Knobs.InterestingWordCount is working\n"
            );

            // Arrange
            var classifier = SampleTestSets.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];
            Enumerable.Range(0, cutoff).ForEach(i => input.Add($".00001highprobtoken{i}", 1));
            Enumerable.Range(0, 5).ForEach(i => input.Add($".40000averagetoken{i}", 0.5));
            Console.WriteLine($"Interesting Word Count: {cutoff}\n");
            input.LogProbabilities("Source List of Probabilities");
            double expected = 1;
            Console.WriteLine(
                $"Expected: {expected:N2} since all entries at 0.50 probability are cut off"
            );

            // Act
            double actual = classifier.CombineProbabilities(input);
            Console.WriteLine($"Actual: {actual:N2}");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CombineProbabilities_02FewEntries_ExpectedBehavior()
        {
            Console.WriteLine("Tests whether properly handles few entries\n");

            // Arrange
            var classifier = SampleTestSets.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];
            Enumerable
                .Range(0, Math.Max(1, cutoff - 2))
                .ForEach(i => input.Add($".00001highprobtoken{i}", 1));
            Console.WriteLine($"Interesting Word Count: {cutoff}\n");
            input.LogProbabilities("Source List of Probabilities");
            double expected = 1;
            Console.WriteLine($"Expected: {expected:N2}");

            // Act
            double actual = classifier.CombineProbabilities(input);
            Console.WriteLine($"Actual: {actual:N2}");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CombineProbabilities_03NoEntries_ExpectedBehavior()
        {
            Console.WriteLine("Tests whether properly handles no entries\n");

            // Arrange
            var classifier = SampleTestSets.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];

            input.LogProbabilities("Source List of Probabilities");
            double expected = 0;
            Console.WriteLine($"Expected: {expected:N2}");

            // Act
            double actual = classifier.CombineProbabilities(input);
            Console.WriteLine($"Actual: {actual:N2}");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Train_01BuildFromEmpty_ExpectedBehavior()
        {
            // Arrange
            var classifier = SampleTestSets.GetClassifier3a().Standardize();

            var input = new Dictionary<string, int>
            {
                ["token00"] = 4,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4,
            };

            var expected = SampleTestSets.GetClassifier3b().Standardize();

            // Act

            classifier.Train(input, 7);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    options =>
                        options
                            .Excluding(x => x.Parent.Tokenize)
                            .Excluding(x => x.Parent.TokenizeAsync)
                );
        }

        [TestMethod]
        public async Task Train_01BuildFromEmptyAsync_ExpectedBehavior()
        {
            // Arrange
            var classifier = SampleTestSets.GetClassifier3a().Standardize();

            var input = new Dictionary<string, int>
            {
                ["token00"] = 4,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4,
            };

            var expected = SampleTestSets.GetClassifier3b().Standardize();

            // Act

            await classifier.TrainAsync(input, 7, default);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    options =>
                        options
                            .Excluding(x => x.Parent.Tokenize)
                            .Excluding(x => x.Parent.TokenizeAsync)
                );
        }

        [TestMethod]
        public void Train_02AddIncremental_ExpectedBehavior()
        {
            // Arrange
            var classifier = SampleTestSets.GetClassifier3b().Standardize();

            var input = new Dictionary<string, int>
            {
                ["token00"] = 1,
                ["token08"] = 4,
                ["token09"] = 5,
                ["token10"] = 11,
            };

            var expected = SampleTestSets.GetClassifier3c().Standardize();

            // Act

            classifier.Train(input, 1);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    options =>
                        options
                            .Excluding(x => x.Parent.Tokenize)
                            .Excluding(x => x.Parent.TokenizeAsync)
                );
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_01AddToParent_False()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent,
                expected.Tag,
                matches,
                expected.MatchEmailCount,
                false,
                token
            );

            var actual = result.ToBayesianClassifierSub().Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_02AddToParent_True()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            parent.SharedTokenBase = new SubCorpus(
                new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                }
            );

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent,
                expected.Tag,
                matches,
                expected.MatchEmailCount,
                true,
                token
            );

            var actual = result.ToBayesianClassifierSub().Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_03NullParent()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            SubClassifierGroup parent = null;

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            Func<Task> act = () =>
                BayesianClassifierShared.FromTokenBaseAsync(
                    parent,
                    expected.Tag,
                    matches,
                    expected.MatchEmailCount,
                    true,
                    token
                );

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_04NullTag()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            parent.SharedTokenBase = new SubCorpus(
                new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                }
            );

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            Func<Task> act = () =>
                BayesianClassifierShared.FromTokenBaseAsync(
                    parent,
                    null,
                    matches,
                    expected.MatchEmailCount,
                    true,
                    token
                );

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_05AllNull()
        {
            // Arrange

            CancellationToken token = default;

            // Act
            Func<Task> act = () =>
                BayesianClassifierShared.FromTokenBaseAsync(null, null, null, 1, true, token);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_06EmailCountOutOfRange()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            parent.SharedTokenBase = new SubCorpus(
                new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                }
            );

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            Func<Task> act = () =>
                BayesianClassifierShared.FromTokenBaseAsync(
                    parent,
                    expected.Tag,
                    matches,
                    0,
                    true,
                    token
                );

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TrainMultiTag_UpdatesMatchCountsWithoutChangingSharedTokenBase()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();
            classifier.Match.TokenFrequency["existing"] = 2;
            classifier.Parent.SharedTokenBase.TokenFrequency["existing"] = 5;
            classifier.Parent.SharedTokenBase.TokenFrequency["shared-only"] = 3;

            // Act
            classifier.TrainMultiTag(new Dictionary<string, int> { ["existing"] = 1 }, 2);

            // Assert
            classifier.MatchEmailCount.Should().Be(4);
            classifier.Match.TokenFrequency["existing"].Should().Be(3);
            classifier.Parent.SharedTokenBase.TokenFrequency["existing"].Should().Be(5);
            classifier.Prob.Should().ContainKey("existing");
        }

        [TestMethod]
        public void UnTrainMultiTag_RemovesCountsWithoutChangingSharedTokenBase()
        {
            // Arrange
            var classifier = SampleTestSets.GetClassifier3c().Standardize();
            var originalSharedCount = classifier.Parent.SharedTokenBase.TokenFrequency["token08"];

            // Act
            classifier.UnTrainMultiTag(
                new Dictionary<string, int>
                {
                    ["token00"] = 1,
                    ["token08"] = 4,
                    ["token09"] = 5,
                    ["token10"] = 11,
                },
                1
            );

            // Assert
            classifier.MatchEmailCount.Should().Be(7);
            classifier.Match.TokenFrequency.Should().NotContainKey("token08");
            classifier
                .Parent.SharedTokenBase.TokenFrequency["token08"]
                .Should()
                .Be(originalSharedCount);
        }

        [TestMethod]
        public void UnTrain_ReversesIncrementalTrainingState()
        {
            // Arrange
            var classifier = SampleTestSets.GetClassifier3c().Standardize();

            // Act
            classifier.UnTrain(
                new Dictionary<string, int>
                {
                    ["token00"] = 1,
                    ["token08"] = 4,
                    ["token09"] = 5,
                    ["token10"] = 11,
                },
                1
            );

            // Assert
            classifier.MatchEmailCount.Should().Be(7);
            classifier.Parent.TotalEmailCount.Should().Be(16);
            classifier
                .Match.TokenFrequency.Should()
                .NotContainKeys("token08", "token09", "token10");
            classifier
                .Parent.SharedTokenBase.TokenFrequency.Should()
                .NotContainKeys("token08", "token09", "token10");
        }

        [TestMethod]
        public void UpdateProbability_RemovesToken_WhenBelowMinimumInclusion()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();
            classifier.Prob["low"] = 0.2;

            // Act
            classifier.UpdateProbability("low", 1, 1);

            // Assert
            classifier.Prob.Should().NotContainKey("low");
        }

        [TestMethod]
        public void UpdateProbability_WithNoNonMatchAndHighMatch_UsesCertainMatchScore()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 20, totalEmailCount: 25);

            // Act
            classifier.UpdateProbability("certain", classifier.Knobs.CertainMatchCount + 1, 0);

            // Assert
            classifier.Prob["certain"].Should().Be(classifier.Knobs.CertainMatchScore);
        }

        [TestMethod]
        public void UpdateProbability_WithNoNonMatchAndLowMatch_UsesLikelyMatchScore()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 20, totalEmailCount: 25);

            // Act
            classifier.UpdateProbability("likely", classifier.Knobs.MinCountForInclusion, 0);

            // Assert
            classifier.Prob["likely"].Should().Be(classifier.Knobs.LikelyMatchScore);
        }

        [TestMethod]
        public void UpdateProbability_WithBothCounts_StoresBoundedProbability()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 4, totalEmailCount: 10);

            // Act
            classifier.UpdateProbability("mixed", 4, 2);

            // Assert
            classifier.Prob["mixed"].Should().BeGreaterThan(classifier.Knobs.MinScore);
            classifier.Prob["mixed"].Should().BeLessThanOrEqualTo(classifier.Knobs.MaxScore);
        }

        [TestMethod]
        public void UpdateProbabilitySb_RemovesToken_WhenMatchCountIsZero()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();
            classifier.Prob["gone"] = 0.3;

            // Act
            classifier.UpdateProbabilitySb("gone", 0, 3);

            // Assert
            classifier.Prob.Should().NotContainKey("gone");
        }

        [TestMethod]
        public void UpdateProbabilitySb_WithWordInfo_ReturnsProbabilityBetweenZeroAndOne()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 4, totalEmailCount: 10);

            // Act
            var probability = classifier.UpdateProbabilitySb(
                new BayesianClassifierShared.WordInfo(3, 2)
            );

            // Assert
            probability.Should().BeGreaterThan(0);
            probability.Should().BeLessThan(1);
        }

        [TestMethod]
        public void UpdateProbabilitySb_WithToken_UsesStoredCounts()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 4, totalEmailCount: 10);
            classifier.Match.TokenFrequency["token"] = 3;
            classifier.Parent.SharedTokenBase.TokenFrequency["token"] = 5;

            // Act
            classifier.UpdateProbabilitySb("token");

            // Assert
            classifier.Prob.Should().ContainKey("token");
            classifier.Prob["token"].Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CombineProbabilities_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            var classifier = SampleTestSets.CreateBayesianClassifier();

            // Act
            Action act = () => classifier.CombineProbabilities(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetProbabilityDrivers_ReturnsCombinedProbabilityAndDrivers()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            var input = new Dictionary<string, int>
            {
                ["shared1"] = 2,
                ["dedicated8"] = 1,
                ["shared4"] = 2,
                ["shared2"] = 1,
            };

            // Act
            var result = classifier.GetProbabilityDrivers(input);

            // Assert
            result.Probability.Should().BeGreaterThan(0);
            result.Item2.Should().NotBeEmpty();
            result.Item2.Select(x => x.Token).Should().Contain("dedicated8");
        }

        [TestMethod]
        public async Task GetMatchProbabilityAsync_ReturnsSameAsSynchronousCalculation()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            var input = new Dictionary<string, int> { ["shared1"] = 2, ["dedicated8"] = 1 };
            var expected = classifier.GetMatchProbability(input);

            // Act
            var actual = await classifier.GetMatchProbabilityAsync(input, CancellationToken.None);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetMatchProbability_WithEnumerableTokens_ReturnsProbability()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();

            // Act
            var probability = classifier.GetMatchProbability(
                new[] { "shared1", "dedicated8", "shared1" }
            );

            // Assert
            probability.Should().BeGreaterThan(0);
            probability.Should().BeLessThan(1);
        }

        [TestMethod]
        public void WordInfo_StoresCounts()
        {
            // Act
            var info = new BayesianClassifierShared.WordInfo(3, 4);

            // Assert
            info.MatchCount.Should().Be(3);
            info.NotMatchCount.Should().Be(4);
        }

        [TestMethod]
        public void WordStream_StoresNameAndWords()
        {
            // Act
            var stream = new BayesianClassifierShared.WordStream("mail", new[] { "a", "b" });

            // Assert
            stream.Name.Should().Be("mail");
            stream.Words.Should().Equal("a", "b");
        }

        [TestMethod]
        public void Chi2SpamProb_WithNoClues_ReturnsHalfProbability()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();

            // Act
            var result = classifier.Chi2SpamProb(Array.Empty<string>(), evidence: false);

            // Assert
            result.Item1.Should().Be(0.5);
            result.Item2.Should().BeNull();
        }

        [TestMethod]
        public void Chi2SpamProb_WithEvidence_ReturnsEvidenceEntries()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();

            // Act
            var result = classifier.Chi2SpamProb(new[] { "shared1", "dedicated8" }, evidence: true);

            // Assert
            result.Item1.Should().BeGreaterThan(0);
            result.Item2.Should().NotBeNull();
            result.Item2.Select(x => x.word).Should().Contain(new[] { "*H*", "*S*" });
        }

        [TestMethod]
        public void Chi2SpamProb_WordStreamAndDictionaryOverloads_ReturnConsistentProbabilities()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            var tokens = new[] { "shared1", "dedicated8" };
            var wordStream = new BayesianClassifierShared.WordStream("mail", tokens);
            var tokenFrequency = new Dictionary<string, int>
            {
                ["shared1"] = 1,
                ["dedicated8"] = 1,
            };

            // Act
            var streamProbability = classifier.Chi2SpamProb(wordStream);
            var dictionaryProbability = classifier.Chi2SpamProb(tokenFrequency);

            // Assert
            streamProbability.Should().Be(dictionaryProbability);
        }

        [TestMethod]
        public async Task Chi2SpamProbAsync_ReturnsSameAsSynchronousCalculation()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            var tokens = new[] { "shared1", "dedicated8" };
            var expected = classifier.chi2_spamprob(tokens);

            // Act
            var actual = await classifier.Chi2SpamProbAsync(tokens);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void Chi2Q_ReturnsValueInExpectedRange()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();

            // Act
            var result = classifier.chi2Q(4, 4);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().BeLessThanOrEqualTo(1);
        }

        [TestMethod]
        public void GetClues_RespectsMinDistanceAndMaximumDiscriminators()
        {
            // Arrange
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            classifier.Knobs.MinDist = 0.2;
            classifier.Knobs.MaxDiscriminators = 1;

            // Act
            var clues = classifier.GetClues(
                new HashSet<string> { "shared1", "shared2", "dedicated8" }
            );

            // Assert
            clues.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetWordDistance_WithUnknownWord_UsesUnknownProbability()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();

            // Act
            var result = classifier.GetWordDistance("unknown");

            // Assert
            result.prob.Should().Be(classifier.Knobs.UnknownWordProb);
            result.record.Should().BeNull();
        }

        [TestMethod]
        public void GetWordInfo_WithNoCounts_ReturnsNull()
        {
            // Arrange
            var classifier = CreateSimpleClassifier();

            // Act
            var result = classifier.GetWordInfo("missing");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetWordInfo_WithCounts_ReturnsMatchAndNotMatchCounts()
        {
            // Arrange
            var classifier = CreateSimpleClassifier(matchEmailCount: 4, totalEmailCount: 10);
            classifier.Match.TokenFrequency["token"] = 3;
            classifier.Parent.SharedTokenBase.TokenFrequency["token"] = 5;

            // Act
            var result = classifier.GetWordInfo("token");

            // Assert
            result.Should().NotBeNull();
            result.MatchCount.Should().Be(3);
            result.NotMatchCount.Should().Be(2);
        }
    }

    public static class ClassifierTestExtensions
    {
        public static SubBayesianClassifier Standardize(this SubBayesianClassifier classifier)
        {
            var tokenFrequency =
                classifier.Match.TokenFrequency ?? new ConcurrentDictionary<string, int>();
            classifier.Match.TokenFrequency = new ConcurrentDictionary<string, int>(
                tokenFrequency.OrderBy(x => x.Key).ToDictionary()
            );

            var sharedTokenBase =
                classifier.Parent.SharedTokenBase.TokenFrequency
                ?? new ConcurrentDictionary<string, int>();
            classifier.Parent.SharedTokenBase.TokenFrequency = new ConcurrentDictionary<
                string,
                int
            >(sharedTokenBase.OrderBy(x => x.Key).ToDictionary());

            var prob = classifier.Prob ?? new ConcurrentDictionary<string, double>();
            classifier.Prob = new ConcurrentDictionary<string, double>(
                prob.Select(x => new KeyValuePair<string, double>(x.Key, Math.Round(x.Value, 5)))
                    .OrderBy(x => x.Key)
                    .ToDictionary()
            );

            return classifier;
        }

        public static SubBayesianClassifier ToBayesianClassifierSub(
            this BayesianClassifierShared classifier
        )
        {
            classifier ??= new BayesianClassifierShared();
            return new SubBayesianClassifier(classifier);
        }

        public static SubBayesianClassifier LogActualVsExpected(
            this SubBayesianClassifier actual,
            SubBayesianClassifier expected
        )
        {
            actual ??= new SubBayesianClassifier();
            expected ??= new SubBayesianClassifier();

            Console.WriteLine("");
            expected.Match.TokenFrequency.LogTokenFrequencyExpectedActual(
                actual.Match.TokenFrequency,
                "MATCH Token Frequency (expected vs actual)"
            );
            //LogTokenFrequency(expected.Match.TokenFrequency, $"Expected Match token frequency");
            //LogTokenFrequency(actual.Match.TokenFrequency, "Actual Match token frequency");
            Console.WriteLine("");

            expected.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequencyExpectedActual(
                actual.Parent.SharedTokenBase.TokenFrequency,
                "TOTAL Token Frequency (expected vs actual)"
            );
            //LogTokenFrequency(expected.Parent.SharedTokenBase.TokenFrequency, "Expected Total Token frequency");
            //LogTokenFrequency(actual.Parent.SharedTokenBase.TokenFrequency, "Actual Total token frequency");
            Console.WriteLine("");

            Console.WriteLine($"Expected Match email count: {expected.MatchEmailCount}");
            Console.WriteLine($"Actual Match email count: {actual.MatchEmailCount}");
            Console.WriteLine("");

            Console.WriteLine($"Expected Total token count: {expected.Parent.TotalEmailCount}");
            Console.WriteLine($"Actual Total token count: {actual.Parent.TotalEmailCount}");
            Console.WriteLine("");

            expected.Prob.LogProbabilitiesExpectedActual(
                actual.Prob,
                "Probabilities (expected vs actual)"
            );
            //LogProbabilities(expected.Prob, "Expected Probabilities");
            //LogProbabilities(actual.Prob, "Actual Probabilities");

            return actual;
        }

        public static SubCorpus ToCorpusSub(this Corpus corpus)
        {
            corpus ??= new Corpus();
            return new SubCorpus(corpus);
        }

        public static void LogProbabilities(
            this IDictionary<string, double> probabilities,
            string title
        )
        {
            probabilities ??= new Dictionary<string, double>();
            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N4"),
                headers: ["Class", "Probability"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title
            );
            Console.WriteLine(text);
        }

        public static void LogProbabilitiesExpectedActual(
            this IDictionary<string, double> expected,
            IDictionary<string, double> actual,
            string title
        )
        {
            expected ??= new Dictionary<string, double>();
            actual ??= new Dictionary<string, double>();

            var keys = expected.Keys.Union(actual.Keys).OrderBy(x => x).ToList();
            var jagged = keys.Select(key =>
                {
                    double expectedValue = 0;
                    expected.TryGetValue(key, out expectedValue);
                    double actualValue = 0;
                    actual.TryGetValue(key, out actualValue);
                    double diff = expectedValue - actualValue;
                    return new string[]
                    {
                        key,
                        expectedValue == 0 ? "" : expectedValue.ToString("N4"),
                        actualValue == 0 ? "" : actualValue.ToString("N4"),
                        diff == 0 ? "" : diff.ToString("N4"),
                    };
                })
                .ToArray();

            var text = jagged.ToFormattedText(
                ["Token", "Expected", "Actual", "Diff"],
                [
                    Enums.Justification.Left,
                    Enums.Justification.Center,
                    Enums.Justification.Center,
                    Enums.Justification.Right,
                ],
                title
            );

            Console.WriteLine(text);
        }

        public static void LogTokens(this IDictionary<string, double> probabilities, string title)
        {
            probabilities ??= new Dictionary<string, double>();
            Console.WriteLine(
                $"\n{title.ToUpper()}:\n[{string.Join(",", probabilities.Select(x => x.Key))}]"
            );
        }

        public static void LogTokens(this IDictionary<string, int> tokenFrequency, string title)
        {
            tokenFrequency ??= new Dictionary<string, int>();
            Console.WriteLine(
                $"\n{title.ToUpper()}:\n[{string.Join(",", tokenFrequency.Select(x => x.Key))}]"
            );
        }

        public static void LogTokenFrequency(
            this IDictionary<string, int> tokenFrequency,
            string title
        )
        {
            tokenFrequency ??= new Dictionary<string, int>();
            var text = tokenFrequency.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N0"),
                headers: ["Token", "Count"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title
            );
            Console.WriteLine(text);
        }

        public static void LogTokenFrequencyExpectedActual(
            this IDictionary<string, int> expected,
            IDictionary<string, int> actual,
            string title
        )
        {
            expected ??= new Dictionary<string, int>();
            actual ??= new Dictionary<string, int>();

            var keys = expected.Keys.Union(actual.Keys).OrderBy(x => x).ToList();
            var jagged = keys.Select(key =>
                {
                    int expectedValue = 0;
                    expected.TryGetValue(key, out expectedValue);
                    int actualValue = 0;
                    actual.TryGetValue(key, out actualValue);
                    int diff = expectedValue - actualValue;
                    return new string[]
                    {
                        key,
                        expectedValue == 0 ? "" : expectedValue.ToString("N0"),
                        actualValue == 0 ? "" : actualValue.ToString("N0"),
                        diff == 0 ? "" : diff.ToString("N0"),
                    };
                })
                .ToArray();

            var text = jagged.ToFormattedText(
                ["Token", "Expected", "Actual", "Diff"],
                [
                    Enums.Justification.Left,
                    Enums.Justification.Center,
                    Enums.Justification.Center,
                    Enums.Justification.Center,
                ],
                title
            );

            Console.WriteLine(text);
        }
    }
}
