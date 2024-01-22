using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierSharedTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());            
            this.mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
            
            this.dedicated = CreateDedicatedTokens();
            this.sharedTokens = CreateSharedTokens();
            var tokenBase = new CorpusSub();
            tokenBase.SetTokenBase(sharedTokens);
            this.sharedTokenBase = tokenBase.GetBase();
            this.classifierGroup = new ClassifierGroupSub
            {
                DedicatedTokens = this.dedicated,
                SharedTokenBase = this.sharedTokenBase,
                TotalEmailCount = this.sharedTokenBase.TokenCount + this.dedicated.Sum(x => x.Value.Count)
            };

            this.dedicated2 = CreateDedicatedTokens2();
            this.sharedTokens2 = CreateSharedTokens2();
            var tokenBase2 = new CorpusSub();
            tokenBase2.SetTokenBase(sharedTokens2);
            this.sharedTokenBase2 = tokenBase2.GetBase();
            this.classifierGroup2 = new ClassifierGroupSub
            {
                DedicatedTokens = this.dedicated2,
                SharedTokenBase = this.sharedTokenBase2,
                TotalEmailCount = this.sharedTokenBase2.TokenCount + this.dedicated2.Sum(x => x.Value.Count)
            };
            
        }

        #region Helper Functions and Classes

        private MockRepository mockRepository;
        private Mock<ClassifierGroupSub> mockClassifierGroup;
        private ClassifierGroupSub classifierGroup;
        private ClassifierGroupSub classifierGroup2;

        private ConcurrentDictionary<string, DedicatedToken> dedicated, dedicated2;
        private Corpus sharedTokenBase, sharedTokenBase2;
        private ConcurrentDictionary<string, int> sharedTokens, sharedTokens2;
        private Mock<BayesianClassifierShared> mockBayesianClassifier;

        private class BayesianClassifierSub : BayesianClassifierShared
        {
            public BayesianClassifierSub() { }
            public BayesianClassifierSub(ConcurrentDictionary<string, double> prob)
            {
                base._prob = prob;
            }

            public new ConcurrentDictionary<string, double> Prob { get => base._prob; set => base._prob = value; }

            public new ClassifierGroupSub Parent
            {
                get => base._parent as ClassifierGroupSub;
                set => base._parent = value;
            }

            public new Corpus Match { get => base._match; set => base._match = value; }

        }

        public class ClassifierGroupSub : BayesianClassifierGroup
        {
            public ClassifierGroupSub() { }

            public ClassifierGroupSub(
                ConcurrentDictionary<string, DedicatedToken> dedicated,
                Corpus sharedTokenBase)
            {
                base._dedicatedTokens = dedicated;
                base._sharedTokenBase = sharedTokenBase;
                base._totalTokenCount = sharedTokenBase.TokenCount + dedicated.Sum(x => x.Value.Count);
            }

            public new virtual ConcurrentDictionary<string, DedicatedToken> DedicatedTokens { get => base._dedicatedTokens; set => base._dedicatedTokens = value; }

            public new virtual Corpus SharedTokenBase { get => base._sharedTokenBase; set => base._sharedTokenBase = value; }
        }

        public class CorpusSub : Corpus
        {
            public CorpusSub() { }
            public CorpusSub(ConcurrentDictionary<string, int> tb)
            {
                this.TokenFrequency = tb;
                this.TokenCount = tb.Sum(x => x.Value);
            }

            public void SetTokenBase(ConcurrentDictionary<string, int> tb)
            {
                this.TokenFrequency = tb;

            }
            public Corpus GetBase() => this;
        }

        private void AddKvp(ConcurrentDictionary<string, DedicatedToken> cd, string token, int count, string folderPath)
        {
            cd.TryAdd(token, new DedicatedToken()
            { Token = token, Count = count, FolderPath = folderPath });
        }

        private ConcurrentDictionary<string, DedicatedToken> CreateDedicatedTokens()
        {
            var cd = new ConcurrentDictionary<string, DedicatedToken>();
            //AddKvp(cd, "dedicated1", 6, "folderA");
            //AddKvp(cd, "dedicated2", 4, "folderA");
            //AddKvp(cd, "dedicated3", 1, "folderA");
            //AddKvp(cd, "dedicated4", 6, "folderB");
            //AddKvp(cd, "dedicated5", 4, "folderB");
            //AddKvp(cd, "dedicated6", 1, "folderB");
            //AddKvp(cd, "dedicated7", 8, "folderC");
            //AddKvp(cd, "dedicated8", 20, "folderC");
            return cd;
        }

        private ConcurrentDictionary<string, DedicatedToken> CreateDedicatedTokens2()
        {
            var cd = new ConcurrentDictionary<string, DedicatedToken>();
            //AddKvp(cd, "dedicated7", 8, "folderC");
            //AddKvp(cd, "dedicated8", 20, "folderC");
            return cd;
        }

        private ConcurrentDictionary<string, int> CreateSharedTokens()
        {
            var cd = new ConcurrentDictionary<string, int>();
            cd.TryAdd("shared1", 6);
            cd.TryAdd("shared2", 4);
            cd.TryAdd("shared3", 2);
            cd.TryAdd("shared4", 6);
            cd.TryAdd("shared5", 4);
            cd.TryAdd("shared6", 1);
            cd.TryAdd("shared7", 50);
            cd.TryAdd("shared8", 40);
            
            cd.TryAdd("dedicated1", 6);
            cd.TryAdd("dedicated4", 6);
            cd.TryAdd("dedicated7", 8);
            cd.TryAdd("dedicated3", 1);
            cd.TryAdd("dedicated6", 1);
            cd.TryAdd("dedicated2", 4);
            cd.TryAdd("dedicated5", 4);
            cd.TryAdd("dedicated8", 20);
            return cd;
        }

        private ConcurrentDictionary<string, int> CreateSharedTokens2()
        {
            var cd = new ConcurrentDictionary<string, int>();
            cd.TryAdd("shared7", 40);
            cd.TryAdd("shared8", 20);
            return cd;
        }

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        private BayesianClassifierSub CreateBayesianClassifier()
        {
            return new BayesianClassifierSub();
        }

        private BayesianClassifierSub SetupClassifierScenario1()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderA";
            classifier.Parent = classifierGroup;
            classifier.Prob = new ConcurrentDictionary<string, double>(
                Enumerable.Range(0, 26)
                .Select(i => new KeyValuePair<string, double>(
                alphabet[i].ToString(), i / (double)100 + 0.6)));
                        
            LogProbabilities(classifier.Prob, "Source token probability");
            LogTokenFrequency(classifier.Parent.SharedTokenBase.TokenFrequency, "Shared tokens");
            LogDedicatedTokenFrequency();

            return classifier;
        }

        private BayesianClassifierSub SetupClassifierScenario1A()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderC";
            classifier.Parent = classifierGroup;
            classifier.Prob = new ConcurrentDictionary<string, double>
            {
                ["shared1"] = 0.714285714285714,
                ["shared2"] = 0.142857142857143,
                ["shared7"] = 0.333333333333333,
                ["shared8"] = 0.333333333333333,
                ["dedicated7"] = 0.99980,
                ["dedicated8"] = 0.99990
            };

            //LogTokens(classifier.Prob, "Source probability tokens");
            //LogTokens(classifier.Parent.SharedTokenBase.TokenFrequency.OrderBy(x => x.Key).ToDictionary(), "Shared tokens");
            //LogDedicatedTokens();
            LogProbabilities(classifier.Prob, "Source token probability");
            LogTokenFrequency(classifier.Parent.SharedTokenBase.TokenFrequency, "Shared tokens");
            LogDedicatedTokenFrequency();
            return classifier;
        }

        private BayesianClassifierSub SetupClassifierScenario2()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderC";
            classifier.Parent = new ClassifierGroupSub(CreateDedicatedTokens2(), new CorpusSub(CreateSharedTokens2()));
            classifier.Prob = new ConcurrentDictionary<string, double>(
                Enumerable.Range(0, 26)
                .Select(i => new KeyValuePair<string, double>(
                alphabet[i].ToString(), i / (double)100 + 0.6)));

            LogTokens(classifier.Prob.OrderBy(x => x.Key).ToDictionary(), "Source probability tokens");
            LogTokens(classifier.Parent.SharedTokenBase.TokenFrequency.OrderBy(x => x.Key).ToDictionary(), "Shared tokens");
            LogDedicatedTokens();
            return classifier;
        }

        private void LogProbabilities(IDictionary<string, double> probabilities, string title)
        {
            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N4"),
                headers: ["Class", "Probability"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        private void LogDedicatedTokens()
        {
            Console.WriteLine($"\nDEDICATED TOKENS:\n[{string.Join(",", this.dedicated.Select(x => x.Value.Token))}]");
        }

        private void LogTokens(IDictionary<string, double> probabilities, string title)
        {
            Console.WriteLine($"\n{title.ToUpper()}:\n[{string.Join(",", probabilities.Select(x => x.Key))}]");
        }

        private void LogTokens(IDictionary<string, int> probabilities, string title)
        {
            Console.WriteLine($"\n{title.ToUpper()}:\n[{string.Join(",", probabilities.Select(x => x.Key))}]");
        }

        private void LogTokenFrequency(IDictionary<string, int> probabilities, string title)
        {
            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N0"),
                headers: ["Token", "Count"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        private void LogDedicatedTokenFrequency()
        {
            var jagged = this.dedicated.Select(x => new string[] 
                { x.Value.Token, x.Value.FolderPath,x.Value.Count.ToString("N0") })
                .ToArray();

            var text = jagged.ToFormattedText(
                ["Token","FolderPath","Count"],
                [Enums.Justification.Left, Enums.Justification.Left, Enums.Justification.Right],
                "Dedicated Token Frequency");

            Console.WriteLine(text);
        }

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
            var classifier = SetupClassifierScenario1A();
            LogProbabilities(classifier.Prob.OrderBy(x => x.Key).ToDictionary(), "Source probabilities");

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
        public void GetProbabilityList_MultiCase_ExpectedBehavior()
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
            var classifier = SetupClassifierScenario1();

            // Set up tokens in the Prob list
            var inputTokens = Enumerable.Range(8, 4).Select(i => alphabet[i].ToString()).ToList();

            // Add two duplicate tokens in the Prob list
            inputTokens.AddRange(Enumerable.Range(9, 2).Select(i => alphabet[i].ToString()));

            // Add Shared and Dedicated tokens that are NOT in the Prob list
            inputTokens.AddRange(["dedicated2", "dedicated3", "shared1", "shared2", "shared3", "new1"]);

            var input = inputTokens.GroupBy(x => x).Select(group =>
                new KeyValuePair<string, int>(group.Key, group.Count()))
                .ToDictionary();

            Console.WriteLine($"\nInput Tokens: \n[{string.Join(", ", inputTokens)}]\n");
            LogTokenFrequency(input, "Input Token Frequency");


            // Set up the expected output
            var expected = new SortedList<string, double>();
            int j = 0;
            Enumerable.Range(8, 4)
                .ForEach(i => expected.Add(
                    $".{40 - i:00}000{alphabet[i]}0",
                    i / (double)100 + 0.6));
            Enumerable.Range(9, 2)
                .ForEach(i => expected.Add(
                    $".{40 - i:00}000{alphabet[i]}1",
                    i / (double)100 + 0.6));
            expected.Add($".01100dedicated20", 0.011);
            expected.Add($".01100shared10", 0.011);
            expected.Add($".01100shared20", 0.011);

            Console.WriteLine("Expected list should exclude:\n" +
                "dedicated3: does not meet minimum token count\n" +
                "shared3:    does not meet minimum token count\n" +
                "new1:       does not exist in any list\n");

            LogProbabilities(expected, "Expected Probability List");


            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            LogProbabilities(actual, "Actual Probability List");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetProbabilityList_NullCase_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests null input");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = null;
            Dictionary<string, int> input = null;

            // Set up the expected output
            var expected = new SortedList<string, double>();
            LogProbabilities(expected, "Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            LogProbabilities(actual, "Actual Output");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetProbabilityList_EmptyCase_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests empty input");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = [];
            Dictionary<string, int> input = new Dictionary<string, int>();

            // Set up the expected output
            var expected = new SortedList<string, double>();
            LogProbabilities(expected, "Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            LogProbabilities(actual, "Actual Output");

            // ===============
            // Assert
            // ===============
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetProbabilityList_AllNew_ExpectedBehavior()
        {
            // Test description
            Console.WriteLine($"Tests all new tokens");

            // ===============
            // Arrange
            // ===============

            // Set up classifier
            var classifier = SetupClassifierScenario1();

            // Set up null token parameter
            //string[] input = ["new1", "new2", "new3"];
            Dictionary<string, int> input = new Dictionary<string, int>
            {
                ["new1"] = 1,
                ["new2"] = 1,
                ["new3"] = 1
            };

            // Set up the expected output
            var expected = new SortedList<string, double>();
            LogProbabilities(expected, "Expected Output");

            // ===============
            // Act
            // ===============
            var actual = classifier.GetInterestingList(input);
            LogProbabilities(actual, "Actual Output");

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
            var classifier = this.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];
            Enumerable.Range(0, cutoff).ForEach(i => input.Add($".00001highprobtoken{i}", 1));
            Enumerable.Range(0, 5).ForEach(i => input.Add($".40000averagetoken{i}", 0.5));
            Console.WriteLine($"Interesting Word Count: {cutoff}\n");
            LogProbabilities(input, "Source List of Probabilities");
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
            var classifier = this.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];
            Enumerable.Range(0, Math.Max(1, cutoff - 2)).ForEach(i => input.Add($".00001highprobtoken{i}", 1));
            Console.WriteLine($"Interesting Word Count: {cutoff}\n");
            LogProbabilities(input, "Source List of Probabilities");
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
            var classifier = this.CreateBayesianClassifier();
            var cutoff = classifier.Knobs.InterestingWordCount;
            SortedList<string, double> input = [];

            LogProbabilities(input, "Source List of Probabilities");
            double expected = 0;
            Console.WriteLine($"Expected: {expected:N2}");

            // Act
            double actual = classifier.CombineProbabilities(input);
            Console.WriteLine($"Actual: {actual:N2}");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AddToMatches_01BuildFromEmpty_ExpectedBehavior(IEnumerable<KeyValuePair<string, int>> tokenFrequency)
        {
            var classifier = CreateBayesianClassifier();

            var sharedTokenFrequency = new ConcurrentDictionary<string,int>(
                new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4
                });

            var tokenBase = new CorpusSub();
            tokenBase.SetTokenBase(sharedTokenFrequency);
            this.classifierGroup = new ClassifierGroupSub
            {
                SharedTokenBase = tokenBase,
                TotalEmailCount = this.sharedTokenBase.TokenCount 
            };

            classifier.Parent = classifierGroup;
        }

        [TestMethod]
        public void AddTokens_IntegrationTest_ExpectedBehavior()
        {
            Assert.Fail();
        //    // Arrange
        //    var bayesianClassifier = new BayesianClassifierSub();


        //    List<string> matchTokens = [];
        //    List<string> notMatchTokens = [];

        //    for (int i = 0; i < 4; i++)
        //    {
        //        matchTokens.AddRange(Enumerable.Range(0, 5).Select(i => $"token{i:00}"));
        //        matchTokens.AddRange(Enumerable.Range(2, 2).Select(i => $"token{i:00}"));
        //        matchTokens.AddRange(Enumerable.Range(2, 2).Select(i => $"token{i:00}"));

        //        notMatchTokens.AddRange(Enumerable.Range(3, 5).Select(i => $"token{i:00}"));
        //        notMatchTokens.AddRange(Enumerable.Range(5, 2).Select(i => $"token{i:00}"));
        //        notMatchTokens.AddRange(Enumerable.Range(5, 2).Select(i => $"token{i:00}"));
        //    }

        //    Console.WriteLine($"Match tokens: [{string.Join(",", matchTokens)}]");
        //    Console.WriteLine($"Not Match tokens: [{string.Join(",", notMatchTokens)}]");

        //    var expectedMatchFrequency = new Dictionary<string, int>()
        //    {
        //        ["token00"] = 4,
        //        ["token01"] = 4,
        //        ["token02"] = 12,
        //        ["token03"] = 12,
        //        ["token04"] = 4,
        //    };

        //    var expectedNotMatchFrequency = new Dictionary<string, int>()
        //    {
        //        ["token03"] = 4,
        //        ["token04"] = 4,
        //        ["token05"] = 12,
        //        ["token06"] = 12,
        //        ["token07"] = 4,
        //    };

        //    var expectedMatchCount = 28;

        //    var expectedNotMatchCount = 36;

        //    Dictionary<string, double> expectedProb = new()
        //    {
        //        ["token02"] = 0.99990,
        //        ["token03"] = 0.65854,
        //        ["token04"] = 0.39130,
        //        ["token05"] = 0.01100,
        //        ["token06"] = 0.01100,
        //        ["token07"] = 0.01100
        //    };

        //    // Act
        //    bayesianClassifier.AddToMatches(matchTokens);

        //    var actualMatchFrequency = bayesianClassifier.Match.TokenFrequency.OrderBy(x => x.Key).ToDictionary();
        //    var actualMatchCount = bayesianClassifier.MatchCount;
        //    var actualNotMatchFrequency = bayesianClassifier.NotMatch.TokenFrequency.OrderBy(x => x.Key).ToDictionary();
        //    var actualNotMatchCount = bayesianClassifier.NotMatchCount;
        //    var actualProb = bayesianClassifier.Prob.Select(kvp =>
        //        new KeyValuePair<string, double>(kvp.Key, Math.Round(kvp.Value, 5)))
        //        .OrderBy(kvp => kvp.Key).ToDictionary();

        //    LogTokenFrequency(expectedMatchFrequency, "Expected Match token frequency");
        //    Console.WriteLine($"Expected Match token count: {expectedMatchCount}");
        //    Console.WriteLine("");

        //    LogTokenFrequency(actualMatchFrequency, "Actual Match token frequency");
        //    Console.WriteLine($"Actual Match token count: {actualMatchCount}");
        //    Console.WriteLine("");

        //    LogTokenFrequency(expectedNotMatchFrequency, "Expected Not Match token frequency");
        //    Console.WriteLine($"Expected Not Match token count: {expectedNotMatchCount}");
        //    Console.WriteLine("");

        //    LogTokenFrequency(actualNotMatchFrequency, "Actual Not Match token frequency");
        //    Console.WriteLine($"Actual Not Match token count: {actualNotMatchCount}");
        //    Console.WriteLine("");

        //    LogProbabilities(expectedProb, "Expected Probabilities");
        //    LogProbabilities(bayesianClassifier.Prob, "Actual Probabilities");

        //    // Assert
        //    actualMatchFrequency.Should().Equal(expectedMatchFrequency);
        //    actualMatchCount.Should().Be(expectedMatchCount);
        //    actualNotMatchFrequency.Should().Equal(expectedNotMatchFrequency);
        //    actualNotMatchCount.Should().Be(expectedNotMatchCount);
        //    actualProb.Should().Equal(expectedProb);
        }
    }
}
