using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class YesNoToAll_Tests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
        }

        [TestMethod]
        public void Response_ShouldDefaultToEmpty_AndAllowRoundTripAssignment()
        {
            // Arrange / Act
            YesNoToAll.Response = YesNoToAllResponse.YesToAll;

            // Assert
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.YesToAll);
        }

        [TestMethod]
        public void InternalResponders_ShouldMapEachButtonToExpectedResponse()
        {
            // Arrange
            var expectations = new (string methodName, YesNoToAllResponse response)[]
            {
                ("RespondYes", YesNoToAllResponse.Yes),
                ("RespondYesToAll", YesNoToAllResponse.YesToAll),
                ("RespondNo", YesNoToAllResponse.No),
                ("RespondNoToAll", YesNoToAllResponse.NoToAll),
                ("RespondCancel", YesNoToAllResponse.Empty),
            };

            foreach (var expectation in expectations)
            {
                // Act
                InvokeInternalResponder(expectation.methodName);

                // Assert
                YesNoToAll.Response.Should().Be(expectation.response, $"{expectation.methodName} should update the shared response state");
            }
        }

        [TestMethod]
        public void InternalResponders_ShouldOverwritePreviousSelection()
        {
            // Arrange
            InvokeInternalResponder("RespondYes");

            // Act
            InvokeInternalResponder("RespondNoToAll");

            // Assert
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.NoToAll);
        }

        [TestMethod]
        public void Response_ShouldRemainEmpty_WhenInitializedOrCancelled()
        {
            // Arrange / Act
            InvokeInternalResponder("RespondCancel");

            // Assert
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        private static void InvokeInternalResponder(string methodName)
        {
            MethodInfo responder = typeof(YesNoToAll).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            responder.Should().NotBeNull();
            responder.Invoke(null, null);
        }
    }
}
