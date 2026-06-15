using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the pure, Outlook-free surface of <see cref="FilerQueue"/> and
    /// <see cref="FilerQueueItem"/>. Only the queue item's construction/validation contract and the
    /// FilerQueue default consumer state are exercised. The FilerQueue.Enqueue/ConsumeAsync path is
    /// intentionally NOT exercised because it dispatches to <c>EmailFiler.SortAsync</c> on a
    /// background task (Outlook-bound and non-deterministic); that exclusion is recorded in the
    /// seam-verification evidence.
    /// </summary>
    [TestClass]
    public class FilerQueueTests
    {
        private static List<MailItemHelper> OneHelper() =>
            new List<MailItemHelper> { new MailItemHelper() };

        [TestMethod]
        public void FilerQueueItem_Constructor_StoresFilerAndHelpers()
        {
            // Arrange
            var filer = new EmailFiler();
            var helpers = OneHelper();

            // Act
            var item = new FilerQueueItem(filer, helpers);

            // Assert
            item.Filer.Should().BeSameAs(filer);
            item.Helpers.Should().BeSameAs(helpers);
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_NullFiler_ThrowsArgumentNullException()
        {
            // Arrange / Act
            Action act = () => new FilerQueueItem(null, OneHelper());

            // Assert
            act.Should().Throw<ArgumentNullException>("a null filer is rejected by ThrowIfNull");
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_NullHelpers_ThrowsArgumentNullException()
        {
            // Arrange / Act
            Action act = () => new FilerQueueItem(new EmailFiler(), null);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("a null helpers list is rejected by ThrowIfNull");
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_HelpersContainingNull_ThrowsArgumentNullException()
        {
            // Arrange: a non-null list whose element is null hits the explicit any-null guard.
            var helpers = new List<MailItemHelper> { null };

            // Act
            Action act = () => new FilerQueueItem(new EmailFiler(), helpers);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("a null element inside the helpers list is rejected");
        }

        [TestMethod]
        public void FilerQueue_NewInstance_HasCompletedConsumerByDefault()
        {
            // Arrange / Act
            var queue = new FilerQueue();

            // Assert
            queue.Consumer.Should().NotBeNull();
            queue
                .Consumer.IsCompleted.Should()
                .BeTrue("a fresh FilerQueue exposes Task.CompletedTask as its consumer");
        }
    }
}
