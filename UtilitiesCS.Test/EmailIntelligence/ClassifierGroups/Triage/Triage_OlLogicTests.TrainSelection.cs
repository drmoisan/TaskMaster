using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public partial class Triage_OlLogicTests
    {
        [TestMethod]
        public async Task TrainSelectionAsync_ShouldTrainSelection()
        {
            _triageOlLogic = new Triage_OlLogic(_triage);
            Assert.IsNotNull(_triageOlLogic);
            await Task.CompletedTask;
        }

        // P78-T2: TrainSelectionAsync skips an empty selection (returns null) without throwing
        // and does not invoke the classifier's train method.
        [TestMethod]
        public async Task TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining()
        {
            // Arrange: mock the globals chain so ActiveExplorer() returns null,
            // which means Selection is null — the method must return early.
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns((Explorer)null);

            // Capture the initial email-count state of the classifier group so we can verify
            // no training was applied after the call.
            var classifierGroup = _triage.ClassifierGroup;
            int emailCountBefore = classifierGroup.TotalEmailCount;

            // Act
            Func<Task> act = () => _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None);

            // Assert: the method completes without error and the classifier state is unchanged.
            await act.Should().NotThrowAsync();
            classifierGroup.TotalEmailCount.Should().Be(emailCountBefore);
        }

        // P78-T3: TrainSelectionAsync maps each selected MailItem to a training example
        // and forwards it to the classifier under the supplied triage label.
        [TestMethod]
        public async Task TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel()
        {
            // Arrange: build the full globals → Ol → App → ActiveExplorer → Selection chain
            // so TrainSelectionAsync finds a non-null Selection and can enumerate one MailItem.
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockSelection = new Mock<Selection>(MockBehavior.Loose);

            // A loose MailItem mock is sufficient because SetUdf (this MailItem overload) is
            // wrapped in try-catch and swallows any COM-access exceptions. The only lazy field
            // that can throw is _attachmentsHelper, so we must return a non-null Attachments
            // object whose enumerator returns an empty sequence.
            var mockMailItem = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem.Setup(m => m.Attachments).Returns(mockAttachments.Object);
            mockAttachments
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());

            // Expose the single MailItem through Selection's IEnumerable interface so
            // the Cast<object>().Where(x => x is MailItem) pipeline in TrainSelectionAsync
            // produces one element.
            mockSelection
                .As<IEnumerable>()
                .Setup(s => s.GetEnumerator())
                .Returns(new List<object> { mockMailItem.Object }.GetEnumerator());

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            // EmailPrefixToStrip is accessed lazily by the tokenizer via CompressPlainText;
            // the strict mock requires an explicit setup to avoid an unexpected-call exception.
            mockOlObjects.Setup(o => o.EmailPrefixToStrip).Returns("");
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.Selection).Returns(mockSelection.Object);

            int emailCountBefore = _triage.ClassifierGroup.TotalEmailCount;

            // Act
            await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None);

            // Assert: training was applied for the "A" label — TotalEmailCount increments
            // once per MailItem processed, even when the item produces empty tokens.
            _triage.ClassifierGroup.TotalEmailCount.Should().BeGreaterThan(emailCountBefore);
            _triage.ClassifierGroup.Classifiers.Should().ContainKey("A");
        }

        // #137 regression: in Outlook conversation view, Selection may contain the entire thread.
        // The fix deduplicates by ConversationID so that only one item per conversation is trained,
        // so TotalEmailCount increments by exactly 1 even when Selection has 2 items with the same ConversationID.
        [TestMethod]
        public async Task TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce()
        {
            // Arrange: two items in mock Selection simulating a conversation-view thread click.
            // Only the first item must be processed after the fix.
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockSelection = new Mock<Selection>(MockBehavior.Loose);

            var mockMailItem1 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments1 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem1.Setup(m => m.Attachments).Returns(mockAttachments1.Object);
            mockAttachments1
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());

            var mockMailItem2 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments2 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem2.Setup(m => m.Attachments).Returns(mockAttachments2.Object);
            mockAttachments2
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());

            // Two items in Selection — simulates conversation view auto-selection of thread items.
            mockSelection
                .As<IEnumerable>()
                .Setup(s => s.GetEnumerator())
                .Returns(
                    new List<object> { mockMailItem1.Object, mockMailItem2.Object }.GetEnumerator()
                );

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockOlObjects.Setup(o => o.EmailPrefixToStrip).Returns("");
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.Selection).Returns(mockSelection.Object);

            int emailCountBefore = _triage.ClassifierGroup.TotalEmailCount;

            // Act
            await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None);

            // Assert: only one item per ConversationID must be trained; the duplicate thread item
            // (added by Outlook conversation view) must not be processed.
            _triage.ClassifierGroup.TotalEmailCount.Should().Be(emailCountBefore + 1);
        }

        // #137 regression: in Outlook conversation view, Selection may contain the entire thread.
        // The fix deduplicates by ConversationID so that only one item per conversation contributes
        // to MatchEmailCount; conversation thread duplicates must not be counted.
        [TestMethod]
        public async Task TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce()
        {
            // Arrange: two items in mock Selection simulating a conversation-view thread click.
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockSelection = new Mock<Selection>(MockBehavior.Loose);

            var mockMailItem1 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments1 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem1.Setup(m => m.Attachments).Returns(mockAttachments1.Object);
            mockAttachments1
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());

            var mockMailItem2 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments2 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem2.Setup(m => m.Attachments).Returns(mockAttachments2.Object);
            mockAttachments2
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());

            mockSelection
                .As<IEnumerable>()
                .Setup(s => s.GetEnumerator())
                .Returns(
                    new List<object> { mockMailItem1.Object, mockMailItem2.Object }.GetEnumerator()
                );

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockOlObjects.Setup(o => o.EmailPrefixToStrip).Returns("");
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.Selection).Returns(mockSelection.Object);

            // Default to 0 if the "A" classifier does not yet exist on a fresh ClassifierGroup.
            int matchCountBefore = _triage.ClassifierGroup.Classifiers.TryGetValue(
                "A",
                out var classifierBefore
            )
                ? classifierBefore.MatchEmailCount
                : 0;

            // Act
            await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None);

            // Assert: only one item per ConversationID must be counted; MatchEmailCount increments
            // by 1 (not 2), because the duplicate conversation-thread item must not be trained.
            _triage
                .ClassifierGroup.Classifiers["A"]
                .MatchEmailCount.Should()
                .Be(matchCountBefore + 1);
        }

        // Issue #183 regression: writing the Triage UDF and training the classifier are two
        // separate concerns. When a conversation-view selection contains multiple MailItems that
        // share the same ConversationID, the Triage UDF must be written to EVERY selected item
        // (AC1), while the Bayesian classifier is still trained at most once per ConversationID
        // (AC2, preserving the #137 dedup). The pre-fix code dedups the whole loop, so the second
        // item is skipped entirely and never receives the UDF.
        [TestMethod]
        public async Task TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem()
        {
            // Arrange: two items in mock Selection simulating a conversation-view thread click.
            // Both leave ConversationID unstubbed so they return the same default value and group
            // together under .GroupBy(m => m.ConversationID).
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockSelection = new Mock<Selection>(MockBehavior.Loose);

            // Observation seam: SetUdf("Triage", "A") is an extension method and CANNOT be verified
            // directly via Moq. On a MailItem mock it binds to the MailItem SetUdf overload, which
            // exercises only MailItem.UserProperties (Find/Add), property.Value assignment, and
            // MailItem.Save(). We stub the UserProperties chain so the write path completes and
            // reaches Save(), then verify the interceptable MailItem.Save() member as the observable
            // proxy for the swallowed SetUdf extension write on each item.
            var mockMailItem1 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments1 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem1.Setup(m => m.Attachments).Returns(mockAttachments1.Object);
            mockAttachments1
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());
            var mockUserProperties1 = new Mock<UserProperties>(MockBehavior.Loose);
            var mockUserProperty1 = new Mock<UserProperty>(MockBehavior.Loose);
            mockUserProperty1.SetupAllProperties();
            mockUserProperties1
                .Setup(p => p.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockUserProperty1.Object);
            mockMailItem1.Setup(m => m.UserProperties).Returns(mockUserProperties1.Object);
            mockMailItem1.Setup(m => m.Save());

            var mockMailItem2 = new Mock<MailItem>(MockBehavior.Loose);
            var mockAttachments2 = new Mock<Attachments>(MockBehavior.Loose);
            mockMailItem2.Setup(m => m.Attachments).Returns(mockAttachments2.Object);
            mockAttachments2
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(new List<Attachment>().GetEnumerator());
            var mockUserProperties2 = new Mock<UserProperties>(MockBehavior.Loose);
            var mockUserProperty2 = new Mock<UserProperty>(MockBehavior.Loose);
            mockUserProperty2.SetupAllProperties();
            mockUserProperties2
                .Setup(p => p.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockUserProperty2.Object);
            mockMailItem2.Setup(m => m.UserProperties).Returns(mockUserProperties2.Object);
            mockMailItem2.Setup(m => m.Save());

            // Two items in Selection — simulates conversation view auto-selection of thread items.
            mockSelection
                .As<IEnumerable>()
                .Setup(s => s.GetEnumerator())
                .Returns(
                    new List<object> { mockMailItem1.Object, mockMailItem2.Object }.GetEnumerator()
                );

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockOlObjects.Setup(o => o.EmailPrefixToStrip).Returns("");
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.Selection).Returns(mockSelection.Object);

            int emailCountBefore = _triage.ClassifierGroup.TotalEmailCount;

            // Act
            await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None);

            // Assert (AC1): the Triage UDF write reached BOTH items. Save() is the chosen observable
            // proxy for the swallowed SetUdf extension write; each item must have been saved once.
            mockMailItem1.Verify(m => m.Save(), Times.Once);
            mockMailItem2.Verify(m => m.Save(), Times.Once);

            // Assert (AC2): training still dedups by ConversationID — TotalEmailCount increments by
            // exactly 1 for the multi-item single-conversation selection.
            _triage.ClassifierGroup.TotalEmailCount.Should().Be(emailCountBefore + 1);
        }
    }
}
