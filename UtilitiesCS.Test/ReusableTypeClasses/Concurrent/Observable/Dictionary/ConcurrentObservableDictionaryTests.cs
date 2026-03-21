using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace ConcurrentObservableCollection.Tests
{
    [TestClass]
    public class ConcurrentObservableDictionaryTests
    {
        private ConcurrentObservableDictionary<string, int> _dictionary;

        [TestInitialize]
        public void Setup()
        {
            _dictionary = new ConcurrentObservableDictionary<string, int>();
        }

        [TestMethod]
        public void AddOrUpdate_AddsNewItem()
        {
            // Arrange
            string key = "key1";
            int value = 1;

            // Act
            _dictionary.AddOrUpdate(key, value);

            // Assert
            Assert.AreEqual(value, _dictionary[key]);
        }

        [TestMethod]
        public void AddOrUpdate_UpdatesExistingItem()
        {
            // Arrange
            string key = "key1";
            int initialValue = 1;
            int updatedValue = 2;
            _dictionary.TryAdd(key, initialValue);

            // Act
            _dictionary.AddOrUpdate(key, updatedValue);

            // Assert
            Assert.AreEqual(updatedValue, _dictionary[key]);
        }

        [TestMethod]
        public void GetOrAdd_AddsNewItem()
        {
            // Arrange
            string key = "key1";
            int value = 1;

            // Act
            var result = _dictionary.GetOrAdd(key, value);

            // Assert
            Assert.AreEqual(value, result);
            Assert.AreEqual(value, _dictionary[key]);
        }

        [TestMethod]
        public void GetOrAdd_ReturnsExistingItem()
        {
            // Arrange
            string key = "key1";
            int initialValue = 1;
            _dictionary.TryAdd(key, initialValue);

            // Act
            var result = _dictionary.GetOrAdd(key, 2);

            // Assert
            Assert.AreEqual(initialValue, result);
        }

        [TestMethod]
        public void TryAdd_AddsNewItem()
        {
            // Arrange
            string key = "key1";
            int value = 1;

            // Act
            var result = _dictionary.TryAdd(key, value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(value, _dictionary[key]);
        }

        [TestMethod]
        public void TryAdd_DoesNotAddExistingItem()
        {
            // Arrange
            string key = "key1";
            int initialValue = 1;
            _dictionary.TryAdd(key, initialValue);

            // Act
            var result = _dictionary.TryAdd(key, 2);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialValue, _dictionary[key]);
        }

        [TestMethod]
        public void TryRemove_RemovesExistingItem()
        {
            // Arrange
            string key = "key1";
            int value = 1;
            _dictionary.TryAdd(key, value);

            // Act
            var result = _dictionary.TryRemove(key, out var removedValue);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(value, removedValue);
            Assert.IsFalse(_dictionary.ContainsKey(key));
        }

        [TestMethod]
        public void TryRemove_DoesNotRemoveNonExistingItem()
        {
            // Arrange
            string key = "key1";

            // Act
            var result = _dictionary.TryRemove(key, out var removedValue);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(default(int), removedValue);
        }

        [TestMethod]
        public void TryUpdate_UpdatesExistingItem()
        {
            // Arrange
            string key = "key1";
            int initialValue = 1;
            int updatedValue = 2;
            _dictionary.TryAdd(key, initialValue);

            // Act
            var result = _dictionary.TryUpdate(key, updatedValue, initialValue);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(updatedValue, _dictionary[key]);
        }

        [TestMethod]
        public void TryUpdate_DoesNotUpdateIfComparisonFails()
        {
            // Arrange
            string key = "key1";
            int initialValue = 1;
            int updatedValue = 2;
            _dictionary.TryAdd(key, initialValue);

            // Act
            var result = _dictionary.TryUpdate(key, updatedValue, 3);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialValue, _dictionary[key]);
        }

        [TestMethod]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            _dictionary.TryAdd("key1", 1);
            _dictionary.TryAdd("key2", 2);

            // Act
            _dictionary.Clear();

            // Assert
            Assert.AreEqual(0, _dictionary.Count);
        }

        [TestMethod]
        public void AddPartialObserver_AddsObserverForKey()
        {
            // Arrange
            string key = "key1";
            var observer = new TestObserver<string, int>();

            // Act
            _dictionary.AddPartialObserver(observer, key);
            _dictionary.AddOrUpdate(key, 1);

            // Assert
            Assert.AreEqual(1, observer.ReceivedEvents.Count);
        }

        [TestMethod]
        public void ContainsKey_ReturnsTrueForExistingAndFalseForMissing()
        {
            // Arrange
            _dictionary.TryAdd("exists", 42);

            // Act / Assert
            Assert.IsTrue(_dictionary.ContainsKey("exists"));
            Assert.IsFalse(_dictionary.ContainsKey("missing"));
        }

        [TestMethod]
        public void Keys_ReturnsAllAddedKeys()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var keys = _dictionary.Keys;

            // Assert
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, keys.ToArray());
        }

        [TestMethod]
        public void Values_ReturnsAllAddedValues()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var values = _dictionary.Values;

            // Assert
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, values.ToArray());
        }

        [TestMethod]
        public void Indexer_ReturnsValueForExistingKey()
        {
            // Arrange
            _dictionary.TryAdd("key", 99);

            // Act
            var value = _dictionary["key"];

            // Assert
            Assert.AreEqual(99, value);
        }

        [TestMethod]
        public void Count_ReturnsNumberOfEntries()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act / Assert
            Assert.AreEqual(2, _dictionary.Count);
        }

        [TestMethod]
        public void GetEnumerator_IteratesOverAllEntries()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var entries = _dictionary.ToList();

            // Assert
            Assert.AreEqual(2, entries.Count);
        }

        [TestMethod]
        public void ToList_ReturnsListOfKeyValuePairs()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var list = _dictionary.ToList();

            // Assert
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void ToArray_ReturnsArrayOfKeyValuePairs()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);

            // Act
            var array = _dictionary.ToArray();

            // Assert
            Assert.AreEqual(1, array.Length);
            Assert.AreEqual("a", array[0].Key);
        }

        [TestMethod]
        public void RemovePartialObserver_RemovesObserverForKey()
        {
            // Arrange
            string key = "key1";
            var observer = new TestObserver<string, int>();
            _dictionary.AddPartialObserver(observer, key);

            // Act
            _dictionary.RemovePartialObserver(observer, key);
            _dictionary.AddOrUpdate(key, 1);

            // Assert
            Assert.AreEqual(0, observer.ReceivedEvents.Count);
        }

        [TestMethod]
        public void RemoveAllObservers_RemovesAllObservers()
        {
            // Arrange
            string key = "key1";
            var observer = new TestObserver<string, int>();
            _dictionary.AddPartialObserver(observer, key);

            // Act
            _dictionary.RemoveAllObservers();
            _dictionary.AddOrUpdate(key, 1);

            // Assert
            Assert.AreEqual(0, observer.ReceivedEvents.Count);
        }

        [TestMethod]
        public void ContainsKey_WhenKeyExists_ReturnsTrue()
        {
            // Arrange
            _dictionary.TryAdd("x", 9);

            // Act / Assert
            _dictionary.ContainsKey("x").Should().BeTrue();
            _dictionary.ContainsKey("missing").Should().BeFalse();
        }

        [TestMethod]
        public void Keys_ReturnsAllKeys()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var keys = _dictionary.Keys;

            // Assert
            keys.Should().BeEquivalentTo(new[] { "a", "b" });
        }

        [TestMethod]
        public void Values_ReturnsAllValues()
        {
            // Arrange
            _dictionary.TryAdd("a", 1);
            _dictionary.TryAdd("b", 2);

            // Act
            var values = _dictionary.Values;

            // Assert
            values.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [TestMethod]
        public void Indexer_Get_ReturnsValueForExistingKey()
        {
            // Arrange
            _dictionary.TryAdd("k", 42);

            // Act
            var result = _dictionary["k"];

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void Indexer_Set_UpdatesExistingKey()
        {
            // Arrange
            _dictionary.TryAdd("k", 1);

            // Act
            _dictionary["k"] = 99;

            // Assert
            _dictionary["k"].Should().Be(99);
        }

        [TestMethod]
        public void GetOrAdd_WhenKeyMissing_AddsAndReturnsValue()
        {
            // Act
            var result = _dictionary.GetOrAdd("new", 7);

            // Assert
            result.Should().Be(7);
            _dictionary["new"].Should().Be(7);
        }

        [TestMethod]
        public void GetOrAdd_WhenKeyExists_ReturnsExistingValue()
        {
            // Arrange
            _dictionary.TryAdd("existing", 5);

            // Act
            var result = _dictionary.GetOrAdd("existing", 99);

            // Assert
            result.Should().Be(5);
        }

        [TestMethod]
        public void AdditionalConstructors_InitializeDictionaryState()
        {
            // Arrange
            var initialValues = new[] { new KeyValuePair<string, int>("a", 1) };

            // Act
            var withCapacity = new ConcurrentObservableDictionary<string, int>(2, 4);
            var withCollectionComparer = new ConcurrentObservableDictionary<string, int>(
                initialValues,
                StringComparer.OrdinalIgnoreCase
            );
            var withCapacityComparer = new ConcurrentObservableDictionary<string, int>(
                2,
                4,
                StringComparer.OrdinalIgnoreCase
            );
            var withCapacityCollectionComparer = new ConcurrentObservableDictionary<string, int>(
                2,
                initialValues,
                StringComparer.OrdinalIgnoreCase
            );

            // Assert
            withCapacity.TryAdd("x", 9).Should().BeTrue();
            withCapacity["x"].Should().Be(9);
            withCollectionComparer.ContainsKey("A").Should().BeTrue();
            withCapacityComparer.TryAdd("b", 2).Should().BeTrue();
            withCapacityComparer.ContainsKey("B").Should().BeTrue();
            withCapacityCollectionComparer.ContainsKey("A").Should().BeTrue();
        }

        [TestMethod]
        public void ProtectedSimpleCollectionChanged_IgnoresUnsupportedActions()
        {
            // Arrange
            var dictionary = new TestableConcurrentObservableDictionary<string, int>();
            var raised = false;
            dictionary.CollectionChanged += (_, _) => raised = true;

            // Act
            dictionary.RaiseSimpleCollectionChanged(NotifyCollectionChangedAction.Reset, "k", 1);

            // Assert
            raised.Should().BeFalse();
        }

        private class TestObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
        {
            public List<DictionaryChangedEventArgs<TKey, TValue>> ReceivedEvents { get; } =
                new List<DictionaryChangedEventArgs<TKey, TValue>>();

            public void OnEventOccur(DictionaryChangedEventArgs<TKey, TValue> args)
            {
                ReceivedEvents.Add(args);
            }
        }

        private sealed class TestableConcurrentObservableDictionary<TKey, TValue>
            : ConcurrentObservableDictionary<TKey, TValue>
        {
            public void RaiseSimpleCollectionChanged(
                NotifyCollectionChangedAction action,
                TKey key,
                TValue value
            )
            {
                OnCollectionChanged(action, key, value);
            }
        }
    }
}
