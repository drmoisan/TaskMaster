using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence.EmailParsingSorting
{
    /// <summary>
    /// Covers the pure, directly-testable tessdata-path-resolution seam extracted from
    /// <see cref="TesseractOcrTextExtractor.ExtractText"/> (see issue #209 remediation).
    /// </summary>
    [TestClass]
    public class TesseractOcrTextExtractor_Tests
    {
        [TestMethod]
        public void ResolveTessdataPath_ReturnsLocalAppDataTaskMasterTessdataPath()
        {
            // Arrange
            string expected =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}TaskMaster{Path.DirectorySeparatorChar}tessdata";

            // Act
            string actual = TesseractOcrTextExtractor.ResolveTessdataPath();

            // Assert
            actual.Should().Be(expected);
        }
    }
}
