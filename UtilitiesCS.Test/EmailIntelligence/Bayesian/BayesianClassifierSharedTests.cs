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
using static UtilitiesCS.Test.EmailIntelligence.Bayesian.BayesianClassifierSharedTests;
using System.Security.Policy;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
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

        public class BayesianClassifierSub : BayesianClassifierShared, ICloneable
        {
            public BayesianClassifierSub() { }
            public BayesianClassifierSub(ConcurrentDictionary<string, double> prob)
            {
                base._prob = prob;
            }
            public BayesianClassifierSub(BayesianClassifierShared classifier): base(classifier) { }

            public new ConcurrentDictionary<string, double> Prob { get => base._prob; set => base._prob = value; }

            public new ClassifierGroupSub Parent
            {
                get => base._parent as ClassifierGroupSub;
                set => base._parent = value;
            }

            public new CorpusSub Match { get => (base._match).ToCorpusSub(); set => base._match = value; }

            public object Clone()
            {
                var result = this.MemberwiseClone() as BayesianClassifierSub;
                result.Match = (CorpusSub)this.Match.Clone();
                result.Prob = new ConcurrentDictionary<string, double>(this.Prob ?? new ConcurrentDictionary<string, double>());
                result.Parent = (ClassifierGroupSub)this.Parent.Clone();
                return result;
            }
        }

        public class ClassifierGroupSub : BayesianClassifierGroup, ICloneable
        {            
            public ClassifierGroupSub() { }

            public ClassifierGroupSub(
                ConcurrentDictionary<string, DedicatedToken> dedicated,
                Corpus sharedTokenBase)
            {
                base._dedicatedTokens = dedicated;
                base._sharedTokenBase = sharedTokenBase;
                base._totalEmailCount = sharedTokenBase.TokenCount + dedicated.Sum(x => x.Value.Count);
            }

            public new virtual ConcurrentDictionary<string, DedicatedToken> DedicatedTokens { get => base._dedicatedTokens; set => base._dedicatedTokens = value; }

            public new virtual CorpusSub SharedTokenBase { get => (CorpusSub)base._sharedTokenBase; set => base._sharedTokenBase = value; }

            public object Clone()
            {
                var result = this.MemberwiseClone() as ClassifierGroupSub;
                result.SharedTokenBase = (CorpusSub)this.SharedTokenBase.Clone();
                return result;
            }
        }

        public class CorpusSub : Corpus
        {
            public CorpusSub() { }
            public CorpusSub(Corpus corpus) : base(corpus) { }
            public CorpusSub(IEnumerable<KeyValuePair<string, int>> tb) : base(tb) { }
            public CorpusSub(ConcurrentDictionary<string, int> tb)
            {
                this.TokenFrequency = tb;
                this.TokenCount = tb.Sum(x => x.Value);
            }

            public void SetTokenBase(ConcurrentDictionary<string, int> tb)
            {
                this.TokenFrequency = tb;

            }

            public new virtual ConcurrentDictionary<string, int> TokenFrequency 
            { 
                get => base._tokenFrequency; 
                set => base._tokenFrequency = value; 
            }
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
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    ["shared1"] = 6,
                    ["shared2"] = 4,
                    ["shared3"] = 2,
                    ["shared4"] = 6,
                    ["shared5"] = 4,
                    ["shared6"] = 1,
                    ["shared7"] = 50,
                    ["shared8"] = 40,
                    ["dedicated1"] = 6,
                    ["dedicated4"] = 6,
                    ["dedicated7"] = 8,
                    ["dedicated3"] = 1,
                    ["dedicated6"] = 1,
                    ["dedicated2"] = 4,
                    ["dedicated5"] = 4,
                    ["dedicated8"] = 20,
                }),
                TotalEmailCount = 163,
                DedicatedTokens = new ConcurrentDictionary<string, DedicatedToken>(),
                
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(
                Enumerable.Range(0, 26)
                .Select(i => new KeyValuePair<string, double>(
                alphabet[i].ToString(), i / (double)100 + 0.6)));

            classifier.Prob.LogProbabilities("Source token probability");

            classifier.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequency("Shared tokens");
            
            return classifier;
        }

        private BayesianClassifierSub SetupClassifierScenario1A()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderC";
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    ["shared1"] = 6,
                    ["shared2"] = 4,
                    ["shared3"] = 2,
                    ["shared4"] = 6,
                    ["shared5"] = 4,
                    ["shared6"] = 1,
                    ["shared7"] = 50,
                    ["shared8"] = 40,
                    ["dedicated1"] = 6,
                    ["dedicated4"] = 6,
                    ["dedicated7"] = 8,
                    ["dedicated3"] = 1,
                    ["dedicated6"] = 1,
                    ["dedicated2"] = 4,
                    ["dedicated5"] = 4,
                    ["dedicated8"] = 20,
                }),
                TotalEmailCount = 163,
                DedicatedTokens = new ConcurrentDictionary<string, DedicatedToken>(),

            };
            classifier.Prob = new ConcurrentDictionary<string, double>
            {
                ["shared1"] = 0.714285714285714,
                ["shared2"] = 0.142857142857143,
                ["shared7"] = 0.333333333333333,
                ["shared8"] = 0.333333333333333,
                ["dedicated7"] = 0.99980,
                ["dedicated8"] = 0.99990
            };

            classifier.Prob.LogProbabilities("Source token probability");
            classifier.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequency("Shared tokens");
            return classifier;
        }

        private BayesianClassifierSub GetClassifier3a()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4
                }),
                TotalEmailCount = 9
            };
            classifier.Tag = "folderA";
            return classifier;
        }

        private BayesianClassifierSub GetClassifier3b() 
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new CorpusSub(new Dictionary<string, int>
            {
                ["token00"] = 4,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4
            });
            classifier.MatchEmailCount = 7;
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    ["token00"] = 4,
                    ["token01"] = 4,
                    ["token02"] = 12,
                    ["token03"] = 16,
                    ["token04"] = 8,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                }),
                TotalEmailCount = 16
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {
                ["token02"] = 0.99990,
                ["token03"] = 0.52941,
                ["token04"] = 0.39130,
            });
            classifier.Tag = "folderA";
            return classifier;
        }

        private BayesianClassifierSub GetClassifier3c()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new CorpusSub(new Dictionary<string, int>
            {
                ["token00"] = 5,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4,
                ["token08"] = 4,
                ["token09"] = 5,
                ["token10"] = 11
            });
            classifier.MatchEmailCount = 8;
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    ["token00"] = 5,
                    ["token01"] = 4,
                    ["token02"] = 12,
                    ["token03"] = 16,
                    ["token04"] = 8,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                    ["token08"] = 4,
                    ["token09"] = 5,
                    ["token10"] = 11
                }),
                TotalEmailCount = 17
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {
                ["token00"] = 0.99980,
                ["token02"] = 0.99990,
                ["token03"] = 0.52941,
                ["token04"] = 0.36000,
                ["token09"] = 0.99980,
                ["token10"] = 0.99990
            });
            classifier.Tag = "folderA";
            return classifier;
        }

        private BayesianClassifierSub GetClassifierTemplate()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new CorpusSub(new Dictionary<string, int>
            {
                
            });
            classifier.MatchEmailCount = 0;
            classifier.Parent = new ClassifierGroupSub
            {
                SharedTokenBase = new CorpusSub(new Dictionary<string, int>
                {
                    
                }),
                TotalEmailCount = 0
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {
                
            });
            classifier.Tag = "";
            return classifier;
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
            input.LogTokenFrequency("Input Token Frequency");

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
            var classifier = this.CreateBayesianClassifier();
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
            var classifier = this.CreateBayesianClassifier();
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
            var classifier = this.CreateBayesianClassifier();
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
            var classifier = GetClassifier3a().Standardize();

            var input = new Dictionary<string, int>
            {
                ["token00"] = 4,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4
            };

            var expected = GetClassifier3b().Standardize();

            // Act

            classifier.Train(input, 7);
            var actual = ((BayesianClassifierSub)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected,
                options => options.Excluding(x => x.Parent.Tokenizer));

        }

        [TestMethod]
        public void Train_02AddIncremental_ExpectedBehavior()
        {
            // Arrange
            var classifier = GetClassifier3b().Standardize();

            var input = new Dictionary<string, int>
            {
                ["token00"] = 1,
                ["token08"] = 4,
                ["token09"] = 5,
                ["token10"] = 11
            };

            var expected = GetClassifier3c().Standardize();

            // Act

            classifier.Train(input, 1);
            var actual = ((BayesianClassifierSub)classifier.Clone()).Standardize();
            actual.LogActualVsExpected(expected);

            // Assert
            actual.Should().BeEquivalentTo(expected, 
                options => options.Excluding(x => x.Parent.Tokenizer));

        }

        [TestMethod]
        public async Task FromTokenBaseAsync_01AddToParent_False()
        {
            // Arrange
            var expected = GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as ClassifierGroupSub;
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
            var expected = GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as ClassifierGroupSub;
            parent.SharedTokenBase = new CorpusSub(new Dictionary<string, int>
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
            var expected = GetClassifier3c().Standardize();
            ClassifierGroupSub parent = null;

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
            var expected = GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as ClassifierGroupSub;
            parent.SharedTokenBase = new CorpusSub(new Dictionary<string, int>
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
            var expected = GetClassifier3c().Standardize();
            var parent = expected.Parent.Clone() as ClassifierGroupSub;
            parent.SharedTokenBase = new CorpusSub(new Dictionary<string, int>
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
        public static BayesianClassifierSub Standardize(this BayesianClassifierSub classifier)
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
        
        public static BayesianClassifierSub ToBayesianClassifierSub(this BayesianClassifierShared classifier)
        {
            classifier ??= new BayesianClassifierShared();
            return new BayesianClassifierSub(classifier);
        }

        public static BayesianClassifierSub LogActualVsExpected(this BayesianClassifierSub actual, BayesianClassifierSub expected)
        {
            actual ??= new BayesianClassifierSub();
            expected ??= new BayesianClassifierSub();

            LogTokenFrequency(expected.Match.TokenFrequency, $"Expected Match token frequency");
            Console.WriteLine($"Expected Match email count: {expected.MatchEmailCount}");
            Console.WriteLine("");

            LogTokenFrequency(actual.Match.TokenFrequency, "Actual Match token frequency");
            Console.WriteLine($"Actual Match email count: {actual.MatchEmailCount}");
            Console.WriteLine("");

            LogTokenFrequency(expected.Parent.SharedTokenBase.TokenFrequency, "Expected Total Token frequency");
            Console.WriteLine($"Expected Total token count: {expected.Parent.TotalEmailCount}");
            Console.WriteLine("");

            LogTokenFrequency(actual.Parent.SharedTokenBase.TokenFrequency, "Actual Total token frequency");
            Console.WriteLine($"Actual Total token count: {actual.Parent.TotalEmailCount}");
            Console.WriteLine("");

            LogProbabilities(expected.Prob, "Expected Probabilities");
            LogProbabilities(actual.Prob, "Actual Probabilities");

            return actual;
        }

        public static CorpusSub ToCorpusSub(this Corpus corpus) 
        { 
            corpus ??= new Corpus();
            return new CorpusSub(corpus);
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

    }

}
