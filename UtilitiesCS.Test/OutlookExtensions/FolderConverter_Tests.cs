using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test.OutlookExtensions
{
    [TestClass]
    public class FolderConverter_Tests
    {
        [TestMethod]
        public void ToFsFolderPath_TestState_Expected()
        {
            // Arrange
            string olBranchPath = "first.last@company.com\\Ol Level 1\\Common Level A\\Common Level B";
            string olAncestorPath = "first.last@company.com\\Ol Level 1";
            string fsAncestorEquivalent = "C:\\Fs Level 1\\Fs Level 2\\Fs Level 3";
            string expected = "C:\\Fs Level 1\\Fs Level 2\\Fs Level 3\\Common Level A\\Common Level B";

            // Act
            string actual = olBranchPath.ToFsFolderpath(olAncestorPath, fsAncestorEquivalent);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
