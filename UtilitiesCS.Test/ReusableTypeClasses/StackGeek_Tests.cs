using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    // Main_RunsSampleScenarioWithoutThrowing captures and restores Console.Out, which is
    // process-wide state. Under the class-level parallel scope declared by the Parallelize
    // attribute at UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, a sibling test
    // class's Console.SetOut overrides this class's redirect mid-test and makes the captured
    // output empty. The assembly attribute, not TaskMaster.runsettings, is what takes effect:
    // the CI vstest invocation passes no /Settings: argument.
    [DoNotParallelize]
    [TestClass]
    public class StackGeek_Tests
    {
        [TestMethod]
        public void CreateMyStack_InitializesEmptyState()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();

            // Act
            var stack = helper.CreateStack();

            // Assert
            helper.GetCount(stack).Should().Be(0);
            helper.GetHead(stack).Should().BeNull();
            helper.GetMiddle(stack).Should().BeNull();
        }

        [TestMethod]
        public void PushAndFindMiddle_WithOddElementCount_ReturnsMiddleElement()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();

            // Act
            helper.Push(stack, 11);
            helper.Push(stack, 22);
            helper.Push(stack, 33);
            var middle = helper.FindMiddle(stack);

            // Assert
            middle.Should().Be(22);
            helper.GetCount(stack).Should().Be(3);
        }

        [TestMethod]
        public void Pop_RemovesMostRecentlyPushedValueAndUpdatesCount()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();
            helper.Push(stack, 11);
            helper.Push(stack, 22);

            // Act
            var popped = helper.Pop(stack);

            // Assert
            popped.Should().Be(22);
            helper.GetCount(stack).Should().Be(1);
            helper.FindMiddle(stack).Should().Be(11);
        }

        [TestMethod]
        public void DeleteMiddle_RemovesCurrentMiddleAndPromotesPreviousMiddleWhenCountBecomesEven()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();
            helper.Push(stack, 11);
            helper.Push(stack, 22);
            helper.Push(stack, 33);
            helper.Push(stack, 44);
            helper.Push(stack, 55);

            // Act
            helper.DeleteMiddle(stack);

            // Assert
            helper.GetCount(stack).Should().Be(4);
            helper.FindMiddle(stack).Should().Be(44);
        }

        [TestMethod]
        public void EmptyStackOperations_ReturnSentinelValueWithoutThrowing()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();

            // Act
            var popped = helper.Pop(stack);
            var middle = helper.FindMiddle(stack);
            Action act = () => helper.DeleteMiddle(stack);

            // Assert
            popped.Should().Be(-1);
            middle.Should().Be(-1);
            act.Should().NotThrow();
            helper.GetCount(stack).Should().Be(0);
        }

        [TestMethod]
        public void DeleteMiddle_WhenCountBecomesOdd_PromotesNextMiddle()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();
            helper.Push(stack, 11);
            helper.Push(stack, 22);
            helper.Push(stack, 33);
            helper.Push(stack, 44);

            // Act
            helper.DeleteMiddle(stack);

            // Assert
            helper.GetCount(stack).Should().Be(3);
            helper.FindMiddle(stack).Should().Be(11);
        }

        [TestMethod]
        public void DeleteMiddle_WithSingleElement_ShouldNotThrow()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var stack = helper.CreateStack();
            helper.Push(stack, 11);

            // Act
            Action act = () => helper.DeleteMiddle(stack);

            // Assert
            act.Should().NotThrow();
            helper.GetCount(stack).Should().Be(0);
            helper.GetHead(stack).Should().BeNull();
            helper.GetMiddle(stack).Should().BeNull();
        }

        [TestMethod]
        public void Main_RunsSampleScenarioWithoutThrowing()
        {
            // Arrange
            var helper = new StackGeekReflectionHelper();
            var originalOut = Console.Out;
            using var writer = new StringWriter();
            Console.SetOut(writer);

            try
            {
                // Act
                helper.InvokeMain(Array.Empty<string>());
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            // Assert
            writer.ToString().Should().Contain("Middle Element :");
            writer.ToString().Should().Contain("New Middle Element :");
        }

        private sealed class StackGeekReflectionHelper
        {
            private readonly Type gfgType;
            private readonly Type stackType;
            private readonly MethodInfo createStackMethod;
            private readonly MethodInfo pushMethod;
            private readonly MethodInfo popMethod;
            private readonly MethodInfo findMiddleMethod;
            private readonly MethodInfo deleteMiddleMethod;
            private readonly MethodInfo mainMethod;
            private readonly PropertyInfo countProperty;
            private readonly FieldInfo countField;
            private readonly FieldInfo headField;
            private readonly FieldInfo middleField;

            public StackGeekReflectionHelper()
            {
                var utilitiesAssembly = typeof(UtilitiesCS.StackObjectCS<int>).Assembly;
                gfgType = utilitiesAssembly.GetType("UtilitiesCS.GFG", throwOnError: true);
                stackType = gfgType.GetNestedType("myStack", BindingFlags.Public);
                createStackMethod = gfgType.GetMethod(
                    "createMyStack",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                pushMethod = gfgType.GetMethod(
                    "push",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                popMethod = gfgType.GetMethod(
                    "pop",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                findMiddleMethod = gfgType.GetMethod(
                    "findMiddle",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                deleteMiddleMethod = gfgType.GetMethod(
                    "deleteMiddle",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                mainMethod = gfgType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);
                countProperty = stackType.GetProperty(
                    "count",
                    BindingFlags.Instance | BindingFlags.Public
                );
                countField = stackType.GetField(
                    "count",
                    BindingFlags.Instance | BindingFlags.Public
                );
                headField = stackType.GetField("head", BindingFlags.Instance | BindingFlags.Public);
                middleField = stackType.GetField(
                    "mid",
                    BindingFlags.Instance | BindingFlags.Public
                );
            }

            public object CreateStack()
            {
                var instance = Activator.CreateInstance(gfgType, nonPublic: true);
                return createStackMethod.Invoke(instance, null);
            }

            public void Push(object stack, int value)
            {
                var instance = Activator.CreateInstance(gfgType, nonPublic: true);
                pushMethod.Invoke(instance, new[] { stack, (object)value });
            }

            public int Pop(object stack)
            {
                var instance = Activator.CreateInstance(gfgType, nonPublic: true);
                return (int)popMethod.Invoke(instance, new[] { stack });
            }

            public int FindMiddle(object stack)
            {
                var instance = Activator.CreateInstance(gfgType, nonPublic: true);
                return (int)findMiddleMethod.Invoke(instance, new[] { stack });
            }

            public void DeleteMiddle(object stack)
            {
                var instance = Activator.CreateInstance(gfgType, nonPublic: true);
                deleteMiddleMethod.Invoke(instance, new[] { stack });
            }

            public void InvokeMain(string[] args)
            {
                mainMethod.Invoke(null, new object[] { args });
            }

            public int GetCount(object stack)
            {
                if (countField != null)
                {
                    return (int)countField.GetValue(stack);
                }

                return (int)countProperty.GetValue(stack);
            }

            public object GetHead(object stack) => headField.GetValue(stack);

            public object GetMiddle(object stack) => middleField.GetValue(stack);
        }
    }
}
