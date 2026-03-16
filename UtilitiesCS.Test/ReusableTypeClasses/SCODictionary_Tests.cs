using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SCODictionary_Tests
    {
        [TestMethod]
        public void AddRemoveTryGetValueAndCount_WorkAsExpected()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            dictionary.Add("alpha", 1);
            dictionary.Add("beta", 2);
            var found = dictionary.TryGetValue("beta", out var betaValue);
            var removed = dictionary.Remove("alpha");

            // Assert
            found.Should().BeTrue();
            betaValue.Should().Be(2);
            removed.Should().BeTrue();
            dictionary.Count.Should().Be(1);
            dictionary.Should().ContainKey("beta");
        }

        [TestMethod]
        public void IndexerKeysAndValues_ReflectCurrentEntries()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            dictionary["alpha"] = 1;
            dictionary["beta"] = 2;
            dictionary["beta"] = 20;
            var keys = dictionary.Keys.OrderBy(key => key).ToArray();
            var values = dictionary.Values.OrderBy(value => value).ToArray();

            // Assert
            dictionary["beta"].Should().Be(20);
            keys.Should().Equal("alpha", "beta");
            values.Should().Equal(1, 20);
        }

        [TestMethod]
        public void MissingKey_TryGetValueReturnsFalse()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            var found = dictionary.TryGetValue("missing", out var value);

            // Assert
            found.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public void DuplicateKey_AddThrowsArgumentException()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();
            dictionary.Add("duplicate", 1);

            // Act
            Action act = () => dictionary.Add("duplicate", 2);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public async Task ConcurrentAccess_AddsAndReadsAllEntries()
        {
            // Arrange
            var dictionary = new ScoDictionary<int, int>();
            var keys = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(keys.Select(key => Task.Run(() => dictionary[key] = key * 10)));
            var readResults = await Task.WhenAll(keys.Select(key => Task.Run(() => dictionary.TryGetValue(key, out var value) ? value : -1)));

            // Assert
            dictionary.Count.Should().Be(keys.Length);
            readResults.OrderBy(value => value).Should().Equal(keys.Select(key => key * 10));
        }
    }
}