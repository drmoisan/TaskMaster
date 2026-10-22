using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;

namespace ToDoModel.Test
{
    /// <summary>
    /// Unit tests for <see cref="IDList.GetNextToDoID(string)"/> and the settings-store seam it
    /// persists through. Every list under test is built with an injected per-test
    /// <see cref="Mock{ISettingsStore}"/>, so <c>MaxLengthOfID</c> persistence is observed on the
    /// mock and never reaches the per-host settings file. No live Outlook is required, and no
    /// <c>Filepath</c> is set, so the internal <c>Serialize()</c> branch is not taken.
    /// </summary>
    [TestClass]
    public class IDListGetNextToDoIDTests
    {
        private const string MaxLengthKey = "MaxLengthOfID";

        [TestMethod]
        public void GetNextToDoID_NoCollision_ReturnsNextBase36ValueAndAddsIt()
        {
            // Arrange: seed "0001" => base10 1; next id is 2 => base36 padded "02".
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string> { "ZZZZ" }, store.Object);

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("02", "the next base-36 value after seed 1 is 2 => \"02\"");
            list.Should().Contain("02", "the produced id is appended to the list");
        }

        [TestMethod]
        public void GetNextToDoID_IdAlreadyPresent_AdvancesPastTheCollision()
        {
            // Arrange: seed "0001" => next candidate "02" collides; loop advances to "03".
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string> { "02" }, store.Object);

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("03", "the increment loop skips the colliding \"02\"");
            list.Should().Contain("03");
        }

        [TestMethod]
        public void GetNextToDoID_MultipleConsecutiveCollisions_AdvancesToFirstFreeValue()
        {
            // Arrange: candidates "02","03","04" all present; first free is "05".
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string> { "02", "03", "04" }, store.Object);

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("05", "the loop advances past every consecutive collision");
        }

        [TestMethod]
        public void GetNextToDoID_LengthBoundaryRollover_ProducesLongerId()
        {
            // Arrange: seed "ZZ" => base10 (35*36 + 35) = 1295; next is 1296 => base36 "100",
            // padded to even length "0100".
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string> { "0000" }, store.Object);

            // Act
            var result = list.GetNextToDoID("ZZ");

            // Assert
            result
                .Should()
                .Be("0100", "1296 in base-36 is \"100\", padded to even length \"0100\"");
            result.Length.Should().Be(4, "the id rolled over to a longer, even-padded string");
        }

        [TestMethod]
        public void GetNextToDoID_NullSeed_ThrowsArgumentException()
        {
            // Arrange
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string>(), store.Object);

            // Act
            Action act = () => list.GetNextToDoID(null);

            // Assert: ThrowIfNullOrEmpty raises an ArgumentException-family exception for null.
            act.Should()
                .Throw<ArgumentException>(
                    "GetNextToDoID guards against a null seed via ThrowIfNullOrEmpty"
                );
            store.Verify(s => s.Save(), Times.Never());
        }

        [TestMethod]
        public void GetNextToDoID_EmptySeed_ThrowsArgumentException()
        {
            // Arrange
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string>(), store.Object);

            // Act
            Action act = () => list.GetNextToDoID(string.Empty);

            // Assert
            act.Should()
                .Throw<ArgumentException>(
                    "GetNextToDoID guards against an empty seed via ThrowIfNullOrEmpty"
                );
            store.Verify(s => s.Save(), Times.Never());
        }

        [TestMethod]
        public void GetNextToDoID_FirstIdOnFreshList_AssignsLengthAndSavesOnce()
        {
            // Arrange: a fresh list caches a maximum length of zero, so its first id persists.
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string>(), store.Object);

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("02");
            store.VerifySet(s => s[MaxLengthKey] = 2, Times.Once());
            store.Verify(s => s.Save(), Times.Once());
        }

        [TestMethod]
        public void GetNextToDoID_IdNotLongerThanCachedLength_DoesNotSaveAgain()
        {
            // Arrange: the first id "0100" caches length 4 and saves once.
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string>(), store.Object);
            list.GetNextToDoID("ZZ").Should().Be("0100");

            // Act: the next id "02" is shorter than the cached length.
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("02");
            store.VerifySet(s => s[MaxLengthKey] = 2, Times.Never());
            store.Verify(s => s.Save(), Times.Once(), "a shorter id does not save");
        }

        [TestMethod]
        public void GetNextToDoID_LongerIdAfterFirst_SavesAgain()
        {
            // Arrange: the first id "02" caches length 2 and saves once.
            var store = new Mock<ISettingsStore>();
            var list = new IDList(new List<string>(), store.Object);
            list.GetNextToDoID("0001").Should().Be("02");

            // Act: the next id "0100" is longer than the cached length.
            var result = list.GetNextToDoID("ZZ");

            // Assert
            result.Should().Be("0100");
            store.VerifySet(s => s[MaxLengthKey] = 4, Times.Once());
            store.Verify(s => s.Save(), Times.Exactly(2), "a longer id saves again");
        }

        [TestMethod]
        public void Constructor_EnumerableWithNullStore_ThrowsArgumentNullException()
        {
            // Arrange
            Action act = () => new IDList(new List<string>(), null);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("settingsStore");
        }

        [TestMethod]
        public void Constructor_NullStoreOnly_ThrowsArgumentNullException()
        {
            // Arrange
            Action act = () => new IDList((ISettingsStore)null);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("settingsStore");
        }

        [TestMethod]
        public void Constructor_StoreOnly_SavesThroughInjectedStore()
        {
            // Arrange
            var store = new Mock<ISettingsStore>();
            var list = new IDList(store.Object);

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("02");
            list.Should().ContainSingle().Which.Should().Be("02");
            store.VerifySet(s => s[MaxLengthKey] = 2, Times.Once());
            store.Verify(s => s.Save(), Times.Once());
        }

        [TestMethod]
        public void Constructor_Default_UsesApplicationSettingsStore()
        {
            // Arrange
            var field = typeof(IDList).GetField(
                "_settingsStore",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act: read the default store without reading or writing any setting.
            var store = field.GetValue(new IDList());

            // Assert
            store
                .Should()
                .BeOfType<ApplicationSettingsStore>(
                    "existing constructors persist through ToDoModel's own settings"
                );
        }
    }
}
