using System;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                YesNoToAll
                    .Response.Should()
                    .Be(
                        expectation.response,
                        $"{expectation.methodName} should update the shared response state"
                    );
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
            MethodInfo responder = typeof(YesNoToAll).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            responder.Should().NotBeNull();
            responder.Invoke(null, null);
        }

        // ---------------------------------------------------------------------------
        // Seam teardown — resets MyBox.DialogInvoker and YesNoToAll.Response after
        // each ShowDialog test to prevent cross-test contamination.
        // ---------------------------------------------------------------------------

        [TestCleanup]
        public void TestCleanup_ResetSeamsAndResponse()
        {
            MyBox.DialogInvoker = viewer => viewer.ShowDialog();
            YesNoToAll.Response = YesNoToAllResponse.Empty;
        }

        // ---------------------------------------------------------------------------
        // P2-T11: ShowDialog returns Yes when the MyBox.DialogInvoker seam reports Yes
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that ShowDialog returns YesNoToAllResponse.Yes when the injected
        /// MyBox.DialogInvoker seam simulates the Yes button delegate being invoked.
        ///
        /// Purpose:
        ///     Cover the ShowDialog body including the five DelegateButton creations,
        ///     the MyBox.ShowDialog call, and the return statement — using the Yes path.
        ///
        /// Returns:
        ///     YesNoToAllResponse.Yes after the seam invokes RespondYes().
        /// </summary>
        [TestMethod]
        [STAThread]
        public void ShowDialog_SeamInvokesRespondYes_ReturnsYesResponse()
        {
            // Arrange: inject seam that simulates the Yes delegate button being clicked
            MyBox.DialogInvoker = _ =>
            {
                // Simulate the Yes button click: invoke the internal responder directly
                YesNoToAll.RespondYes();
                return DialogResult.OK;
            };

            // Act
            YesNoToAllResponse result = YesNoToAll.ShowDialog("Test message");

            // Assert: Yes delegate invocation produces the Yes response
            result.Should().Be(YesNoToAllResponse.Yes);
        }

        // ---------------------------------------------------------------------------
        // P2-T12: ShowDialog returns No when the MyBox.DialogInvoker seam reports No
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that ShowDialog returns YesNoToAllResponse.No when the injected
        /// MyBox.DialogInvoker seam simulates the No button delegate being invoked.
        ///
        /// Purpose:
        ///     Cover the No decision path through ShowDialog.
        ///
        /// Returns:
        ///     YesNoToAllResponse.No after the seam invokes RespondNo().
        /// </summary>
        [TestMethod]
        [STAThread]
        public void ShowDialog_SeamInvokesRespondNo_ReturnsNoResponse()
        {
            // Arrange: simulate No button click
            MyBox.DialogInvoker = _ =>
            {
                YesNoToAll.RespondNo();
                return DialogResult.OK;
            };

            // Act
            YesNoToAllResponse result = YesNoToAll.ShowDialog("Test message");

            // Assert: No delegate invocation produces the No response
            result.Should().Be(YesNoToAllResponse.No);
        }

        // ---------------------------------------------------------------------------
        // P2-T13: ShowDialog returns YesToAll when the seam reports all (YesToAll)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that ShowDialog returns YesNoToAllResponse.YesToAll when the injected
        /// MyBox.DialogInvoker seam simulates the YesToAll button delegate being invoked.
        ///
        /// Purpose:
        ///     Cover the YesToAll (All) decision path through ShowDialog.
        ///
        /// Returns:
        ///     YesNoToAllResponse.YesToAll after the seam invokes RespondYesToAll().
        /// </summary>
        [TestMethod]
        [STAThread]
        public void ShowDialog_SeamInvokesRespondYesToAll_ReturnsYesToAllResponse()
        {
            // Arrange: simulate YesToAll button click
            MyBox.DialogInvoker = _ =>
            {
                YesNoToAll.RespondYesToAll();
                return DialogResult.OK;
            };

            // Act
            YesNoToAllResponse result = YesNoToAll.ShowDialog("Test message");

            // Assert: YesToAll delegate invocation produces the YesToAll response
            result.Should().Be(YesNoToAllResponse.YesToAll);
        }
    }
}
