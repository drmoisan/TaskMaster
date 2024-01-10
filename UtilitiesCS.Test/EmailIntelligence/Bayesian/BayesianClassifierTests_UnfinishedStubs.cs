using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using static UtilitiesCS.Test.EmailIntelligence.Bayesian.BayesianClassifierTests;
using System.Collections.Concurrent;
using UtilitiesCS.EmailIntelligence.Bayesian;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UtilitiesCS.HelperClasses;
using System.Linq;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierTests_UnfinishedStubs
    {
        private MockRepository mockRepository;
        private Mock<ClassifierGroupSub> mockClassifierGroup;
        private ClassifierGroupSub classifierGroup;
        private ConcurrentDictionary<string, DedicatedToken> dedicated;
        private Mock<Corpus> sharedTokenBase;
        private Corpus sharedTokenBase2;
        private ConcurrentDictionary<string, int> sharedTokens;
        private Mock<BayesianClassifier> mockBayesianClassifier;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
            this.dedicated = CreateDedicatedTokens();

            this.sharedTokens = CreateSharedTokens();
            var tokenBase = new CorpusSub();
            tokenBase.SetTokenBase(sharedTokens);
            this.sharedTokenBase2 = tokenBase.GetBase();

            this.classifierGroup = new ClassifierGroupSub();
            this.classifierGroup.DedicatedTokens = this.dedicated;
            this.classifierGroup.SharedTokenBase = this.sharedTokenBase2;
        }

        private class BayesianClassifierSub : BayesianClassifier
        {
            public BayesianClassifierSub() { }
            public BayesianClassifierSub(ConcurrentDictionary<string, double> prob)
            {
                base._prob = prob;
            }

            public new ConcurrentDictionary<string, double> Prob { get => base._prob; set => base._prob = value; }

            public new ClassifierGroupSub Parent
            {
                set => base._parent = value;
            }
        }

        public class ClassifierGroupSub : ClassifierGroup
        {
            public ClassifierGroupSub() { }

            public new virtual ConcurrentDictionary<string, DedicatedToken> DedicatedTokens { get => base._dedicatedTokens; set => base._dedicatedTokens = value; }

            public new virtual Corpus SharedTokenBase { get => base._sharedTokenBase; set => base._sharedTokenBase = value; }
        }

        public class CorpusSub : Corpus
        {
            public CorpusSub() { }
            public void SetTokenBase(ConcurrentDictionary<string, int> tb)
            {
                this.TokenFrequency = tb;
            }
            public Corpus GetBase() => this;
        }

        #region Helper Functions and Classes

        private void AddKvp(ConcurrentDictionary<string, DedicatedToken> cd, string token, int count, string folderPath)
        {
            cd.TryAdd(token, new DedicatedToken()
            { Token = token, Count = count, FolderPath = folderPath });
        }

        private ConcurrentDictionary<string, DedicatedToken> CreateDedicatedTokens()
        {
            var cd = new ConcurrentDictionary<string, DedicatedToken>();
            AddKvp(cd, "dedicated1", 6, "folderA");
            AddKvp(cd, "dedicated2", 4, "folderA");
            AddKvp(cd, "dedicated3", 1, "folderA");
            AddKvp(cd, "dedicated4", 6, "folderB");
            AddKvp(cd, "dedicated5", 4, "folderB");
            AddKvp(cd, "dedicated6", 1, "folderB");
            return cd;
        }

        private ConcurrentDictionary<string, int> CreateSharedTokens()
        {
            var cd = new ConcurrentDictionary<string, int>();
            cd.TryAdd("shared1", 6);
            cd.TryAdd("shared2", 4);
            cd.TryAdd("shared3", 1);
            cd.TryAdd("shared4", 6);
            cd.TryAdd("shared5", 4);
            cd.TryAdd("shared6", 1);
            return cd;
        }

        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        private BayesianClassifierSub CreateBayesianClassifier()
        {
            return new BayesianClassifierSub();
        }

        private void LogProbabilities(IDictionary<string, double> probabilities, string title)
        {
            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N2"),
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

        #endregion Helper Functions and Classes

        #region Unfinished Tests

        [TestMethod]
        public void FromTokenBase_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            ClassifierGroup parent = null;
            string tag = null;
            IEnumerable<string> positiveTokens = null;

            // Act
            var result = BayesianClassifier.FromTokenBase(
                parent,
                tag,
                positiveTokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            ClassifierGroup parent = null;
            string tag = null;
            IEnumerable<string> matchTokens = null;
            CancellationToken token = default(global::System.Threading.CancellationToken);

            // Act
            var result = await BayesianClassifier.FromTokenBaseAsync(
                parent,
                tag,
                matchTokens,
                token);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddPositive_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> tokens = null;

            // Act
            bayesianClassifier.AddPositive(
                tokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddNegative_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> tokens = null;

            // Act
            bayesianClassifier.AddNegative(
                tokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddTokens_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> positiveTokens = null;
            IEnumerable<string> negativeTokens = null;

            // Act
            bayesianClassifier.AddTokens(
                positiveTokens,
                negativeTokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemovePositive_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> tokens = null;

            // Act
            bayesianClassifier.RemovePositive(
                tokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveNegative_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> tokens = null;

            // Act
            bayesianClassifier.RemoveNegative(
                tokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Load_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> positiveTokens = null;
            IEnumerable<string> negativeTokens = null;

            // Act
            bayesianClassifier.Load(
                positiveTokens,
                negativeTokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task InferNegativeTokensAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            CancellationToken token = default(global::System.Threading.CancellationToken);
            SegmentStopWatch sw = null;

            // Act
            await bayesianClassifier.InferNegativeTokensAsync(
                token,
                sw);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task RecalcProbsAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            CancellationToken token = default(global::System.Threading.CancellationToken);
            SegmentStopWatch sw = null;

            // Act
            await bayesianClassifier.RecalcProbsAsync(
                token,
                sw);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task AfterDeserialize_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            CancellationToken token = default(global::System.Threading.CancellationToken);
            SegmentStopWatch sw = null;

            // Act
            await bayesianClassifier.AfterDeserialize(
                token,
                sw);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetMatchProbability_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var bayesianClassifier = this.CreateBayesianClassifier();
            IEnumerable<string> tokens = null;

            // Act
            var result = bayesianClassifier.GetMatchProbability(
                tokens);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        
        #endregion Unfinished Tests

    }
}
