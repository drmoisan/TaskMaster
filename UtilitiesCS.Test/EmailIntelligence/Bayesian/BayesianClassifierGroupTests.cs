using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using UtilitiesCS.EmailIntelligence.Bayesian;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;


namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierGroupTests
    {
        private MockRepository mockRepository;

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
            group.AddOrUpdateClassifier("ham", new string[] { "a", "b", "c" }, 1);
            group.AddOrUpdateClassifier("ham", new string[] { "a", "b" }, 1);
            group.AddOrUpdateClassifier("spam", new string[] { "c", "d" }, 1);

            List<BayesianClassifierShared.WordStream> wordStreams =
            [
                new BayesianClassifierShared.WordStream("test1", ["d"]),
                new BayesianClassifierShared.WordStream("test2", ["a"]),
                new BayesianClassifierShared.WordStream("test3", ["a","b"]),
                new BayesianClassifierShared.WordStream("test4", ["d", "a","b"]),
            ];
            var actual = wordStreams.Select(x => group.Classifiers["spam"].chi2_spamprob(x)).ToList();
            List<double> expected = [0.8448275862068967, 0.09183673469387754, 0.03252482935305728, 0.23394200608952753];
            
            var jagged = Enumerable.Range(0, wordStreams.Count).Select(i => new string[] 
            { 
                wordStreams[i].Words.SentenceJoin(), 
                actual[i].ToString("F6"), 
                expected[i].ToString("F6"),
                (actual[i] - expected[i]) == 0 ? "-": (actual[i] - expected[i]).ToString("F6")
            }).ToArray();

            var text = jagged.ToFormattedText(
                ["WordStream", "Actual", "Expected", "Difference"],
                [Enums.Justification.Left, Enums.Justification.Right, Enums.Justification.Right, Enums.Justification.Center],
                "Probability Integration Test");
            
            Console.WriteLine(text);
            
            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());

        }

        private SubClassifierGroup CreateBayesianClassifierGroup()
        {
            return new SubClassifierGroup();
        }

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
