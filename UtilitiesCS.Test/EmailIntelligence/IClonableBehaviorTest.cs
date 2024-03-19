using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using FluentAssertions;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class IClonableBehaviorTest
    {
        private class TestClass: ICloneable
        {
            public TestClass(){}
            
            private string _name;
            public string Name { get => _name; set => _name = value; }

            private ConcurrentDictionary<string, int> _dict = new();
            public ConcurrentDictionary<string, int> Dict { get => _dict; set => _dict = value; }

            public object Clone()
            {
                var clone = this.MemberwiseClone();
                return clone;
            }

            public object DeepClone()
            {
                var clone = this.MemberwiseClone() as TestClass;
                clone.Dict = new ConcurrentDictionary<string, int>(this.Dict);
                return clone;
            }
        }

        [TestMethod]
        public void ShallowClone_Expected()
        {
            // Arrange
            var original = new TestClass();
            original.Name = "Class";
            original.Dict.TryAdd("One", 1);
            original.Dict.TryAdd("Two", 2);
            original.Dict.TryAdd("Three", 3);

            var expected = new TestClass();
            expected.Name = "Class";
            expected.Dict.TryAdd("One", 1);
            expected.Dict.TryAdd("Three", 3);

            // Act
            var clone = original.Clone() as TestClass;
            clone.Dict.TryRemove("Two", out _);

            // Assert
            original.Should().BeEquivalentTo(expected);

        }

        [TestMethod]
        public void DeepClone_Expected()
        {
            // Arrange
            var original = new TestClass();
            original.Name = "Class";
            original.Dict.TryAdd("One", 1);
            original.Dict.TryAdd("Two", 2);
            original.Dict.TryAdd("Three", 3);

            var expected = new TestClass();
            expected.Name = "Class";
            expected.Dict.TryAdd("One", 1);
            expected.Dict.TryAdd("Two", 2);
            expected.Dict.TryAdd("Three", 3);

            // Act
            var clone = original.DeepClone() as TestClass;
            clone.Dict.TryRemove("Two", out _);

            // Assert
            original.Should().BeEquivalentTo(expected);

        }

    }

    
}
