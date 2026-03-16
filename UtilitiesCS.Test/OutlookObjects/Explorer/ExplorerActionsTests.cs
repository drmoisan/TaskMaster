using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.Explorer
{
    [TestClass]
    public class ExplorerActionsTests
    {
        [TestMethod]
        public void GetCurrentItem_WithNullApplication_ThrowsArgumentNullException()
        {
            // Arrange
            System.Action act = () => UtilitiesCS.ExplorerActions.GetCurrentItem(null);

            // Act / Assert
            act.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("OlApp");
        }

        [TestMethod]
        public void GetCurrentItem_WithExplorerWindow_ReturnsSelectedItem()
        {
            // Arrange
            var application = new Mock<Application>();
            var explorer = new Mock<Microsoft.Office.Interop.Outlook.Explorer>();
            var selection = new Mock<Selection>();
            var expected = new object();

            application.Setup(x => x.ActiveWindow()).Returns(explorer.Object);
            application.Setup(x => x.ActiveExplorer()).Returns(explorer.Object);
            explorer.SetupGet(x => x.Selection).Returns(selection.Object);
            selection.Setup(x => x[0]).Returns(expected);

            // Act
            var result = UtilitiesCS.ExplorerActions.GetCurrentItem(application.Object);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void GetCurrentItem_WithInspectorWindow_ReturnsCurrentItem()
        {
            // Arrange
            var application = new Mock<Application>();
            var inspector = new Mock<Inspector>();
            var expected = new object();

            application.Setup(x => x.ActiveWindow()).Returns(inspector.Object);
            application.Setup(x => x.ActiveInspector()).Returns(inspector.Object);
            inspector.SetupGet(x => x.CurrentItem).Returns(expected);

            // Act
            var result = UtilitiesCS.ExplorerActions.GetCurrentItem(application.Object);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Readable_WithUnreadableMailItem_ReturnsNull()
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note.Secure");

            // Act
            var result = UtilitiesCS.ExplorerActions.Readable(mailItem.Object);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Readable_WithReadableItem_ReturnsOriginalItem()
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");

            // Act
            var result = UtilitiesCS.ExplorerActions.Readable(mailItem.Object);

            // Assert
            result.Should().BeSameAs(mailItem.Object);
        }
    }
}
