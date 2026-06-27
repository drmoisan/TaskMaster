using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcDatamodelTests
    {
        private static QfcDatamodel CreateQueueAdmissionModel(
            bool highConfidenceEnabled,
            double threshold,
            IList<MailItem> added,
            IList<MailItem> hooked,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader
        )
        {
            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(highConfidenceEnabled);
            if (highConfidenceEnabled)
            {
                settings.SetupGet(x => x.HighConfidenceThreshold).Returns(threshold);
            }

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            var model = (QfcDatamodel)
                FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));
            model
                .GetType()
                .GetField(
                    "_globals",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(model, globals.Object);
            model.RemainingQueueScoreLoader = scoreLoader;
            model.MasterQueueAddLastLoader = added.Add;
            model.MoveMonitorHookLoader = (mail, _) => hooked.Add(mail);
            return model;
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_ScoresBeforeQueueAdmission()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var scoreCallCount = 0;
            var model = CreateQueueAdmissionModel(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, _) =>
                {
                    scoreCallCount++;
                    added.Should().BeEmpty("admission must wait until scoring completes");
                    mail.Should().BeSameAs(mailItem);
                    return Task.FromResult(950L);
                }
            );

            // Act
            var queued = await model.TryQueueRemainingMailItemAsync(
                mailItem,
                CancellationToken.None
            );

            // Assert
            queued.Should().BeTrue();
            scoreCallCount.Should().Be(1);
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_ScoreEqualsThreshold_AddsAndHooksMailItem()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var model = CreateQueueAdmissionModel(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) => Task.FromResult(900L)
            );

            // Act
            var queued = await model.TryQueueRemainingMailItemAsync(
                mailItem,
                CancellationToken.None
            );

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_ScoreBelowThreshold_DoesNotAddOrHookMailItem()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var model = CreateQueueAdmissionModel(
                highConfidenceEnabled: true,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) => Task.FromResult(899L)
            );

            // Act
            var queued = await model.TryQueueRemainingMailItemAsync(
                mailItem,
                CancellationToken.None
            );

            // Assert
            queued.Should().BeFalse();
            added.Should().BeEmpty();
            hooked.Should().BeEmpty();
        }

        [TestMethod]
        public async Task TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring()
        {
            // Arrange
            var added = new List<MailItem>();
            var hooked = new List<MailItem>();
            var mailItem = new Mock<MailItem>().Object;
            var model = CreateQueueAdmissionModel(
                highConfidenceEnabled: false,
                threshold: 0.90,
                added,
                hooked,
                (mail, token) =>
                    throw new AssertFailedException("Scoring should not run when disabled.")
            );

            // Act
            var queued = await model.TryQueueRemainingMailItemAsync(
                mailItem,
                CancellationToken.None
            );

            // Assert
            queued.Should().BeTrue();
            added.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
            hooked.Should().ContainSingle().Which.Should().BeSameAs(mailItem);
        }
    }
}
