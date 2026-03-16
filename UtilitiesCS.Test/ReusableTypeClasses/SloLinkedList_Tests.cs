using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SloLinkedList_Tests
    {
        [TestMethod]
        public void Constructor_WithEmptyList_ExposesNoEndpoints()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Assert
            list.Count.Should().Be(0);
            list.First.Should().BeNull();
            list.Last.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithSingleSeed_ExposesHeadAndTail()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 42 });

            // Assert
            list.Count.Should().Be(1);
            list.First.Value.Should().Be(42);
            list.Last.Value.Should().Be(42);
        }

        [TestMethod]
        public void AddFindRemoveAndEnumerate_WorkAsExpected()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            list.AddLast(2);
            list.AddFirst(1);
            list.AddLast(3);
            var found = list.Find(2);
            list.Remove(2);

            // Assert
            found.Should().NotBeNull();
            found.Value.Should().Be(2);
            list.Should().Equal(1, 3);
            list.First.Value.Should().Be(1);
            list.Last.Value.Should().Be(3);
        }

        [TestMethod]
        public async Task ConcurrentOperations_AddAndRemove_LeaveExpectedState()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var values = Enumerable.Range(1, 30).ToArray();

            // Act
            await Task.WhenAll(values.Select(value => Task.Run(() => list.AddLast(value))));
            await Task.WhenAll(values.Where(value => value % 2 == 0).Select(value => Task.Run(() => list.Remove(value))));

            // Assert
            list.Count.Should().Be(15);
            list.OrderBy(value => value).Should().Equal(values.Where(value => value % 2 != 0));
        }
    }
}
