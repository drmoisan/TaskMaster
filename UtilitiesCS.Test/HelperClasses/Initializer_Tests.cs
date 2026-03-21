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

        [TestMethod]
        public void GetOrLoad_WithIsInitializedPredicate_SkipsLoaderWhenAlreadyInitialized()
        {
            // Arrange
            string value = "already";
            bool loaderCalled = false;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                v => v != null,
                () =>
                {
                    loaderCalled = true;
                    return "loaded";
                }
            );

            // Assert
            result.Should().Be("already");
            loaderCalled.Should().BeFalse();
        }

        [TestMethod]
        public void GetOrLoad_WithIsInitializedPredicate_LoadsWhenNotInitialized()
        {
            // Arrange
            string value = null;

            // Act
            var result = Initializer.GetOrLoad(ref value, v => v != null, () => "loaded");

            // Assert
            result.Should().Be("loaded");
        }

        [TestMethod]
        public void DependenciesNotNull_WithNullDependencies_AndStrictTrue_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => Initializer.DependenciesNotNull(strict: true, dependencies: null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*dependencies*");
        }

        [TestMethod]
        public void DependenciesNotNull_WithEmptyDependencies_AndStrictFalse_ReturnsFalse()
        {
            // Act
            var result = Initializer.DependenciesNotNull(strict: false);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void DependenciesNotNull_WithSomeNullElements_AndStrictFalse_ReturnsFalse()
        {
            // Act
            var result = Initializer.DependenciesNotNull(strict: false, "a", null, "b");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Load_WithStrictAndValidDependencies_InvokesLoader()
        {
            // Act
            var result = Initializer.Load(() => 42, strict: true, "dep");

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void Load_WithMissingDependencies_ReturnsDefault()
        {
            // Act
            var result = Initializer.Load(() => 42, strict: false, dependencies: null);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public void Load_WithDefaultValue_WhenDependenciesNull_ReturnsDefaultValue()
        {
            // Act
            var result = Initializer.Load(() => 42, defaultValue: -1, dependencies: null);

            // Assert
            result.Should().Be(-1);
        }

        [TestMethod]
        public void Load_WithDefaultValue_WhenDependenciesValid_InvokesLoader()
        {
            // Act
            var result = Initializer.Load(() => 42, defaultValue: -1, "dep");

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void SetAndSave_WithObjectSetterOnly_SetsVariableAndCallsSetter()
        {
            // Arrange
            string cached = null;
            string setValue = null;

            // Act
            Initializer.SetAndSave(ref cached, "newValue", v => setValue = v);

            // Assert
            cached.Should().Be("newValue");
            setValue.Should().Be("newValue");
        }

        [TestMethod]
        public void SetAndSave_WithoutRef_JustCallsSetter()
        {
            // Arrange
            string setValue = null;

            // Act
            Initializer.SetAndSave("newValue", v => setValue = v);

            // Assert
            setValue.Should().Be("newValue");
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultValue_WhenLoaderReturnsDefault_SetsDefaultValue()
        {
            // Arrange
            string value = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => null,
                dependencies: new object[] { "dep" }
            );

            // Assert
            result.Should().Be("fallback");
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultValue_WhenNoDependencies_SetsDefaultValue()
        {
            // Arrange
            string value = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => "loaded",
                dependencies: null
            );

            // Assert
            result.Should().Be("fallback");
        }

        [TestMethod]
        public void SetAndSave_RefWithConditionNoSaver_WhenConditionTrue_CallsSetter()
        {
            // Arrange
            string cached = "old";
            string setValue = null;

            // Act
            Initializer.SetAndSave(ref cached, "new", v => setValue = v, () => true, strict: false);

            // Assert
            cached.Should().Be("new");
            setValue.Should().Be("new");
        }

        [TestMethod]
        public void SetAndSave_RefWithConditionNoSaver_WhenConditionFalse_SkipsSetter()
        {
            // Arrange
            string cached = "old";
            string setValue = null;

            // Act
            Initializer.SetAndSave(
                ref cached,
                "new",
                v => setValue = v,
                () => false,
                strict: false
            );

            // Assert
            cached.Should().Be("new");
            setValue.Should().BeNull();
        }

        [TestMethod]
        public void SetAndSave_ValueOnlyWithCondition_WhenConditionTrue_CallsSetter()
        {
            // Arrange
            string setValue = null;

            // Act
            Initializer.SetAndSave("val", v => setValue = v, () => true, strict: false);

            // Assert
            setValue.Should().Be("val");
        }

        [TestMethod]
        public void SetAndSave_ValueOnlyWithCondition_WhenConditionFalse_SkipsSetter()
        {
            // Arrange
            string setValue = null;

            // Act
            Initializer.SetAndSave("val", v => setValue = v, () => false, strict: false);

            // Assert
            setValue.Should().BeNull();
        }

        [TestMethod]
        public void SetAndSave_ValueOnlyWithSaverAndCondition_WhenConditionNull_CallsSetterAndSaver()
        {
            // Arrange
            string setValue = null;
            int saveCount = 0;

            // Act
            Initializer.SetAndSave(
                "val",
                v => setValue = v,
                () => saveCount++,
                (Func<bool>)null,
                strict: false
            );

            // Assert
            setValue.Should().Be("val");
            saveCount.Should().Be(1);
        }

        [TestMethod]
        public void SetAndSave_ValueOnlyWithSaverAndCondition_WhenSetterNull_NonStrict_SkipsSetter()
        {
            // Arrange
            int saveCount = 0;

            // Act
            Initializer.SetAndSave(
                "val",
                (Action<string>)null,
                () => saveCount++,
                () => true,
                strict: false
            );

            // Assert
            saveCount.Should().Be(1);
        }

        [TestMethod]
        public void SetAndSave_ValueOnlyWithSaverAndCondition_WhenSaverNull_NonStrict_SkipsSaver()
        {
            // Arrange
            string setValue = null;

            // Act
            Initializer.SetAndSave(
                "val",
                v => setValue = v,
                (Action)null,
                () => true,
                strict: false
            );

            // Assert
            setValue.Should().Be("val");
        }

        [TestMethod]
        public void DependenciesNotNull_EmptyDependencies_StrictTrue_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => Initializer.DependenciesNotNull(strict: true);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void DependenciesNotNull_SomeNullElements_StrictTrue_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => Initializer.DependenciesNotNull(strict: true, "a", null, "b");

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*null value*");
        }

        [TestMethod]
        public void DependenciesNotNull_AllValid_ReturnsTrue()
        {
            // Act
            var result = Initializer.DependenciesNotNull(strict: false, "a", "b");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetOrLoad_WithIsInitializedAndDeps_WhenDepsNull_ReturnsCurrentVariable()
        {
            // Arrange
            string value = "current";

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                v => v != null,
                () => "loaded",
                strict: false,
                dependencies: null
            );

            // Assert
            result.Should().Be("current");
        }

        [TestMethod]
        public void GetOrLoad_WithIsInitializedAndDeps_WhenDepsValid_LoadsValue()
        {
            // Arrange
            string value = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                v => v != null,
                () => "loaded",
                strict: false,
                "dep"
            );

            // Assert
            result.Should().Be("loaded");
        }

        [TestMethod]
        public void GetOrLoad_WithCallbackAndDeps_WhenDepsValid_InvokesCallbackAndLoads()
        {
            // Arrange
            string value = null;
            string callbackValue = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                () => "loaded",
                v => callbackValue = v,
                strict: false,
                "dep"
            );

            // Assert
            result.Should().Be("loaded");
            callbackValue.Should().Be("loaded");
        }

        [TestMethod]
        public void GetOrLoad_WithCallbackAndDeps_WhenDepsNull_ReturnsDefault()
        {
            // Arrange
            string value = null;
            bool callbackCalled = false;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                () => "loaded",
                v => callbackCalled = true,
                strict: false,
                dependencies: null
            );

            // Assert
            result.Should().BeNull();
            callbackCalled.Should().BeFalse();
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultAndSaver_WhenLoaderReturnsValue_DoesNotCallSaver()
        {
            // Arrange
            string value = null;
            bool saverCalled = false;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => "loaded",
                defaultSetAndSaver: v => saverCalled = true,
                dependencies: new object[] { "dep" }
            );

            // Assert
            result.Should().Be("loaded");
            saverCalled.Should().BeFalse();
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultAndSaver_WhenLoaderReturnsNull_CallsSaver()
        {
            // Arrange
            string value = null;
            string savedValue = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => (string)null,
                defaultSetAndSaver: v => savedValue = v,
                dependencies: new object[] { "dep" }
            );

            // Assert
            result.Should().Be("fallback");
            savedValue.Should().Be("fallback");
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultAndSaver_WhenNoDeps_ReturnsDefault()
        {
            // Arrange
            string value = null;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: "fallback",
                loader: () => "loaded",
                defaultSetAndSaver: v => { },
                dependencies: null
            );

            // Assert
            result.Should().Be("fallback");
        }

        [TestMethod]
        public void GetOrLoad_WithDefaultNonNull_WhenLoaderReturnsDefault_FallsBackToDefault()
        {
            // Arrange
            int value = 0;

            // Act
            var result = Initializer.GetOrLoad(
                ref value,
                defaultValue: 99,
                loader: () => 0,
                dependencies: new object[] { "dep" }
            );

            // Assert
            result.Should().Be(99);
        }

        [TestMethod]
        public void SetAndSave_ValueOnly_WithNullSetter_NonStrict_DoesNotThrow()
        {
            // Act
            Action act = () => Initializer.SetAndSave("value", (Action<string>)null);

            // Assert
            act.Should().NotThrow();
        }
    }
}
