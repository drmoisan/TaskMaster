using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Serialization round-trip tests for <see cref="LcppnFolderPredictor"/> (AC15). All
    /// round-trips are in-memory via <see cref="SmartSerializable{T}.SerializeToString"/> and
    /// <see cref="SmartSerializable{T}.DeserializeObject"/>; no temporary files are used. The tests
    /// confirm that <c>Version</c>, the per-parent tree, and counts are preserved, that the
    /// serialized document is a single inline-<c>Corpus</c> JSON (no <c>CorpusInherit</c> side
    /// files), and that an empty tree round-trips cleanly.
    /// </summary>
    [TestClass]
    public class LcppnFolderPredictor_Serialization_Tests
    {
        private static LcppnFolderPredictor CreateTrainedPredictor()
        {
            var predictor = new LcppnFolderPredictor
            {
                Version = 1,
                BeamWidth = 4,
                MinimumPathProbability = 0.42,
                ShrinkageLambda = 0.6,
                MinColdStartExamples = 2,
            };
            predictor.Train(@"Projects\Alpha\2024", new[] { "alpha", "spec" }, 1);
            predictor.Train(@"Projects\Beta", new[] { "beta" }, 1);
            predictor.Train(@"Clients\Acme", new[] { "invoice" }, 1);
            return predictor;
        }

        // Settings mirror the established Bayesian serialization convention: TypeNameHandling.Auto
        // plus PreserveReferencesHandling.Objects, which is required because the reused
        // BayesianClassifierShared subtree holds a back-reference to its parent group.
        private static JsonSerializerSettings Settings()
        {
            var settings = SmartSerializable<LcppnFolderPredictor>.GetDefaultSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            return settings;
        }

        private static LcppnFolderPredictor RoundTrip(LcppnFolderPredictor source)
        {
            var settings = Settings();
            source.Config.JsonSettings = settings;
            var json = source.SerializeToString();
            json.Should().NotBeNullOrEmpty();
            return new LcppnFolderPredictor().DeserializeObject(json, settings);
        }

        // AC15: top-level scalars and Version survive the round-trip.
        [TestMethod]
        public void RoundTrip_PreservesVersionAndTopLevelScalars()
        {
            // Arrange
            var source = CreateTrainedPredictor();

            // Act
            var restored = RoundTrip(source);

            // Assert
            restored.Should().NotBeNull();
            restored.Version.Should().Be(source.Version);
            restored.BeamWidth.Should().Be(source.BeamWidth);
            restored.MinimumPathProbability.Should().Be(source.MinimumPathProbability);
            restored.ShrinkageLambda.Should().Be(source.ShrinkageLambda);
            restored.MinColdStartExamples.Should().Be(source.MinColdStartExamples);
        }

        // AC15: the per-parent tree (nodes and their child segments) survives the round-trip.
        [TestMethod]
        public void RoundTrip_PreservesPerParentTreeStructure()
        {
            // Arrange
            var source = CreateTrainedPredictor();

            // Act
            var restored = RoundTrip(source);

            // Assert: same node keys and same child segments per node
            restored.Nodes.Keys.OrderBy(k => k).Should().Equal(source.Nodes.Keys.OrderBy(k => k));
            foreach (var key in source.Nodes.Keys)
            {
                restored
                    .Nodes[key]
                    .ChildSegments.OrderBy(s => s)
                    .Should()
                    .Equal(
                        source.Nodes[key].ChildSegments.OrderBy(s => s),
                        $"node '{key}' children must round-trip"
                    );
            }

            // The derived tree is rebuilt on deserialization and is usable.
            restored.Tree.GetChildren("Projects").Should().BeEquivalentTo("Alpha", "Beta");
        }

        // AC15: per-child training counts survive the round-trip.
        [TestMethod]
        public void RoundTrip_PreservesCounts()
        {
            // Arrange
            var source = CreateTrainedPredictor();

            // Act
            var restored = RoundTrip(source);

            // Assert: total examples per node match
            foreach (var key in source.Nodes.Keys)
            {
                restored
                    .Nodes[key]
                    .TotalExamples.Should()
                    .Be(source.Nodes[key].TotalExamples, $"node '{key}' total examples");
            }

            // A restored predictor classifies identically to the source for the same query.
            var query = new[] { "alpha", "spec" };
            source.MinimumPathProbability = 0.001;
            restored.MinimumPathProbability = 0.001;
            var restoredTop = restored.Classify(query).ToArray();
            var sourceTop = source.Classify(query).ToArray();
            restoredTop.Select(p => p.Class).Should().Equal(sourceTop.Select(p => p.Class));
        }

        // AC15: the serialized document is a single JSON document with inline Corpus and no
        // CorpusInherit reference.
        [TestMethod]
        public void Serialize_ProducesInlineCorpus_WithNoCorpusInherit()
        {
            // Arrange
            var source = CreateTrainedPredictor();
            source.Config.JsonSettings = Settings();

            // Act
            var json = source.SerializeToString();

            // Assert
            json.Should().Contain("\"Nodes\"", "the serialized shape exposes a Nodes map");
            json.Should().Contain("\"Version\"");
            json.Should().Contain("\"SharedTokenBase\"", "Corpus is serialized inline");
            json.Should()
                .NotContain(
                    "CorpusInherit",
                    "inline Corpus must not produce CorpusInherit side files"
                );
        }

        // AC15: an empty tree serializes and deserializes cleanly.
        [TestMethod]
        public void RoundTrip_EmptyPredictor_Succeeds()
        {
            // Arrange
            var source = new LcppnFolderPredictor { Version = 1 };

            // Act
            var restored = RoundTrip(source);

            // Assert
            restored.Should().NotBeNull();
            restored.Version.Should().Be(1);
            restored.Nodes.Should().BeEmpty();
            restored.Classify(new[] { "anything" }).ToArray().Should().BeEmpty();
        }

        // The serialized JSON is valid (parses) and round-trips through a plain JToken without loss
        // of the top-level keys.
        [TestMethod]
        public void Serialize_ProducesParseableJson()
        {
            // Arrange
            var source = CreateTrainedPredictor();
            source.Config.JsonSettings = Settings();

            // Act
            var json = source.SerializeToString();
            var token = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);

            // Assert
            token.Should().NotBeNull();
            token!["Version"].Should().NotBeNull();
            token["BeamWidth"].Should().NotBeNull();
            token["MinimumPathProbability"].Should().NotBeNull();
            token["Nodes"].Should().NotBeNull();
        }
    }
}
