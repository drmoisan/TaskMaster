using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="ManagerAsyncLazy"/>.
    ///
    /// Purpose:
    ///     Provide dedicated coverage for the lazy-success and deactivation
    ///     paths in ManagerAsyncLazy that are not exercised by the broader
    ///     ClassifierGroups_Tests and Triage_Tests suites.
    ///
    /// Constraints:
    ///     Methods that await the full Configuration pipeline (ReadConfiguration,
    ///     WriteConfigurationAsync, Loader_PropertyChanged, Config_PropertyChanged)
    ///     rely on ManagerResources, the file system, and live Outlook COM, and
    ///     cannot be tested deterministically in isolation.
    ///     Tests here use mock IApplicationGlobals and cover only the code paths
    ///     that do not reach those external systems.
    /// </summary>
    [TestClass]
    public class ManagerAsyncLazy_Tests
    {
        /// <summary>
        /// Lazy-success path (P2-T18): when ClassifierActivated is true,
        /// ResetLoadClassifierAsyncLazy must create an AsyncLazy entry and
        /// register it in the dictionary without awaiting the factory.
        ///
        /// Args:
        ///     None — uses inline Arrange.
        ///
        /// Returns:
        ///     Void; asserts via FluentAssertions.
        ///
        /// Side Effects:
        ///     None; operates on an in-memory ManagerAsyncLazy instance.
        /// </summary>
        [TestMethod]
        [Description(
            "Lazy-success path: when ClassifierActivated=true, ResetLoadClassifierAsyncLazy creates and stores an AsyncLazy entry."
        )]
        public void ResetLoadClassifierAsyncLazy_WhenClassifierActivated_RegistersLazyEntryInDictionary()
        {
            // Arrange: minimal mock globals; the constructor sets up Configuration as an
            // AsyncLazy without executing ReadConfiguration, so no file system or COM is needed.
            var mockGlobals = new Mock<IApplicationGlobals>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            // A loader with ClassifierActivated=true exercises the "activated" registration branch.
            var loader = new SmartSerializableLoader();
            loader.Config.ClassifierActivated = true;

            // Act: invokes GetAsyncLazyClassifierLoader (via the activated branch) and stores the result.
            manager.ResetLoadClassifierAsyncLazy("TestClassifier", loader);

            // Assert: the entry is now present in the dictionary.
            manager
                .ContainsKey("TestClassifier")
                .Should()
                .BeTrue(
                    "an activated classifier should be registered as an AsyncLazy entry in the dictionary"
                );
        }

        /// <summary>
        /// Deactivated-removal path (P2-T19): when ClassifierActivated is false,
        /// ResetLoadClassifierAsyncLazy must remove an existing entry from the dictionary.
        /// This covers the else branch (lines 308-310 in the source).
        ///
        /// Args:
        ///     None — uses inline Arrange.
        ///
        /// Returns:
        ///     Void; asserts via FluentAssertions.
        ///
        /// Side Effects:
        ///     None; operates on an in-memory ManagerAsyncLazy instance.
        /// </summary>
        [TestMethod]
        [Description(
            "Deactivated path: when ClassifierActivated=false, ResetLoadClassifierAsyncLazy removes the entry from the dictionary."
        )]
        public void ResetLoadClassifierAsyncLazy_WhenClassifierDeactivated_RemovesEntryFromDictionary()
        {
            // Arrange: pre-populate the dictionary via the activated path so there is something to remove.
            var mockGlobals = new Mock<IApplicationGlobals>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var activeLoader = new SmartSerializableLoader();
            activeLoader.Config.ClassifierActivated = true;
            manager.ResetLoadClassifierAsyncLazy("TestClassifier", activeLoader);
            manager
                .ContainsKey("TestClassifier")
                .Should()
                .BeTrue("precondition: entry must exist before deactivation");

            // A deactivated loader exercises the else branch that calls TryRemove.
            var inactiveLoader = new SmartSerializableLoader();

            // ClassifierActivated defaults to false; set explicitly for readability.
            inactiveLoader.Config.ClassifierActivated = false;

            // Act: invokes the removal branch.
            manager.ResetLoadClassifierAsyncLazy("TestClassifier", inactiveLoader);

            // Assert: the entry has been removed from the dictionary.
            manager
                .ContainsKey("TestClassifier")
                .Should()
                .BeFalse("deactivating a classifier should remove its entry from the dictionary");
        }

        [TestMethod]
        public void GetAltLoader_WhenLoaderTypeExposesFactory_ReturnsWorkingFactoryDelegate()
        {
            // Arrange
            var manager = new ManagerAsyncLazy(new Mock<IApplicationGlobals>().Object);
            var loader = new SmartSerializableLoader { T = typeof(TestClassifierFactory) };
            var method = typeof(ManagerAsyncLazy).GetMethod(
                "GetAltLoader",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var altLoader =
                (Func<BayesianClassifierGroup>)method!.Invoke(manager, new object[] { loader });

            // Assert
            altLoader.Should().NotBeNull();
            altLoader!().Should().BeOfType<TestClassifierGroup>();
        }

        [TestMethod]
        public void GetAltLoader_WhenLoaderTypeDoesNotExposeFactory_ReturnsNull()
        {
            // Arrange
            var manager = new ManagerAsyncLazy(new Mock<IApplicationGlobals>().Object);
            var loader = new SmartSerializableLoader { T = typeof(string) };
            var method = typeof(ManagerAsyncLazy).GetMethod(
                "GetAltLoader",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var altLoader =
                (Func<BayesianClassifierGroup>)method!.Invoke(manager, new object[] { loader });

            // Assert
            altLoader.Should().BeNull();
        }

        private sealed class TestClassifierGroup : BayesianClassifierGroup;

        private static class TestClassifierFactory
        {
            public static BayesianClassifierGroup CreateNewClassifier() =>
                new TestClassifierGroup();
        }
    }
}
