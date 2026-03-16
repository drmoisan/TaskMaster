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
            var addTasks = Enumerable.Range(51, 25)
                .Select(value => Task.Run(() => collection.Add(value)));
            var removeTasks = Enumerable.Range(1, 25)
                .Select(value => Task.Run(() => collection.Remove(value)));

            // Act
            await Task.WhenAll(addTasks.Concat(removeTasks));
            var ordered = collection.OrderBy(value => value).ToArray();

            // Assert
            collection.Count.Should().Be(50);
            ordered.Should().Equal(Enumerable.Range(26, 50));
        }
    }
}