using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class JsonSerializerExtensions_Tests
    {
        [TestMethod]
        public void DeepCopy_ForJsonSerializer_CopiesSettingsAndConverters()
        {
            // Arrange
            var converter = new StringEnumConverter();
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                Culture = CultureInfo.GetCultureInfo("fr-CA"),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                CheckAdditionalContent = true,
            };
            serializer.Converters.Add(converter);

            // Act
            var copy = serializer.DeepCopy();

            // Assert
            copy.Should().NotBeSameAs(serializer);
            copy.Formatting.Should().Be(serializer.Formatting);
            copy.Culture.Should().Be(serializer.Culture);
            copy.NullValueHandling.Should().Be(serializer.NullValueHandling);
            copy.TypeNameHandling.Should().Be(serializer.TypeNameHandling);
            copy.CheckAdditionalContent.Should().BeTrue();
            copy.Converters.Should().ContainSingle();
            copy.Converters[0].Should().BeSameAs(converter);
        }

        [TestMethod]
        public void DeepCopy_ForJsonSerializerSettings_CopiesSettingsAndConverters()
        {
            // Arrange
            var converter = new StringEnumConverter();
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Culture = CultureInfo.GetCultureInfo("en-GB"),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                CheckAdditionalContent = true,
            };
            settings.Converters.Add(converter);

            // Act
            var copy = settings.DeepCopy();

            // Assert
            copy.Should().NotBeSameAs(settings);
            copy.Formatting.Should().Be(settings.Formatting);
            copy.Culture.Should().Be(settings.Culture);
            copy.NullValueHandling.Should().Be(settings.NullValueHandling);
            copy.TypeNameHandling.Should().Be(settings.TypeNameHandling);
            copy.CheckAdditionalContent.Should().BeTrue();
            copy.Converters.Should().ContainSingle();
            copy.Converters[0].Should().BeSameAs(converter);
        }

        [TestMethod]
        public void ExtractSettings_ReturnsEquivalentSettingsAndSupportsRoundTripSerialization()
        {
            // Arrange
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
            };
            serializer.Converters.Add(new StringEnumConverter());
            var source = new SamplePayload { Name = "alpha", State = SampleState.Ready };

            // Act
            var settings = serializer.ExtractSettings();
            var json = JsonConvert.SerializeObject(source, settings);
            var roundTrip = JsonConvert.DeserializeObject<SamplePayload>(json, settings);

            // Assert
            settings.Formatting.Should().Be(serializer.Formatting);
            settings.NullValueHandling.Should().Be(serializer.NullValueHandling);
            settings.Converters.Should().BeEmpty();
            json.Should().Contain("\"State\":0");
            roundTrip.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void DeepCopyAndExtractSettings_WhenSerializerArgumentsAreNull_ThrowExpectedExceptions()
        {
            // Arrange
            JsonSerializer serializer = null;
            JsonSerializerSettings settings = null;

            // Act
            Action serializerCopyAction = () => serializer.DeepCopy();
            Action settingsCopyAction = () => settings.DeepCopy();
            Action extractAction = () => serializer.ExtractSettings();

            // Assert
            serializerCopyAction.Should().Throw<NullReferenceException>();
            settingsCopyAction.Should().Throw<NullReferenceException>();
            extractAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("serializer");
        }

        private sealed class SamplePayload
        {
            public string Name { get; set; }

            public SampleState State { get; set; }
        }

        private enum SampleState
        {
            Ready,
            Pending,
        }
    }
}
