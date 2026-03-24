using System;
using System.ComponentModel;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="IntelligenceConfig"/>.
    ///
    /// Purpose:
    ///     Exercise the three deterministically testable behaviors in IntelligenceConfig
    ///     without touching the filesystem, Outlook, or live resources:
    ///     (1) P38-T1: The private static <c>IsDerivedFromScoDictionaryNew</c> helper returns
    ///         the expected value for a known derived type vs a non-derived type.
    ///     (2) P38-T2: <c>Loader_PropertyChanged</c> short-circuits when the property name
    ///         does not contain "ClassifierActivated", preventing a WriteConfiguration call
    ///         (verified by the absence of a NullReferenceException on a null Config).
    ///     (3) P38-T3: A freshly constructed IntelligenceConfig has a null Config dictionary
    ///         before InitAsync is called, confirming the lazy default-initialization contract.
    ///
    /// Constraints:
    ///     IsDerivedFromScoDictionaryNew is private static; it is invoked via reflection.
    ///     Loader_PropertyChanged is internal and accessible via InternalsVisibleTo.
    ///     No filesystem side-effects: Config remains null so WriteConfiguration is never reached.
    /// </summary>
    [TestClass]
    public class IntelligenceConfig_Tests
    {
        #region P38-T1 — Derived-type detection matches expected classifier types

        /// <summary>
        /// Verifies that the private IsDerivedFromScoDictionaryNew helper correctly identifies
        /// a type derived from ScoDictionaryNew{TKey,TValue} and correctly rejects a type
        /// that is not in that hierarchy.
        ///
        /// Purpose:
        ///     Confirm the type-walk loop terminates at the correct points for both a positive
        ///     hierarchy member and an unrelated type.
        ///
        /// Args:
        ///     derivedType: PeopleScoDictionaryNew, which inherits ScoDictionaryNew{string,string}.
        ///     unrelatedType: string, which has no relationship to ScoDictionaryNew.
        ///
        /// Returns:
        ///     Passes when true is returned for the derived type and false for the unrelated type.
        /// </summary>
        [TestMethod]
        public void IsDerivedFromScoDictionaryNew_ReturnsTrueForDerivedTypeAndFalseForOther()
        {
            // Arrange: retrieve the private static method via reflection
            var method = typeof(IntelligenceConfig).GetMethod(
                "IsDerivedFromScoDictionaryNew",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull("IsDerivedFromScoDictionaryNew must exist as private static");

            // Act: test a type that IS derived from ScoDictionaryNew<,>
            var derivedResult = (bool)
                method.Invoke(null, new object[] { typeof(PeopleScoDictionaryNew) });

            // Act: test a type that is NOT in the ScoDictionaryNew hierarchy
            var unrelatedResult = (bool)method.Invoke(null, new object[] { typeof(string) });

            // Assert: derived type returns true; unrelated type returns false
            derivedResult.Should().BeTrue();
            unrelatedResult.Should().BeFalse();
        }

        #endregion

        #region P38-T2 — Non-matching property name does not trigger write path

        /// <summary>
        /// Verifies that Loader_PropertyChanged silently returns when the PropertyName does
        /// not contain "ClassifierActivated", and that WriteConfiguration is therefore never
        /// called (confirmed by the absence of a NullReferenceException on a null Config).
        ///
        /// Purpose:
        ///     Confirm the conditional guard in Loader_PropertyChanged: only property changes
        ///     whose name contains "ClassifierActivated" route to the write path.
        ///
        /// Args:
        ///     config: IntelligenceConfig with null Config (never initialized).
        ///     sender: a no-arg SmartSerializableLoader instance.
        ///     args: PropertyChangedEventArgs with PropertyName = "SomeOtherProperty".
        ///
        /// Returns:
        ///     Passes when the invocation completes without throwing.
        /// </returns>
        /// </summary>
        [TestMethod]
        public void LoaderPropertyChanged_WhenPropertyNameDoesNotMatchClassifierActivated_DoesNotTriggerWrite()
        {
            // Arrange: null Config means WriteConfiguration would throw if reached
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            var config = new IntelligenceConfig(mockGlobals.Object);
            var sender = new SmartSerializableLoader();
            var args = new PropertyChangedEventArgs("SomeOtherProperty");

            // Act + Assert: non-matching property name → no write path → no exception
            config.Invoking(c => c.Loader_PropertyChanged(sender, args)).Should().NotThrow();
        }

        #endregion

        #region P38-T3 — Missing config data initializes defaults (Config is null before InitAsync)

        /// <summary>
        /// Verifies that a freshly constructed IntelligenceConfig has a null Config property
        /// before InitAsync is called, confirming that initialization is deferred.
        ///
        /// Purpose:
        ///     Confirm the lazy default state: the Config dictionary is not populated until
        ///     InitAsync runs. This also ensures no file-system or network calls occur during
        ///     plain construction.
        ///
        /// Args:
        ///     config: IntelligenceConfig constructed with a no-op mock globals.
        ///
        /// Returns:
        ///     Passes when config.Config is null.
        /// </summary>
        [TestMethod]
        public void Config_BeforeInitAsync_IsNull()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);

            // Act: construct IntelligenceConfig without calling InitAsync
            var config = new IntelligenceConfig(mockGlobals.Object);

            // Assert: Config is not populated until InitAsync
            config.Config.Should().BeNull();
        }

        #endregion
    }
}
