using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    public partial class ClassifierGroupUtilities_Tests
    {
        [TestMethod]
        public void Deserialize_WhenStoredTextExists_ReturnsDeserializedValue()
        {
            var globals = ClassifierGroupUtilitiesTestSupport.CreateGlobalsWithAppData(
                @"C:\AppDataRoot"
            );
            var utils = new RecordingClassifierGroupUtilities(globals.Object);
            var expected = new BayesianClassifierGroup { TotalEmailCount = 12 };
            utils.StoreText(
                Path.Combine(@"C:\AppDataRoot", "Bayesian", "stored.json"),
                JsonConvert.SerializeObject(
                    expected,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented,
                    }
                )
            );

            var result = utils.Deserialize<BayesianClassifierGroup>("stored");

            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(12);
        }

        [TestMethod]
        public void Deserialize_WhenStoredTextMissing_ReturnsDefault()
        {
            var globals = ClassifierGroupUtilitiesTestSupport.CreateGlobalsWithAppData(
                @"C:\AppDataRoot"
            );
            var utils = new RecordingClassifierGroupUtilities(globals.Object);

            var result = utils.Deserialize<BayesianClassifierGroup>("missing");

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenStoredTextExists_ReturnsDeserializedValue()
        {
            var globals = ClassifierGroupUtilitiesTestSupport.CreateGlobalsWithAppData(
                @"C:\AppDataRoot"
            );
            var utils = new RecordingClassifierGroupUtilities(globals.Object)
            {
                InvokeBaseDeserializeAsync = true,
            };
            utils.StoreText(
                Path.Combine(@"C:\AppDataRoot", "Bayesian", "asyncStored.json"),
                JsonConvert.SerializeObject(
                    new BayesianClassifierGroup { TotalEmailCount = 21 },
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented,
                    }
                )
            );

            var result = await utils.DeserializeAsync<BayesianClassifierGroup>("asyncStored");

            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(21);
        }

        [TestMethod]
        public void SerializeAndSave_CoreWriterStoresJsonAndClearsFileName()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                InvokeBaseSerializeAndSaveCore = true,
            };
            var disk = new FilePathHelper("core.json", @"C:\Store");
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());

            utils.SerializeAndSave(
                new BayesianClassifierGroup { TotalEmailCount = 5 },
                serializer,
                disk
            );

            disk.FileName.Should().BeNull();
            utils.ReadStoredText(Path.Combine(@"C:\Store", "core.json")).Should().Contain("5");
        }

        [TestMethod]
        public void SerializeFsSave_WritesExampleJsonAndClearsFileName()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                InvokeBaseSerializeFsSave = true,
            };
            var disk = new FilePathHelper("ignored.json", @"C:\Store");
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());

            utils.SerializeFsSave(
                new BayesianClassifierGroup { TotalEmailCount = 6 },
                "Group",
                serializer,
                disk
            );

            disk.FileName.Should().BeNull();
            utils
                .ReadStoredText(Path.Combine(@"C:\Store", "Group_Example.json"))
                .Should()
                .Contain("6");
        }

        [TestMethod]
        public void SerializeChunk_WritesChunkJsonAndClearsFileName()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object);
            var disk = new FilePathHelper { FolderPath = @"C:\Store" };
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());

            utils.SerializeChunk([new MinedMailInfo { Tokens = ["alpha"] }], serializer, disk, 1);

            disk.FileName.Should().BeNull();
            utils
                .ReadStoredText(Path.Combine(@"C:\Store", "MinedMailInfo_001.json"))
                .Should()
                .Contain("alpha");
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializeAsyncThrowsWithoutSuffix_ReturnsFalse()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                ValidationException = new InvalidOperationException("bad json"),
            };

            var result = await utils.ValidateJson<BayesianClassifierGroup>("group");

            result.Should().BeFalse();
        }
    }
}
