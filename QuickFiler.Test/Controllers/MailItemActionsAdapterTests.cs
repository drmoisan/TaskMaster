using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Adapter forwarding tests (cycle-2 Phase 6, P6-T6/P6-T12). The <see cref="MailItemActionsAdapter"/>
    /// is a thin 1:1 shim over a live <see cref="MailItem"/>; because <c>MailItem</c> is itself an
    /// interface it can be mocked, so each forward is verified directly.
    /// </summary>
    [TestClass]
    public class MailItemActionsAdapterTests
    {
        private static (MailItemActionsAdapter adapter, Mock<MailItem> mail) Build()
        {
            var mail = new Mock<MailItem>();
            return (new MailItemActionsAdapter(mail.Object), mail);
        }

        [TestMethod]
        public void Reply_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();
            var reply = new Mock<MailItem>();
            mail.Setup(m => m.Reply()).Returns(reply.Object);

            adapter.Reply().Should().BeSameAs(reply.Object);
            mail.Verify(m => m.Reply(), Times.Once());
        }

        [TestMethod]
        public void ReplyAll_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();
            var reply = new Mock<MailItem>();
            mail.Setup(m => m.ReplyAll()).Returns(reply.Object);

            adapter.ReplyAll().Should().BeSameAs(reply.Object);
            mail.Verify(m => m.ReplyAll(), Times.Once());
        }

        [TestMethod]
        public void Forward_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();
            var fwd = new Mock<MailItem>();
            mail.Setup(m => m.Forward()).Returns(fwd.Object);

            adapter.Forward().Should().BeSameAs(fwd.Object);
            mail.Verify(m => m.Forward(), Times.Once());
        }

        [TestMethod]
        public void Display_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();

            adapter.Display();

            mail.Verify(m => m.Display(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void Save_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();

            adapter.Save();

            mail.Verify(m => m.Save(), Times.Once());
        }

        [TestMethod]
        public void UnRead_GetAndSet_ForwardToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();
            mail.SetupGet(m => m.UnRead).Returns(true);

            adapter.UnRead.Should().BeTrue();
            adapter.UnRead = false;
            mail.VerifySet(m => m.UnRead = false, Times.Once());
        }

        [TestMethod]
        public void EntryID_ForwardsToUnderlyingMailItem()
        {
            var (adapter, mail) = Build();
            mail.SetupGet(m => m.EntryID).Returns("entry-99");

            adapter.EntryID.Should().Be("entry-99");
        }
    }
}
