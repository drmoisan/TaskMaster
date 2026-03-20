using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoCollection_Tests
    {
        [TestMethod]
        public void DefaultConstructor_StartsEmpty()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            var items = collection.ToArray();

            // Assert
            collection.Count.Should().Be(0);
            items.Should().BeEmpty();
        }

        [TestMethod]
        public void AddRemoveAndClear_UpdateCollectionContents()
        {
            // Arrange
            var collection = new ScoCollection<string>();

            // Act
            collection.Add("alpha");
            collection.Add("beta");
            var removed = collection.Remove("alpha");
            var afterRemove = collection.ToArray();
            collection.Clear();

            // Assert
            removed.Should().BeTrue();
            afterRemove.Should().Equal("beta");
            collection.Count.Should().Be(0);
        }

        [TestMethod]
        public void SingleItemCollection_SupportsIndexingAndEnumeration()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.Add(42);

            // Assert
            collection.Count.Should().Be(1);
            collection[0].Should().Be(42);
            collection.Should().Equal(42);
        }

        [TestMethod]
        public void EnumerableConstructor_PopulatesCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            var snapshot = collection.ToList();

            // Assert
            snapshot.Should().Equal(1, 2, 3);
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public async Task ConcurrentAddAndRemove_LeaveExpectedFinalSet()
        {
            // Arrange
            var collection = new ScoCollection<int>(Enumerable.Range(1, 50));
            var addTasks = Enumerable
                .Range(51, 25)
                .Select(value => Task.Run(() => collection.Add(value)));
            var removeTasks = Enumerable
                .Range(1, 25)
                .Select(value => Task.Run(() => collection.Remove(value)));

            // Act
            await Task.WhenAll(addTasks.Concat(removeTasks));
            var ordered = collection.OrderBy(value => value).ToArray();

            // Assert
            collection.Count.Should().Be(50);
            ordered.Should().Equal(Enumerable.Range(26, 50));
        }

        [TestMethod]
        public void ByteArrayConstructor_DeserializesFromJson()
        {
            // Arrange
            var json = "[1, 2, 3]";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Act
            var collection = new ScoCollection<int>(bytes);

            // Assert
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void FileName_SetAndGet_Works()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.FileName = "test.json";

            // Assert
            collection.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void FolderPath_SetAndGet_Works()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.FolderPath = @"C:\data";

            // Assert
            collection.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            collection.Add(42);

            // Act
            collection.Serialize();

            // Assert
            collection.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesItems()
        {
            // Arrange
            var original = new ScoCollection<string>(new[] { "a", "b", "c" });
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoCollection<string>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act & Assert
            collection.Contains(2).Should().BeTrue();
        }

        [TestMethod]
        public void Contains_MissingItem_ReturnsFalse()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act & Assert
            collection.Contains(99).Should().BeFalse();
        }

        [TestMethod]
        public void IndexOf_ExistingItem_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 10, 20, 30 });

            // Act
            var index = collection.IndexOf(20);

            // Assert
            index.Should().Be(1);
        }

        [TestMethod]
        public void CopyTo_CopiesAllItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });
            var array = new int[5];

            // Act
            collection.CopyTo(array, 1);

            // Assert
            array.Should().Equal(0, 1, 2, 3, 0);
        }

        [TestMethod]
        public void Insert_AtIndex_ShiftsExistingItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 3 });

            // Act
            collection.Insert(1, 2);

            // Assert
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void RemoveAt_RemovesCorrectItem()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            collection.RemoveAt(1);

            // Assert
            collection.Should().Equal(1, 3);
        }

        [TestMethod]
        public void IsReadOnly_ReturnsFalse()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act & Assert
            collection.IsReadOnly.Should().BeFalse();
        }
    }
}
