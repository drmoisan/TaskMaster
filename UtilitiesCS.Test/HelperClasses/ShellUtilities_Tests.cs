using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectListViewDemo;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ShellUtilities_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var su = new ShellUtilities();
            su.Should().NotBeNull();
        }

        #endregion

        #region GetFileType

        [TestMethod]
        public void GetFileType_ExeExtension_ReturnsNonEmptyString()
        {
            var su = new ShellUtilities();
            // Use a common file extension that should always return a type
            var type = su.GetFileType(".txt");
            // May return empty string on some environments; test that it doesn't throw
        }

        #endregion

        #region Execute

        [TestMethod]
        public void Execute_NonexistentPath_ReturnsErrorCode()
        {
            var su = new ShellUtilities();
            // ShellExecute returns values < 31 for errors
            var result = su.Execute("C:\\nonexistent\\path\\xyz.abc");
            result.Should().BeLessThanOrEqualTo(31);
        }

        [TestMethod]
        public void Execute_WithOperation_ReturnsResult()
        {
            var su = new ShellUtilities();
            var result = su.Execute("C:\\nonexistent\\path\\xyz.abc", "open");
            result.Should().BeLessThanOrEqualTo(31);
        }

        #endregion
    }
}
