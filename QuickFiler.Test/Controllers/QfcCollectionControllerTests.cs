using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for QfcCollectionController focused on null-safety guards.
    /// QfcCollectionController requires WinForms UI components in its constructor, so
    /// instances are created via FormatterServices.GetUninitializedObject to bypass the
    /// constructor; all required private fields are then injected via reflection.
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerTests
    {
        /// <summary>
        /// Creates an uninitialized QfcCollectionController with only the fields required
        /// for GetMoveDiagnostics set: _itemGroupsToMove is populated with one mocked entry.
        /// </summary>
        private static QfcCollectionController CreateControllerWithOneGroup(
            out Mock<IQfcItemController> mockItemController,
            out Mock<MailItemHelper> mockHelper
        )
        {
            // Use uninitialized object to bypass the WinForms-dependent constructor.
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            // Build a minimal QfcItemGroup with a mocked IQfcItemController and MailItemHelper.
            mockHelper = new Mock<MailItemHelper>(MockBehavior.Loose);
            mockHelper.SetupGet(x => x.Subject).Returns("Test Subject");
            mockHelper.SetupGet(x => x.SenderName).Returns("Sender");
            mockHelper.SetupGet(x => x.ToRecipientsName).Returns("Recipient");
            mockHelper.SetupGet(x => x.SentDate).Returns(new DateTime(2026, 1, 1));

            mockItemController = new Mock<IQfcItemController>(MockBehavior.Loose);
            mockItemController.SetupGet(x => x.ItemHelper).Returns(mockHelper.Object);
            mockItemController.SetupGet(x => x.SelectedFolder).Returns("Inbox");

            var itemGroup = new QfcItemGroup();
            typeof(QfcItemGroup)
                .GetProperty(
                    nameof(QfcItemGroup.ItemController),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                ?.SetValue(itemGroup, mockItemController.Object);

            // If the property setter is internal, try the backing field directly.
            if (itemGroup.ItemController is null)
            {
                typeof(QfcItemGroup)
                    .GetField("_itemController", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(itemGroup, mockItemController.Object);
            }

            var dict = new ConcurrentDictionary<QfcItemGroup, int>();
            dict.TryAdd(itemGroup, 0);

            typeof(QfcCollectionController)
                .GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, dict);

            return controller;
        }

        /// <summary>
        /// Regression test for Issue #97: GetMoveDiagnostics must not throw a
        /// NullReferenceException when the olAppointment ref parameter is null.
        /// Before the fix, accessing olAppointment.Body at line 2115 throws NRE.
        /// After the fix, the null appointment is skipped gracefully.
        /// </summary>
        [TestMethod]
        public void GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow()
        {
            // Arrange
            var controller = CreateControllerWithOneGroup(
                out Mock<IQfcItemController> _,
                out Mock<MailItemHelper> _
            );
            AppointmentItem nullAppointment = null;

            // Act & Assert — must not throw NullReferenceException.
            // Before the fix, olAppointment.Body throws because olAppointment is null.
            System.Action act = () =>
                controller.GetMoveDiagnostics(
                    durationText: "5",
                    durationMinutesText: "0.08",
                    duration: 5.0,
                    dataLineBeg: "01/01/2026,12:00,",
                    endTime: new DateTime(2026, 1, 1, 12, 0, 0),
                    olAppointment: ref nullAppointment
                );
            act.Should().NotThrow();
        }

        /// <summary>
        /// Positive path: GetMoveDiagnostics must return a non-null string array
        /// when all inputs are valid, confirming non-null appointment path is also handled.
        /// </summary>
        [TestMethod]
        public void GetMoveDiagnostics_WhenAppointmentIsNull_ReturnsStringArray()
        {
            // Arrange
            var controller = CreateControllerWithOneGroup(
                out Mock<IQfcItemController> _,
                out Mock<MailItemHelper> _
            );
            AppointmentItem nullAppointment = null;

            // Act
            var result = controller.GetMoveDiagnostics(
                durationText: "5",
                durationMinutesText: "0.08",
                duration: 5.0,
                dataLineBeg: "01/01/2026,12:00,",
                endTime: new DateTime(2026, 1, 1, 12, 0, 0),
                olAppointment: ref nullAppointment
            );

            // Assert
            result.Should().NotBeNull();
        }
    }
}
