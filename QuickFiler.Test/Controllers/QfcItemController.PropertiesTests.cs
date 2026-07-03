using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Properties / INotifyPropertyChanged cluster tests (research §5.2). Covers the exposed
    /// value properties, the ItemIndex/ItemNumber invariant including the digit-formatting branches
    /// routed through the narrowed IItemViewer, TopFolderScore's null-handler default, SuppressEvents
    /// round-trip, and PropertyChanged notification.
    /// </summary>
    [TestClass]
    public class QfcItemController_PropertiesTests
    {
        private sealed class PropController : QfcItemController
        {
            internal PropController()
                : base() { }

            internal void RaiseNotify(string propertyName) => NotifyPropertyChanged(propertyName);
        }

        private static void SetViewer(QfcItemController controller, IItemViewer viewer) =>
            typeof(QfcItemController)
                .GetField("_itemViewer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, viewer);

        [TestMethod]
        public void TopFolderScore_WhenFolderHandlerNull_ReturnsZero()
        {
            var controller = new PropController();
            controller.TopFolderScore.Should().Be(0);
        }

        [TestMethod]
        public void ItemIndex_GetSet_IsOneLessThanItemNumber()
        {
            // The viewer is null, so the ItemNumber setter's guarded view write is skipped.
            var controller = new PropController();

            controller.ItemIndex = 4;

            controller.ItemNumber.Should().Be(5);
            controller.ItemIndex.Should().Be(4);
        }

        [TestMethod]
        public void SuppressEvents_RoundTrips()
        {
            var controller = new PropController();

            controller.SuppressEvents = true;
            controller.SuppressEvents.Should().BeTrue();
            controller.SuppressEvents = false;
            controller.SuppressEvents.Should().BeFalse();
        }

        [TestMethod]
        public void NotifyPropertyChanged_WithName_RaisesPropertyChanged()
        {
            var controller = new PropController();
            string raised = null;
            controller.PropertyChanged += (s, e) => raised = e.PropertyName;

            controller.RaiseNotify("ItemNumber");

            raised.Should().Be("ItemNumber");
        }

        [TestMethod]
        public void ScalarProperties_RoundTrip()
        {
            // Arrange / Act / Assert — exercise the simple exposed value properties.
            var controller = new PropController();

            controller.ConvOriginID = "origin-42";
            controller.ConvOriginID.Should().Be("origin-42");

            controller.CounterEnter = 7;
            controller.CounterEnter.Should().Be(7);

            controller.CounterComboRight = 9;
            controller.CounterComboRight.Should().Be(9);

            controller.IsChild = true;
            controller.IsChild.Should().BeTrue();

            controller.IsActiveUI = true;
            controller.IsActiveUI.Should().BeTrue();

            var token = new CancellationToken(false);
            controller.Token = token;
            controller.Token.Should().Be(token);
        }

        [TestMethod]
        public void ReadThroughProperties_ReflectBackingState()
        {
            // Arrange — defaults on a freshly constructed controller.
            var controller = new PropController();

            // Assert — read-only/derived properties return their backing defaults without a view.
            controller.IsExpanded.Should().BeFalse();
            controller.SelectedFolder.Should().BeNull();
            controller.Buttons.Should().BeNull();
            controller.ConversationResolver.Should().BeNull();
            controller.ListTipsDetails.Should().BeNull();
            controller.ListTipsExpanded.Should().BeNull();
            controller.TableLayoutPanels.Should().BeNull();
            controller.Parent.Should().BeNull();
            controller.ItemHelper.Should().BeNull();
        }

        [TestMethod]
        public void ItemNumber_WhenSingleDigit_WritesItemNumberTextThroughViewer()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            var controller = new PropController();
            SetViewer(controller, mock.Object);
            controller.ItemNumberDigits = 1;

            // Act
            controller.ItemNumber = 5;

            // Assert
            controller.ItemNumber.Should().Be(5);
            mock.VerifySet(v => v.ItemNumberText = "5", Times.AtLeastOnce());
        }

        [TestMethod]
        public void ItemNumber_WhenTwoDigit_WritesZeroPaddedItemNumberText()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            var controller = new PropController();
            SetViewer(controller, mock.Object);

            // Act — setting ItemNumberDigits to 2 re-renders the current number zero-padded.
            controller.ItemNumberDigits = 2;
            controller.ItemNumber = 3;

            // Assert
            controller.ItemNumberDigits.Should().Be(2);
            mock.VerifySet(v => v.ItemNumberText = "03", Times.AtLeastOnce());
        }

        [TestMethod]
        public void Height_DelegatesToViewerHeight()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.Height).Returns(123);
            var controller = new PropController();
            SetViewer(controller, mock.Object);

            // Act / Assert
            controller.Height.Should().Be(123);
        }
    }
}
