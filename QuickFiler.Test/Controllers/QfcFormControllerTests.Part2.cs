using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// High-confidence carrier-path tests for <c>QfcFormController</c>. Relocated here from
    /// <c>QfcFormControllerTests.cs</c>, which stood at 827 lines and is already over the 500-line
    /// cap, so it must not grow at all. The issue #678 widening of <see cref="QfcPreScoredItem"/>
    /// adds an argument to the construction below and CSharpier then reflows the call, which would
    /// have pushed that file further past its baseline count. No test is deleted or weakened by the
    /// move; the base part carries the only <c>[TestClass]</c> attribute.
    /// </summary>
    public partial class QfcFormControllerTests
    {
        /// <summary>
        /// [P4-T6] The carrier-list <see cref="QfcFormController.LoadItemsAsync(IList{QfcPreScoredItem})"/>
        /// path never invokes the post-UI removal pass
        /// (<see cref="QfcCollectionController.RemoveBelowThresholdAsync"/> via
        /// <see cref="QfcFormController.ApplyHighConfidenceFilterAsync"/>). Because the carrier
        /// overload constructs a real <see cref="QfcCollectionController"/> internally (no DI seam at
        /// that point) which would require live WinForms/COM, this test exercises the overload via the
        /// guard short-circuit (`_states` is null because Init() is not called) with an injected
        /// collection-controller mock, and verifies no removal interaction occurs on the carrier path.
        /// The positive carrier-overload behavior (LoadControlsAndHandlers_01Async and the carried
        /// PredeterminedFolder) is verified at the collection-controller level in P4-T7 / P6-T2.
        /// </summary>
        [TestMethod]
        public async Task LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval()
        {
            // Arrange — high-confidence mode on so the disabled-path branch is not the reason.
            var settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(s => s.HighConfidenceThreshold).Returns(0.9);
            _mockGlobals.SetupGet(g => g.QfSettings).Returns(settings.Object);

            _controller = CreateQfcFormController();
            var mockGroups = new Mock<IQfcCollectionController>(MockBehavior.Strict);
            SetPrivateField(_controller, "_groups", mockGroups.Object);

            // Issue #678: the carrier now publishes the already-initialised folder search handler
            // as its third member, so this construction site populates it.
            var preScored = new List<QfcPreScoredItem>
            {
                new QfcPreScoredItem(
                    new Mock<MailItem>().Object,
                    @"\\A\folder",
                    new Mock<IFolderSearchHandler>().Object
                ),
            };

            // Act
            Func<Task> act = () => _controller.LoadItemsAsync(preScored);

            // Assert — no exception, and the post-UI removal pass is never invoked on the carrier path.
            await act.Should().NotThrowAsync();
            mockGroups.Verify(g => g.RemoveBelowThresholdAsync(It.IsAny<double>()), Times.Never);
        }
    }
}
