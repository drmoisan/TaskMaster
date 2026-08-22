using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for <see cref="QfcExplorerController"/>. Covers the defect-2 regression (the
    /// controller must act on the explorer it captured at construction, not one re-resolved later),
    /// the injectable modal-dialog seam, and the conversation-view state machine.
    /// </summary>
    /// <remarks>
    /// Branch-control detail that governs every arrangement in this class: the fixture constructs the
    /// controller with <c>QfEnums.InitTypeEnum.Find</c> (value 2, per
    /// <c>QuickFiler/Helper Classes/QfEnums.cs:8</c>), which makes
    /// <c>_initType.HasFlag(QfEnums.InitTypeEnum.Sort)</c> false in <c>OpenQFItem</c>. That does NOT
    /// make the <c>CommandBars</c> setup optional. Both conjunctions in <c>OpenQFItem</c> use the
    /// NON-SHORT-CIRCUITING <c>&amp;</c> operator rather than <c>&amp;&amp;</c>, so
    /// <c>AutoFile.AreConversationsGrouped(_activeExplorer)</c> is still evaluated even when the
    /// left operand is false, and that helper reads
    /// <c>ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations")</c>. The
    /// <c>CommandBars</c> mock setup is therefore MANDATORY on every explorer the controller may
    /// consult, not optional.
    /// </remarks>
    [TestClass]
    public partial class QfcExplorerControllerTests
    {
        private const string ShowInConversationsMso = "ShowInConversations";
        private const string TemporaryViewName = "tmpNoConversation";
        private const string WideViewName = "Wide";

        private MockRepository _repo;
        private Mock<Microsoft.Office.Core.CommandBars> _commandBars;
        private Mock<Outlook.Explorer> _explorer;
        private Mock<Outlook.Application> _olApp;
        private Mock<IApplicationGlobals> _globals;
        private Mock<IFilerFormController> _formController;
        private Mock<IFilerHomeController> _parent;

        /// <summary>
        /// Builds the shared mock graph the constructor requires. The constructor reaches COM at
        /// exactly one point, <c>_globals.Ol.App.ActiveExplorer()</c>, so the chain
        /// <c>IApplicationGlobals -&gt; IOlObjects.App -&gt; Application.ActiveExplorer()</c> is the
        /// whole of the construction dependency. <see cref="MockBehavior.Loose"/> is used so that an
        /// unexpected member access surfaces as an assertion failure rather than as a Moq
        /// strict-mode exception, which keeps pre-fix failures readable.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _repo = new MockRepository(MockBehavior.Loose);

            _commandBars = _repo.Create<Microsoft.Office.Core.CommandBars>();
            _commandBars.Setup(c => c.GetPressedMso(ShowInConversationsMso)).Returns(false);

            _explorer = _repo.Create<Outlook.Explorer>();
            _explorer.Setup(e => e.CommandBars).Returns(_commandBars.Object);

            _olApp = _repo.Create<Outlook.Application>();
            _olApp.Setup(a => a.ActiveExplorer()).Returns(_explorer.Object);

            _globals = _repo.Create<IApplicationGlobals>();
            _globals.SetupGet(g => g.Ol.App).Returns(_olApp.Object);
            _globals.SetupGet(g => g.Ol.ViewWide).Returns(WideViewName);

            _formController = _repo.Create<IFilerFormController>();
            _parent = _repo.Create<IFilerHomeController>();
            _parent.SetupGet(p => p.FormController).Returns(_formController.Object);
        }

        /// <summary>
        /// Creates the system under test against the shared fixture graph.
        /// </summary>
        private QfcExplorerController CreateController()
        {
            return new QfcExplorerController(
                QfEnums.InitTypeEnum.Find,
                _globals.Object,
                _parent.Object
            );
        }

        /// <summary>
        /// Creates a mock folder whose <c>FolderPath</c> is the supplied value.
        /// </summary>
        private Mock<MAPIFolder> CreateFolder(string folderPath)
        {
            var folder = _repo.Create<MAPIFolder>();
            folder.SetupGet(f => f.FolderPath).Returns(folderPath);
            return folder;
        }

        /// <summary>
        /// Creates a mock mail item whose <c>Parent</c> is the supplied folder.
        /// </summary>
        private Mock<MailItem> CreateMailItem(Mock<MAPIFolder> parentFolder)
        {
            var mailItem = _repo.Create<MailItem>();
            mailItem.SetupGet(m => m.Parent).Returns(parentFolder.Object);
            return mailItem;
        }

        /// <summary>
        /// Arranges the <c>Views</c> collection reached by <c>ExplConvView_ToggleOn</c> through
        /// <c>_activeExplorer.CurrentFolder.Views[_objViewMem]</c>, returning the supplied view for any
        /// index.
        /// </summary>
        /// <remarks>
        /// The PIA declares the <c>Views</c> indexer parameter as <c>object</c>, so the setup uses
        /// <c>It.IsAny&lt;object&gt;()</c>. The indexer-mocking form follows the in-repo precedent at
        /// <c>QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:64-65</c>.
        /// </remarks>
        private Mock<Views> ArrangeViewsIndexer(Mock<Outlook.View> view)
        {
            var views = _repo.Create<Views>();
            views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object);
            return views;
        }

        /// <summary>
        /// Defect-2 regression. <c>OpenQFItem</c> reaches the private helper
        /// <c>NavigateToOutlookFolder</c>, which must assign the destination folder to the explorer
        /// the controller CAPTURED at construction, not to one it re-resolves from
        /// <c>_globals.Ol.App.ActiveExplorer()</c> at call time. When the active explorer has changed
        /// between construction and the call, re-resolving navigates the wrong window.
        /// </summary>
        /// <remarks>
        /// The two explorers are made distinguishable by sequencing <c>ActiveExplorer()</c>: the first
        /// call is consumed by the constructor and yields the captured explorer, the second yields a
        /// drifted explorer that only a re-resolution could reach. Before the fix the assignment
        /// lands on the drifted explorer and BOTH assertions fail; after the fix it lands on the
        /// captured explorer and both pass.
        /// </remarks>
        [TestMethod]
        public async Task OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer()
        {
            // Arrange — a drifted explorer that is reachable ONLY by re-resolving ActiveExplorer().
            // MockBehavior.Loose is deliberate: it lets a pre-fix assignment land harmlessly so the
            // failure surfaces as a FluentAssertions/Moq verification message rather than as a
            // strict-mode exception thrown from inside production code.
            var driftedCommandBars = _repo.Create<Microsoft.Office.Core.CommandBars>();
            driftedCommandBars.Setup(c => c.GetPressedMso(ShowInConversationsMso)).Returns(false);
            var driftedExplorer = _repo.Create<Outlook.Explorer>();
            driftedExplorer.Setup(e => e.CommandBars).Returns(driftedCommandBars.Object);

            // First call is consumed by the constructor at QfcExplorerController.cs:35.
            // Second call is what the pre-fix line 140 resolves.
            _olApp
                .SetupSequence(a => a.ActiveExplorer())
                .Returns(_explorer.Object)
                .Returns(driftedExplorer.Object);

            // Arrange the guard so the navigation branch is ENTERED: the captured explorer's current
            // folder path must differ from the mail item's parent folder path.
            var currentFolder = CreateFolder(@"\\Mailbox\A");
            var destination = CreateFolder(@"\\Mailbox\B");
            _explorer.SetupGet(e => e.CurrentFolder).Returns(currentFolder.Object);
            var mailItem = CreateMailItem(destination);

            // Keep the not-in-view dialog branch unreachable; this test is about navigation only.
            _explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);

            var controller = CreateController();

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert — the destination is assigned to the CAPTURED explorer exactly once ...
            _explorer.VerifySet(e => e.CurrentFolder = destination.Object, Times.Once());

            // ... and the drifted explorer is never navigated at all.
            driftedExplorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never());
        }

        /// <summary>
        /// Arranges the not-in-view branch of <c>OpenQFItem</c>: the mail item is not selectable in
        /// the current view, and the mail item's parent folder matches the current folder so the
        /// navigation branch is skipped and the test isolates the dialog path.
        /// </summary>
        private Mock<MailItem> ArrangeNotInViewBranch()
        {
            var sameFolder = CreateFolder(@"\\Mailbox\A");
            _explorer.SetupGet(e => e.CurrentFolder).Returns(sameFolder.Object);
            var mailItem = CreateMailItem(sameFolder);
            _explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(false);
            return mailItem;
        }

        /// <summary>
        /// The not-in-view branch must route its modal prompt through the injectable
        /// <c>NotInViewDialogInvoker</c> seam exactly once, passing the unchanged user-visible text,
        /// caption, buttons, and icon. The seam's production default is never exercised, so no dialog
        /// is displayed and no message pump is required.
        /// </summary>
        [TestMethod]
        public async Task OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce()
        {
            // Arrange
            var mailItem = ArrangeNotInViewBranch();
            var controller = CreateController();

            var invocationCount = 0;
            string observedText = null;
            string observedCaption = null;
            MessageBoxButtons observedButtons = default;
            MessageBoxIcon observedIcon = default;

            controller.NotInViewDialogInvoker = (text, caption, buttons, icon) =>
            {
                invocationCount++;
                observedText = text;
                observedCaption = caption;
                observedButtons = buttons;
                observedIcon = icon;
                return DialogResult.No;
            };

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert
            invocationCount
                .Should()
                .Be(1, "the not-in-view branch must consult the dialog seam exactly once");
            observedText.Should().Be("Selected message is not in view. Would you like to open it?");
            observedCaption.Should().Be("Error");
            observedButtons.Should().Be(MessageBoxButtons.YesNo);
            observedIcon.Should().Be(MessageBoxIcon.Error);
        }

        /// <summary>
        /// When the user answers Yes to the not-in-view prompt, the mail item is displayed.
        /// </summary>
        [TestMethod]
        public async Task OpenQFItem_WhenDialogSeamReturnsYes_DisplaysMailItem()
        {
            // Arrange
            var mailItem = ArrangeNotInViewBranch();
            var controller = CreateController();
            controller.NotInViewDialogInvoker = (text, caption, buttons, icon) => DialogResult.Yes;

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert
            mailItem.Verify(m => m.Display(It.IsAny<object>()), Times.Once());
        }

        /// <summary>
        /// When the user answers No to the not-in-view prompt, the mail item is not displayed.
        /// </summary>
        [TestMethod]
        public async Task OpenQFItem_WhenDialogSeamReturnsNo_DoesNotDisplayMailItem()
        {
            // Arrange
            var mailItem = ArrangeNotInViewBranch();
            var controller = CreateController();
            controller.NotInViewDialogInvoker = (text, caption, buttons, icon) => DialogResult.No;

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert
            mailItem.Verify(m => m.Display(It.IsAny<object>()), Times.Never());
        }

        /// <summary>
        /// Characterises the defect-2 guard at <c>NavigateToOutlookFolder</c>: when the mail item's
        /// parent folder path already equals the explorer's current folder path there is nothing to
        /// navigate to, so no explorer's <c>CurrentFolder</c> may be assigned.
        /// </summary>
        [TestMethod]
        public async Task OpenQFItem_WhenMailIsAlreadyInTheCurrentFolder_DoesNotChangeCurrentFolder()
        {
            // Arrange — a drifted explorer, so the assertion also proves no re-resolution occurred.
            var driftedCommandBars = _repo.Create<Microsoft.Office.Core.CommandBars>();
            driftedCommandBars.Setup(c => c.GetPressedMso(ShowInConversationsMso)).Returns(false);
            var driftedExplorer = _repo.Create<Outlook.Explorer>();
            driftedExplorer.Setup(e => e.CommandBars).Returns(driftedCommandBars.Object);
            _olApp
                .SetupSequence(a => a.ActiveExplorer())
                .Returns(_explorer.Object)
                .Returns(driftedExplorer.Object);

            // The SAME folder object on both sides, so the guard's inequality is false.
            var sameFolder = CreateFolder(@"\\Mailbox\A");
            _explorer.SetupGet(e => e.CurrentFolder).Returns(sameFolder.Object);
            var mailItem = CreateMailItem(sameFolder);
            _explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);

            var controller = CreateController();

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert — neither explorer is navigated.
            _explorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never());
            driftedExplorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never());
        }

        /// <summary>
        /// Characterises the in-view positive path of <c>OpenQFItem</c>: when the item is selectable in
        /// the current view the selection is cleared and the item added, and no dialog is consulted.
        /// </summary>
        [TestMethod]
        public async Task OpenQFItem_WhenItemIsSelectableInView_ClearsAndAddsSelection()
        {
            // Arrange
            var sameFolder = CreateFolder(@"\\Mailbox\A");
            _explorer.SetupGet(e => e.CurrentFolder).Returns(sameFolder.Object);
            var mailItem = CreateMailItem(sameFolder);
            _explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true);

            var controller = CreateController();
            var dialogInvocations = 0;
            controller.NotInViewDialogInvoker = (text, caption, buttons, icon) =>
            {
                dialogInvocations++;
                return DialogResult.No;
            };

            // Act
            await controller.OpenQFItem(mailItem.Object);

            // Assert
            _explorer.Verify(e => e.ClearSelection(), Times.Once());
            _explorer.Verify(e => e.AddToSelection(mailItem.Object), Times.Once());
            dialogInvocations
                .Should()
                .Be(0, "the in-view path must not consult the not-in-view dialog seam");
        }

        /// <summary>
        /// The internal <c>CurrentConversationState</c> property is a direct projection of the
        /// command-bar pressed state, reached from the test assembly through
        /// <c>[assembly: InternalsVisibleTo("QuickFiler.Test")]</c>.
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CurrentConversationState_ReflectsCommandBarPressedState(bool pressed)
        {
            // Arrange
            _commandBars.Setup(c => c.GetPressedMso(ShowInConversationsMso)).Returns(pressed);
            var controller = CreateController();

            // Act
            bool actual = controller.CurrentConversationState;

            // Assert
            actual.Should().Be(pressed);
        }

        /// <summary>
        /// <c>ExplConvView_ReturnState</c> delegates to <c>ExplConvView_ToggleOn</c> when the flag is
        /// set, which is observable as the remembered view being applied and the flag being cleared.
        /// </summary>
        [TestMethod]
        public void ExplConvView_ReturnState_WhenFlagSet_TogglesOn()
        {
            // Arrange
            var rememberedView = _repo.Create<Outlook.View>();
            var views = ArrangeViewsIndexer(rememberedView);
            var currentFolder = _repo.Create<MAPIFolder>();
            currentFolder.SetupGet(f => f.Views).Returns(views.Object);
            _explorer.SetupGet(e => e.CurrentFolder).Returns(currentFolder.Object);

            var controller = CreateController();
            controller.BlShowInConversations = true;

            // Act
            controller.ExplConvView_ReturnState();

            // Assert
            rememberedView.Verify(v => v.Apply(), Times.Once());
            controller.BlShowInConversations.Should().BeFalse();
        }
    }
}
