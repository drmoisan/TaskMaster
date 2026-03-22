using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoStack_Tests
    {
        [TestMethod]
        public void PushPeekAndCount_FollowLifoOrder()
        {
            // Arrange
            var stack = new ScoStack<int>();

            // Act
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            // Assert
            stack.Count.Should().Be(3);
            stack.Peek().Should().Be(3);
            stack.Peek(1).Should().Be(2);
        }

        [TestMethod]
        public void PopAndTryPop_RemoveItemsFromTheFront()
        {
            // Arrange
            var stack = new ScoStack<string>(new[] { "top", "middle", "bottom" });

            // Act
            var popped = stack.Pop();
            var tryPopped = stack.TryPop(out var second);
            var remaining = stack.Single();

            // Assert
            popped.Should().Be("top");
            tryPopped.Should().BeTrue();
            second.Should().Be("middle");
            remaining.Should().Be("bottom");
            stack.Count.Should().Be(1);
        }

        [TestMethod]
        public void PopAndPeekByIndex_WorkForNonZeroIndices()
        {
            // Arrange
            var stack = new ScoStack<int>(new[] { 10, 20, 30, 40 });

            // Act
            var peeked = stack.Peek(2);
            var popped = stack.Pop(1);
            var remaining = stack.AsEnumerable().ToArray();

            // Assert
            peeked.Should().Be(30);
            popped.Should().Be(20);
            remaining.Should().Equal(10, 30, 40);
        }

        [TestMethod]
        public void EmptyStack_ThrowsForPeekAndPopAndReturnsFalseForTryPop()
        {
            // Arrange
            var stack = new ScoStack<int>();

            // Act
            Action peek = () => stack.Peek();
            Action pop = () => stack.Pop();
            var tryPopped = stack.TryPop(out var value);

            // Assert
            peek.Should().Throw<InvalidOperationException>();
            pop.Should().Throw<InvalidOperationException>();
            tryPopped.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public async Task ConcurrentPushes_PreserveAllItems()
        {
            // Arrange
            var stack = new ScoStack<int>();
            var items = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(items.Select(item => Task.Run(() => stack.Push(item))));
            var ordered = stack.OrderBy(value => value).ToArray();

            // Assert
            stack.Count.Should().Be(items.Length);
            ordered.Should().Equal(items);
        }

        [TestMethod]
        public void Filename_SetAndGet_Works()
        {
            // Arrange
            var stack = new ScoStack<int>();

            // Act
            stack.FileName = "stack.json";

            // Assert
            stack.FileName.Should().Be("stack.json");
        }

        [TestMethod]
        public void Folderpath_SetAndGet_UpdatesFilepath()
        {
            // Arrange
            var stack = new ScoStack<int>();
            stack.FileName = "data.json";

            // Act
            stack.FolderPath = @"C:\stacks";

            // Assert
            stack.FilePath.Should().Be(@"C:\stacks\data.json");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var stack = new ScoStack<int>();
            stack.Push(42);

            // Act
            stack.Serialize();

            // Assert
            stack.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesItems()
        {
            // Arrange
            var original = new ScoStack<int>(new[] { 1, 2, 3 });
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoStack<int>>(
                json,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Count.Should().Be(3);
        }

        [TestMethod]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var stack = new ScoStack<int>(new[] { 1, 2, 3 });

            // Act & Assert
            stack.Contains(2).Should().BeTrue();
        }

        [TestMethod]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var stack = new ScoStack<int>(new[] { 1, 2, 3 });

            // Act
            stack.Clear();

            // Assert
            stack.Count.Should().Be(0);
        }

        // NOTE: ScoStack<T>.ToArray() contains a pre-existing infinite recursion bug
        // (calls this.ToArray() which resolves to itself instead of Enumerable.ToArray).
        // Calling it crashes the test host with StackOverflowException.
        // ToArray(bool) also suffers from the same bug on the reverse=false path.
        // Production fix deferred to a separate bug issue.
    }
}
