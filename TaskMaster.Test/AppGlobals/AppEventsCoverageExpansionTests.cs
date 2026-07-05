using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Coverage expansion for AppEvents event registration, dispatch, cleanup, and exception
    /// routing paths using mocked Outlook collaborators only.
    /// </summary>
    [TestClass]
    public class AppEventsCoverageExpansionTests
    {
        [TestMethod]
        public void OlToDoItems_WhenAssigned_RegistersItemAddAndItemChangeHandlers()
        {
            // Arrange
            var sut = new AppEvents(CreateGlobals().Object);
            var items = CreateItemsMock();

            // Act
            SetOlToDoItems(sut, items.Object);

            // Assert
            items.VerifyAdd(
                x => x.ItemAdd += It.IsAny<ItemsEvents_ItemAddEventHandler>(),
                Times.Once
            );
            items.VerifyAdd(
                x => x.ItemChange += It.IsAny<ItemsEvents_ItemChangeEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void Unhook_WhenHandlersRegistered_RemovesHandlersAndCanRunTwice()
        {
            // Arrange
            var sut = new AppEvents(CreateGlobals().Object);
            var items = CreateItemsMock();
            SetOlToDoItems(sut, items.Object);

            // Act
            sut.Unhook();
            sut.Unhook();

            // Assert
            sut.OlToDoItems.Should().BeNull();
            items.VerifyRemove(
                x => x.ItemAdd -= It.IsAny<ItemsEvents_ItemAddEventHandler>(),
                Times.Once
            );
            items.VerifyRemove(
                x => x.ItemChange -= It.IsAny<ItemsEvents_ItemChangeEventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void Unhook_WhenNoHandlersRegistered_DoesNotThrow()
        {
            // Arrange
            var sut = new AppEvents(CreateGlobals().Object);

            // Act
            System.Action act = () => sut.Unhook();

            // Assert
            act.Should().NotThrow();
            sut.OlToDoItems.Should().BeNull();
        }

        [TestMethod]
        public void OlInboxItemsItemAdd_WhenProcessingThrows_RethrowsThroughSynchronizationContext()
        {
            // Arrange
            var expected = new InvalidOperationException("engines unavailable");
            var globals = CreateGlobals();
            globals.SetupGet(x => x.Engines).Throws(expected);
            var sut = new AppEvents(globals.Object);
            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            var context = new CapturingSynchronizationContext();
            var originalContext = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(context);

                // Act
                sut.OlInboxItems_ItemAdd(mailItem.Object);

                // Assert
                context.CapturedException.Should().BeSameAs(expected);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        [TestMethod]
        public void OlInboxItemsItemAdd_WhenProcessingNonMailItem_CompletesWithoutException()
        {
            // Arrange
            var sut = new AppEvents(CreateGlobals().Object);
            var context = new CapturingSynchronizationContext();
            var originalContext = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(context);

                // Act
                sut.OlInboxItems_ItemAdd(new object());

                // Assert
                context.CapturedException.Should().BeNull();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        private static Mock<IApplicationGlobals> CreateGlobals()
        {
            return new Mock<IApplicationGlobals>(MockBehavior.Strict);
        }

        private static Mock<Items> CreateItemsMock()
        {
            var items = new Mock<Items>(MockBehavior.Strict);
            items.SetupAdd(x => x.ItemAdd += It.IsAny<ItemsEvents_ItemAddEventHandler>());
            items.SetupAdd(x => x.ItemChange += It.IsAny<ItemsEvents_ItemChangeEventHandler>());
            items.SetupRemove(x => x.ItemAdd -= It.IsAny<ItemsEvents_ItemAddEventHandler>());
            items.SetupRemove(x => x.ItemChange -= It.IsAny<ItemsEvents_ItemChangeEventHandler>());
            return items;
        }

        private static void SetOlToDoItems(AppEvents sut, Items items)
        {
            var setter = typeof(AppEvents)
                .GetProperty(
                    nameof(AppEvents.OlToDoItems),
                    BindingFlags.Instance | BindingFlags.Public
                )!
                .GetSetMethod(true);
            setter.Should().NotBeNull();
            setter!.Invoke(sut, [items]);
        }

        private sealed class CapturingSynchronizationContext : SynchronizationContext
        {
            internal System.Exception CapturedException { get; private set; }

            public override void Post(SendOrPostCallback callback, object state)
            {
                try
                {
                    callback(state);
                }
                catch (System.Exception ex)
                {
                    CapturedException = ex;
                }
            }
        }
    }
}
