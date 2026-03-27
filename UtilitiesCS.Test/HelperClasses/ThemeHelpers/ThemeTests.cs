using System;
using System.Collections.Generic;
using System.Drawing;
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

    [TestClass]
    public class ThemeControlGroup_Tests
    {
        #region OneField Constructor

        [TestMethod]
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
        public void ApplyTheme_HeterogeneousControls_DoesNotThrow()
        {
            // Arrange: mix of different WinForms control subtypes.
            var controls = new List<Control> { new Label(), new Button(), new Panel() };
            var group = new ThemeControlGroup(controls, Color.White, Color.Black);

            // Act + Assert: ThemeControlGroup treats all Control subtypes uniformly.
            var act = () => group.ApplyTheme();
            act.Should().NotThrow();
        }

        #endregion
    }
}
