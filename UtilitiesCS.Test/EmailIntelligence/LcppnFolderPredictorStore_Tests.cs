using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// AC23 persistence tests for the dedicated LCPPN store. Confirms the predictor is configured to
    /// serialize to its OWN file (LcppnFolder.json), distinct from the flat Folder.json, in the
    /// AppData/Bayesian directory, and that it round-trips losslessly through the in-memory
    /// <see cref="SmartSerializable{T}"/> seam with no temporary files.
    /// </summary>
    [TestClass]
    public class LcppnFolderPredictorStore_Tests
    {
        private const string AppData = @"C:\Users\test\AppData";

        // AC23: the dedicated file name is a single named constant distinct from Folder.json.
        [TestMethod]
        public void FileName_IsDedicatedAndDistinctFromFolderJson()
        {
            // Assert
            LcppnFolderPredictorStore.FileName.Should().Be("LcppnFolder.json");
            LcppnFolderPredictorStore
                .FileName.Should()
                .NotBe("Folder.json", "the predictor must not collide with the flat group file");
        }

        // AC23: BuildConfig targets the dedicated file inside AppData/Bayesian.
        [TestMethod]
        public void BuildConfig_TargetsDedicatedFileInBayesianFolder()
        {
            // Act
            NewSmartSerializableConfig config = LcppnFolderPredictorStore.BuildConfig(AppData);

            // Assert
            config.Should().NotBeNull();
            config.Disk.FileName.Should().Be("LcppnFolder.json");
            config
                .Disk.FolderPath.Should()
                .Be(Path.Combine(AppData, LcppnFolderPredictorStore.BayesianSubFolder));
            config.Disk.FilePath.Should().Be(Path.Combine(AppData, "Bayesian", "LcppnFolder.json"));
        }

        // AC23: BuildConfig fails fast on a null/empty AppData folder rather than producing a bad path.
        [TestMethod]
        public void BuildConfig_NullOrEmptyAppData_Throws()
        {
            // Act / Assert
            var actNull = () => LcppnFolderPredictorStore.BuildConfig(null);
            var actEmpty = () => LcppnFolderPredictorStore.BuildConfig("");
            actNull.Should().Throw<System.Exception>();
            actEmpty.Should().Throw<System.Exception>();
        }

        // AC23: a predictor configured with the dedicated store config round-trips losslessly through
        // the exact production serialize/load settings, and the configured file name is the dedicated
        // name (not Folder.json). In-memory seam, no temp files.
        [TestMethod]
        public void RoundTrip_WithDedicatedConfig_PreservesContentAndFileName()
        {
            // Arrange
            var source = new LcppnFolderPredictor
            {
                Version = 1,
                BeamWidth = 3,
                MinimumPathProbability = 0.2,
                ShrinkageLambda = 0.6,
                MinColdStartExamples = 1,
            };
            source.Train(@"Projects\Alpha", new[] { "alpha", "spec" }, 1);
            source.Train(@"Clients\Acme", new[] { "invoice" }, 1);

            // Use the production store config (dedicated file name + the shared serialize/load
            // settings) so the test exercises exactly what the build/load paths use.
            source.Config = LcppnFolderPredictorStore.BuildConfig(AppData);

            // Assert: dedicated file name is configured (not Folder.json).
            source.Config.Disk.FileName.Should().Be("LcppnFolder.json");
            source.Config.Disk.FileName.Should().NotBe("Folder.json");

            // Act: serialize in-memory with the store settings and deserialize back with them.
            var settings = LcppnFolderPredictorStore.BuildSettings();
            var json = source.SerializeToString();
            var restored = new LcppnFolderPredictor().DeserializeObject(json, settings);

            // Assert: the runtime-only Config is excluded from the document (so the fragile Disk does
            // not break deserialization), yet the predictor content round-trips losslessly.
            json.Should().NotBeNullOrEmpty();
            json.Should()
                .NotContain("\"Disk\"", "the runtime Config is excluded from the document");
            restored.Should().NotBeNull();
            restored.Version.Should().Be(source.Version);
            restored.BeamWidth.Should().Be(source.BeamWidth);
            restored.Nodes.Keys.Should().BeEquivalentTo(source.Nodes.Keys);
        }
    }
}
