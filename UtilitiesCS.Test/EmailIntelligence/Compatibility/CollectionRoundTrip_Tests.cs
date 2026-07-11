using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace UtilitiesCS.Test.EmailIntelligence.Compatibility
{
    /// <summary>
    /// On-disk JSON round-trip compatibility tests for the persisted collections migrated in F2.
    /// Each test proves the clean replacement type reads and writes the historical bare-JSON-array
    /// shape (elements in order; concrete-element arrays carry no <c>$type</c>; polymorphic-element
    /// arrays carry a per-element assembly-qualified <c>$type</c>). Fixtures are in-memory; no temp
    /// files are used.
    /// </summary>
    [TestClass]
    public class CollectionRoundTrip_Tests
    {
        private static readonly JsonSerializerSettings AutoSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        [TestMethod]
        public void CtfMap_RoundTrips_AsBareConcreteElementArray()
        {
            // Arrange — concrete CtfMapEntry elements (no polymorphism, so no $type expected).
            var original = new ConcurrentObservableCollection<CtfMapEntry>
            {
                new CtfMapEntry("inbox", "conv-1", 3),
                new CtfMapEntry("archive", "conv-2", 7),
            };

            // Act
            var json = JsonConvert.SerializeObject(original, AutoSettings);
            var restored = JsonConvert.DeserializeObject<
                ConcurrentObservableCollection<CtfMapEntry>
            >(json, AutoSettings);

            // Assert — bare array shape, no $type, and element order/values preserved.
            json.TrimStart().Should().StartWith("[");
            json.Should().NotContain("$type");
            restored.Should().HaveCount(2);
            restored[0].ConversationID.Should().Be("conv-1");
            restored[0].EmailCount.Should().Be(3);
            restored[1].EmailFolder.Should().Be("archive");
            restored[1].EmailCount.Should().Be(7);
        }

        [TestMethod]
        public void SubjectMapSco_RoundTrips_AsBareConcreteElementArray()
        {
            // Arrange — concrete SubjectMapEntry elements.
            var commonWords = new SerializableList<string>(new List<string>());
            var original = new ConcurrentObservableCollection<SubjectMapEntry>
            {
                new SubjectMapEntry("inbox", "weekly report", 2, commonWords),
                new SubjectMapEntry("clients", "invoice 42", 5, commonWords),
            };

            // Act
            var json = JsonConvert.SerializeObject(original, AutoSettings);
            var restored = JsonConvert.DeserializeObject<
                ConcurrentObservableCollection<SubjectMapEntry>
            >(json, AutoSettings);

            // Assert — bare array shape and element order/values preserved.
            json.TrimStart().Should().StartWith("[");
            restored.Should().HaveCount(2);
            restored[0].EmailSubject.Should().Be("weekly report");
            restored[0].EmailSubjectCount.Should().Be(2);
            restored[1].Folderpath.Should().Be("clients");
            restored[1].EmailSubjectCount.Should().Be(5);
        }
    }
}
