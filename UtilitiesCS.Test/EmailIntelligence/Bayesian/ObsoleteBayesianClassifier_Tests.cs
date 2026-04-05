using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;

#pragma warning disable CS0618 // Obsolete type under test

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class ObsoleteBayesianClassifier_Tests
    {
        private sealed class BayesianClassifierProbe : BayesianClassifier
        {
            public ConcurrentDictionary<string, double> ProbabilityMap
            {
                get => this._prob;
                set => this._prob = value;
            }

            public Corpus MatchCorpus
            {
                get => this._match;
                set => this._match = value;
            }

            public Corpus NotMatchCorpus
            {
                get => this._notMatch;
                set => this._notMatch = value;
            }

            public ClassifierGroup ParentGroup
            {
                get => this._parent;
                set => this._parent = value;
            }

            public void InvokeUpdateProbabilityStandalone(string token)
            {
                this.UpdateProbabilityStandalone(token);
            }

            public void InvokeUpdateProbabilityShared(string token)
            {
                this.UpdateProbabilityShared(token);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        private static void SetNonPublicProperty<TTarget, TValue>(
            TTarget target,
            string propertyName,
            TValue value
        )
        {
            var property = typeof(TTarget).GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            property.Should().NotBeNull();
            property!.GetSetMethod(nonPublic: true)!.Invoke(target, [value]);
        }

        private static ClassifierGroup CreateLegacyClassifierGroup(params string[] sharedTokens)
        {
            var group = new ClassifierGroup();

            if (sharedTokens.Length > 0)
            {
                group.SharedTokenBase.AddOrIncrementTokens(sharedTokens);
            }

            group.DedicatedTokens["dedicated-strong"] = new DedicatedToken
            {
                Token = "dedicated-strong",
                Count = 6,
                FolderPath = "folder-a",
            };
            group.DedicatedTokens["dedicated-weak"] = new DedicatedToken
            {
                Token = "dedicated-weak",
                Count = 4,
                FolderPath = "folder-a",
            };

            return group;
        }

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier();

            // Assert
            classifier.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTag_InitializesProperties()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier("test-tag");

            // Assert
            classifier.Tag.Should().Be("test-tag");
            classifier.Prob.Should().NotBeNull();
            classifier.NotMatch.Should().NotBeNull();
        }

        [TestMethod]
        public void Tag_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifier();

            // Act
            classifier.Tag = "new-tag";

            // Assert
            classifier.Tag.Should().Be("new-tag");
        }

        [TestMethod]
        public void Load_WithTokens_InitializesMatchAndNotMatch()
        {
            // Arrange
            var classifier = new BayesianClassifier("tag");
            var positive = new[] { "good1", "good2", "good1" };
            var negative = new[] { "bad1", "bad2" };

            // Act
            classifier.Load(positive, negative);

            // Assert
            classifier.Match.Should().NotBeNull();
            classifier.NotMatch.Should().NotBeNull();
        }

        [TestMethod]
        public void AddMatch_AddsTokensToMatchCorpus()
        {
            // Arrange
            var parent = new ClassifierGroup();
            var classifier = new BayesianClassifier("tag") { Parent = parent };

            // Act
            classifier.AddMatch(new[] { "word1", "word2" });

            // Assert
            classifier.Match.TokenFrequency.Should().ContainKey("word1");
        }

        [TestMethod]
        public void AddNotMatch_AddsTokensToNotMatchCorpus()
        {
            // Arrange
            var parent = new ClassifierGroup();
            var classifier = new BayesianClassifier("tag") { Parent = parent };

            // Act
            classifier.AddNotMatch(new[] { "word1", "word2" });

            // Assert
            classifier.NotMatch.TokenFrequency.Should().ContainKey("word1");
        }

        [TestMethod]
        public void Loaded_DefaultIsFalse()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier();

            // Assert
            classifier.Loaded.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithPositiveAndNegativeTokens_InitializesCorrectly()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier(
                "tag",
                new[] { "pos1", "pos2", "pos1" },
                new[] { "neg1", "neg2" }
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Prob.Should().NotBeNull();
        }

        [TestMethod]
        public void FromTokenBase_WithValidInputs_ReturnsClassifier()
        {
            // Arrange
            var parent = new ClassifierGroup();
            parent.SharedTokenBase.AddOrIncrementTokens(
                new[] { "hello", "world", "hello", "test" }
            );

            // Act
            var classifier = BayesianClassifier.FromTokenBase(
                parent,
                "tag",
                new[] { "hello", "hello" }
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Parent.Should().BeSameAs(parent);
        }

        [TestMethod]
        public void NonPublicPropertySetters_RoundTripExpectedState()
        {
            // Arrange
            var classifier = new BayesianClassifier();
            var match = new Corpus(new[] { "match-token" });
            var notMatch = new Corpus(new[] { "not-match-token" });
            var probabilities = new ConcurrentDictionary<string, double>();
            probabilities["match-token"] = 0.75;

            // Act
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.Match), match);
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.MatchCount), 1);
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.NotMatch), notMatch);
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.NotMatchCount), 1);
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.Prob), probabilities);
            SetNonPublicProperty(classifier, nameof(BayesianClassifier.Loaded), true);
            classifier.Knobs = new BayesianClassifier.KnobList { InterestingWordCount = 3 };

            // Assert
            classifier.Match.Should().BeSameAs(match);
            classifier.MatchCount.Should().Be(1);
            classifier.NotMatch.Should().BeSameAs(notMatch);
            classifier.NotMatchCount.Should().Be(1);
            classifier.Prob.Should().BeSameAs(probabilities);
            classifier.Loaded.Should().BeTrue();
            classifier.Knobs.InterestingWordCount.Should().Be(3);
        }

        [TestMethod]
        public void GetProbabilityList_WithMixedSources_ReturnsOnlyIncludedTokens()
        {
            // Arrange
            var classifier = new BayesianClassifierProbe
            {
                ParentGroup = CreateLegacyClassifierGroup(
                    "shared-strong",
                    "shared-strong",
                    "shared-strong",
                    "shared-strong",
                    "shared-strong",
                    "shared-strong"
                ),
                ProbabilityMap = new ConcurrentDictionary<string, double>(),
            };
            classifier.ProbabilityMap["known"] = 0.8;

            // Act
            var probabilities = classifier.GetProbabilityList(
                new[]
                {
                    "known",
                    "known",
                    "shared-strong",
                    "dedicated-strong",
                    "dedicated-weak",
                    "missing",
                }
            );

            // Assert
            probabilities.Should().HaveCount(5);
            probabilities.Values.Count(x => x == 0.8).Should().Be(2);
            probabilities.Values.Count(x => x == classifier.Knobs.MinScore).Should().Be(3);
        }

        [TestMethod]
        public void CombineProbabilities_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            var classifier = new BayesianClassifier();

            // Act
            Action act = () => classifier.CombineProbabilities(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetMatchProbability_WithKnownTokens_ReturnsExpectedCombinedScore()
        {
            // Arrange
            var classifier = new BayesianClassifierProbe
            {
                ProbabilityMap = new ConcurrentDictionary<string, double>(),
                ParentGroup = CreateLegacyClassifierGroup(),
            };
            classifier.ProbabilityMap["high"] = 0.8;
            classifier.ProbabilityMap["low"] = 0.2;

            // Act
            var probability = classifier.GetMatchProbability(new[] { "high", "low" });

            // Assert
            probability.Should().BeApproximately(0.5, 0.000001);
        }

        [TestMethod]
        public void RemovePositiveAndRemoveNegative_UpdateTokenFrequencies()
        {
            // Arrange
            var classifier = new BayesianClassifierProbe
            {
                ParentGroup = CreateLegacyClassifierGroup(
                    "match-token",
                    "match-token",
                    "match-token",
                    "match-token",
                    "match-token",
                    "match-token",
                    "not-match-token",
                    "not-match-token",
                    "not-match-token",
                    "not-match-token",
                    "not-match-token",
                    "not-match-token"
                ),
            };
            classifier.AddTokens(
                Enumerable.Repeat("match-token", 6),
                Enumerable.Repeat("not-match-token", 6)
            );

            // Act
            classifier.RemovePositive(Enumerable.Repeat("not-match-token", 6));
            classifier.RemoveNegative(Enumerable.Repeat("match-token", 6));

            // Assert
            classifier.NotMatch.TokenFrequency.Should().NotContainKey("not-match-token");
            classifier.Match.TokenFrequency.Should().NotContainKey("match-token");
        }

        [TestMethod]
        public async Task AsyncRefreshWorkflow_RebuildsProbabilitiesAndMarksClassifierLoaded()
        {
            // Arrange
            var sharedTokens = Enumerable
                .Range(0, 48)
                .SelectMany(index => Enumerable.Repeat($"shared-{index:00}", 6))
                .ToArray();
            var notMatchTokens = Enumerable
                .Range(0, 24)
                .SelectMany(index => Enumerable.Repeat($"shared-{index:00}", 6))
                .ToArray();
            var parent = CreateLegacyClassifierGroup(sharedTokens);
            var classifier = new BayesianClassifierProbe
            {
                ParentGroup = parent,
                NotMatchCorpus = new Corpus(notMatchTokens),
            };
            var stopwatch = new SegmentStopWatch().Start();

            // Act
            await classifier.InferNegativeTokensAsync(CancellationToken.None, stopwatch);
            classifier.ProbabilityMap = null;
            await classifier.RecalcProbsAsync(CancellationToken.None, stopwatch);
            classifier.ProbabilityMap = null;
            await classifier.AfterDeserialize(CancellationToken.None, stopwatch);

            // Assert
            classifier.Match.Should().NotBeNull();
            classifier.MatchCount.Should().BeGreaterThan(0);
            classifier.NotMatchCount.Should().BeGreaterThan(0);
            classifier.Prob.Should().NotBeNullOrEmpty();
            classifier.Loaded.Should().BeTrue();
        }

        [TestMethod]
        public async Task FromTokenBaseAsync_WithValidInputs_ReturnsClassifierWithProbabilities()
        {
            // Arrange
            var parent = CreateLegacyClassifierGroup(
                "shared-1",
                "shared-1",
                "shared-1",
                "shared-1",
                "shared-1",
                "shared-1",
                "shared-2",
                "shared-2",
                "shared-2",
                "shared-2",
                "shared-2",
                "shared-2"
            );

            // Act
            var classifier = await BayesianClassifier.FromTokenBaseAsync(
                parent,
                "folder-a",
                Enumerable.Repeat("shared-1", 6),
                CancellationToken.None
            );

            // Assert
            classifier.Tag.Should().Be("folder-a");
            classifier.Parent.Should().BeSameAs(parent);
            classifier.Prob.Should().NotBeNullOrEmpty();
        }
    }

    [TestClass]
    public class ObsoleteClassifierGroup_Tests
    {
        private sealed class ClassifierGroupProbe : ClassifierGroup
        {
            public void AssignClassifiers(
                ConcurrentDictionary<string, BayesianClassifier> classifiers
            )
            {
                this.Classifiers = classifiers;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        private static ClassifierGroup CreateConfiguredGroup()
        {
            var group = new ClassifierGroup();
            group.DedicatedTokens["dedicated"] = new DedicatedToken
            {
                Token = "dedicated",
                Count = 6,
                FolderPath = "folder-a",
            };
            group.SharedTokenBase.AddOrIncrementTokens(
                Enumerable.Repeat("shared", 6).Concat(Enumerable.Repeat("other", 6))
            );
            group.ForceClassifierUpdate(
                "tag1",
                Enumerable.Repeat("shared", 6),
                Enumerable.Repeat("other", 6)
            );
            group.ForceClassifierUpdate(
                "tag2",
                Enumerable.Repeat("other", 6),
                Enumerable.Repeat("shared", 6)
            );
            return group;
        }

        [TestMethod]
        public void DefaultConstructor_InitializesEmptyClassifiers()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.Classifiers.Should().NotBeNull();
            group.Classifiers.Should().BeEmpty();
        }

        [TestMethod]
        public void SharedTokenBase_DefaultIsEmpty()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public void DedicatedTokens_DefaultIsEmpty()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.DedicatedTokens.Should().NotBeNull();
            group.DedicatedTokens.Should().BeEmpty();
        }

        [TestMethod]
        public void ForceClassifierUpdate_CreatesNewClassifier()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.ForceClassifierUpdate("tag1", new[] { "pos1", "pos2", "pos1" }, new[] { "neg1" });

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers["tag1"].Parent.Should().BeSameAs(group);
        }

        [TestMethod]
        public void AddOrUpdateClassifier_CreatesNewIfNotExists()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.AddOrUpdateClassifier("tag1", new[] { "pos1", "pos2", "pos1" }, new[] { "neg1" });

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
        }

        [TestMethod]
        public void Classify_WithTokens_ReturnsResults()
        {
            // Arrange
            var group = new ClassifierGroup();
            group.ForceClassifierUpdate(
                "tag1",
                new[] { "hello", "world", "hello" },
                new[] { "bye", "world" }
            );

            // Act
            var results = group.Classify(new[] { "hello" });

            // Assert
            results.Should().NotBeNull();
        }

        [TestMethod]
        public void TotalTokenCount_GetSet_RoundTrips()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.TotalTokenCount = 100;

            // Assert
            group.TotalTokenCount.Should().Be(100);
        }

        [TestMethod]
        public void GetReportMessage_WithCompletedItems_FormatsCorrectly()
        {
            // Arrange
            var group = new ClassifierGroup();
            var sw = new UtilitiesCS.HelperClasses.SegmentStopWatch();
            sw.Start();
            System.Threading.Thread.Sleep(10);

            // Act
            var message = group.GetReportMessage(1, 10, sw);

            // Assert
            message.Should().Contain("Completed 1 of 10");
        }

        [TestMethod]
        public void GetReportMessage_ZeroCompleted_FormatsCorrectly()
        {
            // Arrange
            var group = new ClassifierGroup();
            var sw = new UtilitiesCS.HelperClasses.SegmentStopWatch();
            sw.Start();

            // Act
            var message = group.GetReportMessage(0, 10, sw);

            // Assert
            message.Should().Be("Completed 0 of 10");
        }

        [TestMethod]
        public void SetterPaths_RoundTripExpectedState()
        {
            // Arrange
            var group = new ClassifierGroupProbe();
            var classifiers = new ConcurrentDictionary<string, BayesianClassifier>();
            var dedicated = new ConcurrentDictionary<string, DedicatedToken>();
            dedicated["dedicated"] = new DedicatedToken
            {
                Token = "dedicated",
                Count = 6,
                FolderPath = "folder-a",
            };
            Func<object, IApplicationGlobals, IEnumerable<string>> tokenizer = static (_, _) =>
                new[] { "token-a", "token-b" };
            var tokenBase = new Corpus(
                new[] { "shared", "shared", "shared", "shared", "shared", "shared" }
            );

            // Act
            group.AssignClassifiers(classifiers);
            group.DedicatedTokens = dedicated;
            group.SharedTokenBase = tokenBase;
            group.Tokenizer = tokenizer;

            // Assert
            group.Classifiers.Should().BeSameAs(classifiers);
            group.DedicatedTokens.Should().BeSameAs(dedicated);
            group.SharedTokenBase.Should().BeSameAs(tokenBase);
            group.Tokenizer.Should().BeSameAs(tokenizer);
        }

        [TestMethod]
        public void Classify_ObjectInput_UsesTokenizerDelegate()
        {
            // Arrange
            var group = CreateConfiguredGroup();
            var source = new object();
            group.Tokenizer = (input, _) =>
                ReferenceEquals(input, source)
                    ? new[] { "shared", "shared" }
                    : Array.Empty<string>();

            // Act
            var results = group.Classify(source).ToArray();

            // Assert
            results.Should().HaveCount(2);
        }

        [TestMethod]
        public void LogMetricsAndLogState_WithConfiguredGroup_DoNotThrow()
        {
            // Arrange
            var group = CreateConfiguredGroup();

            // Act
            Action logMetrics = group.LogMetrics;
            Action logState = group.LogState;

            // Assert
            logMetrics.Should().NotThrow();
            logState.Should().NotThrow();
        }

        [TestMethod]
        public void OnDeserializedMethod_WithConfiguredGroup_DoesNotThrow()
        {
            // Arrange
            var group = CreateConfiguredGroup();

            // Act
            Action act = () => group.OnDeserializedMethod(default(StreamingContext));

            // Assert
            act.Should().NotThrow();
        }
    }
}

#pragma warning restore CS0618
