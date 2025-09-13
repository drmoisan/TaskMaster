using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using UtilitiesCS.EmailIntelligence.Bayesian;
using System.Security.Policy;
using C;
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
                
        

        
        #endregion Helper Functions and Classes

        [TestMethod]
        public void GetMatchProbability_StateUnderTest_ExpectedBehavior()
        {
            Console.WriteLine("Integration test of GetMatchProbability method which \n" +
                "calls GetProbabilityList and CombineProbabilities");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1A();
            classifier.Prob.OrderBy(x => x.Key).ToDictionary().LogProbabilities("Source probabilities");

            // Set up tokens in the Prob list
            Dictionary<string, int> input = new Dictionary<string, int>
            {
                ["shared1"] = 2,
                ["dedicated8"] = 1,
                ["shared4"] = 2,
                ["shared2"] = 1,
                ["shared7"] = 1
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
            Console.WriteLine($"Tests several conditions:\n1) A subset of tokens are found in the probability list." +
                $"\n2) A subset of tokens are not found in the probability list but are found in either the shared " +
                $"token list or the dedicated token list, and\n3) Some of the tokens found in those lists do not meet " +
                $"the minimum threshhold for inclusion and are excluded from the list. \n   " +
                $"When included, they should carry the minimum probability of " +
                $"a match to the current classifier because they are important to other classifiers\n" +
                $"4) There is one new token, which should be excluded\n" +
                $"5) There are two duplicated tokens which should have two entries");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SampleTestSets.SetupClassifierScenario1();

            // Set up tokens in the Prob list
            var inputTokens = Enumerable.Range(8, 4).Select(i => SampleTestSets.alphabet[i].ToString()).ToList();

            // Add two duplicate tokens in the Prob list
            inputTokens.AddRange(Enumerable.Range(9, 2).Select(i => SampleTestSets.alphabet[i].ToString()));

            // Add Shared and Dedicated tokens that are NOT in the Prob list
            inputTokens.AddRange(["dedicated2", "dedicated3", "shared1", "shared2", "shared3", "new1"]);

            var input = inputTokens.GroupBy(x => x).Select(group =>
                new KeyValuePair<string, int>(group.Key, group.Count()))
                .ToDictionary();

            Console.WriteLine($"\nInput Tokens: \n[{string.Join(", ", inputTokens)}]\n");
            input.LogTokenFrequency("Input Token Frequency");

            // Set up the expected output
            var expected = new SortedList<string, double>();
            
            Enumerable.Range(8, 4)
                .ForEach(i => expected.Add(
                    $".{40 - i:00}000{SampleTestSets.alphabet[i]}0",
                    i / (double)100 + 0.6));
            Enumerable.Range(9, 2)
                .ForEach(i => expected.Add(
                    $".{40 - i:00}000{SampleTestSets.alphabet[i]}1",
                    i / (double)100 + 0.6));
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
                ["new3"] = 1
            };

            // Set up the expected output
            var expected = new SortedList<string, double>() 
            {
                { ".50000new10", 0.5000 },
                { ".50000new20", 0.5000 },
                { ".50000new30", 0.5000 }
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
            Console.WriteLine("Tests whether the cutoff for Knobs.InterestingWordCount is working\n");

            // Arrange
            var classifier = SampleTestSets.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];
            Enumerable.Range(0, cutoff).ForEach(i => input.Add($".00001highprobtoken{i}", 1));
            Enumerable.Range(0, 5).ForEach(i => input.Add($".40000averagetoken{i}", 0.5));
            Console.WriteLine($"Interesting Word Count: {cutoff}\n");
            input.LogProbabilities("Source List of Probabilities");
            double expected = 1;
            Console.WriteLine($"Expected: {expected:N2} since all entries at 0.50 probability are cut off");

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
            Enumerable.Range(0, Math.Max(1, cutoff - 2)).ForEach(i => input.Add($".00001highprobtoken{i}", 1));
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
                ["token04"] = 4
            };

            var expected = SampleTestSets.GetClassifier3b().Standardize();

            // Act

            classifier.Train(input, 7);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected,
                options => options
                    .Excluding(x => x.Parent.Tokenize)
                    .Excluding(x => x.Parent.TokenizeAsync));

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
                ["token04"] = 4
            };

            var expected = SampleTestSets.GetClassifier3b().Standardize();

            // Act

            await classifier.TrainAsync(input, 7, default);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected,
                options => options
                    .Excluding(x => x.Parent.Tokenize)
                    .Excluding(x => x.Parent.TokenizeAsync));

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
                ["token10"] = 11
            };

            var expected = SampleTestSets.GetClassifier3c().Standardize();

            // Act

            classifier.Train(input, 1);
            var actual = ((SubBayesianClassifier)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected,
                options => options
                    .Excluding(x => x.Parent.Tokenize)
                    .Excluding(x => x.Parent.TokenizeAsync));

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
                parent, expected.Tag, matches, expected.MatchEmailCount, false, token);

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
            parent.SharedTokenBase = new SubCorpus(new Dictionary<string, int>
            {
                ["token03"] = 4,
                ["token04"] = 4,
                ["token05"] = 12,
                ["token06"] = 12,
                ["token07"] = 4,
            });

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent, expected.Tag, matches, expected.MatchEmailCount, true, token);

            var actual = result.ToBayesianClassifierSub().Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FromTokenBaseAsync_03NullParent()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            SubClassifierGroup parent = null;

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent, expected.Tag, matches, expected.MatchEmailCount, true, token);

            // Assert
            Console.WriteLine("Test failed because did not throw ArgumentNullException");
            Assert.Fail();

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FromTokenBaseAsync_04NullTag()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            parent.SharedTokenBase = new SubCorpus(new Dictionary<string, int>
            {
                ["token03"] = 4,
                ["token04"] = 4,
                ["token05"] = 12,
                ["token06"] = 12,
                ["token07"] = 4,
            });

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent, null, matches, expected.MatchEmailCount, true, token);

            // Assert
            Console.WriteLine("Test failed because did not throw ArgumentNullException");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FromTokenBaseAsync_05AllNull()
        {
            // Arrange
            
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                null, null, null, 1, true, token);

            // Assert
            Console.WriteLine("Test failed because did not throw ArgumentNullException");
            Assert.Fail(); 
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task FromTokenBaseAsync_06EmailCountOutOfRange()
        {
            // Arrange
            var expected = SampleTestSets.GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as SubClassifierGroup;
            parent.SharedTokenBase = new SubCorpus(new Dictionary<string, int>
            {
                ["token03"] = 4,
                ["token04"] = 4,
                ["token05"] = 12,
                ["token06"] = 12,
                ["token07"] = 4,
            });

            var matches = expected.Match.TokenFrequency.ToDictionary();
            CancellationToken token = default;

            // Act
            var result = await BayesianClassifierShared.FromTokenBaseAsync(
                parent, expected.Tag, matches, 0, true, token);

            // Assert
            Console.WriteLine("Test failed because did not throw ArgumentOutOfRangeException");
            Assert.Fail(); 
        }


    }

    public static class ClassifierTestExtensions
    {
        public static SubBayesianClassifier Standardize(this SubBayesianClassifier classifier)
        {
            var tokenFrequency = classifier.Match.TokenFrequency ?? new ConcurrentDictionary<string, int>();
            classifier.Match.TokenFrequency = new ConcurrentDictionary<string, int>(
                tokenFrequency.OrderBy(x => x.Key).ToDictionary());

            var sharedTokenBase = classifier.Parent.SharedTokenBase.TokenFrequency ?? new ConcurrentDictionary<string, int>();
            classifier.Parent.SharedTokenBase.TokenFrequency = new ConcurrentDictionary<string, int>(
                sharedTokenBase.OrderBy(x => x.Key).ToDictionary());

            var prob = classifier.Prob ?? new ConcurrentDictionary<string, double>();
            classifier.Prob = new ConcurrentDictionary<string, double>(
                prob.Select(x => new KeyValuePair<string, double>(x.Key, Math.Round(x.Value, 5)))
                .OrderBy(x => x.Key).ToDictionary());
            
            return classifier;
        }
        
        public static SubBayesianClassifier ToBayesianClassifierSub(this BayesianClassifierShared classifier)
        {
            classifier ??= new BayesianClassifierShared();
            return new SubBayesianClassifier(classifier);
        }

        public static SubBayesianClassifier LogActualVsExpected(this SubBayesianClassifier actual, SubBayesianClassifier expected)
        {
            actual ??= new SubBayesianClassifier();
            expected ??= new SubBayesianClassifier();

            Console.WriteLine("");
            expected.Match.TokenFrequency.LogTokenFrequencyExpectedActual(actual.Match.TokenFrequency, "MATCH Token Frequency (expected vs actual)");
            //LogTokenFrequency(expected.Match.TokenFrequency, $"Expected Match token frequency");
            //LogTokenFrequency(actual.Match.TokenFrequency, "Actual Match token frequency");
            Console.WriteLine("");

            expected.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequencyExpectedActual(actual.Parent.SharedTokenBase.TokenFrequency, "TOTAL Token Frequency (expected vs actual)");
            //LogTokenFrequency(expected.Parent.SharedTokenBase.TokenFrequency, "Expected Total Token frequency");
            //LogTokenFrequency(actual.Parent.SharedTokenBase.TokenFrequency, "Actual Total token frequency");
            Console.WriteLine("");

            Console.WriteLine($"Expected Match email count: {expected.MatchEmailCount}");
            Console.WriteLine($"Actual Match email count: {actual.MatchEmailCount}");
            Console.WriteLine("");

            Console.WriteLine($"Expected Total token count: {expected.Parent.TotalEmailCount}");
            Console.WriteLine($"Actual Total token count: {actual.Parent.TotalEmailCount}");
            Console.WriteLine("");

            expected.Prob.LogProbabilitiesExpectedActual(actual.Prob, "Probabilities (expected vs actual)");
            //LogProbabilities(expected.Prob, "Expected Probabilities");
            //LogProbabilities(actual.Prob, "Actual Probabilities");

            return actual;
        }

        public static SubCorpus ToCorpusSub(this Corpus corpus) 
        { 
            corpus ??= new Corpus();
            return new SubCorpus(corpus);
        }
        
        public static void LogProbabilities(this IDictionary<string, double> probabilities, string title)
        {
            probabilities ??= new Dictionary<string, double>();
            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N4"),
                headers: ["Class", "Probability"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        public static void LogProbabilitiesExpectedActual(this IDictionary<string, double> expected, IDictionary<string, double> actual, string title)
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
                    diff == 0 ? "" : diff.ToString("N4") };
            }).ToArray();

            var text = jagged.ToFormattedText(
                ["Token", "Expected", "Actual", "Diff"],
                [Enums.Justification.Left, Enums.Justification.Center, Enums.Justification.Center, Enums.Justification.Right],
                title);

            Console.WriteLine(text);
        }

        public static void LogTokens(this IDictionary<string, double> probabilities, string title)
        {
            probabilities ??= new Dictionary<string, double>();
            Console.WriteLine($"\n{title.ToUpper()}:\n[{string.Join(",", probabilities.Select(x => x.Key))}]");
        }

        public static void LogTokens(this IDictionary<string, int> tokenFrequency, string title)
        {
            tokenFrequency ??= new Dictionary<string, int>();
            Console.WriteLine($"\n{title.ToUpper()}:\n[{string.Join(",", tokenFrequency.Select(x => x.Key))}]");
        }

        public static void LogTokenFrequency(this IDictionary<string, int> tokenFrequency, string title)
        {
            tokenFrequency ??= new Dictionary<string, int>();
            var text = tokenFrequency.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N0"),
                headers: ["Token", "Count"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        public static void LogTokenFrequencyExpectedActual(this IDictionary<string, int> expected, IDictionary<string, int> actual, string title)
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
                    diff == 0 ? "" : diff.ToString("N0") };
            }).ToArray();

            var text = jagged.ToFormattedText(
                ["Token", "Expected", "Actual", "Diff"], 
                [Enums.Justification.Left, Enums.Justification.Center, Enums.Justification.Center, Enums.Justification.Center],
                title);
                        
            Console.WriteLine(text);
        }

    }

}
