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
            parent.ForAllControls(0, (c, val) => { callCount++; return val + 1; });

            callCount.Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion
    }
}
