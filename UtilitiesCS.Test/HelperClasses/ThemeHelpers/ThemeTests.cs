using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses.ThemeHelpers
{
    [TestClass]
    public class Theme_Tests
    {
        #region Constructor

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var theme = new Theme();
            theme.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNameAndControlGroups_SetsProperties()
        {
            var groups = new Dictionary<string, ThemeControlGroup>();
            var theme = new Theme("Dark", groups);

            theme.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_NullControlGroups_ThrowsArgumentNullException()
        {
            Action act = () => new Theme("Dark", null);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Color Properties

        [TestMethod]
        public void NavBackColor_SetAndGet()
        {
            var theme = new Theme();
            theme.NavBackColor = Color.DarkBlue;
            theme.NavBackColor.Should().Be(Color.DarkBlue);
        }

        [TestMethod]
        public void NavForeColor_SetAndGet()
        {
            var theme = new Theme();
            theme.NavForeColor = Color.White;
            theme.NavForeColor.Should().Be(Color.White);
        }

        #endregion
    }

    [STATestClass]
    public class ThemeControlGroup_Tests
    {
        #region OneField Constructor

        [TestMethod]
        public void Constructor_OneField_CreatesInstance()
        {
            var controls = new List<Control> { new Label() };
            var group = new ThemeControlGroup(controls, Color.Red);

            group.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_OneField_NullControls_ThrowsArgumentNullException()
        {
            Action act = () => new ThemeControlGroup(null, Color.Red);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_OneField_EmptyControls_ThrowsArgumentOutOfRange()
        {
            Action act = () => new ThemeControlGroup(new List<Control>(), Color.Red);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region TwoField Constructor

        [TestMethod]
        public void Constructor_TwoField_CreatesInstance()
        {
            var controls = new List<Control> { new Label() };
            var group = new ThemeControlGroup(controls, Color.White, Color.Black);

            group.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_TwoField_NullControls_ThrowsArgumentNullException()
        {
            Action act = () => new ThemeControlGroup(null, Color.White, Color.Black);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_TwoField_EmptyControls_ThrowsArgumentOutOfRange()
        {
            Action act = () => new ThemeControlGroup(new List<Control>(), Color.White, Color.Black);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region TwoFieldAlt Constructor

        [TestMethod]
        public void Constructor_TwoFieldAlt_CreatesInstance()
        {
            var controls = new List<Control> { new Label() };
            var group = new ThemeControlGroup(
                controls,
                Color.White,
                Color.Black,
                Color.Gray,
                Color.DarkGray,
                () => false
            );

            group.Should().NotBeNull();
        }

        #endregion

        #region TwoFieldAltHover Constructor

        [TestMethod]
        public void Constructor_TwoFieldAltHover_CreatesInstance()
        {
            var controls = new List<Control> { new Label() };
            var group = new ThemeControlGroup(
                controls,
                Color.White,
                Color.Black,
                Color.Gray,
                Color.DarkGray,
                Color.LightBlue,
                (obj) => false
            );

            group.Should().NotBeNull();
        }

        #endregion

        #region ApplyTheme

        // -----------------------------------------------------------------------
        // P60-T1 — ApplyTheme (TwoField) sets ForeColor and BackColor on all controls
        // -----------------------------------------------------------------------

        [TestMethod]
        public void ApplyTheme_TwoField_SetsExpectedColors()
        {
            // Arrange: two controls with a known fore/back pair.
            var label = new Label();
            var button = new Button();
            var controls = new List<Control> { label, button };
            var group = new ThemeControlGroup(controls, Color.White, Color.Black);

            // Act: apply the theme.
            group.ApplyTheme();

            // Assert: every control received the exact colors from the group config.
            label.ForeColor.Should().Be(Color.White);
            label.BackColor.Should().Be(Color.Black);
            button.ForeColor.Should().Be(Color.White);
            button.BackColor.Should().Be(Color.Black);
        }

        // -----------------------------------------------------------------------
        // P60-T2 — ApplyTheme (TwoFieldAlt, isAlt = true) applies the alternate
        //           color set to all controls in the group.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void ApplyTheme_TwoFieldAlt_IsAltTrue_SetsAltColors()
        {
            // Arrange: alternate-selector always returns true so alt colors apply.
            var label = new Label();
            var controls = new List<Control> { label };
            var group = new ThemeControlGroup(
                controls,
                foreMain: Color.White,
                backMain: Color.Black,
                foreAlt: Color.Yellow,
                backAlt: Color.DarkBlue,
                isAlt: () => true
            );

            // Act: apply the theme — IsAlt=true path should route to alt colors.
            group.ApplyTheme();

            // Assert: alt colors applied, not the main colors.
            label
                .ForeColor.Should()
                .Be(Color.Yellow, "IsAlt=true must use the alternate fore color");
            label
                .BackColor.Should()
                .Be(Color.DarkBlue, "IsAlt=true must use the alternate back color");
        }

        // -----------------------------------------------------------------------
        // P60-T3 — ApplyTheme with heterogeneous control types (Label, Button,
        //           Panel) does not throw — all Control subtypes share ForeColor /
        //           BackColor and are treated uniformly.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void ApplyTheme_HeterogeneousControls_DoesNotThrow()
        {
            // Arrange: mix of different WinForms control subtypes.
            var controls = new List<Control> { new Label(), new Button(), new Panel() };
            var group = new ThemeControlGroup(controls, Color.White, Color.Black);

            // Act + Assert: ThemeControlGroup treats all Control subtypes uniformly.
            var act = () => group.ApplyTheme();
            act.Should().NotThrow();
        }

        [TestMethod]
        public void GroupName_SetAndGet_RoundTripsAssignedValue()
        {
            var group = new ThemeControlGroup(new List<Control> { new Label() }, Color.Red);

            group.GroupName = "Navigation";

            group.GroupName.Should().Be("Navigation");
        }

        [TestMethod]
        public void ApplyTheme_OneField_SetsBackColorOnAllControls()
        {
            var label = new Label();
            var panel = new Panel();
            var group = new ThemeControlGroup(new List<Control> { label, panel }, Color.DarkRed);

            group.ApplyTheme();

            label.BackColor.Should().Be(Color.DarkRed);
            panel.BackColor.Should().Be(Color.DarkRed);
        }

        [TestMethod]
        public void ApplyTheme_TwoFieldAlt_IsAltFalse_SetsMainColors()
        {
            var label = new Label();
            var group = new ThemeControlGroup(
                new List<Control> { label },
                foreMain: Color.White,
                backMain: Color.Black,
                foreAlt: Color.Yellow,
                backAlt: Color.DarkBlue,
                isAlt: () => false
            );

            group.ApplyTheme();

            label.ForeColor.Should().Be(Color.White);
            label.BackColor.Should().Be(Color.Black);
        }

        [TestMethod]
        public void ApplyTheme_BoolOverload_WithObjectSetterGroup_InvokesSetterThroughElseBranch()
        {
            var objects = new List<object> { "alpha", "beta" };
            IList<object> assignedObjects = null;
            Color assignedFore = default;
            Color assignedBack = default;
            var group = new ThemeControlGroup(
                objects,
                Color.Gold,
                Color.Navy,
                (targets, fore, back) =>
                {
                    assignedObjects = targets;
                    assignedFore = fore;
                    assignedBack = back;
                }
            );

            group.ApplyTheme(async: false);

            assignedObjects.Should().BeSameAs(objects);
            assignedFore.Should().Be(Color.Gold);
            assignedBack.Should().Be(Color.Navy);
        }

        [TestMethod]
        public void ApplyTheme_WithUnsupportedGroupType_ThrowsArgumentOutOfRangeException()
        {
            var group = (ThemeControlGroup)
                Activator.CreateInstance(typeof(ThemeControlGroup), true);

            Action act = () => group.ApplyTheme();

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ApplyTheme_TwoFieldAltHover_SetsEventColorsForAltAndMainControls()
        {
            var mainControl = new Label();
            var altControl = new Button();
            var group = new ThemeControlGroup(
                new List<Control> { mainControl, altControl },
                foreMain: Color.White,
                backMain: Color.Black,
                foreAlt: Color.Yellow,
                backAlt: Color.DarkBlue,
                hover: Color.Orange,
                isAltHover: control => ReferenceEquals(control, altControl)
            );

            group.ApplyTheme();

            mainControl.ForeColor.Should().Be(Color.White);
            mainControl.BackColor.Should().Be(Color.Black);
            altControl.ForeColor.Should().Be(Color.Yellow);
            altControl.BackColor.Should().Be(Color.DarkBlue);
        }

        [TestMethod]
        public void HoverHandlers_UpdateBackColorForMouseEnterAndLeave()
        {
            var mainControl = new Label();
            var altControl = new Button();
            var group = new ThemeControlGroup(
                new List<Control> { mainControl, altControl },
                foreMain: Color.White,
                backMain: Color.Black,
                foreAlt: Color.Yellow,
                backAlt: Color.DarkBlue,
                hover: Color.Orange,
                isAltHover: control => ReferenceEquals(control, altControl)
            );

            group.ApplyTheme();
            InvokeNonPublic(group, "Control_MouseEnter", altControl, EventArgs.Empty);
            altControl.BackColor.Should().Be(Color.Orange);
            InvokeNonPublic(group, "Control_MouseLeave", altControl, EventArgs.Empty);
            altControl.BackColor.Should().Be(Color.DarkBlue);

            InvokeNonPublic(group, "Control_MouseEnter", mainControl, EventArgs.Empty);
            mainControl.BackColor.Should().Be(Color.Orange);
            InvokeNonPublic(group, "Control_MouseLeave", mainControl, EventArgs.Empty);
            mainControl.BackColor.Should().Be(Color.Black);
        }

        [TestMethod]
        public void DeactivateEvents_TwoFieldAltHover_DoesNotThrowAfterWiringHandlers()
        {
            var group = new ThemeControlGroup(
                new List<Control> { new Label(), new Button() },
                foreMain: Color.White,
                backMain: Color.Black,
                foreAlt: Color.Yellow,
                backAlt: Color.DarkBlue,
                hover: Color.Orange,
                isAltHover: _ => false
            );
            group.ApplyTheme();

            Action act = () => group.DeactivateEvents();

            act.Should().NotThrow();
        }

        [TestMethod]
        public void DeactivateEvents_NonHoverGroup_DefaultBranchDoesNothing()
        {
            var group = new ThemeControlGroup(new List<Control> { new Label() }, Color.Red);

            Action act = () => group.DeactivateEvents();

            act.Should().NotThrow();
        }

        private static void InvokeNonPublic(
            object instance,
            string methodName,
            params object[] args
        )
        {
            instance
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(instance, args);
        }

        #endregion
    }
}
