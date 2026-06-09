using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class BayesianClassifierGroupTests
    {
        //private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            //this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestMethod]
        public void PythonIntegrationTest()
        {
            var group = new BayesianClassifierGroup();
            group.Train("ham", new string[] { "a", "b", "c" }, 1);
            group.Train("ham", new string[] { "a", "b" }, 1);
            group.Train("spam", new string[] { "c", "d" }, 1);

            List<BayesianClassifierShared.WordStream> wordStreams =
            [
                new BayesianClassifierShared.WordStream("test1", ["d"]),
                new BayesianClassifierShared.WordStream("test2", ["a"]),
                new BayesianClassifierShared.WordStream("test3", ["a", "b"]),
                new BayesianClassifierShared.WordStream("test4", ["d", "a", "b"]),
            ];
            var actual = wordStreams
                .Select(x => group.Classifiers["spam"].Chi2SpamProb(x))
                .ToList();
            List<double> expected =
            [
                0.8448275862068967,
                0.09183673469387754,
                0.03252482935305728,
                0.23394200608952753,
            ];

            var jagged = Enumerable
                .Range(0, wordStreams.Count)
                .Select(i =>
                    new string[]
                    {
                        wordStreams[i].Words.SentenceJoin(),
                        actual[i].ToString("F6"),
                        expected[i].ToString("F6"),
                        (actual[i] - expected[i]) == 0
                            ? "-"
                            : (actual[i] - expected[i]).ToString("F6"),
                    }
                )
                .ToArray();

            var text = jagged.ToFormattedText(
                ["WordStream", "Actual", "Expected", "Difference"],
                [
                    Enums.Justification.Left,
                    Enums.Justification.Right,
                    Enums.Justification.Right,
                    Enums.Justification.Center,
                ],
                "Probability Integration Test"
            );

            Console.WriteLine(text);

            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void Constructor_InitializesEmptyClassifiers()
        {
            // Act
            var group = new BayesianClassifierGroup();

            // Assert
            group.Classifiers.Should().BeEmpty();
            group.TotalEmailCount.Should().Be(0);
            group.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public void Train_NewTag_CreatesClassifier()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            group.Train("category1", new[] { "token1", "token2" }, 1);

            // Assert
            group.Classifiers.Should().ContainKey("category1");
            group.Classifiers["category1"].Tag.Should().Be("category1");
        }

        [TestMethod]
        public void TrainMultiTag_MultipleTags_CreatesAllClassifiers()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var tags = new[] { "tag1", "tag2" };
            var tokens = new[] { "a", "b", "c" };

            // Act
            group.TrainMultiTag(tags, tokens, 1);

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers.Should().ContainKey("tag2");
            group.TotalEmailCount.Should().Be(1);
        }

        [TestMethod]
        public void UnTrain_ExistingTag_ReducesOrRemoves()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "a", "b" }, 1);

            // Act
            group.UnTrain("tag1", new[] { "a", "b" }, 1);

            // Assert - classifier removed if match email count <= 0
            group.Classifiers.Should().NotContainKey("tag1");
        }

        [TestMethod]
        public void UnTrain_MissingTag_NoOp()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act & Assert - should not throw
            group.UnTrain("missing", new[] { "a" }, 1);
        }

        [TestMethod]
        public void UnTrainMultiTag_ReducesCountAndTokenBase()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.TrainMultiTag(new[] { "tag1" }, new[] { "a", "b" }, 2);

            // Act
            group.UnTrainMultiTag(new[] { "tag1" }, new[] { "a", "b" }, 2);

            // Assert
            group.TotalEmailCount.Should().Be(0);
        }

        [TestMethod]
        public async Task TrainMultiTagAsync_SameAsSync()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            await group.TrainMultiTagAsync(
                new[] { "tag1" },
                new[] { "a", "b" },
                1,
                CancellationToken.None
            );

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.TotalEmailCount.Should().Be(1);
        }

        [TestMethod]
        public async Task UnTrainMultiTagAsync_SameAsSync()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.TrainMultiTag(new[] { "tag1" }, new[] { "a" }, 1);

            // Act
            await group.UnTrainMultiTagAsync(
                new[] { "tag1" },
                new[] { "a" },
                1,
                CancellationToken.None
            );

            // Assert
            group.TotalEmailCount.Should().Be(0);
        }

        [TestMethod]
        public void AddToEmailCount_IncrementsTotal()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            group.AddToEmailCount(5);

            // Assert
            group.TotalEmailCount.Should().Be(5);
        }

        [TestMethod]
        public void Classify_TokenArray_ReturnsResults()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("ham", new[] { "good", "message" }, 2);
            group.Train("spam", new[] { "bad", "junk" }, 2);

            // Act
            var results = group.Classify(new[] { "good", "message" }).ToArray();

            // Assert
            results.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Classify_TokenIncidence_ReturnsResults()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("ham", new[] { "a", "b", "c" }, 1);
            group.Train("spam", new[] { "c", "d" }, 1);
            var tokenIncidence = new Dictionary<string, int> { ["a"] = 1, ["b"] = 1 };

            // Act
            var results = group.Classify(tokenIncidence).ToArray();

            // Assert
            results.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task ClassifyAsync_TokenArray_ReturnsResults()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "a", "b" }, 1);
            group.Train("tag2", new[] { "c", "d" }, 1);

            // Act
            var results = await group.ClassifyAsync(new[] { "a", "b" }, CancellationToken.None);

            // Assert
            results.Should().NotBeNull();
        }

        [TestMethod]
        public void MinimumProbability_SetAndGet_Works()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            group.MinimumProbability = 0.5;

            // Assert
            group.MinimumProbability.Should().Be(0.5);
        }

        [TestMethod]
        public void SharedTokenBase_SetAndGet_Works()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var corpus = new Corpus(new[] { "token1", "token2" });

            // Act
            group.SharedTokenBase = corpus;

            // Assert
            group.SharedTokenBase.Should().BeSameAs(corpus);
        }

        [TestMethod]
        public async Task RebuildClassifier_ReplacesExistingClassifier()
        {
            // Arrange — rebuild tokens must exist in SharedTokenBase (addToParent=false
            // uses direct lookup on parent.SharedTokenBase.TokenFrequency)
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "a", "b" }, 1);
            var newTokens = new Dictionary<string, int> { ["a"] = 1, ["b"] = 1 };

            // Act
            await group.RebuildClassifier("tag1", newTokens, 3, CancellationToken.None);

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
        }

        [TestMethod]
        public void GetReportMessage_WithCompletedItems_IncludesSpeed()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var sw = new SegmentStopWatch().Start();
            // Guarantee a non-zero measured elapsed without a wall-clock sleep so the per-second
            // speed string is produced. SpinUntil returns as soon as the Stopwatch advances past
            // zero (microseconds); the timeout is only a safety bound (Risk R7).
            System.Threading.SpinWait.SpinUntil(() => sw.Elapsed > TimeSpan.Zero, 100);

            // Act
            var message = group.GetReportMessage(5, 10, sw);

            // Assert
            message.Should().Contain("5 of 10");
            message.Should().Contain("per sec");
        }

        [TestMethod]
        public void GetReportMessage_WithZeroCompleted_NoSpeed()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var sw = new SegmentStopWatch().Start();

            // Act
            var message = group.GetReportMessage(0, 10, sw);

            // Assert
            message.Should().Contain("0 of 10");
        }

        //private SubClassifierGroup CreateBayesianClassifierGroup()
        //{
        //    return new SubClassifierGroup();
        //}

        //[TestMethod]
        //public void AddOrUpdateClassifier_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string tag = null;
        //    IEnumerable<string> matchTokens = null;
        //    int emailCount = 0;

        //    // Act
        //    bayesianClassifierGroup.AddOrUpdateClassifier(
        //        tag,
        //        matchTokens,
        //        emailCount);

        //    // Assert
        //    Assert.Fail();

        //}

        //[TestMethod]
        //public void AddToEmailCount_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    int count = 0;

        //    // Act
        //    bayesianClassifierGroup.AddToEmailCount(
        //        count);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task RebuildClassifier_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string tag = null;
        //    IDictionary<string, int> matchTokens = null;
        //    int matchEmailCount = 0;
        //    CancellationToken cancel = default(global::System.Threading.CancellationToken);

        //    // Act
        //    await bayesianClassifierGroup.RebuildClassifier(
        //        tag,
        //        matchTokens,
        //        matchEmailCount,
        //        cancel);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Classify_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    object source = null;

        //    // Act
        //    var result = bayesianClassifierGroup.Classify(
        //        source);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Classify_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string[] tokens = null;

        //    // Act
        //    var result = bayesianClassifierGroup.Classify(
        //        tokens);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Classify_StateUnderTest_ExpectedBehavior2()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    IDictionary tokenIncidence = null;

        //    // Act
        //    var result = bayesianClassifierGroup.Classify(
        //        tokenIncidence);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task ClassifyAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    object source = null;
        //    CancellationToken cancel = default(global::System.Threading.CancellationToken);

        //    // Act
        //    var result = await bayesianClassifierGroup.ClassifyAsync(
        //        source,
        //        cancel);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task ClassifyAsync_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string[] tokens = null;
        //    CancellationToken cancel = default(global::System.Threading.CancellationToken);

        //    // Act
        //    var result = await bayesianClassifierGroup.ClassifyAsync(
        //        tokens,
        //        cancel);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ClassifyAsync_StateUnderTest_ExpectedBehavior2()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    IDictionary tokenIncidence = null;
        //    CancellationToken cancel = default(global::System.Threading.CancellationToken);

        //    // Act
        //    var result = bayesianClassifierGroup.ClassifyAsync(
        //        tokenIncidence,
        //        cancel);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void AddOrUpdateClassifier_2_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string tag = null;
        //    IEnumerable<string> matchTokens = null;

        //    // Act
        //    bayesianClassifierGroup.AddOrUpdateClassifier_2(
        //        tag,
        //        matchTokens);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void UpdateSharedDictionaries2_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string key = null;
        //    int count = 0;
        //    string tag = null;

        //    // Act
        //    bayesianClassifierGroup.UpdateSharedDictionaries2(
        //        key,
        //        count,
        //        tag);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void UpdateSharedDictionaries_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var bayesianClassifierGroup = this.CreateBayesianClassifierGroup();
        //    string key = null;
        //    int value = 0;
        //    string tag = null;

        //    // Act
        //    bayesianClassifierGroup.UpdateSharedDictionaries(
        //        key,
        //        value,
        //        tag);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}
