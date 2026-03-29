using System;
using System.Dynamic;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="AutoFile"/>.
    ///
    /// Purpose:
    ///     Verify the three testable logical paths in the static AutoFile helper:
    ///     (1) <see cref="AutoFile.AreConversationsGrouped"/> — COM Explorer query,
    ///     (2) the private <c>Category_IsAlreadySelected</c> guard,
    ///     (3) <see cref="AutoFile.AutoFindPeople"/> — person-lookup loop.
    ///
    /// Constraints:
    ///     AreConversationsGrouped requires a mocked Explorer + CommandBars COM chain.
    ///     Category_IsAlreadySelected is private static and exercised via reflection with a
    ///     synthetic dynamic ExpandoObject so no live Outlook COM objects are needed.
    ///     AutoFindPeople requires a MailItemHelper; TestMailItemHelper subclass overrides
    ///     ToRecipients/CcRecipients/Sender so no Outlook COM objects are needed.
    ///     blNotifyMissing=false prevents the MessageBox side-effect for missing recipients.
    ///     blExcludeFlagged=false bypasses Category_IsAlreadySelected (which requires a live
    ///     MailItem.Categories COM property) so the tests remain isolated and deterministic.
    /// </summary>
    [TestClass]
    public class AutoFile_Tests
    {
        /// <summary>
        /// Restores the real modal dialog invoker after each test so that any test
        /// using the <see cref="MyBox.DialogInvoker"/> seam cannot leak state.
        /// </summary>
        [TestCleanup]
        public void TestCleanup_ResetMyBoxDialogInvokerSeam()
        {
            MyBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

        #region Phase 8-T1: AreConversationsGrouped

        /// <summary>
        /// Verifies that AreConversationsGrouped returns true when the Office CommandBars
        /// ribbon mso query reports the conversation-grouping toggle is pressed.
        /// </summary>
        [TestMethod]
        public void AreConversationsGrouped_WhenGetPressedMsoReturnsTrue_ReturnsTrue()
        {
            // Arrange: chain mock Explorer → CommandBars → GetPressedMso
            var mockCommandBars = new Mock<CommandBars>(MockBehavior.Loose);
            mockCommandBars.Setup(cb => cb.GetPressedMso("ShowInConversations")).Returns(true);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Loose);
            mockExplorer.Setup(e => e.CommandBars).Returns(mockCommandBars.Object);

            // Act
            bool result = AutoFile.AreConversationsGrouped(mockExplorer.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that AreConversationsGrouped returns false when the CommandBars query
        /// reports the conversation-grouping toggle is not pressed.
        /// </summary>
        [TestMethod]
        public void AreConversationsGrouped_WhenGetPressedMsoReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var mockCommandBars = new Mock<CommandBars>(MockBehavior.Loose);
            mockCommandBars.Setup(cb => cb.GetPressedMso("ShowInConversations")).Returns(false);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Loose);
            mockExplorer.Setup(e => e.CommandBars).Returns(mockCommandBars.Object);

            // Act
            bool result = AutoFile.AreConversationsGrouped(mockExplorer.Object);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Phase 8-T2: Category_IsAlreadySelected (private, via reflection)

        /// <summary>
        /// Verifies that Category_IsAlreadySelected returns true when the target category
        /// is present in the comma-separated Categories string on the dynamic item.
        ///
        /// An ExpandoObject is used as a lightweight dynamic proxy for the Outlook MailItem
        /// COM object so the test remains isolated from Outlook.
        /// </summary>
        [TestMethod]
        public void CategoryIsAlreadySelected_WhenCategoryInList_ReturnsTrue()
        {
            // Arrange: synthetic dynamic item with known categories
            dynamic item = new ExpandoObject();
            item.Categories = "Cat1, Cat2, Cat3";

            // Invoke private static helper via reflection
            MethodInfo method = typeof(AutoFile).GetMethod(
                "Category_IsAlreadySelected",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // Act
            bool result = (bool)method.Invoke(null, new object[] { item, "Cat2" });

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that Category_IsAlreadySelected returns false when the target category
        /// is absent from the comma-separated Categories string on the dynamic item.
        /// </summary>
        [TestMethod]
        public void CategoryIsAlreadySelected_WhenCategoryNotInList_ReturnsFalse()
        {
            // Arrange
            dynamic item = new ExpandoObject();
            item.Categories = "Cat1, Cat2, Cat3";

            MethodInfo method = typeof(AutoFile).GetMethod(
                "Category_IsAlreadySelected",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // Act
            bool result = (bool)method.Invoke(null, new object[] { item, "Cat4" });

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Phase 8-T3: AutoFindPeople

        /// <summary>
        /// Verifies that AutoFindPeople returns the mapped person name when a recipient
        /// address is present in the people dictionary.
        ///
        /// Flow:
        ///     A TestMailItemHelper with a single To-recipient is constructed.
        ///     The mocked dictionary reports ContainsKey=true for that address.
        ///     blExcludeFlagged=false is used to bypass Category_IsAlreadySelected (which
        ///     requires a live Outlook MailItem.Categories COM call).
        ///     blNotifyMissing=false prevents the MessageBox side-effect.
        /// </summary>
        [TestMethod]
        public void AutoFindPeople_WhenRecipientAddressInDict_ReturnsMatchedPerson()
        {
            // Arrange: recipient whose address matches a dict entry
            var mockToRecipient = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockToRecipient.Setup(r => r.Address).Returns("alice@example.com");

            var mockSender = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockSender.Setup(r => r.Address).Returns("sender@example.com");

            var helper = new TestMailItemHelper(
                toRecipients: new[] { mockToRecipient.Object },
                ccRecipients: Array.Empty<IRecipientInfo>(),
                sender: mockSender.Object
            );

            var mockDict = new Mock<IScoDictionaryNew<string, string>>(MockBehavior.Loose);
            mockDict.Setup(d => d.ContainsKey("alice@example.com")).Returns(true);
            mockDict.Setup(d => d["alice@example.com"]).Returns("Alice Smith");

            // sender@example.com is not in the dict — it will land in strMissing but
            // blNotifyMissing=false means no MessageBox is shown
            mockDict.Setup(d => d.ContainsKey("sender@example.com")).Returns(false);

            // Act
            var result = AutoFile.AutoFindPeople(
                helper,
                mockDict.Object,
                blNotifyMissing: false,
                blExcludeFlagged: false
            );

            // Assert: the matched person appears exactly once
            result.Should().ContainSingle().Which.Should().Be("Alice Smith");
        }

        /// <summary>
        /// Verifies that AutoFindPeople returns an empty list when no recipient address
        /// matches any entry in the people dictionary.
        /// </summary>
        [TestMethod]
        public void AutoFindPeople_WhenNoRecipientAddressInDict_ReturnsEmptyList()
        {
            // Arrange: all addresses are absent from the dict
            var mockRecipient = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockRecipient.Setup(r => r.Address).Returns("unknown@example.com");

            var mockSender = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockSender.Setup(r => r.Address).Returns("sender@example.com");

            var helper = new TestMailItemHelper(
                toRecipients: new[] { mockRecipient.Object },
                ccRecipients: Array.Empty<IRecipientInfo>(),
                sender: mockSender.Object
            );

            var mockDict = new Mock<IScoDictionaryNew<string, string>>(MockBehavior.Loose);
            mockDict.Setup(d => d.ContainsKey(It.IsAny<string>())).Returns(false);

            // Act
            var result = AutoFile.AutoFindPeople(
                helper,
                mockDict.Object,
                blNotifyMissing: false,
                blExcludeFlagged: false
            );

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that AutoFindPeople reports missing recipients through the
        /// <see cref="MyBox.DialogInvoker"/> seam when notification is enabled.
        ///
        /// Purpose:
        ///     Exercises the missing-recipient aggregation branch and the final
        ///     MyBox warning dialog path without displaying a real modal dialog.
        ///
        /// Side Effects:
        ///     Temporarily replaces <see cref="MyBox.DialogInvoker"/> with a
        ///     capturing stub; <see cref="TestCleanup_ResetMyBoxDialogInvokerSeam"/>
        ///     restores the real implementation after the test.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void AutoFindPeople_WhenMissingRecipientsAndNotifyEnabled_ShowsUnknownRecipientsDialog()
        {
            // Arrange: one known sender and one unknown recipient force the warning path
            var mockRecipient = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockRecipient.Setup(r => r.Address).Returns("unknown@example.com");

            var mockSender = new Mock<IRecipientInfo>(MockBehavior.Strict);
            mockSender.Setup(r => r.Address).Returns("sender@example.com");

            var helper = new TestMailItemHelper(
                toRecipients: new[] { mockRecipient.Object },
                ccRecipients: Array.Empty<IRecipientInfo>(),
                sender: mockSender.Object
            );

            var mockDict = new Mock<IScoDictionaryNew<string, string>>(MockBehavior.Loose);
            mockDict.Setup(d => d.ContainsKey("unknown@example.com")).Returns(false);
            mockDict.Setup(d => d.ContainsKey("sender@example.com")).Returns(true);
            mockDict.Setup(d => d["sender@example.com"]).Returns("Sender Person");

            string capturedTitle = string.Empty;
            string capturedMessage = string.Empty;
            MyBox.DialogInvoker = viewer =>
            {
                capturedTitle = viewer.Text;
                capturedMessage = viewer.TextMessage.Text;
                return DialogResult.OK;
            };

            // Act
            var result = AutoFile.AutoFindPeople(
                helper,
                mockDict.Object,
                blNotifyMissing: true,
                blExcludeFlagged: false
            );

            // Assert
            result.Should().ContainSingle().Which.Should().Be("Sender Person");
            capturedTitle.Should().Be("Unknown Recipients");
            capturedMessage.Should().Be("Recipients not in list of people: unknown@example.com");
        }

        #endregion

        #region Test helper: injectable MailItemHelper subclass

        /// <summary>
        /// Lightweight MailItemHelper subclass that exposes settable recipient
        /// collections so unit tests can inject synthetic recipients without
        /// requiring live Outlook COM objects.
        ///
        /// Purpose:
        ///     Overrides ToRecipients, CcRecipients, and Sender so that AutoFindPeople
        ///     tests can run in isolation.  The default MailItemHelper() constructor is
        ///     used to initialise the base with safe-default lazy fields so no Outlook
        ///     object access is triggered during construction.
        /// </summary>
        private sealed class TestMailItemHelper : MailItemHelper
        {
            private readonly IRecipientInfo[] _toRecipients;
            private readonly IRecipientInfo[] _ccRecipients;

            /// <summary>
            /// Initialises a test helper with fully synthetic recipient data.
            /// </summary>
            /// <param name="toRecipients">Recipients in the To field; may be empty.</param>
            /// <param name="ccRecipients">Recipients in the CC field; may be empty.</param>
            /// <param name="sender">Sender recipient; may be null to represent no sender.</param>
            public TestMailItemHelper(
                IRecipientInfo[] toRecipients,
                IRecipientInfo[] ccRecipients,
                IRecipientInfo sender
            )
            {
                // Store synthetic collections for property overrides
                _toRecipients = toRecipients ?? Array.Empty<IRecipientInfo>();
                _ccRecipients = ccRecipients ?? Array.Empty<IRecipientInfo>();

                // Sender has a public set on MailItemHelper — assign directly
                Sender = sender;
            }

            /// <inheritdoc />
            public override IRecipientInfo[] ToRecipients => _toRecipients;

            /// <inheritdoc />
            public override IRecipientInfo[] CcRecipients => _ccRecipients;
        }

        #endregion
    }
}
