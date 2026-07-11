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

        [TestMethod]
        public void Filters_SerializeProducesBareConcreteElementArray()
        {
            // Arrange — concrete FilterEntry elements (declared type == concrete type, so no $type
            // at the element root). This asserts the collection's on-disk array contract, which is
            // what F2 changes. (FilterEntry.Flags carries non-round-trippable delegate state — a
            // pre-existing FilterEntry concern outside F2 scope — so element order/value round-trip
            // is asserted separately below via a Flags-free fixture.)
            var original = new ConcurrentObservableCollection<FilterEntry>
            {
                new FilterEntry
                {
                    Name = "Newsletters",
                    Description = "bulk",
                    Folders = new List<string> { @"Inbox\News" },
                },
                new FilterEntry
                {
                    Name = "Receipts",
                    Description = "finance",
                    Folders = new List<string> { @"Inbox\Finance" },
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(original, AutoSettings);

            // Assert — bare array shape with no root $type object wrapper.
            json.TrimStart().Should().StartWith("[");
            json.TrimStart().Should().NotStartWith("{");
        }

        [TestMethod]
        public void Filters_DeserializesBareArrayFixture_PreservingElementOrderAndValues()
        {
            // Arrange — a bare JSON array of FilterEntry-shaped objects (Flags omitted so the fixture
            // stays on the round-trippable subset). This proves the clean collection reads the
            // historical bare-array Filters shape with element order/values intact.
            const string fixture =
                "[{\"Name\":\"Newsletters\",\"Description\":\"bulk\",\"Folders\":[\"Inbox\\\\News\"]},"
                + "{\"Name\":\"Receipts\",\"Description\":\"finance\",\"Folders\":[\"Inbox\\\\Finance\"]}]";

            // Act
            var restored = JsonConvert.DeserializeObject<
                ConcurrentObservableCollection<FilterEntry>
            >(fixture, AutoSettings);

            // Assert
            restored.Should().HaveCount(2);
            restored[0].Name.Should().Be("Newsletters");
            restored[0].Folders.Should().ContainSingle().Which.Should().Be(@"Inbox\News");
            restored[1].Name.Should().Be("Receipts");
        }

        [TestMethod]
        public void PrefixList_RoundTrips_AsPolymorphicElementArray_WithTypeMetadata()
        {
            // Arrange — an interface-typed collection with concrete elements. Under
            // TypeNameHandling.Auto, Newtonsoft writes a per-element assembly-qualified $type so the
            // concrete implementation is recovered on load. The production concrete type is
            // ToDoModel.PrefixItem, which this test assembly does not reference; a local IPrefix
            // implementer models the identical polymorphic on-disk shape ($type per element).
            var original = new ConcurrentObservableCollection<IPrefix>
            {
                new TestPrefix
                {
                    PrefixType = PrefixTypeEnum.Context,
                    Key = "Context",
                    Value = "_@",
                    OlUserFieldName = "ctx",
                },
                new TestPrefix
                {
                    PrefixType = PrefixTypeEnum.Project,
                    Key = "Project",
                    Value = "Tag PROJECT",
                    OlUserFieldName = "prj",
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(original, AutoSettings);
            var restored = JsonConvert.DeserializeObject<ConcurrentObservableCollection<IPrefix>>(
                json,
                AutoSettings
            );

            // Assert — polymorphic array shape: $type present per element, order/values preserved.
            json.TrimStart().Should().StartWith("[");
            json.Should()
                .Contain("$type", "polymorphic IPrefix elements must carry $type metadata");
            restored.Should().HaveCount(2);
            restored[0].Should().BeOfType<TestPrefix>();
            restored[0].Key.Should().Be("Context");
            restored[0].Value.Should().Be("_@");
            restored[1].Key.Should().Be("Project");
            restored[1].PrefixType.Should().Be(PrefixTypeEnum.Project);
        }

        /// <summary>
        /// Local concrete <see cref="IPrefix"/> implementer used to model the polymorphic
        /// PrefixList on-disk shape. Stands in for the production <c>ToDoModel.PrefixItem</c>, which
        /// is not referenced by this test assembly.
        /// </summary>
        private sealed class TestPrefix : IPrefix
        {
            public PrefixTypeEnum PrefixType { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public Microsoft.Office.Interop.Outlook.OlCategoryColor Color { get; set; }
            public string OlUserFieldName { get; set; }
        }
    }
}
