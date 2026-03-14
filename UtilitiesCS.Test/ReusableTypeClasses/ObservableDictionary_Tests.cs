using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ObservableDictionary_Tests
    {
        [TestMethod]
        public void NewDictionary_StartsEmptyWithNoKeysOrValues()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>();

            // Assert
            dictionary.Count.Should().Be(0);
            dictionary.Keys.Should().BeEmpty();
            dictionary.Values.Should().BeEmpty();
        }

        [TestMethod]
        public void Add_RaisesAddNotificationAndSupportsTryGetValue()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs lastArgs = null;
            dictionary.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            dictionary.Add("alpha", 1);
            var found = dictionary.TryGetValue("alpha", out var value);

            // Assert
            found.Should().BeTrue();
            value.Should().Be(1);
            dictionary["alpha"].Should().Be(1);
            lastArgs.Should().NotBeNull();
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Add);
        }

        [TestMethod]
        public void IndexerSettingExistingKey_UpdatesValueAndRaisesReplaceNotification()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>
            {
                ["alpha"] = 1,
            };
            NotifyCollectionChangedEventArgs lastArgs = null;
            dictionary.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            dictionary["alpha"] = 5;

            // Assert
            dictionary["alpha"].Should().Be(5);
            lastArgs.Should().NotBeNull();
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Replace);
        }

        [TestMethod]
        public void IndexerSettingMissingKey_AddsEntryAndRaisesAddNotification()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs lastArgs = null;
            dictionary.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            dictionary["beta"] = 2;

            // Assert
            dictionary.Count.Should().Be(1);
            dictionary["beta"].Should().Be(2);
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Add);
        }

        [TestMethod]
        public void Remove_ExistingAndMissingKeys_ReturnExpectedResultsAndNotification()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
            };
            NotifyCollectionChangedEventArgs lastArgs = null;
            dictionary.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            var removedExisting = dictionary.Remove("alpha");
            var removedMissing = dictionary.Remove("missing");

            // Assert
            removedExisting.Should().BeTrue();
            removedMissing.Should().BeFalse();
            dictionary.Keys.Should().Equal("beta");
            lastArgs.Should().NotBeNull();
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Remove);
        }

        [TestMethod]
        public void Clear_RemovesAllEntriesAndRaisesResetNotification()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
            };
            NotifyCollectionChangedEventArgs lastArgs = null;
            dictionary.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            dictionary.Clear();

            // Assert
            dictionary.Should().BeEmpty();
            lastArgs.Should().NotBeNull();
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        [TestMethod]
        public void Add_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("alpha", 1);
            Action act = () => dictionary.Add("alpha", 2);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
