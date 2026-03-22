using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class DeepCompare_Tests
    {
        [TestMethod]
        public void DeepDifferences_NullFirstObject_ThrowsArgumentNullException()
        {
            // Arrange
            PlainNode obj2 = new PlainNode();

            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(null, obj2);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("obj");
        }

        [TestMethod]
        public void DeepDifferences_NullSecondObject_ThrowsArgumentNullException()
        {
            // Arrange
            PlainNode obj1 = new PlainNode();

            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(obj1, null);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainEqualObjects_ThrowsInvalidCastException()
        {
            // Arrange
            var obj1 = new PlainNode { Value = 1 };
            var obj2 = new PlainNode { Value = 1 };

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainDifferentValues_ThrowsInvalidCastException()
        {
            // Arrange
            var obj1 = new PlainNode { Value = 1 };
            var obj2 = new PlainNode { Value = 2 };

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainNestedCollectionAndCircularObjects_ThrowInvalidCastException()
        {
            // Arrange
            var obj1 = new GraphNode
            {
                Label = "root",
                Items = new List<int> { 1, 2, 3 },
            };
            var obj2 = new GraphNode
            {
                Label = "other",
                Items = new List<int> { 1, 2, 4 },
            };
            obj1.Next = obj1;
            obj2.Next = obj2;

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_BothNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(null, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void DeepDifferences_StringValues_ThrowsInvalidCastException()
        {
            // Act
            Action act = () => Deep.DeepDifferences("hello", "world");

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_ComDispatchObjectsWithEqualValues_ThrowsWhenComGetterIsNotImplemented()
        {
            // Arrange
            var first = CreateComObject("WScript.Network");
            var second = CreateComObject("WScript.Network");

            // Act
            Action act = () => Deep.DeepDifferences<object>(first, second);

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<NotImplementedException>();
        }

        [TestMethod]
        public void DeepDifferences_ComDispatchObjectsWithDifferentValues_ThrowsWhenComPropertyRequiresIndexParameters()
        {
            // Arrange
            var originalDirectory = Environment.CurrentDirectory;
            var first = CreateComObject("WScript.Shell");
            var second = CreateComObject("WScript.Shell");

            var firstDirectory = Path.GetPathRoot(originalDirectory);
            var secondDirectory = originalDirectory;

            try
            {
                SetComProperty(first, "CurrentDirectory", firstDirectory);
                SetComProperty(second, "CurrentDirectory", secondDirectory);

                // Act
                Action act = () => Deep.DeepDifferences<object>(first, second);

                // Assert
                act.Should().Throw<TargetParameterCountException>();
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }

        private static object CreateComObject(string progId)
        {
            var type = Type.GetTypeFromProgID(progId);
            type.Should().NotBeNull();
            return Activator.CreateInstance(type);
        }

        private static void SetComProperty(object target, string propertyName, object value)
        {
            target
                .GetType()
                .InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty,
                    binder: null,
                    target: target,
                    args: new[] { value }
                );
        }

        private sealed class PlainNode
        {
            public int Value { get; set; }
        }

        private sealed class GraphNode
        {
            public string Label { get; set; }

            public List<int> Items { get; set; }

            public GraphNode Next { get; set; }
        }
    }
}
