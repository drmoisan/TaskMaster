using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    [TestClass]
    public class TlpCellStatesTests
    {
        [TestMethod]
        public void EmptyConstructor_CreatesEmptyStateDictionary()
        {
            var states = new TlpCellStates();

            states.Should().BeEmpty();
        }

        [TestMethod]
        public void TypedCollectionConstructor_PreservesSnapshotListsByKey()
        {
            var expandedSnapshots = new TlpCellSnapShotList { CreateSnapshot("Expanded") };
            var compressedSnapshots = new TlpCellSnapShotList { CreateSnapshot("Compressed") };
            var source = new[]
            {
                new KeyValuePair<string, TlpCellSnapShotList>("expanded", expandedSnapshots),
                new KeyValuePair<string, TlpCellSnapShotList>("compressed", compressedSnapshots),
            };

            var states = new TlpCellStates(source);

            states.Should().ContainKey("expanded");
            states["expanded"].Should().BeSameAs(expandedSnapshots);
            states["compressed"].Should().BeSameAs(compressedSnapshots);
        }

        [TestMethod]
        public void RawCollectionConstructor_ConvertsListsToTlpCellSnapShotLists()
        {
            var rawSnapshots = new List<TlpCellSnapShot> { CreateSnapshot("Raw") };
            var source = new[]
            {
                new KeyValuePair<string, List<TlpCellSnapShot>>("raw", rawSnapshots),
            };

            var states = new TlpCellStates(source);

            states["raw"].Should().BeOfType<TlpCellSnapShotList>();
            states["raw"].Should().ContainSingle().Which.ControlName.Should().Be("Raw");
            states["raw"].Should().NotBeSameAs(rawSnapshots);
        }

        [TestMethod]
        public void CollectionConstructors_WithEmptyInputs_CreateEmptyStateDictionary()
        {
            var typedStates = new TlpCellStates(
                Array.Empty<KeyValuePair<string, TlpCellSnapShotList>>()
            );
            var rawStates = new TlpCellStates(
                Array.Empty<KeyValuePair<string, List<TlpCellSnapShot>>>()
            );

            typedStates.Should().BeEmpty();
            rawStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TypedCollectionConstructor_WithDuplicateKeys_ThrowsArgumentException()
        {
            var source = new[]
            {
                new KeyValuePair<string, TlpCellSnapShotList>(
                    "duplicate",
                    new TlpCellSnapShotList()
                ),
                new KeyValuePair<string, TlpCellSnapShotList>(
                    "duplicate",
                    new TlpCellSnapShotList()
                ),
            };

            Action act = () => _ = new TlpCellStates(source);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TryAddState_WithoutSnapshots_AddsOnlyMissingState()
        {
            var states = new TlpCellStates();

            bool firstAdd = states.TryAddState("normal");
            bool secondAdd = states.TryAddState("normal");

            firstAdd.Should().BeTrue();
            secondAdd.Should().BeFalse();
            states["normal"].Should().BeEmpty();
        }

        [TestMethod]
        public void TryAddState_WithSnapshots_AddsConvertedListOnlyForMissingState()
        {
            var snapshots = new List<TlpCellSnapShot> { CreateSnapshot("Button") };
            var states = new TlpCellStates();

            bool firstAdd = states.TryAddState("expanded", snapshots);
            bool secondAdd = states.TryAddState(
                "expanded",
                new List<TlpCellSnapShot> { CreateSnapshot("Other") }
            );

            firstAdd.Should().BeTrue();
            secondAdd.Should().BeFalse();
            states["expanded"].Should().ContainSingle().Which.ControlName.Should().Be("Button");
        }

        [TestMethod]
        public void TypedCollectionConstructor_WithNullInput_ThrowsArgumentNullException()
        {
            Action act = () =>
                _ = new TlpCellStates((IEnumerable<KeyValuePair<string, TlpCellSnapShotList>>)null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("collection");
        }

        [TestMethod]
        public void RawCollectionConstructor_WithNullInput_ThrowsArgumentNullException()
        {
            Action act = () =>
                _ = new TlpCellStates(
                    (IEnumerable<KeyValuePair<string, List<TlpCellSnapShot>>>)null
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("collection");
        }

        private static TlpCellSnapShot CreateSnapshot(string controlName)
        {
            return new TlpCellSnapShot { ControlName = controlName };
        }
    }
}
