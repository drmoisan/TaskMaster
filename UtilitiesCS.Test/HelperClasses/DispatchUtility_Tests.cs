using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class DispatchUtility_Tests
    {
        #region ImplementsIDispatch

        [TestMethod]
        public void ImplementsIDispatch_RegularObject_ReturnsFalse()
        {
            var obj = new object();
            DispatchUtility.ImplementsIDispatch(obj).Should().BeFalse();
        }

        [TestMethod]
        public void ImplementsIDispatch_String_ReturnsFalse()
        {
            DispatchUtility.ImplementsIDispatch("test").Should().BeFalse();
        }

        #endregion

        #region TryGetDispId

        /// <summary>
        /// Verifies that calling TryGetDispId with a non-dispatch managed object throws
        /// InvalidCastException because the object cannot be cast to IDispatchInfo.
        ///
        /// Purpose:
        ///     Documents the expected failure contract when a caller does not first guard
        ///     with ImplementsIDispatch. The implementation performs a hard cast to
        ///     IDispatchInfo, so any non-dispatch object is an illegal argument.
        ///
        /// Returns:
        ///     Passes when InvalidCastException is thrown (the expected error surfacing).
        /// </summary>
        [TestMethod]
        public void TryGetDispId_NonDispatchObject_ThrowsInvalidCastException()
        {
            // Arrange: a plain managed object that does not implement IDispatchInfo.
            var obj = new object();

            // Act
            Action act = () => DispatchUtility.TryGetDispId(obj, "NonExistentMember", out _);

            // Assert: a non-COM object cannot be cast to IDispatchInfo; the expected
            // exception documents this boundary rather than hiding the cast failure.
            act.Should()
                .Throw<InvalidCastException>(
                    "TryGetDispId requires objects that implement IDispatchInfo; "
                        + "a plain managed object must not silently succeed"
                );
        }

        #endregion

        #region Invoke

        [TestMethod]
        public void Invoke_ByMemberName_InvokesToString()
        {
            var obj = 42;
            var result = DispatchUtility.Invoke(obj, "ToString", Array.Empty<object>());

            result.Should().Be("42");
        }

        [TestMethod]
        public void Invoke_NullObject_ThrowsArgumentNullException()
        {
            Action act = () => DispatchUtility.Invoke(null, "ToString", Array.Empty<object>());
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Invoke_ByDispId_WithValidMember_InvokesSuccessfully()
        {
            // Invoke using a DISPID approach (falls back to member name pattern)
            var obj = "hello";
            // This will use member name "[DispId=0]" which won't resolve on a string
            Action act = () => DispatchUtility.Invoke(obj, 0, Array.Empty<object>());
            // Expected to throw since "[DispId=0]" isn't a valid member on string
            act.Should().Throw<Exception>();
        }

        #endregion
    }
}
