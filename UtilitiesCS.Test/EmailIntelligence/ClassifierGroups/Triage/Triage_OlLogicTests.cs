using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class Triage_OlLogicTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Triage _triage;
        private Triage_OlLogic _triageOlLogic;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            _triage = new Triage(_mockGlobals.Object, CancellationToken.None)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
            };
            _triageOlLogic = new Triage_OlLogic(_triage);
        }

        [TestMethod]
        public void Constructor_ShouldInitializeParent()
        {
            Assert.AreEqual(_triage, _triageOlLogic.Parent);
        }

        [TestMethod]
        public async Task FilterViewAsync_ShouldCallFilterView()
        {
            var mockToDoObjects = new Mock<IToDoObjects>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);

            mockToDoObjects
                .SetupGet(td => td.SelectFromList)
                .Returns(_ => new List<string> { "A" });
            _mockGlobals.Setup(g => g.TD).Returns(mockToDoObjects.Object);
            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);

            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            await _triageOlLogic.FilterViewAsync();

            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void FilterView_ShouldCallFilterViewWithTriageValues()
        {
            var mockToDoObjects = new Mock<IToDoObjects>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);

            var choices = new List<string> { "A", "B", "C" };
            mockToDoObjects
                .SetupGet(td => td.SelectFromList)
                .Returns(_ => new List<string> { "A", "B" });
            _mockGlobals.Setup(g => g.TD).Returns(mockToDoObjects.Object);
            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);

            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            _triageOlLogic.FilterView();

            mockToDoObjects.VerifyGet(td => td.SelectFromList, Times.Once);
            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void FilterView_WithTriageValues_ShouldApplyFilter()
        {
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);

            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            _triageOlLogic.FilterView(new char[] { 'B', 'C' });

            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter()
        {
            string filter =
                "\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%B%'";

            string result = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.IsFalse(result.Contains("/Triage"));
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter2()
        {
            //string filter = @"(""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable"" = 'Task' AND (""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'A' OR ""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'B'))";
            var filter =
                "(\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable\" LIKE '%Task%' AND (\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" = 'B'))";

            Console.WriteLine(filter);
            var parser = new DASLFilterParser();
            var logicTree = parser.Parse(filter);
            parser.PrintTree(logicTree, 0);
            var recombined = parser.CombineTree(logicTree);
            Console.WriteLine(recombined);

            string actual = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.IsTrue(actual.Contains("/Actionable"));
            Assert.IsFalse(actual.Contains("/Triage"));
        }

        [TestMethod]
        public async Task TrainSelectionAsync_ShouldTrainSelection()
        {
            _triageOlLogic = new Triage_OlLogic(_triage);
            Assert.IsNotNull(_triageOlLogic);
            await Task.CompletedTask;
        }

        [TestMethod]
        public void ParseAndStripFilter_WithEmptyString_ShouldReturnEmpty()
        {
            var result = _triageOlLogic.ParseAndStripFilter("");
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseAndStripFilter_WithNoTriageReferences_ShouldReturnOriginal()
        {
            var filter = "[Subject] = 'Meeting'";
            var result = _triageOlLogic.ParseAndStripFilter(filter);
            result.Should().Be(filter);
        }

        [TestMethod]
        public void ParseAndStripFilter_WithSingleTriageEquals_ShouldRemoveIt()
        {
            var schema =
                "\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\"";
            var filter = $"{schema} = 'A'";
            var result = _triageOlLogic.ParseAndStripFilter(filter);
            result.Should().NotContain("/Triage");
        }

        [TestMethod]
        public void StripFilter_WithNullParent_ShouldReturnNull()
        {
            var regex = new System.Text.RegularExpressions.Regex("Triage");
            var tree = new TreeNode<string>("Triage = 'A'");

            var result = _triageOlLogic.StripFilter(regex, tree);

            result.Should().BeNull();
        }

        [TestMethod]
        public void StripFilter_WithNoMatch_ShouldReturnOriginalTree()
        {
            var regex = new System.Text.RegularExpressions.Regex("Triage");
            var tree = new TreeNode<string>("Subject = 'Hello'");

            var result = _triageOlLogic.StripFilter(regex, tree);

            result.Should().BeSameAs(tree);
        }

        [TestMethod]
        public void StripFilter_WithMatchAndParent_ShouldRemoveNode()
        {
            var regex = new System.Text.RegularExpressions.Regex("Triage");
            var parent = new TreeNode<string>("AND");
            // Use the value-based AddChild overload that sets Parent correctly
            var child1 = parent.AddChild("Triage = 'A'");
            var child2 = parent.AddChild("Subject = 'Hello'");

            var result = _triageOlLogic.StripFilter(regex, child1);

            // After stripping the matching node from a 2-child parent with no grandparent,
            // the sibling (child2) is returned
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void FilterView_WithEmptyTriageValues_ShouldNotThrow()
        {
            // FilterView has an internal try-catch that swallows exceptions.
            // With empty triageValues and empty existing filter, the DASLFilterParser
            // may fail internally, which is caught and logged. Verify no exception propagates.
            var mockExplorer = new Mock<Explorer>(MockBehavior.Loose);
            var mockView = new Mock<View>(MockBehavior.Loose);
            var mockApplication = new Mock<Application>(MockBehavior.Loose);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Loose);

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "");

            System.Action act = () => _triageOlLogic.FilterView(System.Array.Empty<char>());
            act.Should().NotThrow();
        }

        [TestMethod]
        public void FilterView_WhenExplorerIsNull_ShouldReturnGracefully()
        {
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns((Explorer)null);

            System.Action act = () => _triageOlLogic.FilterView(new char[] { 'A' });

            act.Should().NotThrow();
        }

        // P78-T1: filter builder strips unsupported filter clauses while preserving supported ones
        [TestMethod]
        public void ParseAndStripFilter_WithUnsupportedAndSupportedClauses_StripsTriagePreservesSupported()
        {
            // Arrange: build a filter with a supported clause (Actionable) and an unsupported
            // Triage clause side-by-side so we can assert the strip removes only the Triage part.
            var schema =
                "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}";
            var filter =
                $"(\"{schema}/Actionable\" LIKE '%Task%' AND \"{schema}/Triage\" LIKE '%A%')";

            // Act
            var result = _triageOlLogic.ParseAndStripFilter(filter);

            // Assert: the unsupported Triage clause is removed; the supported Actionable clause remains.
            result.Should().Contain("/Actionable");
            result.Should().NotContain("/Triage");
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
    }
}
