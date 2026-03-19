using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class Initializer_Tests
    {
        [TestMethod]
        public void GetOrLoad_WhenVariableIsDefault_LoadsValueOnlyOnce()
        {
            // Arrange
            string value = null;
            int loadCount = 0;

            string Loader()
            {
                loadCount++;
                return "loaded";
            }

            // Act
            var first = Initializer.GetOrLoad(ref value, Loader);
            var second = Initializer.GetOrLoad(ref value, Loader);

            // Assert
            first.Should().Be("loaded");
            second.Should().Be("loaded");
            value.Should().Be("loaded");
            loadCount.Should().Be(1);
        }

        [TestMethod]
        public void GetOrLoad_WithCallback_InvokesCallbackWhenValueIsLoaded()
        {
            // Arrange
            string value = null;
            string callbackValue = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                () => "loaded",
                loaded => callbackValue = loaded
            );

            // Assert
            result.Should().Be("loaded");
            callbackValue.Should().Be("loaded");
        }

        [TestMethod]
        public void GetOrLoad_WithStrictDependenciesAndMissingDependency_ReturnsDefaultWithoutLoading()
        {
            // Arrange
            string value = null;
            bool loaderCalled = false;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                () =>
                {
                    loaderCalled = true;
                    return "loaded";
                },
                strict: false,
                dependencies: null
            );

            // Assert
            result.Should().BeNull();
            value.Should().BeNull();
            loaderCalled.Should().BeFalse();
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultValue_WhenLoaderThrows_SetsDefaultAndInvokesSaver()
        {
            // Arrange
            string value = null;
            string savedValue = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => throw new InvalidOperationException("boom"),
                defaultSetAndSaver: loaded => savedValue = loaded,
                dependencies: new object[] { "dependency" }
            );

            // Assert
            result.Should().Be("fallback");
            value.Should().Be("fallback");
            savedValue.Should().Be("fallback");
        }

        [TestMethod]
        public void SetAndSave_WhenConditionIsTrue_UpdatesVariableAndInvokesSetterAndSaver()
        {
            // Arrange
            string cached = "before";
            string setValue = null;
            int saveCount = 0;

            // Act
            Initializer.SetAndSave(
                ref cached,
                "after",
                value => setValue = value,
                () => saveCount++,
                () => true,
                strict: true
            );

            // Assert
            cached.Should().Be("after");
            setValue.Should().Be("after");
            saveCount.Should().Be(1);
        }

        [TestMethod]
        public void SetAndSave_WhenConditionIsFalse_UpdatesVariableWithoutCallingDelegates()
        {
            // Arrange
            string cached = "before";
            string setValue = null;
            int saveCount = 0;

            // Act
            Initializer.SetAndSave(
                ref cached,
                "after",
                value => setValue = value,
                () => saveCount++,
                () => false,
                strict: true
            );

            // Assert
            cached.Should().Be("after");
            setValue.Should().BeNull();
            saveCount.Should().Be(0);
        }

        [TestMethod]
        public void SetAndSave_WithStrictTrueAndNullSetter_ThrowsArgumentNullException()
        {
            // Arrange
            string cached = "before";

            // Act
            Action act = () =>
                Initializer.SetAndSave(
                    ref cached,
                    "after",
                    (Action<string>)null,
                    () => true,
                    strict: true
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void DependenciesNotNull_WhenCalledConcurrentlyWithValidDependencies_ReturnsTrueForAllCalls()
        {
            // Arrange
            var tasks = Enumerable
                .Range(0, 8)
                .Select(_ =>
                    Task.Run(() => Initializer.DependenciesNotNull(strict: true, "dependency", 5))
                )
                .ToArray();

            // Act
            Task.WaitAll(tasks);

            // Assert
            tasks.Select(task => task.Result).Should().OnlyContain(result => result);
        }
    }
}
