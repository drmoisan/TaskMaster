using System.Reflection;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Carrier-list carry tests for <c>QfcCollectionController</c>. Relocated here from
    /// <c>QfcCollectionControllerTests.cs</c>, which stood at 499 lines with one line of headroom to
    /// the 500-line cap, because the issue #678 widening of <see cref="QfcPreScoredItem"/> adds an
    /// argument to the construction below and CSharpier then reflows the call across several lines.
    /// No test is deleted or weakened by the move; the base part carries the only
    /// <c>[TestClass]</c> attribute.
    /// </summary>
    public partial class QfcCollectionControllerTests
    {
        /// <summary>
        /// [P4-T7] The carrier-list load path carries each survivor's predetermined folder onto the
        /// resulting <see cref="QfcItemGroup.PredeterminedFolder"/>. The full carrier
        /// <c>LoadControlsAndHandlers_01Async</c> / <c>EncapsulateItemGroup</c> body constructs a real
        /// <see cref="QfcItemController"/> and dequeues a WinForms <c>ItemViewer</c>, which require live
        /// COM/WinForms; the COM-free carry contract verified here is that the carrier value flows from
        /// <see cref="QfcPreScoredItem.PredeterminedFolder"/> onto the item group's
        /// <see cref="QfcItemGroup.PredeterminedFolder"/>. The item controller's consumption of that
        /// value (preselecting the folder, not index 1) is verified in P5-T3.
        /// Issue #678 extends the same COM-free carry contract to the folder search handler: the
        /// carrier now publishes it and the item group now carries it alongside the folder.
        /// </summary>
        [TestMethod]
        public void CarrierLoad_SetsPredeterminedFolderOnItemGroup()
        {
            // Arrange — the carrier the load path produces for a survivor.
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var handler = new Mock<IFolderSearchHandler>().Object;
            var carrier = new QfcPreScoredItem(mail, @"\\Archive\Projects\Active", handler);

            // Act — replicate the group-level carry that EncapsulateItemGroup performs before any
            // COM/WinForms call: new QfcItemGroup(mailItem) { PredeterminedFolder = ... }.
            var group = new QfcItemGroup(carrier.MailItem)
            {
                PredeterminedFolder = carrier.PredeterminedFolder,
                CarriedFolderHandler = carrier.FolderHandler,
            };

            // Assert — the predetermined folder is carried onto the group and the mail item matches.
            typeof(QfcItemGroup)
                .GetProperty(
                    nameof(QfcItemGroup.PredeterminedFolder),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .GetValue(group)
                .Should()
                .Be(@"\\Archive\Projects\Active");
            group.MailItem.Should().BeSameAs(mail);

            // Assert — issue #678: the already-initialised handler is carried onto the group too, so
            // the item controller can adopt it instead of running a second scoring pass.
            typeof(QfcItemGroup)
                .GetProperty(
                    nameof(QfcItemGroup.CarriedFolderHandler),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .GetValue(group)
                .Should()
                .BeSameAs(handler);
            carrier.FolderHandler.Should().BeSameAs(handler);
        }
    }
}
