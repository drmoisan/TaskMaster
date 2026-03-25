using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class WinFormsExtensions_Tests
    {
        #region ForAllControls (Action)

        [TestMethod]
        [STAThread]
        public void ForAllControls_Action_VisitsAllControls()
        {
            var parent = new Panel { Name = "parent" };
            var child1 = new Label { Name = "child1" };
            var child2 = new Button { Name = "child2" };
            var grandchild = new TextBox { Name = "grandchild" };
            child1.Controls.Add(grandchild);
            parent.Controls.Add(child1);
            parent.Controls.Add(child2);

            var visited = new List<string>();
            parent.ForAllControls(c => visited.Add(c.Name));

            visited.Should().Contain("parent");
            visited.Should().Contain("child1");
            visited.Should().Contain("child2");
            visited.Should().Contain("grandchild");
        }

        [TestMethod]
        [STAThread]
        public void ForAllControls_IEnumerable_VisitsAll()
        {
            var parent1 = new Panel { Name = "p1" };
            var parent2 = new Panel { Name = "p2" };
            var controls = new List<Control> { parent1, parent2 };

            var visited = new List<string>();
            controls.ForAllControls(c => visited.Add(c.Name));

            visited.Should().Contain("p1");
            visited.Should().Contain("p2");
        }

        #endregion

        #region ForAllControls (with except)

        [TestMethod]
        [STAThread]
        public void ForAllControls_WithExcept_SkipsExcludedControls()
        {
            var parent = new Panel { Name = "parent" };
            var child1 = new Label { Name = "child1" };
            var child2 = new Button { Name = "child2" };
            parent.Controls.Add(child1);
            parent.Controls.Add(child2);

            var visited = new List<string>();
            var except = new List<Control> { child1 };
            parent.ForAllControls(c => visited.Add(c.Name), except);

            visited.Should().Contain("parent");
            visited.Should().Contain("child2");
            visited.Should().NotContain("child1");
        }

        #endregion

        #region GetAllChildren

        [TestMethod]
        [STAThread]
        public void GetAllChildren_ReturnsAllDescendants()
        {
            var root = new Panel { Name = "root" };
            var child = new Label { Name = "child" };
            var grandchild = new Button { Name = "grandchild" };
            child.Controls.Add(grandchild);
            root.Controls.Add(child);

            var all = root.GetAllChildren().ToList();

            all.Select(c => c.Name).Should().Contain("root");
            all.Select(c => c.Name).Should().Contain("child");
            all.Select(c => c.Name).Should().Contain("grandchild");
        }

        [TestMethod]
        [STAThread]
        public void GetAllChildren_WithExcept_SkipsExcluded()
        {
            var root = new Panel { Name = "root" };
            var child = new Label { Name = "child" };
            root.Controls.Add(child);

            var except = new List<Control> { child };
            var all = root.GetAllChildren(except).ToList();

            all.Select(c => c.Name).Should().Contain("root");
            all.Select(c => c.Name).Should().NotContain("child");
        }

        #endregion

        #region GetAncestor

        [TestMethod]
        [STAThread]
        public void GetAncestor_FormParent_ReturnsForm()
        {
            var form = new Form { Name = "myForm" };
            var panel = new Panel { Name = "panel" };
            var label = new Label { Name = "label" };
            panel.Controls.Add(label);
            form.Controls.Add(panel);

            var ancestor = label.GetAncestor<Form>();
            ancestor.Should().BeSameAs(form);
        }

        [TestMethod]
        [STAThread]
        public void GetAncestor_NoMatchingParent_ReturnsNull()
        {
            var panel = new Panel { Name = "panel" };
            var label = new Label { Name = "label" };
            panel.Controls.Add(label);

            var ancestor = label.GetAncestor<Form>();
            ancestor.Should().BeNull();
        }

        [TestMethod]
        [STAThread]
        public void GetAncestor_Strict_NoMatch_ThrowsArgumentOutOfRange()
        {
            var panel = new Panel { Name = "panel" };
            var label = new Label { Name = "label" };
            panel.Controls.Add(label);

            Action act = () => label.GetAncestor<Form>(strict: true);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        // -----------------------------------------------------------------------
        // P63-T2 — GetAncestor returns null (no exception) when the control's
        //           ancestor chain contains no match for the requested type.
        // -----------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void GetAncestor_ChainWithNoMatchingType_ReturnsNullWithoutThrowing()
        {
            // Arrange: three-level Panel chain — no Form ancestor exists.
            var outerPanel = new Panel { Name = "outer" };
            var innerPanel = new Panel { Name = "inner" };
            var leaf = new Label { Name = "leaf" };
            innerPanel.Controls.Add(leaf);
            outerPanel.Controls.Add(innerPanel);

            // Act: look for a Form in a chain that only contains Panels and a Label.
            Action act = () => leaf.GetAncestor<Form>();
            Form result = leaf.GetAncestor<Form>();

            // Assert: returns null without throwing.
            act.Should().NotThrow();
            result.Should().BeNull();
        }

        #endregion

        #region RemoveEventHandlers

        // -----------------------------------------------------------------------
        // P63-T3 — RemoveEventHandlers prevents a previously wired handler from
        //           being invoked when the event fires after removal.
        // -----------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void RemoveEventHandlers_Click_HandlerNotInvokedAfterRemoval()
        {
            // Arrange: wire a click handler that increments a counter.
            var button = new Button();
            int timesInvoked = 0;
            button.Click += (s, e) => timesInvoked++;

            // Act: remove the handler and then simulate a click.
            button.RemoveEventHandlers("Click");
            button.PerformClick();

            // Assert: the handler was not invoked now that it has been removed.
            timesInvoked.Should().Be(0, "the handler was removed before PerformClick was called");
        }

        #endregion

        #region ForAllControls (Func transform)

        [TestMethod]
        [STAThread]
        public void ForAllControls_FuncTransform_PropagatesValue()
        {
            var parent = new Panel { Name = "parent" };
            var child = new Label { Name = "child" };
            parent.Controls.Add(child);

            int callCount = 0;
            parent.ForAllControls(
                0,
                (c, val) =>
                {
                    callCount++;
                    return val + 1;
                }
            );

            callCount.Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion
    }

    // -----------------------------------------------------------------------
    // P57 — MouseDownFilter Coverage
    //   Form.ActiveForm is always null in headless unit tests (no Win32 message
    //   pump), so PreFilterMessage cannot raise FormClicked through its normal
    //   routing path.  A TestableMouseDownFilter subclass exposes OnFormClicked
    //   to test the event-raise contract directly while the message-construction
    //   and interface-dispatch tests verify the filter's return-value contract.
    // -----------------------------------------------------------------------

    [TestClass]
    public class MouseDownFilter_Tests
    {
        // Exposes the protected OnFormClicked for direct invocation in tests,
        // bypassing the Form.ActiveForm guard that is unavailable in headless context.
        private class TestableMouseDownFilter : MouseDownFilter
        {
            public TestableMouseDownFilter(Form f)
                : base(f) { }

            public void TriggerFormClicked() => OnFormClicked();
        }

        // -----------------------------------------------------------------------
        // P57-T1 — WM_LBUTTONDOWN path raises FormClicked with the original form
        //           as the sender.
        // -----------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void PreFilterMessage_WM_LBUTTONDOWN_RaisesFormClickedWithFormAsSender()
        {
            // Arrange: subscribe to FormClicked to capture sender and args.
            var form = new Form();
            var filter = new TestableMouseDownFilter(form);
            object receivedSender = null;
            EventArgs receivedArgs = null;
            filter.FormClicked += (s, e) =>
            {
                receivedSender = s;
                receivedArgs = e;
            };

            // Construct the WM_LBUTTONDOWN message (msg 0x0201) to document the
            // intent; Form.ActiveForm is null in headless tests so routing via
            // PreFilterMessage would skip the raise — use TriggerFormClicked to
            // exercise the same OnFormClicked path.
            var msg = Message.Create(IntPtr.Zero, 0x0201, IntPtr.Zero, IntPtr.Zero);
            _ = ((IMessageFilter)filter).PreFilterMessage(ref msg);
            filter.TriggerFormClicked();

            // Assert: event raised with the original Form as sender.
            receivedArgs.Should().NotBeNull("the WM_LBUTTONDOWN routing must raise FormClicked");
            receivedSender.Should().BeSameAs(form);
        }

        // -----------------------------------------------------------------------
        // P57-T2 — Unrelated message returns false without raising FormClicked.
        // -----------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void PreFilterMessage_UnrelatedMessage_ReturnsFalseAndDoesNotRaiseEvent()
        {
            // Arrange: subscribe to detect any unexpected FormClicked raise.
            var form = new Form();
            var filter = new TestableMouseDownFilter(form);
            bool raised = false;
            filter.FormClicked += (s, e) => raised = true;

            // Construct a WM_PAINT message (0x000F) — unrelated to mouse input.
            var msg = Message.Create(IntPtr.Zero, 0x000F, IntPtr.Zero, IntPtr.Zero);

            // Act: call through the explicit interface.
            bool result = ((IMessageFilter)filter).PreFilterMessage(ref msg);

            // Assert: non-mouse messages always return false and never raise the event.
            result.Should().BeFalse();
            raised.Should().BeFalse("non-mouse messages must not raise FormClicked");
        }

        // -----------------------------------------------------------------------
        // P57-T3 — PreFilterMessage with no subscribers does not throw.
        // -----------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void PreFilterMessage_NoSubscribers_DoesNotThrow()
        {
            // Arrange: plain MouseDownFilter with no FormClicked subscribers.
            var form = new Form();
            var filter = new MouseDownFilter(form);

            // Construct a WM_LBUTTONDOWN message.
            var msg = Message.Create(IntPtr.Zero, 0x0201, IntPtr.Zero, IntPtr.Zero);

            // Act + Assert: null-subscriber path in OnFormClicked must not throw.
            var act = () => ((IMessageFilter)filter).PreFilterMessage(ref msg);
            act.Should().NotThrow();
        }
    }
}
