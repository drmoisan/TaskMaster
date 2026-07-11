using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.ReusableTypeClasses.SerializableNew.Concurrent.Observable
{
    /// <summary>
    /// Unit tests for the vendored-dependency-free <see cref="SloStack{T}"/>. Ports the positional-semantics
    /// assertions from the legacy <c>ScoStack_Tests</c>: top-of-stack == index 0 == front, the
    /// indexed <c>Peek</c>/<c>Pop</c> members (including shift-down of higher indices), the
    /// <c>TryPeek</c>/<c>TryPop</c> success/failure paths, empty-stack throw behavior, the async
    /// serialize wrapper, and a bare JSON-array round-trip. No temp files are used.
    /// </summary>
    [TestClass]
    public class SloStack_Tests
    {
        private static readonly JsonSerializerSettings AutoSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        private static SloStack<string> BuildStack(params string[] topToBottom)
        {
            // The first argument is the intended top-of-stack. Push in reverse so the last Push
            // (the first argument) ends up at the front (index 0).
            var stack = new SloStack<string>();
            for (int i = topToBottom.Length - 1; i >= 0; i--)
            {
                stack.Push(topToBottom[i]);
            }
            return stack;
        }

        #region push / pop / peek front semantics

        [TestMethod]
        public void Push_PlacesItemAtFront_TopIsIndexZero()
        {
            var stack = new SloStack<int>();

            stack.Push(1);
            stack.Push(2);

            stack[0].Should().Be(2, "the most recently pushed item is the top == index 0");
            stack.Peek().Should().Be(2);
            stack.Count.Should().Be(2);
        }

        [TestMethod]
        public void Pop_RemovesAndReturnsTop()
        {
            var stack = BuildStack("top", "mid", "bottom");

            stack.Pop().Should().Be("top");
            stack.Peek().Should().Be("mid");
            stack.Count.Should().Be(2);
        }

        [TestMethod]
        public void Peek_ReturnsTopWithoutRemoving()
        {
            var stack = BuildStack("top", "mid");

            stack.Peek().Should().Be("top");
            stack.Count.Should().Be(2);
        }

        [TestMethod]
        public void Pop_OnEmptyStack_ThrowsInvalidOperationException()
        {
            var stack = new SloStack<int>();

            Action act = () => stack.Pop();

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Peek_OnEmptyStack_ThrowsInvalidOperationException()
        {
            var stack = new SloStack<int>();

            Action act = () => stack.Peek();

            act.Should().Throw<InvalidOperationException>();
        }

        #endregion push / pop / peek front semantics

        #region indexed access

        [TestMethod]
        public void Indexer_ReturnsElementAtOrdinal_TopIsZero()
        {
            var stack = BuildStack("a", "b", "c");

            stack[0].Should().Be("a");
            stack[1].Should().Be("b");
            stack[2].Should().Be("c");
        }

        [TestMethod]
        public void PeekInt_ReturnsElementAtOrdinal()
        {
            var stack = BuildStack("a", "b", "c");

            stack.Peek(1).Should().Be("b");
        }

        [TestMethod]
        public void PeekInt_OutOfRange_ThrowsIndexOutOfRange()
        {
            var stack = BuildStack("a", "b");

            Action act = () => stack.Peek(5);

            act.Should().Throw<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void PopInt_RemovesAtOrdinal_AndShiftsHigherIndicesDown()
        {
            var stack = BuildStack("a", "b", "c", "d");

            var removed = stack.Pop(1);

            removed.Should().Be("b");
            stack.Count.Should().Be(3);
            // Higher indices shift down: what was index 2 ("c") is now index 1.
            stack[0].Should().Be("a");
            stack[1].Should().Be("c");
            stack[2].Should().Be("d");
        }

        [TestMethod]
        public void PopInt_OutOfRange_ThrowsIndexOutOfRange()
        {
            var stack = BuildStack("a");

            Action act = () => stack.Pop(3);

            act.Should().Throw<IndexOutOfRangeException>();
        }

        #endregion indexed access

        #region TryPeek / TryPop

        [TestMethod]
        public void TryPeek_Front_SuccessAndFailure()
        {
            var stack = BuildStack("top");

            stack.TryPeek(out var value).Should().BeTrue();
            value.Should().Be("top");

            var empty = new SloStack<string>();
            empty.TryPeek(out var missing).Should().BeFalse();
            missing.Should().BeNull();
        }

        [TestMethod]
        public void TryPeek_Indexed_SuccessAndFailure()
        {
            var stack = BuildStack("a", "b");

            stack.TryPeek(out var value, 1).Should().BeTrue();
            value.Should().Be("b");

            stack.TryPeek(out var missing, 9).Should().BeFalse();
            missing.Should().BeNull();
        }

        [TestMethod]
        public void TryPop_Front_SuccessAndFailure()
        {
            var stack = BuildStack("top", "next");

            stack.TryPop(out var value).Should().BeTrue();
            value.Should().Be("top");
            stack.Count.Should().Be(1);

            var empty = new SloStack<string>();
            empty.TryPop(out var missing).Should().BeFalse();
            missing.Should().BeNull();
        }

        [TestMethod]
        public void TryPop_Indexed_SuccessAndFailure_ShiftsHigherDown()
        {
            var stack = BuildStack("a", "b", "c");

            stack.TryPop(out var value, 1).Should().BeTrue();
            value.Should().Be("b");
            stack.Count.Should().Be(2);
            stack[1].Should().Be("c");

            stack.TryPop(out var missing, 9).Should().BeFalse();
            missing.Should().BeNull();
        }

        #endregion TryPeek / TryPop

        #region serialize / round-trip

        [TestMethod]
        public async Task SerializeAsync_WithNoConfiguredPath_CompletesWithoutMutation()
        {
            var stack = BuildStack("a", "b");

            await stack.SerializeAsync();

            stack[0].Should().Be("a");
            stack.Count.Should().Be(2);
        }

        [TestMethod]
        public void JsonRoundTrip_ProducesBareArray_IndexZeroIsTop()
        {
            var stack = BuildStack("top", "mid", "bottom");

            var json = JsonConvert.SerializeObject(stack, AutoSettings);
            json.TrimStart().Should().StartWith("[");

            var restored = JsonConvert.DeserializeObject<SloStack<string>>(json, AutoSettings);

            restored[0].Should().Be("top", "the array head deserializes to the top-of-stack");
            restored[1].Should().Be("mid");
            restored[2].Should().Be("bottom");
        }

        #endregion serialize / round-trip

        #region file-based deserialize path (P2-T5, injected seam, no temp files)

        [TestMethod]
        public void FileBasedDeserialize_ThroughInjectedSeam_LoadsElementsInOrder()
        {
            // The seam overrides ReadAllText/DiskExists to return an in-memory JSON array, driving
            // the exact DeserializeJson path SloStack<T>.Static.Deserialize uses — with no temp files
            // and without touching the four stubbed ISmartSerializable members.
            var seam = new SeamedStackSerializable("[10,20,30]");

            var restored = seam.Deserialize("moved.json", @"C:\staging", askUserOnError: false);

            restored.Should().NotBeNull();
            restored[0].Should().Be(10, "the array head is the top-of-stack after file load");
            restored[1].Should().Be(20);
            restored[2].Should().Be(30);
        }

        [TestMethod]
        public void StaticDeserialize_WithInvalidPath_ReturnsEmptyStack_WithoutStubbedMembers()
        {
            // A non-existent path drives the CreateEmpty branch. This proves the file-based path
            // used by LoadMovedMails runs to completion without invoking any NotImplementedException
            // stub. The illegal '*' filename keeps the deferred write from producing a temp file.
            var restored = SloStack<int>.Static.Deserialize(
                "*invalid-slostack.json",
                @"C:\nonexistent-staging",
                askUserOnError: false
            );

            restored.Should().NotBeNull();
            restored.Count.Should().Be(0);
        }

        private sealed class SeamedStackSerializable : SmartSerializable<SloStack<int>>
        {
            public SeamedStackSerializable(string json)
            {
                ReadAllText = _ => json;
                DiskExists = _ => true;
            }
        }

        #endregion file-based deserialize path

        #region ISmartSerializable delegation and stubbed-member coverage

        [TestMethod]
        public void EnumerableConstructor_PopulatesInOrder()
        {
            var stack = new SloStack<int>(new[] { 1, 2, 3 });

            stack.Count.Should().Be(3);
            stack[0].Should().Be(1);
        }

        [TestMethod]
        public void Config_GetSet_RoundTrips()
        {
            var stack = new SloStack<int>();
            var config = new NewSmartSerializableConfig();

            stack.Config = config;

            stack.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Serialize_NoConfiguredPath_IsNoOp_AndSerializeThreadSafeInvalidPathIsSwallowed()
        {
            var stack = new SloStack<int>(new[] { 1 });

            // No configured path → deferred serialize is a no-op; an invalid path is swallowed by
            // the production error handling in the inherited SmartSerializable.
            stack.Invoking(s => s.Serialize()).Should().NotThrow();
            stack
                .Invoking(s => s.Serialize(@"C:\nonexistent-slostack\file.json"))
                .Should()
                .NotThrow();
            stack
                .Invoking(s => s.SerializeThreadSafe("*invalid-slostack.json"))
                .Should()
                .NotThrow();
        }

        [TestMethod]
        public void InstanceDeserializeOverloads_WithInvalidPath_ReturnEmptyStack()
        {
            var stack = new SloStack<int>();

            var byName = stack.Deserialize("*invalid-a.json", @"C:\nonexistent-slostack");
            var byNameAsk = stack.Deserialize(
                "*invalid-b.json",
                @"C:\nonexistent-slostack",
                askUserOnError: false
            );
            var byNameSettings = stack.Deserialize(
                "*invalid-c.json",
                @"C:\nonexistent-slostack",
                askUserOnError: false,
                settings: SmartSerializable<SloStack<int>>.GetDefaultSettings()
            );

            byName.Should().NotBeNull();
            byNameAsk.Should().NotBeNull();
            byNameSettings.Should().NotBeNull();
        }

        [TestMethod]
        public async Task DeserializeAsyncOverloads_WithLoader_ReturnEmptyStack()
        {
            var stack = new SloStack<int>();
            var loader = new SmartSerializable<SloStack<int>>();

            // The simple overload returns null when the loader's file is absent; the
            // askUserOnError overload creates an empty instance. Both exercise the typed
            // DeserializeAsync delegation to the inherited SmartSerializable.
            var a = await stack.DeserializeAsync(loader);
            var b = await stack.DeserializeAsync(loader, askUserOnError: false);

            a.Should().BeNull();
            b.Should().NotBeNull();
            b.Count.Should().Be(0);
        }

        [TestMethod]
        public void StaticDeserializeOverloads_WithInvalidPath_ReturnEmptyStack()
        {
            var byName = SloStack<int>.Static.Deserialize("*a.json", @"C:\nonexistent-slostack");
            var bySettings = SloStack<int>.Static.Deserialize(
                "*c.json",
                @"C:\nonexistent-slostack",
                askUserOnError: false,
                settings: SmartSerializable<SloStack<int>>.GetDefaultSettings()
            );

            byName.Should().NotBeNull();
            bySettings.Should().NotBeNull();
        }

        [TestMethod]
        public async Task StubbedInterfaceMembers_ThrowNotImplementedException()
        {
            ISmartSerializable<SloStack<int>> stack = new SloStack<int>();
            var loader = new SmartSerializable<SloStack<int>>();

            stack.Invoking(s => s.Deserialize(loader)).Should().Throw<NotImplementedException>();
            stack
                .Invoking(s => s.Deserialize(loader, false, () => new SloStack<int>()))
                .Should()
                .Throw<NotImplementedException>();
            await stack
                .Awaiting(s => s.DeserializeAsync(loader, false, () => new SloStack<int>()))
                .Should()
                .ThrowAsync<NotImplementedException>();
            stack
                .Invoking(s => s.DeserializeObject("{}", null))
                .Should()
                .Throw<NotImplementedException>();
        }

        #endregion ISmartSerializable delegation and stubbed-member coverage
    }
}
