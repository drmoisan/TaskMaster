using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Deterministic unit tests for <see cref="StoreLockupResponder"/> (issue #264). All boundaries
    /// (F1's <see cref="IStoreDisableService"/>, <see cref="IUiDispatcher"/>, the notify seam, the
    /// WARN sink) are Moq/delegate seams: no live Outlook, no temp files, no real waits. The
    /// dispatcher mock is a synchronous pass-through so <c>BeginInvoke</c>'d work runs inline.
    /// </summary>
    [TestClass]
    public class StoreLockupResponderTests
    {
        private const string Identity = "Mailbox A";

        private static LockupAttribution Attribution(string identity) =>
            new LockupAttribution(TimeSpan.FromMilliseconds(6000), identity);

        private static Mock<IUiDispatcher> PassThroughDispatcher()
        {
            var dispatcher = new Mock<IUiDispatcher>();
            dispatcher
                .Setup(d => d.BeginInvoke(It.IsAny<Action>()))
                .Callback<Action>(a => a())
                .Returns((IAsyncResult)null);
            return dispatcher;
        }

        [TestMethod]
        public void OnLockupDetected_ValidNotDisabled_DisablesThenNotifies_InOrder()
        {
            // Arrange
            var order = new List<string>();
            var disable = new Mock<IStoreDisableService>();
            disable.Setup(d => d.IsDisabled(It.IsAny<StoreIdentity>())).Returns(false);
            disable
                .Setup(d => d.DisableSessionOnly(It.IsAny<StoreIdentity>()))
                .Callback(() => order.Add("disable"));
            var dispatcher = PassThroughDispatcher();
            var notifyCount = 0;
            StoreLockupNotifier notify = (id, a, b, c) =>
            {
                notifyCount++;
                order.Add("notify");
            };
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify,
                logSink: _ => { }
            );

            // Act
            responder.OnLockupDetected(Attribution(Identity));

            // Assert: disable exactly once, notify exactly once, disable strictly before notify.
            disable.Verify(d => d.DisableSessionOnly(It.IsAny<StoreIdentity>()), Times.Once);
            notifyCount.Should().Be(1);
            order.Should().Equal("disable", "notify");
        }

        [TestMethod]
        public void OnLockupDetected_NoContext_DoesNothing()
        {
            // Arrange
            var disable = new Mock<IStoreDisableService>(MockBehavior.Strict);
            var dispatcher = new Mock<IUiDispatcher>(MockBehavior.Strict);
            var notifyCount = 0;
            StoreLockupNotifier notify = (id, a, b, c) => notifyCount++;
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify,
                logSink: _ => { }
            );

            // Act: null identity == "no context"
            responder.OnLockupDetected(Attribution(null));

            // Assert: no disable, no notify, no dispatch (strict mocks would throw on any call).
            notifyCount.Should().Be(0);
            disable.VerifyNoOtherCalls();
            dispatcher.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void OnLockupDetected_AlreadyDisabled_DoesNotDisableOrNotifyAgain()
        {
            // Arrange
            var disable = new Mock<IStoreDisableService>();
            disable.Setup(d => d.IsDisabled(It.IsAny<StoreIdentity>())).Returns(true);
            var dispatcher = PassThroughDispatcher();
            var notifyCount = 0;
            StoreLockupNotifier notify = (id, a, b, c) => notifyCount++;
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify,
                logSink: _ => { }
            );

            // Act
            responder.OnLockupDetected(Attribution(Identity));

            // Assert
            disable.Verify(d => d.DisableSessionOnly(It.IsAny<StoreIdentity>()), Times.Never);
            notifyCount.Should().Be(0);
            dispatcher.Verify(d => d.BeginInvoke(It.IsAny<Action>()), Times.Never);
        }

        [TestMethod]
        public void OnLockupDetected_NotifyIsDispatchedViaBeginInvoke_NeverInvoke()
        {
            // Arrange
            var disable = new Mock<IStoreDisableService>();
            disable.Setup(d => d.IsDisabled(It.IsAny<StoreIdentity>())).Returns(false);
            var dispatcher = PassThroughDispatcher();
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify: (id, a, b, c) => { },
                logSink: _ => { }
            );

            // Act
            responder.OnLockupDetected(Attribution(Identity));

            // Assert
            dispatcher.Verify(d => d.BeginInvoke(It.IsAny<Action>()), Times.Once);
            dispatcher.Verify(d => d.Invoke(It.IsAny<Action>()), Times.Never);
        }

        [TestMethod]
        public void OnLockupDetected_EmitsOneStoreLockupWarnLine()
        {
            // Arrange
            var disable = new Mock<IStoreDisableService>();
            disable.Setup(d => d.IsDisabled(It.IsAny<StoreIdentity>())).Returns(false);
            var dispatcher = PassThroughDispatcher();
            var logs = new List<string>();
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify: (id, a, b, c) => { },
                logSink: logs.Add
            );

            // Act
            responder.OnLockupDetected(Attribution(Identity));

            // Assert: exactly one [store-lockup] line, carrying identity and auto-disable outcome.
            logs.Should().ContainSingle();
            logs[0]
                .Should()
                .Be("[store-lockup] identity=Mailbox A stallMs=6000.0 autoDisabled=true");
        }

        [TestMethod]
        public void OnLockupDetected_ReenableButtonAction_CallsF1ReenableAsync_NoDirectF3()
        {
            // Arrange: capture the three button actions handed to the notify seam and invoke each,
            // asserting they route to the correct F1 call (F4 makes no direct F3 call).
            var disable = new Mock<IStoreDisableService>();
            disable.Setup(d => d.IsDisabled(It.IsAny<StoreIdentity>())).Returns(false);
            disable
                .Setup(d => d.ReenableAsync(It.IsAny<StoreIdentity>()))
                .Returns(Task.CompletedTask);
            var dispatcher = PassThroughDispatcher();
            Action captiveSession = null;
            Action captiveFuture = null;
            Action captiveReenable = null;
            StoreLockupNotifier notify = (id, a, b, c) =>
            {
                captiveSession = a;
                captiveFuture = b;
                captiveReenable = c;
            };
            var responder = new StoreLockupResponder(
                disable.Object,
                dispatcher.Object,
                notify,
                logSink: _ => { }
            );

            // Act
            responder.OnLockupDetected(Attribution(Identity));
            captiveFuture();
            captiveReenable();

            // Assert: the buttons route to F1's future-disable and reenable (which sequences F3).
            disable.Verify(d => d.DisableForFutureSessions(It.IsAny<StoreIdentity>()), Times.Once);
            disable.Verify(d => d.ReenableAsync(It.IsAny<StoreIdentity>()), Times.Once);
        }
    }
}
