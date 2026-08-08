using System;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers.Tests;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #438: the AC-6 end-to-end typing scenario, held on a second partial-class part so the
    /// primary <c>BreadcrumbDropDownSearchIntegrationTests.cs</c> stays under the repositorys
    /// 500-line ceiling. No <c>[TestClass]</c> attribute is repeated here: it is declared once on
    /// the primary partial (<c>TestClassAttribute</c> is <c>AllowMultiple = false</c>).
    /// </summary>
    public sealed partial class BreadcrumbDropDownSearchIntegrationTests
    {
        /// <summary>
        /// AC-6: a multi-character search string delivered keystroke-by-keystroke through the real
        /// controller-to-viewer seam reaches <c>SearchText</c> in full, and the presented row set
        /// reflects the complete query.
        /// <para>
        /// This is the end-to-end shape of the reported defect: before the fix, focus left the
        /// textbox after one to two characters, so the remaining characters never reached
        /// <c>SearchText</c> and the selector showed matches for a truncated query. Here the real
        /// <c>QfcItemController.TextBoxSearch_TextChanged</c> handler drives the real
        /// <c>ItemViewer</c> presentation path, one invocation per typed character, and the final
        /// row set is asserted against the results for the complete eight-character query.
        /// </para>
        /// </summary>
        [TestMethod]
        public void EightCharacterQueryTypedThroughTheSeam_DeliversTheFullTextAndCompleteRowSet()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                const string fullQuery = "invoices";
                string typedSoFar = string.Empty;
                var observedQueries = new List<string>();

                Mock<IFolderSearchHandler> folderHandler = new Mock<IFolderSearchHandler>();
                folderHandler
                    .Setup(f =>
                        f.FindFolder(
                            It.IsAny<string>(),
                            It.IsAny<object>(),
                            It.IsAny<bool>(),
                            It.IsAny<List<string>>(),
                            It.IsAny<bool>(),
                            It.IsAny<
                                IEnumerable<(
                                    string root,
                                    string excludedFolder,
                                    bool excludeChildren
                                )>
                            >()
                        )
                    )
                    .Returns(
                        (
                            string searchString,
                            object objItem,
                            bool reload,
                            List<string> roots,
                            bool recalc,
                            IEnumerable<(
                                string root,
                                string excludedFolder,
                                bool excludeChildren
                            )> exclusions
                        ) =>
                        {
                            observedQueries.Add(searchString);
                            return ResultsFor(searchString);
                        }
                    );

                // The controller seam supplies the typed search text and forwards the presentation
                // intent into the real headless ItemViewer pipeline, so the genuine viewer,
                // coordinator, router, and session code all execute.
                Mock<IItemViewer> viewer = new Mock<IItemViewer>();
                viewer.SetupGet(v => v.SearchText).Returns(() => typedSoFar);
                viewer
                    .Setup(v => v.PresentFolderSearchResults(It.IsAny<string[]>()))
                    .Callback<string[]>(items => harness.Viewer.PresentFolderSearchResults(items));

                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_folderHandler",
                    folderHandler.Object
                );

                // Act — type the eight characters one at a time.
                foreach (char typed in fullQuery)
                {
                    typedSoFar += typed;
                    controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
                }

                // Assert — the complete string reached SearchText on the final keystroke.
                typedSoFar
                    .Should()
                    .Be(fullQuery, "no keystroke may be redirected away from the textbox");
                observedQueries.Should().HaveCount(fullQuery.Length);
                observedQueries[observedQueries.Count - 1]
                    .Should()
                    .Be(
                        "*" + fullQuery + "*",
                        "the last query carries the complete eight-character text"
                    );

                // Assert — the presented row set reflects the complete query, not a truncation.
                harness.Viewer.GetFolderItems().Should().Equal(ResultsFor("*" + fullQuery + "*"));
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
                harness.Host.Verify(
                    host => host.Close(It.IsAny<BreadcrumbDropDownCloseReason>()),
                    Times.Never()
                );
            }
        }

        /// <summary>
        /// Error handling: presenting a null result set is rejected explicitly rather than producing
        /// a corrupt row set.
        /// </summary>
        [TestMethod]
        public void PresentSearchResults_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                // Act
                Action act = () => harness.Viewer.BreadcrumbCoordinator.PresentSearchResults(null);

                // Assert
                act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("items");
            }
        }

        /// <summary>
        /// Error handling: a late keystroke that arrives after the pipeline has been disposed is a
        /// deterministic no-op, not a throw. A pooled viewer can be torn down while a queued
        /// <c>TextChanged</c> is still in flight, so the presentation composite bails out as soon as
        /// the upgrade lifetime reports it is no longer live.
        /// </summary>
        [TestMethod]
        public void PresentSearchResults_AfterDisposal_IsADeterministicNoOp()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                BreadcrumbBridgeCoordinator coordinator = harness.Viewer.BreadcrumbCoordinator;
                coordinator.Dispose();

                // Act
                Action act = () => coordinator.PresentSearchResults(FirstResults);

                // Assert
                act.Should().NotThrow();
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Never()
                );
                coordinator.IsSelectorOpen.Should().BeFalse();
            }
        }
    }
}
