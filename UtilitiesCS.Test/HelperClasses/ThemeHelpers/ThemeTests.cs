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
    }
}
