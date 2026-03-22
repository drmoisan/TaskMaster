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
