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
            await Task.WhenAll(
                values
                    .Where(value => value % 2 == 0)
                    .Select(value => Task.Run(() => list.Remove(value)))
            );

            // Assert
            list.Count.Should().Be(15);
            list.OrderBy(value => value).Should().Equal(values.Where(value => value % 2 != 0));
        }

        [TestMethod]
        public void Config_IsNotNull()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act & Assert
            list.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Config_Set_UpdatesConfig()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var config = new UtilitiesCS.ReusableTypeClasses.NewSmartSerializableConfig();

            // Act
            list.Config = config;

            // Assert
            list.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Name_SetAndGet_Works()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            list.Name = "TestList";

            // Assert
            list.Name.Should().Be("TestList");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            list.AddLast(42);

            // Act
            list.Serialize();

            // Assert
            list.Count.Should().Be(1);
        }

        [TestMethod]
        public void DeserializeObject_ThrowsNotImplementedException()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var settings = new Newtonsoft.Json.JsonSerializerSettings();

            // Act
            System.Action act = () => list.DeserializeObject("{}", settings);

            // Assert
            act.Should().Throw<System.NotImplementedException>();
        }

        [TestMethod]
        public void PropertyChanged_RaisedOnConfigChange()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var raised = new System.Collections.Generic.List<string>();
            list.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            list.Notify("TestProp");

            // Assert
            raised.Should().Contain("TestProp");
        }

        [TestMethod]
        public void Clear_RemovesAllNodes()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act
            list.Clear();

            // Assert
            list.Count.Should().Be(0);
            list.First.Should().BeNull();
            list.Last.Should().BeNull();
        }

        [TestMethod]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act & Assert
            list.Contains(2).Should().BeTrue();
        }

        [TestMethod]
        public void Contains_MissingItem_ReturnsFalse()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act & Assert
            list.Contains(99).Should().BeFalse();
        }
    }
}
